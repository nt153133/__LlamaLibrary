using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LlamaLibrary.Helpers;

/// <summary>
/// Uses PandaAuth's protected duty-profile client when the loaded PandaAuth version supports it,
/// and otherwise retains the legacy LlamaLibrary implementation for compatibility.
/// </summary>
/// <remarks>
/// This bridge is deliberately limited to the standard duty catalog needed by existing Panda Farmer
/// integrations. Repository URL resolution and quest-by-ID libraries belong to PandaAuth and must not
/// be reintroduced under LlamaLibrary. Reflection avoids forcing every LlamaLibrary consumer to load
/// PandaAuth, while strict API-shape checks preserve the legacy path for older or incompatible versions.
/// </remarks>
public abstract class ProtectedServerProfileSource : IServerProfileSource
{
    private const string CatalogResourceName = "profile.catalog";
    private const string PandaAuthAssemblyName = "PandaAuth";
    private const string PandaAuthClientTypeName = "PandaAuth.ProtectedProfiles.ProtectedDutyProfileClient";

    private static readonly Version MinimumPandaAuthVersion = new(4, 1, 1);
    private static readonly JsonSerializerOptions CatalogJsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    private readonly SemaphoreSlim _resourceGate = new(1, 1);
    private readonly object _pandaAuthClientGate = new();
    private readonly object _profileFileGate = new();
    private readonly Dictionary<string, byte[]> _profileCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly string _cacheNamespace;
    private readonly string _profileDirectory;
    private IReadOnlyList<ProtectedServerProfile>? _catalog;
    private PandaAuthDutyClient? _pandaAuthClient;
    private bool _pandaAuthClientChecked;
    private string? _credentialFingerprint;
    private volatile bool _clearRequested;

