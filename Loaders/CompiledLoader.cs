//!CompilerOption:AddRef:System.Diagnostics.FileVersionInfo.dll
//!CompilerOption:AddRef:System.Formats.Tar.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Clio.Utilities;
using ff14bot.AClasses;
using ff14bot.Interfaces;
using LlamaLibrary.Logging;
using SevenZip;
// ReSharper disable VirtualMemberCallInConstructor

namespace LlamaLibrary.Loaders;

public abstract class BotBaseLoader : CompiledLoader<BotBase>
{
}

public abstract class PluginLoader : CompiledLoader<IBotPlugin>
{
}

public abstract class RoutineLoader : CompiledLoader<CombatRoutine>
{
}

/*
public class IslandGathererLoader : BotBaseLoader
{
    private const string VersionFileName = "Version.txt";
    protected override string ProjectName => "IslandGatherer";
    protected override Color LogColor => Colors.Lime;
    protected override bool Debug => true;
    protected override string CompiledAssemblyName { get; } = "IslandGathererWPF.dll";
}
*/

public abstract class CompiledLoader<T> : IAddonProxy<T> where T : class
{
    protected readonly LLogger Log;
    private HttpClient? _client;

    protected CompiledLoader()
    {
        Log = new LLogger(ProjectName, LogColor);
    }

    protected HttpClient GetHttpClient()
    {
        if (_client != null)
        {
            return _client;
        }
        _client = new()
        {
            Timeout = new TimeSpan(0, 0, 30)
        };
        return _client;
    }

    protected string LocalFolderName { get; private set; }
    protected abstract string ProjectName { get; }
    protected virtual string CompiledAssemblyName => $"{ProjectName}.dll";
    protected bool ForceChineseDownload = false;
    protected virtual string ChineseDataUrl => $"http://update.ffxivbots.com:3000/Download/cn?product={ProjectName}&force={ForceChineseDownload.ToString().ToLower()}";
    protected virtual string GlobalDataUrl => $"http://update.ffxivbots.com:3000/Download?product={ProjectName}";

#if RB_CN
    protected virtual string DataUrl => ChineseDataUrl;
#else
    protected virtual string DataUrl => GlobalDataUrl;
#endif

