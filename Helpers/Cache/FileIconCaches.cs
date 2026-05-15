using System;
using System.Drawing;
using System.IO;
using ff14bot.Managers;
using LlamaLibrary.Helpers.Cache.BaseClasses;

namespace LlamaLibrary.Helpers.Cache;

/// <summary>
/// An <see cref="IFileStorable{T}"/> implementation that persists <see cref="Image"/> objects
/// as PNG files on disk. Used by <see cref="IconFileCache"/> and <see cref="CustomIconFileCache"/>
/// to cache FFXIV item icons between sessions.
/// </summary>
public class ItemIconStore : IFileStorable<Image>
{
    /// <summary>Gets the file extension used for stored icon images.</summary>
    /// <value>Always <c>".png"</c>.</value>
    public string FileExtension => ".png";

    /// <summary>Gets or sets the root directory where icon PNG files are written and read.</summary>
    public string BasePath { get; set; } = "";

    /// <summary>Initializes a new <see cref="ItemIconStore"/> with an empty base path.</summary>
    public ItemIconStore()
    {
    }

    /// <summary>
    /// Initializes a new <see cref="ItemIconStore"/> rooted at <paramref name="basePath"/>,
    /// creating the directory if it does not exist.
    /// </summary>
    /// <param name="basePath">The directory in which icon PNG files will be stored.</param>
    public ItemIconStore(string basePath)
    {
        BasePath = basePath;
        Directory.CreateDirectory(BasePath);
    }

    /// <summary>Saves the given <paramref name="obj"/> image to disk as a PNG file.</summary>
    /// <param name="obj">The <see cref="Image"/> to persist.</param>
    /// <param name="name">The file name (without extension) used to identify the icon.</param>
    public virtual void Save(Image obj, string name)
    {
        obj.Save(GetPath(name));
    }

    /// <summary>Loads a previously saved icon image from disk.</summary>
    /// <param name="name">The file name (without extension) of the stored icon.</param>
    /// <returns>The loaded <see cref="Image"/>, or <see langword="null"/> if the file does not exist.</returns>
    public virtual Image? Load(string name)
    {
        return Exists(name) ? Image.FromFile(GetPath(name)) : null;
    }

    /// <summary>Checks whether an icon file for <paramref name="name"/> exists on disk.</summary>
    /// <param name="name">The file name (without extension) to check.</param>
    /// <returns><see langword="true"/> if the PNG file exists; otherwise <see langword="false"/>.</returns>
    public bool Exists(string name)
    {
        return File.Exists(GetPath(name));
    }

    /// <summary>Returns the full filesystem path for the icon identified by <paramref name="name"/>.</summary>
    /// <param name="name">The file name (without extension).</param>
    /// <returns>The full path including <see cref="BasePath"/> and <see cref="FileExtension"/>.</returns>
    public virtual string GetPath(string name)
    {
        return Path.Combine(BasePath, name + FileExtension);
    }
}

/// <summary>
/// An <see cref="ICacheable{T}"/> implementation that fetches FFXIV item icons from game data
/// via <see cref="DataManager.GetItem"/> on a cache miss.
/// </summary>
public class ItemIconCacheable : ICacheable<Image>
{
    /// <summary>
    /// Fetches the icon image for the item identified by <paramref name="key"/> from game data.
    /// </summary>
    /// <param name="key">A string representation of the item ID (must be parseable as a <see cref="uint"/>).</param>
    /// <returns>
    /// The item's <see cref="Image"/> icon, or <see langword="null"/> if the item does not exist
    /// or the key is not a valid item ID.
    /// </returns>
    public Image? CacheMiss(string key)
    {
        if (!uint.TryParse(key, out var id))
        {
            return null;
        }

        var item = DataManager.GetItem(id);

        return item?.IconImage;
    }
}

/// <summary>
/// A variant of <see cref="ItemIconStore"/> that saves icons at a custom square size
/// rather than their original resolution.
/// </summary>
public class ItemIconStoreCustom : ItemIconStore
{
    /// <summary>Gets or sets the pixel dimension of the square thumbnail to generate.</summary>
    /// <value>Default is <c>20</c>.</value>
    public int Size { get; set; } = 20;

    /// <summary>
    /// Returns the full filesystem path for the thumbnail icon, including a <c>_{Size}x{Size}</c> suffix
    /// to distinguish it from full-size icons stored in the same directory.
    /// </summary>
    /// <param name="name">The file name (without extension).</param>
    /// <returns>The full path for the thumbnail PNG.</returns>
    public override string GetPath(string name)
    {
        return Path.Combine(BasePath, name + $"_{Size}x{Size}" + FileExtension);
    }

    /// <summary>
    /// Scales <paramref name="obj"/> to <see cref="Size"/>×<see cref="Size"/> pixels and saves
    /// the thumbnail to disk.
    /// </summary>
    /// <param name="obj">The original full-size icon image.</param>
    /// <param name="name">The file name (without extension) used to identify the icon.</param>
    public override void Save(Image obj, string name)
    {
        var thumb = obj.GetThumbnailImage(Size, Size, () => false, IntPtr.Zero);
        thumb.Save(GetPath(name));
    }
}

/// <summary>
/// A <see cref="FileCache{T}"/>-based icon cache that stores icons at a custom square size.
/// Uses <see cref="ItemIconStoreCustom"/> for on-disk storage and <see cref="ItemIconCacheable"/>
/// to fetch missing icons from game data.
/// </summary>
public class CustomIconFileCache : FileCache<Image>
{
    /// <summary>
    /// Initializes a new <see cref="CustomIconFileCache"/> that stores thumbnails of the specified size.
    /// </summary>
    /// <param name="basePath">The directory in which thumbnail PNG files are stored.</param>
    /// <param name="size">The square pixel dimension for thumbnail icons (e.g. <c>20</c> for 20×20).</param>
    public CustomIconFileCache(string basePath, int size) : base(new ItemIconStoreCustom { Size = size }, new ItemIconCacheable(), basePath)
    {
    }

    /// <summary>
    /// Retrieves the thumbnail icon for the given FFXIV item ID, fetching and caching it if necessary.
    /// </summary>
    /// <param name="id">The raw FFXIV item ID. The value is normalized to <c>id % 1_000_000</c> before lookup.</param>
    /// <returns>The thumbnail <see cref="Image"/>, or <see langword="null"/> if it cannot be produced.</returns>
    public Image? GetIcon(uint id)
    {
        return Get((id % 1_000_000).ToString());
    }
}

/// <summary>
/// A strongly-typed <see cref="FileCache{T, TFileStore, TCacheable}"/> that caches full-size
/// FFXIV item icon images. Icons are stored as PNG files and fetched from game data on a miss.
/// </summary>
public class IconFileCache : FileCache<Image, ItemIconStore, ItemIconCacheable>
{
    /// <summary>
    /// Initializes a new <see cref="IconFileCache"/> rooted at the given directory.
    /// </summary>
    /// <param name="basePath">The directory in which full-size icon PNG files are stored.</param>
    public IconFileCache(string basePath) : base(basePath)
    {
    }

    /// <summary>
    /// Retrieves the full-size icon for the given FFXIV item ID, fetching and caching it if necessary.
    /// </summary>
    /// <param name="id">The raw FFXIV item ID. The value is normalized to <c>id % 1_000_000</c> before lookup.</param>
    /// <returns>The full-size <see cref="Image"/>, or <see langword="null"/> if it cannot be produced.</returns>
    public Image? GetIcon(uint id)
    {
        return Get((id % 1_000_000).ToString());
    }
}