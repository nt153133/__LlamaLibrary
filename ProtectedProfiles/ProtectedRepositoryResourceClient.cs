using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace LlamaLibrary.ProtectedProfiles;

/// <summary>
/// Describes one file published through the protected repository catalog.
/// </summary>
public sealed class ProtectedRepositoryResourceEntry
{
    internal ProtectedRepositoryResourceEntry(
        string repository,
        string path,
        string resourceName,
        string sha256,
        int size)
    {
        Repository = repository;
        Path = path;
        ResourceName = resourceName;
        Sha256 = sha256;
        Size = size;
    }

    public string Repository { get; }

    public string Path { get; }

    public string ResourceName { get; }

    public string Sha256 { get; }

    public int Size { get; }
}

/// <summary>
/// Contains authenticated, hash-verified profile content and its catalog identity.
/// </summary>
public sealed class ProtectedRepositoryResource
{
    internal ProtectedRepositoryResource(ProtectedRepositoryResourceEntry entry, byte[] data, string xml)
    {
        Entry = entry;
        Data = data;
        Xml = xml;
    }

    public ProtectedRepositoryResourceEntry Entry { get; }

    public byte[] Data { get; }

    public string Xml { get; }
}

/// <summary>
/// Resolves protected repository paths to authenticated resources without owning credentials.
/// The calling plugin remains responsible for authorizing every catalog and resource request.
/// </summary>
public sealed class ProtectedRepositoryResourceClient
{
    public const string DefaultCatalogResourceName = "profile.repository.catalog.v1";

    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private readonly object _stateSync = new();
    private readonly SemaphoreSlim _catalogGate = new(1, 1);
    private readonly ConcurrentDictionary<string, Lazy<Task<ProtectedRepositoryResource>>> _contentCache =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _allowedRepositories;
    private readonly Func<string> _credentialFingerprint;
    private readonly Func<string, Task<byte[]?>> _fetchResourceAsync;
    private readonly Action<string>? _debugLog;
    private readonly string _catalogResourceName;

    private RepositoryResourceCatalog? _catalog;
    private IReadOnlyDictionary<string, ProtectedRepositoryResourceEntry>? _catalogIndex;
    private string? _activeCredentialFingerprint;

    /// <summary>
    /// Creates a protected repository client.
    /// </summary>
    /// <param name="credentialFingerprint">
    /// Returns a non-secret value that changes whenever the calling plugin's credential changes.
    /// </param>
    /// <param name="fetchResourceAsync">
    /// Fetches one resource through the calling plugin's authenticated provider. The delegate
    /// must throw for rejected credentials and must never fall back to a public repository.
    /// </param>
    /// <param name="allowedRepositories">
    /// Repository identifiers, in owner/name form, that this client may resolve.
    /// </param>
    /// <param name="debugLog">Optional non-sensitive diagnostic logger.</param>
    /// <param name="catalogResourceName">Protected catalog resource name.</param>
    public ProtectedRepositoryResourceClient(
        Func<string> credentialFingerprint,
        Func<string, Task<byte[]?>> fetchResourceAsync,
        IEnumerable<string> allowedRepositories,
        Action<string>? debugLog = null,
        string catalogResourceName = DefaultCatalogResourceName)
    {
        ArgumentNullException.ThrowIfNull(credentialFingerprint);
        ArgumentNullException.ThrowIfNull(fetchResourceAsync);
        ArgumentNullException.ThrowIfNull(allowedRepositories);
        ArgumentException.ThrowIfNullOrWhiteSpace(catalogResourceName);

        _allowedRepositories = new HashSet<string>(
            allowedRepositories.Where(repository => !string.IsNullOrWhiteSpace(repository))
                .Select(repository => repository.Trim('/')),
            StringComparer.OrdinalIgnoreCase);
        if (_allowedRepositories.Count == 0)
        {
            throw new ArgumentException("At least one allowed repository is required.", nameof(allowedRepositories));
        }

        _credentialFingerprint = credentialFingerprint;
        _fetchResourceAsync = fetchResourceAsync;
        _debugLog = debugLog;
        _catalogResourceName = catalogResourceName;
    }

