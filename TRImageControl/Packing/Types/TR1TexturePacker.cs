﻿using TRLevelControl;
using TRLevelControl.Model;

namespace TRImageControl.Packing;

public class TR1TexturePacker : TRTexturePacker<TR1Type, TR1Level>
{
    private const int _maximumTiles = 16;

    public TRPalette8Control PaletteManager { get; set; }

    public override int NumLevelImages => Level.Images8.Count;

    public TR1TexturePacker(TR1Level level, ITextureClassifier classifier = null)
        : base(level, _maximumTiles, classifier) { }

    protected override List<TRTextileSegment> LoadObjectTextures()
    {
        List<TRTextileSegment> textures = new(Level.ObjectTextures.Count);
        for (int i = 0; i < Level.ObjectTextures.Count; i++)
        {
            TRObjectTexture texture = Level.ObjectTextures[i];
            //if (texture.IsValid())
            {
                textures.Add(new IndexedTRObjectTexture
                {
                    Index = i,
                    Classification = _levelClassifier,
                    Texture = texture
                });
            }
        }
        return textures;
    }

    protected override List<TRTextileSegment> LoadSpriteTextures()
    {
        List<TRSpriteTexture> sprites = Level.Sprites.SelectMany(s => s.Value.Textures).ToList();
        List<TRTextileSegment> textures = new();
        for (int i = 0; i < sprites.Count; i++)
        {
            TRSpriteTexture texture = sprites[i];
            //if (texture.IsValid())
            {
                textures.Add(new IndexedTRSpriteTexture
                {
                    Index = i,
                    Classification = _levelClassifier,
                    Texture = texture
                });
            }
        }
        return textures;
    }

    protected override List<TRMesh> GetModelMeshes(TR1Type modelEntity)
    {
        return Level.Models[modelEntity]?.Meshes;
    }

    protected override TRSpriteSequence GetSpriteSequence(TR1Type entity)
    {
        return Level.Sprites[entity];
    }

    protected override IEnumerable<TR1Type> GetAllModelTypes()
    {
        return Level.Models.Keys.ToList();
    }

    protected override void CreateImageSpace(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Level.Images8.Add(new()
            {
                Pixels = new byte[TRConsts.TPageSize]
            });
        }
    }

    public override TRImage  GetTile(int tileIndex)
    {
        return new(Level.Images8[tileIndex].Pixels, Level.Palette);
    }

    public override void SetTile(int tileIndex, TRImage image)
    {
        PaletteManager ??= new()
        {
            Level = Level
        };
        PaletteManager.ChangedTiles[tileIndex] = image;
    }

    protected override void PostCommit()
    {
        PaletteManager?.MergeTiles();
    }
}
