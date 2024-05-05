﻿using System.Drawing;
using System.Drawing.Imaging;

namespace TRImageControl.Textures;

public class StaticTextureSource<E> : AbstractTextureSource, IDisposable
    where E : Enum
{
    public string PNGPath { get; set; }
    public E SpriteSequence { get; set; }
    public bool IsSpriteSequence => !EqualityComparer<E>.Default.Equals(SpriteSequence, default);
    public Dictionary<E, Dictionary<Color, int>> EntityColourMap { get; set; }
    public Dictionary<E, Dictionary<Color, int>> EntityColourMap8 { get; set; }
    public Dictionary<E, Dictionary<int, int>> EntityTextureMap { get; set; }
    public IEnumerable<E> ColourEntities => EntityColourMap?.Keys;
    public IEnumerable<E> TextureEntities => EntityTextureMap?.Keys;
    public Dictionary<string, List<Rectangle>> VariantMap { get; set; }
    public override string[] Variants => VariantMap.Keys.ToArray();
    public bool HasVariants => VariantMap.Count > 0;

    private Bitmap _bitmap;
    public Bitmap Bitmap
    {
        get => _bitmap ??= new(PNGPath);
    }

    public Bitmap ClonedBitmap => Bitmap.Clone(new Rectangle(0, 0, Bitmap.Width, Bitmap.Height), PixelFormat.Format32bppArgb);

    public void Dispose()
    {
        _bitmap?.Dispose();
        GC.SuppressFinalize(this);
    }

    public override bool Equals(object obj)
    {
        return obj is StaticTextureSource<E> source && PNGPath == source.PNGPath;
    }

    public override int GetHashCode()
    {
        return -193438157 + EqualityComparer<string>.Default.GetHashCode(PNGPath);
    }
}