    /// <summary>
    /// Initializes a protected duty-profile source with an isolated temporary-file cache.
    /// </summary>
    /// <param name="cacheNamespace">
    /// A stable, non-secret product identifier used only to separate one plugin's materialized
    /// profiles from another plugin's files.
    /// </param>
    protected ProtectedServerProfileSource(string cacheNamespace)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cacheNamespace);
        _cacheNamespace = cacheNamespace;
        _profileDirectory = Path.Combine(
            Path.GetTempPath(),
            "LlamaLibrary",
            "ProtectedProfiles",
            SafePathPart(cacheNamespace));
    }

    /// <summary>
    /// Returns a non-secret fingerprint that changes whenever the active credential changes.
    /// </summary>
    protected abstract string CredentialFingerprint { get; }

    /// <summary>
    /// Retrieves one authorized resource using credentials owned by the calling plugin.
    /// </summary>
    protected abstract Task<byte[]?> FetchResourceAsync(string resourceName);

    /// <summary>
    /// Gets the loaded PandaAuth version, or null when PandaAuth is not loaded.
    /// </summary>
    public static Version? LoadedPandaAuthVersion
        => FindLoadedPandaAuthAssembly()?.GetName().Version;

    /// <summary>
    /// Gets whether the loaded PandaAuth assembly exposes the protected duty-profile capability.
    /// </summary>
    public static bool LoadedPandaAuthSupportsProtectedDutyProfiles
        => TryGetPandaAuthDutyClientType(out _, out _);

    /// <inheritdoc/>
    public async Task<IReadOnlyList<ServerProfile>> GetProfilesAsync()
    {
        var pandaAuthClient = GetPandaAuthClient();
        if (pandaAuthClient != null)
        {
            return await pandaAuthClient.GetProfilesAsync().ConfigureAwait(false);
        }

        return await GetLegacyProfilesAsync();
    }

    /// <inheritdoc/>
    public async Task<string?> MaterializeProfileAsync(ServerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var pandaAuthClient = GetPandaAuthClient();
        if (pandaAuthClient != null)
        {
            return await pandaAuthClient.MaterializeProfileAsync(profile).ConfigureAwait(false);
        }

        return await MaterializeLegacyProfileAsync(profile);
    }

    /// <summary>
    /// Clears data cached for the current plugin session.
    /// </summary>
    public void ClearCache()
    {
        _pandaAuthClient?.ClearCache();

        if (!_resourceGate.Wait(0))
        {
            _clearRequested = true;
            return;
        }

        try
        {
            ClearLegacyCacheCore();
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

    private PandaAuthDutyClient? GetPandaAuthClient()
    {
        lock (_pandaAuthClientGate)
        {
            if (_pandaAuthClient != null || _pandaAuthClientChecked)
            {
                return _pandaAuthClient;
            }

            if (TryGetPandaAuthDutyClientType(out var clientType, out var loadedVersion))
            {
                _pandaAuthClient = PandaAuthDutyClient.Create(
                    clientType!,
                    _cacheNamespace,
                    () => CredentialFingerprint,
                    FetchResourceAsync);
            }
            else if (loadedVersion != null)
            {
                // An already-loaded older PandaAuth cannot be replaced within this AppDomain.
                _pandaAuthClientChecked = true;
            }

            return _pandaAuthClient;
        }
    }

    private async Task<IReadOnlyList<ServerProfile>> GetLegacyProfilesAsync()
    {
        await WaitForResourceGateAsync();
        try
        {
            EnsureCredentialScope();
            if (_catalog == null)
            {
                var data = await FetchRequiredResourceAsync(CatalogResourceName);
                var json = Encoding.UTF8.GetString(data);
                var profiles = JsonSerializer.Deserialize<List<ProtectedServerProfile>>(
                                   json,
                                   CatalogJsonOptions)
                               ?? throw new InvalidOperationException("The protected profile catalog is empty.");

                var invalidCount = profiles.Count(profile =>
                    string.IsNullOrWhiteSpace(profile.Name) || string.IsNullOrWhiteSpace(profile.ResourceName));
                if (invalidCount > 0)
                {
                    throw new InvalidOperationException(
                        $"The protected profile catalog contains {invalidCount} invalid entries.");
                }

                _catalog = profiles;
            }

            return _catalog.Cast<ServerProfile>().ToArray();
        }
        finally
        {
            ClearLegacyCacheIfRequested();
            _resourceGate.Release();
        }
    }

    private async Task<string?> MaterializeLegacyProfileAsync(ServerProfile profile)
    {
        var resourceName = (profile as ProtectedServerProfile)?.ResourceName;
        if (string.IsNullOrWhiteSpace(resourceName) && !string.IsNullOrWhiteSpace(profile.Name))
        {
            var profiles = await GetLegacyProfilesAsync();
            resourceName = (profiles.FirstOrDefault(candidate =>
                string.Equals(candidate.Name, profile.Name, StringComparison.OrdinalIgnoreCase))
                as ProtectedServerProfile)?.ResourceName;
        }

        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return null;
        }

        var data = await GetLegacyProfileDataAsync(resourceName);
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

    private async Task<byte[]> GetLegacyProfileDataAsync(string resourceName)
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
            ClearLegacyCacheIfRequested();
            _resourceGate.Release();
        }
    }

    private async Task<byte[]> FetchRequiredResourceAsync(string resourceName)
        => await FetchResourceAsync(resourceName)
           ?? throw new InvalidOperationException(
               $"The authorized resource request returned no data for '{resourceName}'.");

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

    private void ClearLegacyCacheIfRequested()
    {
        if (_clearRequested)
        {
            ClearLegacyCacheCore();
        }
    }

    private void ClearLegacyCacheCore()
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
            // Retained files are harmless because filenames include the resource/content identity.
        }
    }

    private async Task WaitForResourceGateAsync()
        => await _resourceGate.WaitAsync().ConfigureAwait(false);

    private static Assembly? FindLoadedPandaAuthAssembly()
        => AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(assembly =>
                string.Equals(
                    assembly.GetName().Name,
                    PandaAuthAssemblyName,
                    StringComparison.OrdinalIgnoreCase));

    private static bool TryGetPandaAuthDutyClientType(out Type? clientType, out Version? version)
    {
        var assembly = FindLoadedPandaAuthAssembly();
        version = assembly?.GetName().Version;
        clientType = version != null && version >= MinimumPandaAuthVersion
            ? assembly!.GetType(PandaAuthClientTypeName, throwOnError: false)
            : null;
        var profileType = version != null && version >= MinimumPandaAuthVersion
            ? assembly!.GetType(
                "PandaAuth.ProtectedProfiles.ProtectedDutyProfile",
                throwOnError: false)
            : null;
        var getProfiles = clientType?.GetMethod("GetProfilesAsync", Type.EmptyTypes);
        var materializeProfile = profileType == null
            ? null
            : clientType?.GetMethod("MaterializeProfileAsync", new[] { profileType });
        var clearCache = clientType?.GetMethod("ClearCache", Type.EmptyTypes);
        var expectedProfileCollection = profileType == null
            ? null
            : typeof(IReadOnlyList<>).MakeGenericType(profileType);

        return clientType != null &&
               profileType != null &&
               FindPandaAuthConstructor(clientType) != null &&
               getProfiles?.ReturnType == typeof(Task<>).MakeGenericType(expectedProfileCollection!) &&
               materializeProfile?.ReturnType == typeof(Task<string>) &&
               clearCache?.ReturnType == typeof(void) &&
               HasProfileProperty(profileType, "Name", typeof(string)) &&
               HasProfileProperty(profileType, "Level", typeof(int)) &&
               HasProfileProperty(profileType, "Quality", typeof(string)) &&
               HasProfileProperty(profileType, "Difficulty", typeof(string)) &&
               HasProfileProperty(profileType, "Type", typeof(int)) &&
               HasProfileProperty(profileType, "DutyType", typeof(int)) &&
               HasProfileProperty(profileType, "Url", typeof(string)) &&
               HasProfileProperty(profileType, "ZoneId", typeof(ushort)) &&
               HasProfileProperty(profileType, "DutyId", typeof(ushort)) &&
               HasProfileProperty(profileType, "UnlockQuest", typeof(int)) &&
               HasProfileProperty(profileType, "ItemLevel", typeof(int)) &&
               HasProfileProperty(profileType, "TrustId", typeof(int));
    }

    private static bool HasProfileProperty(Type profileType, string name, Type propertyType)
        => profileType.GetProperty(name)?.PropertyType == propertyType;

    private static ConstructorInfo? FindPandaAuthConstructor(Type clientType)
        => clientType.GetConstructors()
            .FirstOrDefault(constructor =>
            {
                var parameters = constructor.GetParameters();
                if (parameters.Length != 5 ||
                    parameters[0].ParameterType != typeof(string) ||
                    parameters[1].ParameterType != typeof(Func<string>) ||
                    parameters[3].ParameterType != typeof(Action<string>) ||
                    parameters[4].ParameterType != typeof(string))
                {
                    return false;
                }

                var fetchDelegateType = parameters[2].ParameterType;
                if (!fetchDelegateType.IsGenericType ||
                    fetchDelegateType.GetGenericTypeDefinition() != typeof(Func<,>))
                {
                    return false;
                }

                var delegateArguments = fetchDelegateType.GetGenericArguments();
                var taskType = delegateArguments[1];
                if (delegateArguments[0] != typeof(string) ||
                    !taskType.IsGenericType ||
                    taskType.GetGenericTypeDefinition() != typeof(Task<>))
                {
                    return false;
                }

                var tupleType = taskType.GetGenericArguments()[0];
                if (!tupleType.IsGenericType ||
                    tupleType.GetGenericTypeDefinition() != typeof(ValueTuple<,>))
                {
                    return false;
                }

                var tupleArguments = tupleType.GetGenericArguments();
                return tupleArguments[0].IsEnum &&
                       tupleArguments[1] == typeof(byte[]) &&
                       Enum.GetNames(tupleArguments[0]).Contains("Success", StringComparer.Ordinal);
            });

    private static string ResourceFileName(string resourceName, byte[] data)
    {
        var resourceHash = SHA256.HashData(Encoding.UTF8.GetBytes(resourceName));
        var contentHash = SHA256.HashData(data);
        using var identityHash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        identityHash.AppendData(resourceHash);
        identityHash.AppendData(contentHash);
        return Convert.ToHexString(identityHash.GetHashAndReset().AsSpan(0, 16));
    }

    private static string SafePathPart(string value)
        => string.Concat(value.Select(character =>
            Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));

    private static void ValidateProfileXml(string resourceName, byte[] data)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            XmlResolver = null,
        };

        using var stream = new MemoryStream(data, false);
        using var reader = XmlReader.Create(stream, settings);
        var document = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
        if (!string.Equals(document.Root?.Name.LocalName, "Profile", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"Protected resource '{resourceName}' is not an OrderBot profile.");
        }
    }

    private sealed class ProtectedServerProfile : ServerProfile
    {
        public string ResourceName { get; set; } = string.Empty;
    }

    private sealed class PandaAuthServerProfile : ServerProfile
    {
        internal PandaAuthServerProfile(object source)
        {
            Source = source;
        }

        internal object Source { get; }
    }

    private sealed class PandaAuthDutyClient
    {
        private readonly object _client;
        private readonly MethodInfo _clearCache;
        private readonly MethodInfo _getProfiles;
        private readonly MethodInfo _materializeProfile;

        private PandaAuthDutyClient(
            object client,
            MethodInfo getProfiles,
            MethodInfo materializeProfile,
            MethodInfo clearCache)
        {
            _client = client;
            _getProfiles = getProfiles;
            _materializeProfile = materializeProfile;
            _clearCache = clearCache;
        }

        internal static PandaAuthDutyClient Create(
            Type clientType,
            string cacheNamespace,
            Func<string> credentialFingerprint,
            Func<string, Task<byte[]?>> fetchResourceAsync)
        {
            var constructor = FindPandaAuthConstructor(clientType)
                              ?? throw new MissingMethodException(
                                  clientType.FullName,
                                  "protected duty-profile constructor");
            var getProfiles = clientType.GetMethod("GetProfilesAsync", Type.EmptyTypes)
                              ?? throw new MissingMethodException(clientType.FullName, "GetProfilesAsync");
            var profileType = clientType.Assembly.GetType(
                                  "PandaAuth.ProtectedProfiles.ProtectedDutyProfile",
                                  throwOnError: true)!
                              ?? throw new TypeLoadException("PandaAuth protected duty profile type was not found.");
            var materializeProfile = clientType.GetMethod("MaterializeProfileAsync", new[] { profileType })
                                     ?? throw new MissingMethodException(
                                         clientType.FullName,
                                         "MaterializeProfileAsync");
            var clearCache = clientType.GetMethod("ClearCache", Type.EmptyTypes)
                             ?? throw new MissingMethodException(clientType.FullName, "ClearCache");
            var fetchDelegateType = constructor.GetParameters()[2].ParameterType;
            var statusType = fetchDelegateType
                .GetGenericArguments()[1]
                .GetGenericArguments()[0]
                .GetGenericArguments()[0];
            var createFetchAdapter = typeof(PandaAuthDutyClient)
                .GetMethod(
                    nameof(CreateFetchAdapter),
                    BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(statusType);
            var fetchAdapter = createFetchAdapter.Invoke(null, new object[] { fetchResourceAsync })
                               ?? throw new InvalidOperationException(
                                   "Unable to create the PandaAuth resource-fetch adapter.");
            var client = constructor.Invoke(new object[]
            {
                cacheNamespace,
                credentialFingerprint,
                fetchAdapter,
                null!,
                CatalogResourceName,
            });

            return new PandaAuthDutyClient(client, getProfiles, materializeProfile, clearCache);
        }

        internal async Task<IReadOnlyList<ServerProfile>> GetProfilesAsync()
        {
            var task = (Task)(_getProfiles.Invoke(_client, null)
                              ?? throw new InvalidOperationException(
                                  "PandaAuth did not return a profile catalog task."));
            await task.ConfigureAwait(false);
            var profiles = (IEnumerable)(task.GetType().GetProperty("Result")?.GetValue(task)
                                         ?? throw new InvalidOperationException(
                                             "PandaAuth returned no protected duty profile catalog."));

            return profiles.Cast<object>()
                .Select(MapProfile)
                .Cast<ServerProfile>()
                .ToArray();
        }

        internal async Task<string?> MaterializeProfileAsync(ServerProfile profile)
        {
            var source = (profile as PandaAuthServerProfile)?.Source;
            if (source == null && !string.IsNullOrWhiteSpace(profile.Name))
            {
                var profiles = await GetProfilesAsync().ConfigureAwait(false);
                source = (profiles.FirstOrDefault(candidate =>
                        string.Equals(candidate.Name, profile.Name, StringComparison.OrdinalIgnoreCase))
                    as PandaAuthServerProfile)?.Source;
            }

            if (source == null)
            {
                return null;
            }

            var task = (Task)(_materializeProfile.Invoke(_client, new[] { source })
                              ?? throw new InvalidOperationException(
                                  "PandaAuth did not return a profile materialization task."));
            await task.ConfigureAwait(false);
            return task.GetType().GetProperty("Result")?.GetValue(task) as string;
        }

        internal void ClearCache()
            => _clearCache.Invoke(_client, null);

        private static Func<string, Task<(TStatus Status, byte[]? Data)>>
            CreateFetchAdapter<TStatus>(Func<string, Task<byte[]?>> fetchResourceAsync)
            where TStatus : struct, Enum
        {
            var success = (TStatus)Enum.Parse(typeof(TStatus), "Success");
            return async resourceName =>
                (success, await fetchResourceAsync(resourceName).ConfigureAwait(false));
        }

        private static PandaAuthServerProfile MapProfile(object source)
            => new(source)
            {
                Name = GetValue<string>(source, "Name"),
                Level = GetValue<int>(source, "Level"),
                Quality = GetValue<string>(source, "Quality") ?? string.Empty,
                Difficulty = GetValue<string>(source, "Difficulty") ?? string.Empty,
                Type = (ProfileType)GetValue<int>(source, "Type"),
                DutyType = (DutyType)GetValue<int>(source, "DutyType"),
                URL = GetValue<string>(source, "Url"),
                ZoneId = GetValue<ushort>(source, "ZoneId"),
                DutyId = GetValue<ushort>(source, "DutyId"),
                UnlockQuest = GetValue<int>(source, "UnlockQuest"),
                ItemLevel = GetValue<int>(source, "ItemLevel"),
                TrustId = GetValue<int>(source, "TrustId"),
            };

        private static T? GetValue<T>(object source, string propertyName)
        {
            var value = source.GetType().GetProperty(propertyName)?.GetValue(source);
            return value is null ? default : (T)value;
        }
    }
}