    /// <summary>
    /// Creates a non-secret SHA-256 fingerprint suitable for credential-scoped caches.
    /// </summary>
    public static string Fingerprint(string? credential)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(credential ?? string.Empty)));

    /// <summary>
    /// Gets one protected profile by repository and normalized path.
    /// </summary>
    public async Task<ProtectedRepositoryResource> GetProfileAsync(string repository, string path)
    {
        repository = NormalizeRepository(repository);
        path = NormalizePath(path);
        EnsureAllowedRepository(repository);
        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentException("A protected profile path is required.", nameof(path));
        }

        var state = await GetCatalogStateAsync().ConfigureAwait(false);
        if (!state.Index.TryGetValue(IndexKey(repository, path), out var entry))
        {
            throw new InvalidOperationException(
                $"The protected profile catalog does not contain '{repository}/{path}'.");
        }

        return await GetProfileAsync(entry, state.CredentialFingerprint).ConfigureAwait(false);
    }

    /// <summary>
    /// Gets the first catalog entry matching an ordered set of candidate paths.
    /// This supports controlled path migrations without masking authorization failures.
    /// </summary>
    public async Task<ProtectedRepositoryResource> GetFirstProfileAsync(
        string repository,
        params string[] candidatePaths)
    {
        repository = NormalizeRepository(repository);
        EnsureAllowedRepository(repository);
        if (candidatePaths == null || candidatePaths.Length == 0)
        {
            throw new ArgumentException("At least one candidate path is required.", nameof(candidatePaths));
        }

        var state = await GetCatalogStateAsync().ConfigureAwait(false);
        foreach (var candidatePath in candidatePaths)
        {
            var path = NormalizePath(candidatePath);
            if (state.Index.TryGetValue(IndexKey(repository, path), out var entry))
            {
                return await GetProfileAsync(entry, state.CredentialFingerprint).ConfigureAwait(false);
            }
        }

        throw new InvalidOperationException(
            $"The protected profile catalog contains none of the requested paths in '{repository}'.");
    }

    /// <summary>
    /// Gets protected XML for an allowlisted GitHub or Gitea URL. Returns null when the URL is
    /// not a recognized protected-repository URL so legacy callers may handle public sources.
    /// </summary>
    public async Task<string?> GetProfileXmlByUrlAsync(string url)
    {
        if (!TryGetRepositoryPath(url, out var repository, out var path))
        {
            return null;
        }

        return (await GetProfileAsync(repository, path).ConfigureAwait(false)).Xml;
    }

    /// <summary>
    /// Gets all entries below a repository path prefix.
    /// </summary>
    public async Task<IReadOnlyList<ProtectedRepositoryResourceEntry>> GetRepositoryEntriesAsync(
        string repository,
        string pathPrefix)
    {
        repository = NormalizeRepository(repository);
        EnsureAllowedRepository(repository);
        var normalizedPrefix = NormalizePath(pathPrefix);
        var state = await GetCatalogStateAsync().ConfigureAwait(false);
        return state.Index.Values
            .Where(entry =>
                string.Equals(entry.Repository, repository, StringComparison.OrdinalIgnoreCase) &&
                (string.Equals(entry.Path, normalizedPrefix, StringComparison.OrdinalIgnoreCase) ||
                 entry.Path.StartsWith(normalizedPrefix + "/", StringComparison.OrdinalIgnoreCase)))
            .OrderBy(entry => entry.Path, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets protected XML for an entry returned by <see cref="GetRepositoryEntriesAsync"/>.
    /// </summary>
    public async Task<string> GetProfileXmlAsync(ProtectedRepositoryResourceEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        return (await GetProfileAsync(entry.Repository, entry.Path).ConfigureAwait(false)).Xml;
    }

    /// <summary>
    /// Tries to read the current catalog SHA for a protected URL without performing I/O.
    /// </summary>
    public bool TryGetProfileSha256(string url, out string sha256)
    {
        sha256 = string.Empty;
        if (!TryGetRepositoryPath(url, out var repository, out var path))
        {
            return false;
        }

        lock (_stateSync)
        {
            EnsureCredentialScopeLocked(CurrentCredentialFingerprint());
            if (_catalogIndex == null ||
                !_catalogIndex.TryGetValue(IndexKey(repository, path), out var entry))
            {
                return false;
            }

            sha256 = entry.Sha256;
            return !string.IsNullOrWhiteSpace(sha256);
        }
    }

    /// <summary>
    /// Parses an allowlisted GitHub or Gitea URL into repository and path components.
    /// </summary>
    public bool TryGetRepositoryPath(string url, out string repository, out string path)
    {
        repository = string.Empty;
        path = string.Empty;
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return false;
        }

        var segments = Uri.UnescapeDataString(uri.AbsolutePath)
            .Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();
        if (segments.Count < 3)
        {
            return false;
        }

        if (uri.Host.Equals("raw.githubusercontent.com", StringComparison.OrdinalIgnoreCase))
        {
            repository = $"{segments[0]}/{segments[1]}";
            segments.RemoveRange(0, Math.Min(3, segments.Count));
        }
        else if (uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase))
        {
            repository = $"{segments[0]}/{segments[1]}";
            if (segments.Count < 5 ||
                (!segments[2].Equals("blob", StringComparison.OrdinalIgnoreCase) &&
                 !segments[2].Equals("raw", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            segments.RemoveRange(0, 4);
        }
        else if (uri.Host.Equals("gitea.llamamagic.net", StringComparison.OrdinalIgnoreCase))
        {
            repository = $"{segments[0]}/{segments[1]}";
            if (segments.Count < 6 ||
                (!segments[2].Equals("src", StringComparison.OrdinalIgnoreCase) &&
                 !segments[2].Equals("raw", StringComparison.OrdinalIgnoreCase)) ||
                (!segments[3].Equals("branch", StringComparison.OrdinalIgnoreCase) &&
                 !segments[3].Equals("commit", StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            segments.RemoveRange(0, 5);
        }
        else
        {
            return false;
        }

        repository = NormalizeRepository(repository);
        path = NormalizePath(string.Join("/", segments));
        return _allowedRepositories.Contains(repository) && !string.IsNullOrWhiteSpace(path);
    }

    /// <summary>
    /// Reloads the catalog. Content whose catalog SHA is unchanged remains cached.
    /// </summary>
    public async Task RefreshCatalogAsync()
    {
        await _catalogGate.WaitAsync().ConfigureAwait(false);
        try
        {
            var credentialFingerprint = CurrentCredentialFingerprint();
            lock (_stateSync)
            {
                EnsureCredentialScopeLocked(credentialFingerprint);
            }

            var catalog = await LoadCatalogAsync(credentialFingerprint).ConfigureAwait(false);
            lock (_stateSync)
            {
                EnsureUnchangedCredentialLocked(credentialFingerprint);
                InstallCatalogLocked(catalog);
            }

            _debugLog?.Invoke($"Refreshed protected repository catalog with {catalog.Entries.Count} entries.");
        }
        finally
        {
            _catalogGate.Release();
        }
    }

    /// <summary>
    /// Clears all catalog and content state for this client instance.
    /// </summary>
    public void ClearCache()
    {
        lock (_stateSync)
        {
            _catalog = null;
            _catalogIndex = null;
            _activeCredentialFingerprint = null;
            _contentCache.Clear();
        }
    }

    private async Task<ProtectedRepositoryResource> GetProfileAsync(
        ProtectedRepositoryResourceEntry entry,
        string credentialFingerprint)
    {
        var cacheKey = $"{credentialFingerprint}\n{IndexKey(entry.Repository, entry.Path)}\n{entry.Sha256}";
        var pending = _contentCache.GetOrAdd(
            cacheKey,
            _ => new Lazy<Task<ProtectedRepositoryResource>>(
                () => FetchAndVerifyAsync(entry, credentialFingerprint),
                LazyThreadSafetyMode.ExecutionAndPublication));

        try
        {
            return await pending.Value.ConfigureAwait(false);
        }
        catch
        {
            _contentCache.TryRemove(cacheKey, out _);
            throw;
        }
    }

    private async Task<ProtectedRepositoryResource> FetchAndVerifyAsync(
        ProtectedRepositoryResourceEntry entry,
        string credentialFingerprint)
    {
        _debugLog?.Invoke($"Loading protected profile '{entry.Repository}/{entry.Path}'.");
        var data = await _fetchResourceAsync(entry.ResourceName).ConfigureAwait(false)
                   ?? throw new InvalidOperationException(
                       $"The authorized resource request returned no data for '{entry.Repository}/{entry.Path}'.");

        EnsureUnchangedCredential(credentialFingerprint);
        if (entry.Size > 0 && data.Length != entry.Size)
        {
            throw new InvalidOperationException(
                $"Protected profile '{entry.Repository}/{entry.Path}' failed its catalog size check.");
        }

        var actualSha256 = Convert.ToHexString(SHA256.HashData(data));
        if (!string.Equals(actualSha256, entry.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Protected profile '{entry.Repository}/{entry.Path}' failed SHA-256 validation.");
        }

        var xml = StrictUtf8.GetString(data).TrimStart('\uFEFF');
        return new ProtectedRepositoryResource(entry, data, xml);
    }

    private async Task<CatalogState> GetCatalogStateAsync()
    {
        var credentialFingerprint = CurrentCredentialFingerprint();
        lock (_stateSync)
        {
            EnsureCredentialScopeLocked(credentialFingerprint);
            if (_catalog != null && _catalogIndex != null)
            {
                return new CatalogState(_catalogIndex, credentialFingerprint);
            }
        }

        await _catalogGate.WaitAsync().ConfigureAwait(false);
        try
        {
            credentialFingerprint = CurrentCredentialFingerprint();
            lock (_stateSync)
            {
                EnsureCredentialScopeLocked(credentialFingerprint);
                if (_catalog != null && _catalogIndex != null)
                {
                    return new CatalogState(_catalogIndex, credentialFingerprint);
                }
            }

            var catalog = await LoadCatalogAsync(credentialFingerprint).ConfigureAwait(false);
            lock (_stateSync)
            {
                EnsureUnchangedCredentialLocked(credentialFingerprint);
                InstallCatalogLocked(catalog);
                return new CatalogState(_catalogIndex!, credentialFingerprint);
            }
        }
        finally
        {
            _catalogGate.Release();
        }
    }

    private async Task<RepositoryResourceCatalog> LoadCatalogAsync(string credentialFingerprint)
    {
        var data = await _fetchResourceAsync(_catalogResourceName).ConfigureAwait(false)
                   ?? throw new InvalidOperationException(
                       "The authorized resource request returned no protected repository catalog.");
        EnsureUnchangedCredential(credentialFingerprint);

        var catalog = JsonSerializer.Deserialize<RepositoryResourceCatalog>(
                          data,
                          new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                      ?? throw new InvalidOperationException("The protected repository catalog is empty.");
        if (catalog.Version != 1 || catalog.Entries.Count == 0)
        {
            throw new InvalidOperationException("The protected repository catalog has an unsupported format.");
        }

        return catalog;
    }

    private void InstallCatalogLocked(RepositoryResourceCatalog catalog)
    {
        var index = new Dictionary<string, ProtectedRepositoryResourceEntry>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in catalog.Entries)
        {
            var repository = NormalizeRepository(source.Repository);
            var path = NormalizePath(source.Path);
            if (!_allowedRepositories.Contains(repository))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(path) ||
                string.IsNullOrWhiteSpace(source.ResourceName) ||
                string.IsNullOrWhiteSpace(source.Sha256))
            {
                throw new InvalidOperationException(
                    "The protected repository catalog contains an incomplete allowlisted entry.");
            }

            var entry = new ProtectedRepositoryResourceEntry(
                repository,
                path,
                source.ResourceName,
                source.Sha256,
                source.Size);
            var key = IndexKey(repository, path);
            if (!index.TryAdd(key, entry))
            {
                throw new InvalidOperationException(
                    $"The protected repository catalog contains duplicate entry '{key}'.");
            }
        }

        if (index.Count == 0)
        {
            throw new InvalidOperationException(
                "The protected repository catalog contains no entries for an allowed repository.");
        }

        _catalog = catalog;
        _catalogIndex = index;

        // Cache identities include the credential fingerprint and content SHA. Removing entries
        // absent from the new catalog prevents stale protected bytes from accumulating in memory.
        var validKeys = new HashSet<string>(
            index.Values.Select(entry =>
                $"{_activeCredentialFingerprint}\n{IndexKey(entry.Repository, entry.Path)}\n{entry.Sha256}"),
            StringComparer.OrdinalIgnoreCase);
        foreach (var cacheKey in _contentCache.Keys)
        {
            if (!validKeys.Contains(cacheKey))
            {
                _contentCache.TryRemove(cacheKey, out _);
            }
        }
    }

    private string CurrentCredentialFingerprint()
        => _credentialFingerprint() ?? string.Empty;

    private void EnsureCredentialScopeLocked(string credentialFingerprint)
    {
        if (string.Equals(_activeCredentialFingerprint, credentialFingerprint, StringComparison.Ordinal))
        {
            return;
        }

        _catalog = null;
        _catalogIndex = null;
        _contentCache.Clear();
        _activeCredentialFingerprint = credentialFingerprint;
    }

    private void EnsureUnchangedCredential(string expectedFingerprint)
    {
        lock (_stateSync)
        {
            EnsureUnchangedCredentialLocked(expectedFingerprint);
        }
    }

    private void EnsureUnchangedCredentialLocked(string expectedFingerprint)
    {
        var currentFingerprint = CurrentCredentialFingerprint();
        if (!string.Equals(expectedFingerprint, currentFingerprint, StringComparison.Ordinal))
        {
            EnsureCredentialScopeLocked(currentFingerprint);
            throw new InvalidOperationException(
                "The protected-profile credential changed while a resource was loading. Retry the request.");
        }
    }

    private void EnsureAllowedRepository(string repository)
    {
        if (!_allowedRepositories.Contains(repository))
        {
            throw new InvalidOperationException(
                $"Repository '{repository}' is not approved for protected profile loading.");
        }
    }

    private static string NormalizeRepository(string repository)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        return repository.Replace('\\', '/').Trim('/');
    }

    private static string NormalizePath(string path)
        => (path ?? string.Empty).Replace('\\', '/').Trim('/');

    private static string IndexKey(string repository, string path)
        => $"{NormalizeRepository(repository)}/{NormalizePath(path)}";

    private sealed class CatalogState
    {
        internal CatalogState(
            IReadOnlyDictionary<string, ProtectedRepositoryResourceEntry> index,
            string credentialFingerprint)
        {
            Index = index;
            CredentialFingerprint = credentialFingerprint;
        }

        internal IReadOnlyDictionary<string, ProtectedRepositoryResourceEntry> Index { get; }

        internal string CredentialFingerprint { get; }
    }

    private sealed class RepositoryResourceCatalog
    {
        public int Version { get; set; }

        public List<RepositoryResourceCatalogEntry> Entries { get; set; } = new();
    }

    private sealed class RepositoryResourceCatalogEntry
    {
        public string Repository { get; set; } = string.Empty;

        public string Path { get; set; } = string.Empty;

        public string ResourceName { get; set; } = string.Empty;

        public string Sha256 { get; set; } = string.Empty;

        public int Size { get; set; }
    }
}
