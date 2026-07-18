using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Buddy.Coroutines;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Handles catalog parsing and session-scoped profile materialization while leaving authorization to the plugin.
/// </summary>
public abstract class ProtectedServerProfileSource : IServerProfileSource
{
    private const string CatalogResourceName = "profile.catalog";

    private readonly SemaphoreSlim _resourceGate = new(1, 1);
    private readonly object _profileFileGate = new();
    private readonly Dictionary<string, byte[]> _profileCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _profileDirectory;
    private IReadOnlyList<ProtectedServerProfile>? _catalog;
    private string? _credentialFingerprint;
    private volatile bool _clearRequested;

    protected ProtectedServerProfileSource(string cacheNamespace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheNamespace);
        _profileDirectory = Path.Combine(Path.GetTempPath(), "LlamaLibrary", "ProtectedProfiles", SafePathPart(cacheNamespace));
    }

    /// <summary>
    /// Returns a non-secret fingerprint that changes whenever the active credential changes.
    /// </summary>
    protected abstract string CredentialFingerprint { get; }

    /// <summary>
    /// Retrieves one authorized resource using credentials owned by the calling plugin.
    /// </summary>
    protected abstract Task<byte[]?> FetchResourceAsync(string resourceName);

    public async Task<IReadOnlyList<ServerProfile>> GetProfilesAsync()
    {
        await WaitForResourceGateAsync();
        try
        {
            EnsureCredentialScope();
            if (_catalog == null)
            {
                var data = await FetchRequiredResourceAsync(CatalogResourceName);
                var json = Encoding.UTF8.GetString(data);
                var profiles = JsonConvert.DeserializeObject<List<ProtectedServerProfile>>(json)
                               ?? throw new InvalidOperationException("The protected profile catalog is empty.");

                var invalidCount = profiles.Count(profile =>
                    string.IsNullOrWhiteSpace(profile.Name) || string.IsNullOrWhiteSpace(profile.ResourceName));
                if (invalidCount > 0)
                {
                    throw new InvalidOperationException($"The protected profile catalog contains {invalidCount} invalid entries.");
                }

                _catalog = profiles;
            }

            return _catalog.Cast<ServerProfile>().ToArray();
        }
        finally
        {
            ClearCacheIfRequested();
            _resourceGate.Release();
        }
    }

    public async Task<string?> MaterializeProfileAsync(ServerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        var resourceName = (profile as ProtectedServerProfile)?.ResourceName;
        if (string.IsNullOrWhiteSpace(resourceName) && !string.IsNullOrWhiteSpace(profile.Name))
        {
            var profiles = await GetProfilesAsync();
            resourceName = (profiles.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, profile.Name, StringComparison.OrdinalIgnoreCase)) as ProtectedServerProfile)?.ResourceName;
        }

        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return null;
        }

        var data = await GetProfileDataAsync(resourceName);
        if (data is not { Length: > 0 })
        {
            return null;
        }

        Directory.CreateDirectory(_profileDirectory);
        var path = Path.Combine(_profileDirectory, $"{ResourceFileName(resourceName, data)}.xml");
        lock (_profileFileGate)
        {
            if (!File.Exists(path))
            {
                File.WriteAllBytes(path, data);
            }
        }

        return path;
    }

    /// <summary>
    /// Clears data cached for the current plugin session.
    /// </summary>
    public void ClearCache()
    {
        if (!_resourceGate.Wait(0))
        {
            _clearRequested = true;
            return;
        }

        try
        {
            ClearCacheCore();
        }
        finally
        {
            _resourceGate.Release();
        }
    }

    /// <summary>
    /// Creates a stable fingerprint without exposing the credential in logs or filenames.
    /// </summary>
    protected static string Fingerprint(string? credential)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential ?? string.Empty)));

    private async Task<byte[]?> GetProfileDataAsync(string resourceName)
    {
        await WaitForResourceGateAsync();
        try
        {
            EnsureCredentialScope();
            if (_profileCache.TryGetValue(resourceName, out var cachedProfile))
            {
                return cachedProfile;
            }

            var data = await FetchRequiredResourceAsync(resourceName);
            ValidateProfileXml(resourceName, data);
            _profileCache[resourceName] = data;
            return data;
        }
        finally
        {
            ClearCacheIfRequested();
            _resourceGate.Release();
        }
    }

    private async Task<byte[]> FetchRequiredResourceAsync(string resourceName)
        => await FetchResourceAsync(resourceName)
           ?? throw new InvalidOperationException($"The authorized resource request returned no data for '{resourceName}'.");

    private void EnsureCredentialScope()
    {
        var fingerprint = CredentialFingerprint;
        if (string.Equals(_credentialFingerprint, fingerprint, StringComparison.Ordinal))
        {
            return;
        }

        _catalog = null;
        _profileCache.Clear();
        _credentialFingerprint = fingerprint;
    }

    private void ClearCacheIfRequested()
    {
        if (_clearRequested)
        {
            ClearCacheCore();
        }
    }

    private void ClearCacheCore()
    {
        _clearRequested = false;
        _catalog = null;
        _credentialFingerprint = null;
        _profileCache.Clear();

        if (!Directory.Exists(_profileDirectory))
        {
            return;
        }

        try
        {
            Directory.Delete(_profileDirectory, true);
        }
        catch (IOException)
        {
            // NeoProfileManager can retain the active profile file until the next profile is loaded.
        }
        catch (UnauthorizedAccessException)
        {
            // A retained profile file is harmless because materialized filenames include the content hash.
        }
    }

    private async Task WaitForResourceGateAsync()
    {
        if (Coroutine.Current == null)
        {
            await _resourceGate.WaitAsync().ConfigureAwait(false);
            return;
        }

        while (!_resourceGate.Wait(0))
        {
            await Coroutine.Yield();
        }
    }

    private static string ResourceFileName(string resourceName, byte[] data)
    {
        var resourceHash = SHA256.HashData(Encoding.UTF8.GetBytes(resourceName));
        var contentHash = SHA256.HashData(data);
        return $"{Convert.ToHexString(resourceHash)}-{Convert.ToHexString(contentHash)}";
    }

    private static string SafePathPart(string value)
        => string.Concat(value.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

    private static void ValidateProfileXml(string resourceName, byte[] data)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null
        };

        using var stream = new MemoryStream(data, false);
        using var reader = XmlReader.Create(stream, settings);
        var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        if (!string.Equals(document.Root?.Name.LocalName, "Profile", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"Protected resource '{resourceName}' is not an OrderBot profile.");
        }
    }

    private sealed class ProtectedServerProfile : ServerProfile
    {
        [JsonProperty("ResourceName")]
        public string ResourceName { get; set; } = string.Empty;
    }
}
