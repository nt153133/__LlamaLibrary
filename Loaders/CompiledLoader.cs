//!CompilerOption:AddRef:System.Diagnostics.FileVersionInfo.dll
//!CompilerOption:AddRef:System.Formats.Tar.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot.AClasses;
using ff14bot.Interfaces;
using LlamaLibrary.Logging;
using LlamaLibrary.Memory;
using SevenZip;

// ReSharper disable VirtualMemberCallInConstructor

namespace LlamaLibrary.Loaders;

public abstract class BotBaseLoader : CompiledLoader<BotBase> { }
public abstract class PluginLoader : CompiledLoader<IBotPlugin> { }
public abstract class RoutineLoader : CompiledLoader<CombatRoutine> { }

public abstract class CompiledLoader<T> : IDisposable, IAddonProxy<T> where T : class
{
    // Shared HTTP client across every loader instance/type (connection pooling, DNS caching).
    // HttpClient is thread-safe and designed to be long-lived.
    private static readonly HttpClient SharedHttpClient = CreateSharedHttpClient();

    private static HttpClient CreateSharedHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(5),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All,
            MaxConnectionsPerServer = 8,
        };

        var client = new HttpClient(handler, disposeHandler: true)
        {
            Timeout = TimeSpan.FromSeconds(45),
        };
        client.DefaultRequestHeaders.AcceptEncoding.ParseAdd("gzip, deflate, br");
        return client;
    }

    protected readonly LLogger Log;

    protected CompiledLoader()
    {
        Log = new LLogger(ProjectName, LogColor);
    }

    // Kept for binary/source compatibility with existing callers/overrides.
    protected HttpClient GetHttpClient() => SharedHttpClient;

    protected string LocalFolderName { get; private set; } = string.Empty;
    protected abstract string ProjectName { get; }
    protected virtual string CompiledAssemblyName => $"{ProjectName}.dll";
    protected bool ForceChineseDownload = false;

    protected virtual bool ReleaseChannelsEnabled => false;
    protected virtual string ReleaseChannel => "Stable";
    protected virtual string PackagedReleaseChannel => "Unknown";
    protected virtual string PackagedRuntime => "Unknown";
    protected virtual string PackagedVersion => "Unknown";
    protected virtual string RuntimeChannel => Environment.Version.Major >= 10 ? "net10" : "net8";

    private string ReleaseRequestQuery => ReleaseChannelsEnabled
        ? $"&channel={Uri.EscapeDataString(ReleaseChannel)}&runtime={Uri.EscapeDataString(RuntimeChannel)}"
        : string.Empty;

    protected virtual string ChineseDataUrl =>
        $"http://update.ffxivbots.com:3000/Download/cn?product={Uri.EscapeDataString(ProjectName)}&force={(ForceChineseDownload ? "true" : "false")}{ReleaseRequestQuery}";
    protected virtual string GlobalDataUrl => $"http://update.ffxivbots.com:3000/Download?product={Uri.EscapeDataString(ProjectName)}{ReleaseRequestQuery}";

#if RB_CN || RB_TC
    protected virtual string DataUrl => ChineseDataUrl;
#else
    protected virtual string DataUrl => GlobalDataUrl;
