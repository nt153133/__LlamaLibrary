using System;
using System.Drawing;
using System.IO;
using ff14bot.Managers;
using LlamaLibrary.Helpers.Cache.BaseClasses;

namespace LlamaLibrary.Helpers.Cache;

public class ItemIconStore : IFileStorable<Image>
{
    public string FileExtension => ".png";
    public string BasePath { get; set; } = "";

    public ItemIconStore()
    {
    }

    public ItemIconStore(string basePath)
    {
        BasePath = basePath;
        Directory.CreateDirectory(BasePath);
    }

    public virtual void Save(Image obj, string name)
    {
        obj.Save(GetPath(name));
    }

    public virtual Image? Load(string name)
    {
        return Exists(name) ? Image.FromFile(GetPath(name)) : null;
    }

    public bool Exists(string name)
    {
        return File.Exists(GetPath(name));
    }

    public virtual string GetPath(string name)
    {
        return Path.Combine(BasePath, name + FileExtension);
    }
}

public class ItemIconCacheable : ICacheable<Image>
{
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

public class ItemIconStoreCustom : ItemIconStore
{
    public int Size { get; set; } = 20;

    public override string GetPath(string name)
    {
        return Path.Combine(BasePath, name + $"_{Size}x{Size}" + FileExtension);
    }

    public override void Save(Image obj, string name)
    {
        var thumb = obj.GetThumbnailImage(Size, Size, () => false, IntPtr.Zero);
        thumb.Save(GetPath(name));
    }
}

public class CustomIconFileCache : FileCache<Image>
{
    public CustomIconFileCache(string basePath, int size) : base(new ItemIconStoreCustom { Size = size }, new ItemIconCacheable(), basePath)
    {
    }

    public Image? GetIcon(uint id)
    {
        return Get((id % 1_000_000).ToString());
    }
}

public class IconFileCache : FileCache<Image, ItemIconStore, ItemIconCacheable>
{
    public IconFileCache(string basePath) : base(basePath)
    {
    }

    public Image? GetIcon(uint id)
    {
        return Get((id % 1_000_000).ToString());
    }
}