    protected virtual string VersionUrl => $"http://update.ffxivbots.com:3000/version?product={ProjectName}";
    protected virtual bool Debug => false;
    protected FileInfo CompiledAssembly => new(Path.Combine(LocalFolderName, CompiledAssemblyName));
    protected virtual Color LogColor => Colors.Lime;
    protected virtual LogLevel LogLevel => LogLevel.Information;
    protected virtual List<(string Name, Assembly Assembly)> AddedAssemblies => new();

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
            baseType = assembly.DefinedTypes.FirstOrDefault(i => typeof(T).IsAssignableFrom(i));
        }
        catch (ReflectionTypeLoadException ex)
        {
            var sb = new StringBuilder();
            foreach (var exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub?.Message);
                if (exSub is FileNotFoundException exFileNotFound)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }

                sb.AppendLine();
            }

            var errorMessage = sb.ToString();
            Log.Error(errorMessage);
            return null;
            //Display or log the error based on your application.
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

        T? compiledInstance;
        try
        {
            compiledInstance = Activator.CreateInstance(baseType) as T;
        }
        catch (ReflectionTypeLoadException ex)
        {
            var sb = new StringBuilder();
            foreach (var exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub?.Message);
                if (exSub is FileNotFoundException exFileNotFound)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }

                sb.AppendLine();
            }

            var errorMessage = sb.ToString();
            Log.Error(errorMessage);
            return null;
            //Display or log the error based on your application.
        }
        catch (Exception e)
        {
            Log.Error("Other Exception2");
            Log.Exception(e);
            return null;
        }

        return compiledInstance;
    }

    protected virtual async Task<bool> Update()
    {
        if (!await NeedsUpdate())
        {
            return false;
        }

        Log.Information($"Updating {ProjectName}...");
        var downloadInfo = await Download();
        if (downloadInfo.Stream == null)
        {
            Log.Error($"Failed to download {ProjectName}");
            return false;
        }

        try
        {
            Clean(LocalFolderName);
            CompiledAssembly.Directory?.Create();

            switch (downloadInfo.ContentType)
            {
                case "application/zip":
                {
                    var sw = Stopwatch.StartNew();
                    downloadInfo.Stream.ExtractFromStream(new DirectoryInfo(LocalFolderName));
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
                    outStream.Flush();
                    outStream.Position = 0;
                    await TarFile.ExtractToDirectoryAsync(outStream, LocalFolderName, true);
                    break;
                }

                default:
                    Log.Error($"Unknown content type: {downloadInfo.ContentType}");
                    return false;
            }
        }
        catch (Exception e)
        {
            Log.Error($"Failed to extract {ProjectName}");
            Log.Exception(e);
            return false;
        }
        finally
        {
            await downloadInfo.Stream.DisposeAsync();
        }

        Log.Information($"{ProjectName} updated");
        return true;
    }

    protected void UnblockAll()
    {
        foreach (var file in new DirectoryInfo(LocalFolderName).GetFiles("*.dll"))
        {
            file.Unblock();
        }
    }

    protected virtual async Task<(MemoryStream? Stream, string? ContentType)> Download()
    {
        try
        {
            var sw = Stopwatch.StartNew();
            using var response = await GetHttpClient().GetAsync(DataUrl, HttpCompletionOption.ResponseContentRead);
            Log.Verbose($"{DataUrl}");
            Log.Verbose($"Content Type: {response.Content.Headers.ContentType?.MediaType} Size: {response.Content.Headers.ContentLength:N0}");
            response.EnsureSuccessStatusCode();
            var stream = new MemoryStream();
            await response.Content.CopyToAsync(stream);
            sw.Stop();
            stream.Position = 0;
            Log.Information($"Downloaded {response.Content.Headers.ContentLength / 1024f:N0}kb in {sw.ElapsedMilliseconds:N0}ms. Estimated Speed: {(stream.Length / sw.Elapsed.TotalSeconds / 1024 / 1024):N2} MB/s");
            return (stream, response.Content.Headers.ContentType?.MediaType);
        }
        catch (Exception e)
        {
            Log.Error("Failed to download");
            Log.Exception(e);
            return (null, null);
        }
    }

    protected virtual async Task<bool> NeedsUpdate()
    {
        var localVersion = GetLocalVersion();
        var remoteVersion = await GetRemoteVersion();
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

        return localVersion != remoteVersion;
    }

    protected virtual Version? GetLocalVersion()
    {
        if (!CompiledAssembly.Exists)
        {
            return null;
        }

        var versionStr = FileVersionInfo.GetVersionInfo(CompiledAssembly.FullName).FileVersion;
        return Version.TryParse(versionStr, out var version) ? version : null;
    }

    protected virtual async Task<Version?> GetRemoteVersion()
    {
        try
        {
            using var response = await GetHttpClient().SendAsync(new HttpRequestMessage(HttpMethod.Get, VersionUrl), HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            var versionStr = (await response.Content.ReadAsStringAsync()).Trim();
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

    private void RedirectAssembly()
    {
        AssemblyProxy.Init();
        AppDomain.CurrentDomain.AppendPrivatePath(LocalFolderName);
        AddedAssemblies.ForEach(a => AssemblyProxy.AddAssembly(a.Name, a.Assembly));
    }

    private static Assembly? LoadAssembly(string path)
    {
        if (!File.Exists(path))
        {
            return null;
        }

        Assembly? assembly = null;
        try
        {
            assembly = Assembly.LoadFrom(path);
        }
        catch (Exception e)
        {
            ff14bot.Helpers.Logging.WriteException(e);
        }

        return assembly;
    }

    private static void Clean(string directory)
    {
        foreach (var file in new DirectoryInfo(directory).GetFiles())
        {
            file.Delete();
        }

        foreach (var dir in new DirectoryInfo(directory).GetDirectories())
        {
            dir.Delete(true);
        }
    }

    public async Task<T> Load(string directory)
    {
        Log.Information($"Loading {ProjectName}...");
        LocalFolderName = directory;

        if (!Debug)
        {
            await Update();
            _client?.Dispose();
        }

        UnblockAll();

        if (LibraryClass.SafeMode)
        {
            Log.Information($"Safe mode enabled, skipping load of {ProjectName}");
            return null;
        }

        CompiledAssembly.Refresh();

        if (CompiledAssembly.Exists)
        {
            return Load();
        }

        return null;
    }
}

public static class UnblockHelper
{
    public static void Unblock(this FileInfo file)
    {
        Unblock(file.FullName);
    }

    public static void Unblock(string fileName)
    {
        DeleteFile(fileName + ":Zone.Identifier");
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool DeleteFile(string name);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ExtractFromStream(this Stream zipStreamIn, DirectoryInfo outFolderIn)
    {
        using ZipArchive archive = new(zipStreamIn, ZipArchiveMode.Read);
        archive.ExtractToDirectory(outFolderIn.FullName, true);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ExtractZip(this FileInfo zipFileIn, DirectoryInfo outFolderIn)
    {
        using ZipArchive archive = new(zipFileIn.OpenRead(), ZipArchiveMode.Read);
        archive.ExtractToDirectory(outFolderIn.FullName, true);
    }
}