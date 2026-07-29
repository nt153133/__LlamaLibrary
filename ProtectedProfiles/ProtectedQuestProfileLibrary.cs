using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LlamaLibrary.ProtectedProfiles;

/// <summary>
/// Provides quest-ID-only access to protected OrderBot profiles.
/// </summary>
/// <remarks>
/// Feature code supplies only a quest ID. Repository layout, migration fallback, authenticated
/// fetching, integrity verification, and XML extraction remain behind this boundary.
/// </remarks>
public sealed class ProtectedQuestProfileLibrary
{
    public const string DefaultRepository = "DomesticWarlord/PrivateProfiles";
    public const string DefaultRoot = "Misc Quests";

    private readonly ProtectedRepositoryResourceClient _resources;
    private readonly string _repository;
    private readonly string _root;
    private readonly ConcurrentDictionary<string, Lazy<string>> _orderCache = new();

    public ProtectedQuestProfileLibrary(
        ProtectedRepositoryResourceClient resources,
        string repository = DefaultRepository,
        string root = DefaultRoot)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentException.ThrowIfNullOrWhiteSpace(repository);
        ArgumentException.ThrowIfNullOrWhiteSpace(root);

        _resources = resources;
        _repository = repository.Trim('/');
        _root = root.Replace('\\', '/').Trim('/');
    }

    /// <summary>
    /// Gets the authenticated, hash-verified resource for a quest ID.
    /// </summary>
    public Task<ProtectedRepositoryResource> GetProfileAsync(uint questId)
    {
        ValidateQuestId(questId);
        return _resources.GetFirstProfileAsync(
            _repository,
            GetCanonicalPath(questId),
            GetLegacyPath(questId));
    }

    /// <summary>
    /// Gets the complete protected profile XML for a quest ID.
    /// </summary>
    public async Task<string> GetProfileXmlAsync(uint questId)
        => (await GetProfileAsync(questId).ConfigureAwait(false)).Xml;

    /// <summary>
    /// Gets only the child XML inside the quest profile's single Order element.
    /// </summary>
    public async Task<string> GetOrderXmlAsync(uint questId)
    {
        var resource = await GetProfileAsync(questId).ConfigureAwait(false);
        var cacheKey = $"{questId}:{resource.Entry.Sha256}";
        return _orderCache.GetOrAdd(
            cacheKey,
            _ => new Lazy<string>(
                () => ExtractOrderXml(questId, resource.Xml),
                LazyThreadSafetyMode.ExecutionAndPublication)).Value;
    }

    /// <summary>
    /// Lists unique quest IDs represented by valid canonical or legacy catalog paths.
    /// </summary>
    public async Task<IReadOnlyList<uint>> GetAvailableQuestIdsAsync()
    {
        var entries = await _resources.GetRepositoryEntriesAsync(_repository, _root).ConfigureAwait(false);
        return entries
            .Select(entry => TryGetQuestId(entry.Path))
            .Where(questId => questId.HasValue)
            .Select(questId => questId!.Value)
            .Distinct()
            .OrderBy(questId => questId)
            .ToArray();
    }

    /// <summary>
    /// Computes the canonical bucketed repository path for a quest ID.
    /// </summary>
    public string GetCanonicalPath(uint questId)
    {
        ValidateQuestId(questId);
        var bucketStart = questId / 1000 * 1000;
        var bucketEnd = bucketStart + 999;
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}/{1:D5}-{2:D5}/{3}.xml",
            _root,
            bucketStart,
            bucketEnd,
            questId);
    }

    /// <summary>
    /// Clears extracted Order content. The repository client's cache is managed separately.
    /// </summary>
    public void ClearCache()
        => _orderCache.Clear();

    private string GetLegacyPath(uint questId)
        => string.Format(CultureInfo.InvariantCulture, "{0}/{1}.xml", _root, questId);

    private uint? TryGetQuestId(string path)
    {
        var normalized = path.Replace('\\', '/').Trim('/');
        var prefix = _root + "/";
        if (!normalized.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var relative = normalized[prefix.Length..];
        var fileName = relative.Split('/').LastOrDefault();
        if (fileName == null ||
            !fileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase) ||
            !uint.TryParse(fileName[..^4], NumberStyles.None, CultureInfo.InvariantCulture, out var questId) ||
            questId == 0)
        {
            return null;
        }

        return string.Equals(normalized, GetCanonicalPath(questId), StringComparison.OrdinalIgnoreCase) ||
               string.Equals(normalized, GetLegacyPath(questId), StringComparison.OrdinalIgnoreCase)
            ? questId
            : null;
    }

    private static string ExtractOrderXml(uint questId, string xml)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
        };

        XDocument document;
        using (var textReader = new StringReader(xml))
        using (var xmlReader = XmlReader.Create(textReader, settings))
        {
            document = XDocument.Load(xmlReader, LoadOptions.PreserveWhitespace);
        }

        var orders = document.Descendants()
            .Where(element => element.Name.LocalName.Equals("Order", StringComparison.OrdinalIgnoreCase))
            .ToList();
        if (orders.Count != 1)
        {
            throw new InvalidDataException(
                $"Protected quest {questId} must contain exactly one Order element; found {orders.Count}.");
        }

        return string.Concat(orders[0].Nodes().Select(node => node.ToString())).Trim('\r', '\n');
    }

    private static void ValidateQuestId(uint questId)
    {
        if (questId == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(questId), "Quest ID must be greater than zero.");
        }
    }
}
