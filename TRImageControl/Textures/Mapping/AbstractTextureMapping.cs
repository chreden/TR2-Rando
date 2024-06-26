﻿using Newtonsoft.Json;
using System.Drawing;
using TRLevelControl.Model;

namespace TRImageControl.Textures;

public abstract class AbstractTextureMapping<E, L> : IDisposable
    where E : Enum
    where L : class
{
    private static readonly Color _defaultSkyBox = Color.FromArgb(88, 152, 184);

    public Dictionary<DynamicTextureSource, DynamicTextureTarget> DynamicMapping { get; set; }
    public Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> StaticMapping { get; set; }
    public List<ReplacementTextureTarget> ReplacementMapping { get; set; }
    public Dictionary<StaticTextureSource<E>, Dictionary<int, List<LandmarkTextureTarget>>> LandmarkMapping { get; set; }
    public List<TextureGrouping<E>> StaticGrouping { get; set; }
    public Color DefaultSkyBox { get; set; }
    public Dictionary<E, E> EntityMap { get; set; }

    protected readonly Dictionary<int, TRImage> _tileMap;
    protected readonly L _level;
    protected bool _committed;

    protected AbstractTextureMapping(L level)
    {
        _level = level;
        _tileMap = new();
        _committed = false;
    }

    protected abstract List<TRMesh> GetModelMeshes(E entity);
    protected abstract List<TRColour> GetPalette8();
    protected abstract List<TRColour4> GetPalette16();
    protected abstract int ImportColour(Color colour);
    protected abstract TRDictionary<E, TRSpriteSequence> GetSpriteSequences();
    protected abstract TRImage GetTile(int tileIndex);
    protected abstract void SetTile(int tileIndex, TRImage image);

    protected static void LoadMapping(AbstractTextureMapping<E, L> levelMapping, string mapFile, TextureDatabase<E> database, Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> predefinedMapping = null, List<E> entitiesToIgnore = null)
    {
        Dictionary<DynamicTextureSource, DynamicTextureTarget> dynamicMapping = new();
        Dictionary<StaticTextureSource<E>, List<StaticTextureTarget>> staticMapping = new();
        List<ReplacementTextureTarget> replacementMapping = new();
        Dictionary<StaticTextureSource<E>, Dictionary<int, List<LandmarkTextureTarget>>> landmarkMapping = new();
        Color skyBoxColour = _defaultSkyBox;

        Dictionary<string, object> rootMapping = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(mapFile));

        // Read the dynamic mapping - this holds object and sprite texture indices for the level to which we will apply an HSB operation
        if (rootMapping.ContainsKey("Dynamic"))
        {
            SortedDictionary<string, Dictionary<string, object>> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, Dictionary<string, object>>>(rootMapping["Dynamic"].ToString());
            foreach (string sourceName in mapping.Keys)
            {
                DynamicTextureSource source = database.GetDynamicSource(sourceName);
                DynamicTextureTarget target = new()
                {
                    DefaultTileTargets = JsonConvert.DeserializeObject<Dictionary<int, List<Rectangle>>>(mapping[sourceName]["Default"].ToString())
                };

                if (mapping[sourceName].ContainsKey("Optional"))
                {
                    target.OptionalTileTargets = JsonConvert.DeserializeObject<Dictionary<TextureCategory, Dictionary<int, List<Rectangle>>>>(mapping[sourceName]["Optional"].ToString());
                }

                dynamicMapping[source] = target;
            }
        }

        // The static mapping contains basic texture segment source to tile target locations
        if (rootMapping.ContainsKey("Static"))
        {
            SortedDictionary<string, object> mapping = JsonConvert.DeserializeObject<SortedDictionary<string, object>>(rootMapping["Static"].ToString());
            foreach (string sourceName in mapping.Keys)
            {
                staticMapping[database.GetStaticSource(sourceName)] = JsonConvert.DeserializeObject<List<StaticTextureTarget>>(mapping[sourceName].ToString());
            }
        }

        // This allows replacing colours in specific areas of tiles with another and will be
        // performed post static and dynamic redrawing. Best example is fixing TR3 fake sky
        // textures in the likes of Jungle after replacing the main skybox.
        if (rootMapping.ContainsKey("ColourReplacements"))
        {
            replacementMapping = JsonConvert.DeserializeObject<List<ReplacementTextureTarget>>(rootMapping["ColourReplacements"].ToString());
        }

        // Landmark mapping links static sources to room number -> rectangle/triangle indices
        if (rootMapping.ContainsKey("Landmarks"))
        {
            Dictionary<string, Dictionary<int, List<LandmarkTextureTarget>>> mapping = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<int, List<LandmarkTextureTarget>>>>(rootMapping["Landmarks"].ToString());
            foreach (string sourceName in mapping.Keys)
            {
                landmarkMapping[database.GetStaticSource(sourceName)] = mapping[sourceName];
            }
        }

        // If a level has had textures removed externally, but the JSON file has static
        // imports ready for it, we need to make sure they are ignored.
        if (entitiesToIgnore != null)
        {
            List<StaticTextureSource<E>> sources = new(staticMapping.Keys);
            for (int i = 0; i < sources.Count; i++)
            {
                StaticTextureSource<E> source = sources[i];
                if (source.TextureEntities != null)
                {
                    foreach (E entity in source.TextureEntities)
                    {
                        if (entitiesToIgnore.Contains(entity))
                        {
                            staticMapping.Remove(source);
                            break;
                        }
                    }
                }
            }
        }

        // Allows for dynamic mapping to be targeted at levels e.g. when importing non-native
        // models that are otherwise undefined in the default level JSON data.
        // This should be done after removing ignored entity textures, for the likes of when
        // Lara is being replaced.
        if (predefinedMapping != null)
        {
            foreach (StaticTextureSource<E> source in predefinedMapping.Keys)
            {
                staticMapping[source] = predefinedMapping[source];
            }
        }

        // Add global sources, unless they are already defined. These tend to be sprite sequences
        // so they will be mapped per GenerateSpriteSequenceTargets, but there is also scope to
        // define global targets if relevant.
        foreach (StaticTextureSource<E> source in database.GlobalGrouping.Sources.Keys)
        {
            if (!staticMapping.ContainsKey(source))
            {
                staticMapping[source] = new List<StaticTextureTarget>(database.GlobalGrouping.Sources[source]);
            }
        }

        // Apply grouping to what has been selected as source elements
        List<TextureGrouping<E>> staticGrouping = database.GlobalGrouping.GetGrouping(staticMapping.Keys);

        levelMapping.DynamicMapping = dynamicMapping;
        levelMapping.StaticMapping = staticMapping;
        levelMapping.ReplacementMapping = replacementMapping;
        levelMapping.StaticGrouping = staticGrouping;
        levelMapping.LandmarkMapping = landmarkMapping;
        levelMapping.DefaultSkyBox = skyBoxColour;
    }

    public void RedrawTargets(AbstractTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
    {
        if (source is DynamicTextureSource dynamicSource)
        {
            RedrawDynamicTargets(dynamicSource, variant, options);
        }
        else if (source is StaticTextureSource<E> staticSource)
        {
            RedrawStaticTargets(staticSource, variant, options);
        }
    }

    public void RedrawDynamicTargets(DynamicTextureSource source, string variant, Dictionary<TextureCategory, bool> options)
    {
        HSBOperation op = source.OperationMap[variant];
        DynamicTextureTarget target = DynamicMapping[source];

        if (options.ContainsKey(TextureCategory.LevelColours) && options[TextureCategory.LevelColours])
        {
            RedrawDynamicTargets(target.DefaultTileTargets, op);
        }

        foreach (TextureCategory category in target.OptionalTileTargets.Keys)
        {
            if (options.ContainsKey(category) && options[category])
            {
                RedrawDynamicTargets(target.OptionalTileTargets[category], op);
            }
        }

        RecolourDynamicTargets(target.ModelColourTargets, op);
    }

    private void RedrawDynamicTargets(Dictionary<int, List<Rectangle>> targets, HSBOperation operation)
    {
        foreach (int tileIndex in targets.Keys)
        {
            TRImage bg = GetImage(tileIndex);
            foreach (Rectangle rect in targets[tileIndex])
            {
                bg.AdjustHSB(rect, operation);
            }
        }
    }

    private void RecolourDynamicTargets(List<TRMesh> meshes, HSBOperation operation)
    {
        List<TRColour> palette = GetPalette8();
        ISet<ushort> colourIndices = new HashSet<ushort>();
        Dictionary<int, int> remapIndices = new();

        foreach (TRMesh mesh in meshes)
        {
            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                colourIndices.Add(face.Texture);
            }
        }

        foreach (ushort colourIndex in colourIndices)
        {
            if (colourIndex == 0)
            {
                continue;
            }
            TRColour col = palette[colourIndex];
            HSB hsb = col.ToTR1Color().ToHSB();
            hsb.H = operation.ModifyHue(hsb.H);
            hsb.S = operation.ModifySaturation(hsb.S);
            hsb.B = operation.ModifyBrightness(hsb.B);

            int newColourIndex = ImportColour(hsb.ToColour());
            remapIndices.Add(colourIndex, newColourIndex);
        }

        foreach (TRMesh mesh in meshes)
        {
            foreach (TRMeshFace face in mesh.ColouredFaces)
            {
                if (remapIndices.ContainsKey(face.Texture))
                {
                    face.Texture = (ushort)remapIndices[face.Texture];
                }
            }
        }
    }

    public void RedrawStaticTargets(StaticTextureSource<E> source, string variant, Dictionary<TextureCategory, bool> options)
    {
        if (source.Categories != null)
        {
            // Exclude it if any of its categories are in the options and switched off
            foreach (TextureCategory category in source.Categories)
            {
                if (options.ContainsKey(category) && !options[category])
                {
                    return;
                }
            }
        }

        // For sprite sequence sources, the targets are mapped dynamically.
        if (source.IsSpriteSequence && (!StaticMapping.ContainsKey(source) || StaticMapping[source].Count == 0))
        {
            GenerateSpriteSequenceTargets(source);
        }

        // This can happen if we have a source grouped for this level, but the source is actually only
        // in place on certain conditions - an example is the flame in Venice, which is only added if
        // the Flamethrower has been imported.
        if (!StaticMapping.ContainsKey(source))
        {
            return;
        }

        List<Rectangle> segments = source.VariantMap[variant];
        foreach (StaticTextureTarget target in StaticMapping[source])
        {
            if (target.Segment < 0 || target.Segment >= segments.Count)
            {
                throw new IndexOutOfRangeException(string.Format("Segment {0} is invalid for texture source {1}.", target.Segment, source.PNGPath));
            }

            GetImage(target.Tile).ImportSegment(source.Image, target, segments[target.Segment]);
        }

        if (source.EntityColourMap != null)
        {
            List<TRColour4> palette = GetPalette16();

            foreach (E entity in source.EntityColourMap.Keys)
            {
                E translatedEntity = entity;
                if (EntityMap != null && EntityMap.ContainsKey(entity))
                {
                    translatedEntity = EntityMap[entity];
                }
                List<TRMesh> meshes = GetModelMeshes(translatedEntity);
                if (meshes == null)
                {
                    continue;
                }

                List<int> colourIndices = meshes.SelectMany(m => m.ColouredFaces.Select(f => f.Texture >> 8))
                    .Distinct().ToList();

                Dictionary<int, int> remapIndices = new();
                foreach (Color targetColour in source.EntityColourMap[entity].Keys)
                {
                    int matchedIndex = colourIndices.FindIndex(
                        i => palette[i].Red == targetColour.R && palette[i].Green == targetColour.G && palette[i].Blue == targetColour.B);
                    if (matchedIndex == -1)
                    {
                        continue;
                    }

                    matchedIndex = colourIndices[matchedIndex];

                    // Extract the colour from the top-left of the rectangle specified in the source, and import that into the level
                    int sourceRectangle = source.EntityColourMap[entity][targetColour];
                    int newColourIndex = ImportColour(source.Image.GetPixel(segments[sourceRectangle].X, segments[sourceRectangle].Y));
                    remapIndices[matchedIndex] = newColourIndex;
                }

                // Remap the affected mesh textures to the newly inserted colours
                foreach (TRMeshFace face in meshes.SelectMany(m => m.ColouredFaces))
                {
                    int oldColour = face.Texture >> 8;
                    if (remapIndices.ContainsKey(oldColour))
                    {
                        face.Texture = (ushort)(remapIndices[oldColour] | (face.Texture & 0xFF));
                    }
                }
            }
        }

        if (source.EntityColourMap8 != null)
        {
            foreach (E entity in source.EntityColourMap8.Keys)
            {
                E translatedEntity = entity;
                if (EntityMap != null && EntityMap.ContainsKey(entity))
                {
                    translatedEntity = EntityMap[entity];
                }
                List<TRMesh> meshes = GetModelMeshes(translatedEntity);
                if (meshes == null || meshes.Count == 0)
                {
                    continue;
                }

                Dictionary<int, int> remapIndices = new();

                List<TRColour> palette = GetPalette8();
                foreach (Color targetColour in source.EntityColourMap8[entity].Keys)
                {
                    TRColour col = targetColour.ToTRColour();
                    int matchedIndex = palette.FindIndex(c => c.Red == col.Red && c.Green == col.Green && c.Blue == col.Blue);
                    if (matchedIndex == -1)
                    {
                        continue;
                    }

                    int sourceRectangle = source.EntityColourMap8[entity][targetColour];
                    int newColourIndex = ImportColour(source.Image.GetPixel(segments[sourceRectangle].X, segments[sourceRectangle].Y));
                    remapIndices.Add(matchedIndex, newColourIndex);
                }

                foreach (TRMesh mesh in meshes)
                {
                    foreach (TRMeshFace face in mesh.ColouredFaces)
                    {
                        if (remapIndices.ContainsKey(face.Texture))
                        {
                            face.Texture = (ushort)remapIndices[face.Texture];
                        }
                    }
                }
            }
        }
    }

    public void DrawReplacements()
    {
        foreach (ReplacementTextureTarget replacement in ReplacementMapping)
        {
            Color search = GetImage(replacement.Search.Tile).GetPixel(replacement.Search.X, replacement.Search.Y);
            Color replace = GetImage(replacement.Replace.Tile).GetPixel(replacement.Replace.X, replacement.Replace.Y);
            // Scan each tile and replace colour Search with above
            foreach (int tile in replacement.ReplacementMap.Keys)
            {
                TRImage graphics = GetImage(tile);
                foreach (Rectangle rect in replacement.ReplacementMap[tile])
                {
                    graphics.Replace(rect, search, replace);
                }
            }
        }
    }

    private void GenerateSpriteSequenceTargets(StaticTextureSource<E> source)
    {
        if (!source.HasVariants)
        {
            throw new ArgumentException(string.Format("SpriteSequence {0} cannot be dynamically mapped without at least one source rectangle.", source.SpriteSequence));
        }

        TRDictionary<E, TRSpriteSequence> spriteSequences = GetSpriteSequences();
        TRSpriteSequence sequence = spriteSequences[source.SpriteSequence];
        if (sequence == null)
        {
            return;
        }

        StaticMapping[source] = new();

        // We only want to define targets for the number of source rectangles, rather
        // than the total number of sprites.
        int numTargets = source.VariantMap[source.Variants[0]].Count;
        for (int j = 0; j < numTargets && j < sequence.Textures.Count; j++)
        {
            TRSpriteTexture sprite = sequence.Textures[j];
            StaticMapping[source].Add(new()
            {
                Segment = j,
                Tile = sprite.Atlas,
                X = sprite.X,
                Y = sprite.Y
            });
        }
    }

    private TRImage GetImage(int tile)
    {
        if (!_tileMap.ContainsKey(tile))
        {
            _tileMap.Add(tile, GetTile(tile));
        }

        return _tileMap[tile];
    }

    public void Dispose()
    {
        CommitGraphics();
        GC.SuppressFinalize(this);
    }

    public virtual void CommitGraphics()
    {
        if (!_committed)
        {
            foreach (int tile in _tileMap.Keys)
            {
                SetTile(tile, _tileMap[tile]);
            }
            _committed = true;
        }
    }
}
