﻿using System.Drawing;
using TRLevelControl.Model;
using TRTexture16Importer.Helpers;

namespace TRTexture16Importer.Textures;

public class TR1TextureMapping : AbstractTextureMapping<TR1Type, TR1Level>
{
    public TRPalette8Control PaletteManager { get; set; }

    protected TR1TextureMapping(TR1Level level)
        : base(level) { }

    public static TR1TextureMapping Get(TR1Level level, string mappingFilePrefix, TR1TextureDatabase database, Dictionary<StaticTextureSource<TR1Type>, List<StaticTextureTarget>> predefinedMapping = null, List<TR1Type> entitiesToIgnore = null, Dictionary<TR1Type, TR1Type> entityMap = null)
    {
        string mapFile = Path.Combine(@"Resources\TR1\Textures\Mapping\", mappingFilePrefix + "-Textures.json");
        if (!File.Exists(mapFile))
        {
            return null;
        }

        TR1TextureMapping mapping = new(level);
        LoadMapping(mapping, mapFile, database, predefinedMapping, entitiesToIgnore);
        mapping.EntityMap = entityMap;
        return mapping;
    }

    protected override List<TRColour> GetPalette8()
    {
        return _level.Palette;
    }

    protected override List<TRColour4> GetPalette16()
    {
        return null;
    }

    protected override int ImportColour(Color colour)
    {
        PaletteManager ??= new()
        {
            Level = _level
        };
        return PaletteManager.AddPredefinedColour(colour);
    }

    protected override List<TRMesh> GetModelMeshes(TR1Type entity)
    {
        return _level.Models.Find(m => m.ID == (uint)entity)?.Meshes;
    }

    protected override List<TRSpriteSequence> GetSpriteSequences()
    {
        return _level.SpriteSequences;
    }

    protected override List<TRSpriteTexture> GetSpriteTextures()
    {
        return _level.SpriteTextures;
    }

    protected override Bitmap GetTile(int tileIndex)
    {
        return _level.Images8[tileIndex].ToBitmap(_level.Palette);
    }

    protected override void SetTile(int tileIndex, Bitmap bitmap)
    {
        PaletteManager ??= new()
        {
            Level = _level
        };
        PaletteManager.ChangedTiles[tileIndex] = bitmap;
    }

    public override void CommitGraphics()
    {
        if (!_committed)
        {
            foreach (int tile in _tileMap.Keys)
            {
                SetTile(tile, _tileMap[tile].Bitmap);
            }

            PaletteManager?.MergeTiles();

            foreach (int tile in _tileMap.Keys)
            {
                _tileMap[tile].Dispose();
            }

            _committed = true;
        }
    }
}