#endif

    protected virtual string VersionUrl => $"http://update.ffxivbots.com:3000/version?product={Uri.EscapeDataString(ProjectName)}{ReleaseRequestQuery}";
    protected virtual bool Debug => false;
    private FileInfo? _compiledAssembly;
    protected FileInfo CompiledAssembly => _compiledAssembly ??= new FileInfo(Path.Combine(LocalFolderName, CompiledAssemblyName));
    protected virtual Color LogColor => Colors.Lime;
    protected virtual LogLevel LogLevel => LogLevel.Information;
    protected virtual List<(string Name, Assembly Assembly)> AddedAssemblies => new();

    protected virtual string CheckUrl => $"http://update.ffxivbots.com:3000/Version/check?product={Uri.EscapeDataString(ProjectName)}&version=";

    private LoaderCheckOutcome _lastCheckOutcome;

    // ----------------------------------------------------------------------------------
    // Loading
    // ----------------------------------------------------------------------------------

    public T? Load()
    {
        RedirectAssembly();
        CompiledAssembly.Refresh();

        if (!CompiledAssembly.Exists)
        {
            Log.Error("Compiled assembly does not exist");
            return null;
        }

        var assembly = LoadAssembly(CompiledAssembly.FullName);
        if (assembly == null)
        {
            return null;
        }

        Type? baseType;
        try
        {
            baseType = FindBaseType(assembly);
        }
        catch (ReflectionTypeLoadException ex)
        {
            Log.Error(FormatLoaderExceptions(ex));
            return null;
        }
        catch (Exception e)
        {
            Log.Error("Other Exception");
            Log.Exception(e);
            return null;
        }

        if (baseType == null)
        {
            Log.Error("Base type is null");
            return null;
        }

        try
        {
            return Activator.CreateInstance(baseType) as T;
        }
        catch (ReflectionTypeLoadException ex)
        {
            Log.Error(FormatLoaderExceptions(ex));
            return null;
        }
        catch (Exception e)
        {
            Log.Error("Other Exception2");
            Log.Exception(e);
            return null;
        }
    }

    private static Type? FindBaseType(Assembly assembly)
    {
        // Iterate types defensively so a single broken type doesn't sink the whole lookup.
        Type[] types;
        try
        {
            types = assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException rtle)
        {
            foreach (var t in rtle.Types)
            {
                if (t != null && typeof(T).IsAssignableFrom(t))
                {
                    return t;
                }
            }

            return null;
        }

        foreach (var t in types)
        {
            if (t != null && typeof(T).IsAssignableFrom(t))
            {
                return t;
            }
        }
        return null;
    }

    // ----------------------------------------------------------------------------------
    // Update
    // ----------------------------------------------------------------------------------

    protected virtual async Task<bool> Update()
    {
        if (ReleaseChannelsEnabled)
        {
            Log.Information($"Release channel: {ReleaseChannel}; runtime lane: {RuntimeChannel}");
        }
        else
        {
            Log.Verbose("Release channels disabled; using legacy update routing.");
        }

        var assemblyPath = CompiledAssembly.FullName;

        if (File.Exists(assemblyPath) && IsFileLocked(assemblyPath))
        {
            Log.Error($"Cannot update {ProjectName} because the compiled assembly is currently in use by another process.");
            return false;
        }

        _lastCheckOutcome = LoaderCheckOutcome.Unknown;
        var downloadInfo = await CheckAndDownload().ConfigureAwait(false);

        if (downloadInfo.Stream == null)
        {
            if (_lastCheckOutcome == LoaderCheckOutcome.Failed)
            {
                Log.Warning(
                    $"Update failed for requested {ReleaseChannel}/{RuntimeChannel}; " +
                    $"continuing with packaged {PackagedReleaseChannel}/{PackagedRuntime} version {PackagedVersion}.");
                return false;
            }

            Log.Verbose($"No update needed for {ProjectName}");
            return true;
        }

        Log.Information($"Updating {ProjectName}...");

        var oldVersion = GetLocalVersion();
        try
        {
            // Extract into a sibling temporary directory first, then atomically swap.
            // This avoids a half-extracted state if extraction fails or the process is killed,
            // and greatly reduces the window during which another process could try to load partial files.
            CompiledAssembly.Directory?.Create();
            var extractedTo = await ExtractToTempAsync(downloadInfo).ConfigureAwait(false);
            if (extractedTo == null)
            {
                return false;
            }

            SwapDirectories(extractedTo, LocalFolderName);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to extract {ProjectName}");
            Log.Exception(e);
            return false;
        }
        finally
        {
            await downloadInfo.Stream.DisposeAsync().ConfigureAwait(false);
        }

        var newVersion = GetLocalVersion();

        if (newVersion == null)
        {
            Log.Error($"Failed to get new version after update for {ProjectName}");
            return false;
        }

        if (newVersion != oldVersion)
        {
            Log.Information($"{ProjectName} updated (from {oldVersion?.ToString() ?? "<none>"} to {newVersion})");
            return true;
        }

        Log.Information($"{ProjectName} release identity refreshed at version {newVersion}.");
        return true;
    }

    private async Task<string?> ExtractToTempAsync((MemoryStream? Stream, string? ContentType) downloadInfo)
    {
        if (downloadInfo.Stream == null)
        {
            return null;
        }

        var tempDir = Path.Combine(
                                   Path.GetDirectoryName(LocalFolderName) ?? LocalFolderName,
                                   $".{Path.GetFileName(LocalFolderName)}.tmp_{Environment.ProcessId}_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempDir);

        try
        {
            switch (downloadInfo.ContentType)
            {
                case "application/zip":
                {
                    var sw = Stopwatch.StartNew();
                    downloadInfo.Stream.ExtractFromStream(new DirectoryInfo(tempDir));
                    sw.Stop();
                    Log.Information($"Decompressed(zip) in {sw.ElapsedMilliseconds}ms");
                    break;
                }
                case "application/octet-stream":
                {
                    using var outStream = new MemoryStream();
                    var sw = Stopwatch.StartNew();
                    Helper.Decompress(downloadInfo.Stream, outStream);
                    sw.Stop();
                    Log.Information($"Decompressed(LZMA) in {sw.ElapsedMilliseconds}ms");
                    outStream.Position = 0;
                    await TarFile.ExtractToDirectoryAsync(outStream, tempDir, overwriteFiles: true).ConfigureAwait(false);
                    break;
                }
                default:
                    Log.Error($"Unknown content type: {downloadInfo.ContentType}");
                    try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
                    return null;
            }
        }
        catch
        {
            try { Directory.Delete(tempDir, true); } catch { /* ignore */ }
            throw;
        }

        return tempDir;
    }

    /// <summary>
    /// Replace <paramref name="target"/>'s contents with <paramref name="source"/>'s contents.
    /// Tries to delete files first; anything currently locked (by a second copy of this app)
    /// is renamed to a .old_* sidecar so the new files can still be laid down.
    /// </summary>
    private void SwapDirectories(string source, string target)
    {
        Directory.CreateDirectory(target);
        DeleteOrRenameContents(target);

        // Move over each top-level entry. Use File.Move/Directory.Move which are fast rename operations
        // when source and target are on the same volume.
        foreach (var entry in new DirectoryInfo(source).EnumerateFileSystemInfos())
        {
            var destPath = Path.Combine(target, entry.Name);
            try
            {
                if (entry is DirectoryInfo d)
                {
                    if (Directory.Exists(destPath))
                    {
                        Directory.Delete(destPath, true);
                    }

                    Directory.Move(d.FullName, destPath);
                }
                else
                {
                    if (File.Exists(destPath))
                    {
                        File.Delete(destPath);
                    }

                    File.Move(entry.FullName, destPath);
                }
            }
            catch (Exception moveEx)
            {
                // Move can fail across volumes or if something else grabbed the file between cleanup and move;
                // fall back to a copy. This keeps us resilient when temp and LocalFolder live on different drives.
                try
                {
                    if (entry is DirectoryInfo d)
                    {
                        CopyDirectory(d.FullName, destPath);
                    }
                    else
                    {
                        File.Copy(entry.FullName, destPath, overwrite: true);
                    }
                }
                catch (Exception copyEx)
                {
                    Log.Error($"Failed to place '{entry.Name}': move={moveEx.Message}; copy={copyEx.Message}");
                }
            }
        }

        try { Directory.Delete(source, true); } catch { /* best effort */ }
    }

    /// <summary>
    /// Delete every entry under <paramref name="directory"/>. For files that are in use
    /// by another process copy, rename them out of the way (Windows allows rename on open files).
    /// </summary>
    private void DeleteOrRenameContents(string directory)
    {
        foreach (var entry in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            if (entry is DirectoryInfo dir)
            {
                try { dir.Delete(true); }
                catch
                {
                    foreach (var f in dir.EnumerateFiles("*", SearchOption.AllDirectories))
                    {
                        TryDeleteOrRenameFile(f.FullName);
                    }

                    try { dir.Delete(true); } catch { /* leave stale empty dirs; harmless */ }
                }
            }
            else
            {
                TryDeleteOrRenameFile(entry.FullName);
            }
        }

        // Opportunistically clean old .old_* sidecars from previous runs.
        try
        {
            foreach (var old in Directory.EnumerateFiles(directory, "*.old_*", SearchOption.AllDirectories))
            {
                try { File.Delete(old); } catch { /* still locked, leave it */ }
            }
        }
        catch { /* ignore */ }
    }

    private static void TryDeleteOrRenameFile(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // File in use (another copy of the program has the assembly loaded).
            // Rename to a unique sidecar so the update can still proceed.
            var newName = $"{path}.old_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
            try { File.Move(path, newName); } catch { /* give up on this file */ }
        }
        catch (UnauthorizedAccessException)
        {
            var newName = $"{path}.old_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}";
            try { File.Move(path, newName); } catch { /* give up on this file */ }
        }
    }

    private static void CopyDirectory(string source, string target)
    {
        Directory.CreateDirectory(target);
        foreach (var dir in Directory.EnumerateDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, dir)));
        }
        foreach (var file in Directory.EnumerateFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(target, Path.GetRelativePath(source, file)), overwrite: true);
        }
    }

    protected void UnblockAll()
    {
        if (!Directory.Exists(LocalFolderName))
        {
            return;
        }

        foreach (var file in new DirectoryInfo(LocalFolderName).EnumerateFiles("*.dll"))
        {
            file.Unblock();
        }
    }

    // ----------------------------------------------------------------------------------
    // Update check / download
    // ----------------------------------------------------------------------------------

    protected virtual async Task<(MemoryStream? Stream, string? ContentType)> CheckAndDownload()
    {
        var localVersion = GetLocalVersion();
        if (localVersion == null)
        {
            Log.Information($"No local version found for {ProjectName}, falling back to full download.");
            var initialDownload = await Download().ConfigureAwait(false);
            _lastCheckOutcome = initialDownload.Stream == null ? LoaderCheckOutcome.Failed : LoaderCheckOutcome.UpdateAvailable;
            return initialDownload;
        }

        var checkUrl = CheckUrl + Uri.EscapeDataString(localVersion.ToString()) + BuildCheckIdentityQuery();
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Head, checkUrl);
            using var response = await SharedHttpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            var statusCode = (int)response.StatusCode;
            var resolvedChannel = TryFirstHeader(response.Headers, "X-Release-Channel");
            var resolvedRuntime = TryFirstHeader(response.Headers, "X-Release-Runtime");
            var releaseSource = TryFirstHeader(response.Headers, "X-Release-Source");
            LogResolvedRelease(resolvedChannel, resolvedRuntime, releaseSource);

            if (statusCode == 200)
            {
                if (ReleaseChannelsEnabled
                    && !string.IsNullOrWhiteSpace(resolvedChannel)
                    && !string.IsNullOrWhiteSpace(resolvedRuntime)
                    && !PackagedIdentityMatches(localVersion, resolvedChannel, resolvedRuntime))
                {
                    Log.Information(
                        $"Numeric version is current, but packaged identity {PackagedReleaseChannel}/{PackagedRuntime}/{PackagedVersion} " +
                        $"does not match resolved {resolvedChannel}/{resolvedRuntime}/{localVersion}. Refreshing the release.");
                    var identityDownload = await Download().ConfigureAwait(false);
                    _lastCheckOutcome = identityDownload.Stream == null ? LoaderCheckOutcome.Failed : LoaderCheckOutcome.UpdateAvailable;
                    return identityDownload;
                }

                if (ReleaseChannelsEnabled && string.IsNullOrWhiteSpace(releaseSource))
                {
                    Log.Warning("UpdateServer returned no release identity; accepting legacy version behavior for compatibility.");
                }

                _lastCheckOutcome = LoaderCheckOutcome.UpToDate;
                Log.Information($"{ProjectName} is up to date. Version: {localVersion}");
                return (null, null);
            }

            if (statusCode == 226)
            {
                var downloadUrl = TryFirstHeader(response.Headers, "X-Download-Url");
                var chinaUrl = TryFirstHeader(response.Headers, "X-China-Url");

                bool useChinaUrl = ForceChineseDownload
                    || OffsetManager.ActiveRegion == ClientRegion.China
                    || OffsetManager.ActiveRegion == ClientRegion.TraditionalChinese;

                if (useChinaUrl)
                {
                    ForceChineseDownload = true;
                }

                var primaryUrl = useChinaUrl ? (chinaUrl ?? downloadUrl) : (downloadUrl ?? chinaUrl);
                var fallbackUrl = useChinaUrl ? null : chinaUrl;

                if (string.IsNullOrEmpty(primaryUrl))
                {
                    Log.Error($"Update available for {ProjectName} but no download URL was provided by the server.");
                    _lastCheckOutcome = LoaderCheckOutcome.Failed;
                    return (null, null);
                }

                var location = useChinaUrl ? "Tencent (Hong Kong)" : "Global";

                Log.Information($"Update available for {ProjectName}. Downloading from {location}...");
                var result = await DownloadFromUrl(primaryUrl!).ConfigureAwait(false);

                if (result.Stream == null && !string.IsNullOrEmpty(fallbackUrl) && primaryUrl != fallbackUrl)
                {
                    ForceChineseDownload = true;
                    Log.Warning("Primary download failed. Attempting alternate download url.");
                    result = await DownloadFromUrl(fallbackUrl!).ConfigureAwait(false);
                }

                _lastCheckOutcome = result.Stream == null ? LoaderCheckOutcome.Failed : LoaderCheckOutcome.UpdateAvailable;
                return result;
            }

            Log.Error($"Unexpected status code {statusCode} from check endpoint for {ProjectName}.");
            _lastCheckOutcome = LoaderCheckOutcome.Failed;
            return (null, null);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to check for updates for {ProjectName}.");
            Log.Exception(e);
            _lastCheckOutcome = LoaderCheckOutcome.Failed;
            return (null, null);
        }
    }

    private string BuildCheckIdentityQuery()
    {
        if (!ReleaseChannelsEnabled)
        {
            return string.Empty;
        }

        return ReleaseRequestQuery
               + $"&packagedChannel={Uri.EscapeDataString(PackagedReleaseChannel)}"
               + $"&packagedRuntime={Uri.EscapeDataString(PackagedRuntime)}"
               + $"&packagedVersion={Uri.EscapeDataString(PackagedVersion)}";
    }

    private bool PackagedIdentityMatches(Version localVersion, string resolvedChannel, string resolvedRuntime)
    {
        return string.Equals(PackagedReleaseChannel, resolvedChannel, StringComparison.OrdinalIgnoreCase)
               && string.Equals(PackagedRuntime, resolvedRuntime, StringComparison.OrdinalIgnoreCase)
               && Version.TryParse(PackagedVersion, out var packagedVersion)
               && VersionsEquivalent(packagedVersion, localVersion);
    }

    private void LogResolvedRelease(string? resolvedChannel, string? resolvedRuntime, string? releaseSource)
    {
        if (!ReleaseChannelsEnabled || string.IsNullOrWhiteSpace(resolvedChannel) || string.IsNullOrWhiteSpace(resolvedRuntime))
        {
            return;
        }

        if (!string.Equals(ReleaseChannel, resolvedChannel, StringComparison.OrdinalIgnoreCase)
            || !string.Equals(RuntimeChannel, resolvedRuntime, StringComparison.OrdinalIgnoreCase))
        {
            Log.Warning(
                $"Requested {ReleaseChannel}/{RuntimeChannel}; UpdateServer resolved {resolvedChannel}/{resolvedRuntime} " +
                $"from {releaseSource ?? "an unspecified source"}.");
        }
        else
        {
            Log.Verbose($"Resolved release: {resolvedChannel}/{resolvedRuntime}; source: {releaseSource ?? "Unknown"}.");
        }
    }

    private static bool VersionsEquivalent(Version left, Version right)
    {
        return left.Major == right.Major
               && left.Minor == right.Minor
               && left.Build == right.Build
               && Math.Max(0, left.Revision) == Math.Max(0, right.Revision);
    }

    private static string? TryFirstHeader(HttpHeaders headers, string name)
    {
        if (!headers.TryGetValues(name, out var values))
        {
            return null;
        }

        foreach (var value in values)
        {
            return value;
        }

        return null;
    }

    protected virtual async Task<(MemoryStream? Stream, string? ContentType)> Download()
    {
        if (OffsetManager.ActiveRegion == ClientRegion.China ||
            OffsetManager.ActiveRegion == ClientRegion.TraditionalChinese)
        {
            ForceChineseDownload = true;
        }

        var result = await DownloadFromUrl(DataUrl).ConfigureAwait(false);

        if (result.Stream == null && DataUrl != ChineseDataUrl)
        {
            ForceChineseDownload = true;
            Log.Warning($"Primary download failed. Attempting alternate download url: {ChineseDataUrl}");
            result = await DownloadFromUrl(ChineseDataUrl).ConfigureAwait(false);
        }

        return result;
    }

    private async Task<(MemoryStream? Stream, string? ContentType)> DownloadFromUrl(string url)
    {
        try
        {
            var sw = Stopwatch.StartNew();
            using var response = await SharedHttpClient
                .GetAsync(url, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var contentLength = response.Content.Headers.ContentLength;
            var mediaType = response.Content.Headers.ContentType?.MediaType;
            Log.Verbose($"Content Type: {mediaType} Size: {contentLength:N0}");

            // Presize the buffer when we know the length. Avoids repeated reallocations.
            var stream = contentLength is > 0 and < int.MaxValue
                ? new MemoryStream((int)contentLength.Value)
                : new MemoryStream();

            await using (var http = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                await http.CopyToAsync(stream, 81920).ConfigureAwait(false);
            }

            sw.Stop();
            stream.Position = 0;

            var size = stream.Length;
            var seconds = sw.Elapsed.TotalSeconds;
            Log.Information(
                $"Downloaded {size / 1024f:N0}kb in {sw.ElapsedMilliseconds:N0}ms. " +
                $"Estimated Speed: {(seconds > 0 ? (size / seconds / 1024d / 1024d) : 0):N2} MB/s");

            return (stream, mediaType);
        }
        catch (Exception e)
        {
            Log.Error($"Failed to download from {url}");
            Log.Exception(e);
            return (null, null);
        }
    }

    [Obsolete("Use CheckAndDownload instead which supports separate check and download URLs and better error handling.")]
    protected virtual async Task<bool> NeedsUpdate()
    {
        var localVersion = GetLocalVersion();
        var remoteVersion = await GetRemoteVersion().ConfigureAwait(false);
        Log.Verbose($"Local Version: {localVersion}");
        Log.Verbose($"Remote Version: {remoteVersion}");
        if (localVersion == null)
        {
            return true;
        }

        if (remoteVersion == null)
        {
            return false;
        }

        var result = localVersion != remoteVersion;
        if (result)
        {
            Log.Information($"Update needed: Local Version {localVersion} != Remote Version {remoteVersion}");
        }
        return result;
    }

    protected virtual Version? GetLocalVersion()
    {
        var info = CompiledAssembly;
        info.Refresh();
        if (!info.Exists)
        {
            return null;
        }

        try
        {
            var versionStr = FileVersionInfo.GetVersionInfo(info.FullName).FileVersion;
            return Version.TryParse(versionStr, out var version) ? version : null;
        }
        catch
        {
            return null;
        }
    }

    [Obsolete("Use CheckAndDownload instead which supports separate check and download URLs and better error handling.")]
    protected virtual async Task<Version?> GetRemoteVersion()
    {
        try
        {
            using var response = await SharedHttpClient
                .GetAsync(VersionUrl, HttpCompletionOption.ResponseHeadersRead)
                .ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var versionStr = (await response.Content.ReadAsStringAsync().ConfigureAwait(false)).Trim();
            if (Version.TryParse(versionStr, out var version))
            {
                return version;
            }

            Log.Error($"Failed to parse remote version: {versionStr} from {VersionUrl}");
            return null;
        }
        catch (Exception e)
        {
            Log.Error("Failed to get remote version");
            Log.Exception(e);
            return null;
        }
    }

    // ----------------------------------------------------------------------------------
    // Assembly infra
    // ----------------------------------------------------------------------------------

    private void RedirectAssembly()
    {
        AssemblyProxy.Init();
        AppDomain.CurrentDomain.AppendPrivatePath(LocalFolderName);
        foreach (var a in AddedAssemblies)
        {
            AssemblyProxy.AddAssembly(a.Name, a.Assembly);
        }
    }

    private static Assembly? LoadAssembly(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            return Assembly.LoadFrom(path);
        }
        catch (Exception e)
        {
            ff14bot.Helpers.Logging.WriteException(e);
            return null;
        }
    }

    // ----------------------------------------------------------------------------------
    // Entry point
    // ----------------------------------------------------------------------------------

    // IAddonProxy<T> uses null to represent load failure but its external contract is not nullable-annotated.
    public async Task<T> Load(string directory)
    {
        var timer = Stopwatch.StartNew();
        try
        {
            Log.Information($"Loading {ProjectName}...");
            LocalFolderName = directory;
            _compiledAssembly = null;
            Directory.CreateDirectory(LocalFolderName);

            if (!Debug)
            {
                await Update().ConfigureAwait(false);
            }

            UnblockAll();

            if (LibraryClass.SafeMode)
            {
                Log.Information($"Safe mode enabled, skipping load of {ProjectName}");
                return null!;
            }

            CompiledAssembly.Refresh();

            if (CompiledAssembly.Exists)
            {
                return Load()!;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to load {ProjectName}");
            Log.Exception(e);
        }
        finally
        {
            timer.Stop();
            Log.Information($"Load finished in {timer.ElapsedMilliseconds:N0}ms");
        }

        return null!;
    }

    public void Dispose()
    {
        // SharedHttpClient is process-wide and intentionally not disposed here.
        GC.SuppressFinalize(this);
    }

    // ----------------------------------------------------------------------------------
    // Utilities
    // ----------------------------------------------------------------------------------

    private static string FormatLoaderExceptions(ReflectionTypeLoadException ex)
    {
        var sb = new StringBuilder();
        foreach (var exSub in ex.LoaderExceptions)
        {
            if (exSub == null)
            {
                continue;
            }

            sb.AppendLine(exSub.Message);
            if (exSub is FileNotFoundException { FusionLog: { Length: > 0 } fusionLog })
            {
                sb.AppendLine("Fusion Log:");
                sb.AppendLine(fusionLog);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }

    public bool IsFileLocked(string filePath)
    {
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
        {
            return false;
        }

        try
        {
            using var fs = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return false;
        }
        catch (IOException e)
        {
            const int ERROR_SHARING_VIOLATION = 0x20;
            const int ERROR_LOCK_VIOLATION = 0x21;
            var hResult = Marshal.GetHRForException(e) & 0xFFFF;
            Log.Warning($"IOException when checking if file is locked: {e.Message} (HRESULT: 0x{hResult:X})");
            return hResult is ERROR_SHARING_VIOLATION or ERROR_LOCK_VIOLATION;
        }
        catch (UnauthorizedAccessException)
        {
            Log.Warning("UnauthorizedAccessException when checking if file is locked. This may indicate the file is locked or ACLs are preventing access.");
            // Another user/process has exclusive access, or ACLs block us — treat as locked.
            return true;
        }
    }

    private enum LoaderCheckOutcome
    {
        Unknown,
        UpToDate,
        UpdateAvailable,
        Failed
    }
}

public static class CompiledLoaderReleaseChannels
{
    private const string ReleaseStatusUrl = "http://update.ffxivbots.com:3000/Release/status";
    private const string SelectorMarker = "LL_RELEASE_CHANNEL";
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(30) };
    private static readonly Regex SelectorRegex = new(
        "^(?<prefix>\\s*protected\\s+override\\s+string\\s+ReleaseChannel\\s*=>\\s*\\\")(?<channel>Stable|Beta)(?<suffix>\\\"\\s*;\\s*//\\s*LL_RELEASE_CHANNEL\\s*)$",
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.CultureInvariant);

    public static async Task<ReleaseStatus> GetReleaseStatusAsync(
        string product,
        string runtime,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(product);
        ArgumentException.ThrowIfNullOrWhiteSpace(runtime);
        var url = $"{ReleaseStatusUrl}?product={Uri.EscapeDataString(product)}&runtime={Uri.EscapeDataString(runtime)}";
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        return await JsonSerializer.DeserializeAsync<ReleaseStatus>(
                   stream,
                   new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
                   cancellationToken)
               .ConfigureAwait(false)
               ?? throw new InvalidDataException("UpdateServer returned an empty release status response.");
    }

    public static bool TryGetSelectedChannel(Assembly productAssembly, out string channel, out string error)
    {
        channel = string.Empty;
        if (!TryFindSelector(productAssembly, out var match, out error))
        {
            return false;
        }

        channel = match.Match.Groups["channel"].Value;
        return true;
    }

    public static bool TrySetSelectedChannel(
        Assembly productAssembly,
        string channel,
        out string error)
    {
        if (channel is not ("Stable" or "Beta"))
        {
            error = "Release channel must be exactly 'Stable' or 'Beta'.";
            return false;
        }

        if (!TryFindSelector(productAssembly, out var selector, out error))
        {
            return false;
        }

        if (string.Equals(selector.Match.Groups["channel"].Value, channel, StringComparison.Ordinal))
        {
            return true;
        }

        var replacement = selector.Match.Groups["prefix"].Value + channel + selector.Match.Groups["suffix"].Value;
        var updatedSource = selector.Source.Remove(selector.Match.Index, selector.Match.Length)
            .Insert(selector.Match.Index, replacement);
        var tempPath = $"{selector.Path}.llchannel.{Guid.NewGuid():N}.tmp";

        try
        {
            File.WriteAllText(tempPath, updatedSource, new UTF8Encoding(false));
            File.Move(tempPath, selector.Path, true);
            error = string.Empty;
            return true;
        }
        catch (Exception exception)
        {
            error = $"Failed to update release channel in '{selector.Path}': {exception.Message}";
            return false;
        }
        finally
        {
            try
            {
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            catch
            {
                // Best-effort cleanup; the original source remains unchanged on failure.
            }
        }
    }

    private static bool TryFindSelector(Assembly productAssembly, out LoaderSelector selector, out string error)
    {
        selector = default;
        ArgumentNullException.ThrowIfNull(productAssembly);
        var assemblyLocation = productAssembly.Location;
        if (string.IsNullOrWhiteSpace(assemblyLocation))
        {
            error = "The product assembly has no file location.";
            return false;
        }

        var directory = Path.GetDirectoryName(assemblyLocation);
        if (string.IsNullOrWhiteSpace(directory) || !Directory.Exists(directory))
        {
            error = "The product loader source directory does not exist.";
            return false;
        }

        var matches = new List<LoaderSelector>();
        foreach (var path in Directory.EnumerateFiles(directory, "*.cs", SearchOption.TopDirectoryOnly))
        {
            var source = File.ReadAllText(path);
            if (!source.Contains(SelectorMarker, StringComparison.Ordinal))
            {
                continue;
            }

            foreach (Match match in SelectorRegex.Matches(source))
            {
                matches.Add(new LoaderSelector(path, source, match));
            }
        }

        if (matches.Count != 1)
        {
            error = matches.Count == 0
                ? $"No valid {SelectorMarker} selector was found beside the product assembly."
                : $"Found {matches.Count} {SelectorMarker} selectors; exactly one is required.";
            return false;
        }

        selector = matches[0];
        error = string.Empty;
        return true;
    }

    private readonly record struct LoaderSelector(string Path, string Source, Match Match);
}

public sealed class ReleaseStatus
{
    public string Product { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string? RequestedChannel { get; set; }
    public string? RequestedRuntime { get; set; }
    public string? ResolvedChannel { get; set; }
    public string? ResolvedRuntime { get; set; }
    public string? Version { get; set; }
    public string? Message { get; set; }
    public bool IsActive => string.Equals(State, "Active", StringComparison.OrdinalIgnoreCase);
}

public static class UnblockHelper
{
    public static void Unblock(this FileInfo file) => Unblock(file.FullName);

    public static void Unblock(string fileName) => DeleteFile(fileName + ":Zone.Identifier");

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteFile(string name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ExtractFromStream(this Stream zipStreamIn, DirectoryInfo outFolderIn)
    {
        using var archive = new ZipArchive(zipStreamIn, ZipArchiveMode.Read, leaveOpen: true);
        archive.ExtractToDirectory(outFolderIn.FullName, overwriteFiles: true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ExtractZip(this FileInfo zipFileIn, DirectoryInfo outFolderIn)
    {
        using var fs = zipFileIn.OpenRead();
        using var archive = new ZipArchive(fs, ZipArchiveMode.Read);
        archive.ExtractToDirectory(outFolderIn.FullName, overwriteFiles: true);
    }
}
