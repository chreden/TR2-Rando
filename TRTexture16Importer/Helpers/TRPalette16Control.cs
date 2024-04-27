﻿using System.Drawing;
using TRLevelControl.Model;

namespace TRTexture16Importer.Helpers;

public class TRPalette16Control
{
    private readonly List<TRColour4> _palette;
    private readonly Queue<int> _freeIndices;

    public TRPalette16Control(TR2Level level)
        : this(level.Palette16, level.Models.SelectMany(m => m.Meshes).Concat(level.StaticMeshes.Select(s => s.Mesh))) { }

    public TRPalette16Control(TR3Level level)
        : this(level.Palette16, level.Models.SelectMany(m => m.Meshes).Concat(level.StaticMeshes.Select(s => s.Mesh))) { }

    public TRPalette16Control(List<TRColour4> palette16, IEnumerable<TRMesh> meshes)
    {
        _palette = palette16;

        IEnumerable<int> colourRefs = meshes
            .SelectMany(m => m.ColouredRectangles)
            .Select(f => f.Texture >> 8)
            .Concat(meshes
                .SelectMany(m => m.ColouredTriangles)
                .Select(f => f.Texture >> 8));

        List<int> range = new(Enumerable.Range(0, palette16.Count));
        _freeIndices = new(range.Except(colourRefs));
    }

    public int Import(Color colour)
        => Import(colour.ToTRColour4());

    public int Import(TRColour4 colour)
    {
        int index = _palette.FindIndex(c
            => c.Red == colour.Red && c.Green == colour.Green && c.Blue == colour.Blue);

        if (index == -1)
        {
            if (_freeIndices.Count > 0)
            {
                index = _freeIndices.Dequeue();
                _palette[index] = colour;
            }
            else
            {
                index = FindClosestColour(colour);
            }
        }

        return index;
    }

    private int FindClosestColour(TRColour4 colour)
    {
        return FindClosestColour(
            colour.ToColor(), _palette.Select(c => c.ToColor()));
    }

    public static int FindClosestColour(Color colour, IEnumerable<Color> palette)
    {
        return palette.FindClosest(colour);
    }
}
