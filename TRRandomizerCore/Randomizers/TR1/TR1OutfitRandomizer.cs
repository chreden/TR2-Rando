﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TREnvironmentEditor.Helpers;
using TRGE.Core;
using TRLevelReader.Helpers;
using TRLevelReader.Model;
using TRLevelReader.Model.Enums;
using TRModelTransporter.Transport;
using TRRandomizerCore.Helpers;
using TRRandomizerCore.Levels;
using TRRandomizerCore.Meshes;
using TRRandomizerCore.Processors;
using TRRandomizerCore.Textures;

namespace TRRandomizerCore.Randomizers
{
    public class TR1OutfitRandomizer : BaseTR1Randomizer
    {
        internal TR1TextureMonitorBroker TextureMonitor { get; set; }

        private List<TR1ScriptedLevel> _braidLevels;
        private List<TR1ScriptedLevel> _invisibleLevels;
        private List<TR1ScriptedLevel> _gymLevels;

        public override void Randomize(int seed)
        {
            _generator = new Random(seed);

            ChooseFilteredLevels();

            List<OutfitProcessor> processors = new List<OutfitProcessor>();
            for (int i = 0; i < _maxThreads; i++)
            {
                processors.Add(new OutfitProcessor(this));
            }

            List<TR1CombinedLevel> levels = new List<TR1CombinedLevel>(Levels.Count);
            foreach (TR1ScriptedLevel lvl in Levels)
            {
                levels.Add(LoadCombinedLevel(lvl));
                if (!TriggerProgress())
                {
                    return;
                }
            }

            int processorIndex = 0;
            foreach (TR1CombinedLevel level in levels)
            {
                processors[processorIndex].AddLevel(level);
                processorIndex = processorIndex == _maxThreads - 1 ? 0 : processorIndex + 1;
            }

            foreach (OutfitProcessor processor in processors)
            {
                processor.Start();
            }

            foreach (OutfitProcessor processor in processors)
            {
                processor.Join();
            }

            if (_processingException != null)
            {
                _processingException.Throw();
            }
        }

        private void ChooseFilteredLevels()
        {
            TR1ScriptedLevel assaultCourse = Levels.Find(l => l.Is(TRLevelNames.ASSAULT));
            ISet<TR1ScriptedLevel> exlusions = new HashSet<TR1ScriptedLevel> { assaultCourse };

            // "Haircut" settings = hair extensions in TR1
            _braidLevels = Levels.RandomSelection(_generator, (int)Settings.HaircutLevelCount, exclusions: exlusions);
            if (Settings.AssaultCourseHaircut)
            {
                _braidLevels.Add(assaultCourse);
            }

            _invisibleLevels = Levels.RandomSelection(_generator, (int)Settings.InvisibleLevelCount, exclusions: exlusions);
            if (Settings.AssaultCourseInvisible)
            {
                _invisibleLevels.Add(assaultCourse);
            }

            if (Settings.AllowGymOutfit)
            {
                // Gym outfits are only available in the following levels and we can only use it
                // if the T-Rex isn't present because that overwrites the MiscAnim's textures.
                List<string> allowedGymLevels = new List<string>
                {
                    TRLevelNames.CAVES, TRLevelNames.VILCABAMBA, TRLevelNames.FOLLY,
                    TRLevelNames.COLOSSEUM, TRLevelNames.CISTERN, TRLevelNames.TIHOCAN
                };
                _gymLevels = Levels.FindAll(l => allowedGymLevels.Contains(l.LevelFileBaseName.ToUpper())).RandomSelection(_generator, _generator.Next(1, allowedGymLevels.Count + 1));
            }

            if (ScriptEditor.Edition.IsCommunityPatch && _braidLevels.Count > 0)
            {
                (ScriptEditor.Script as TR1Script).EnableBraid = true;
            }
        }

        private bool IsBraidLevel(TR1ScriptedLevel lvl)
        {
            return ScriptEditor.Edition.IsCommunityPatch && _braidLevels.Contains(lvl);
        }

        private bool IsInvisibleLevel(TR1ScriptedLevel lvl)
        {
            return _invisibleLevels.Contains(lvl);
        }

        private bool IsGymLevel(TR1ScriptedLevel lvl)
        {
            return _gymLevels != null && _gymLevels.Contains(lvl);
        }

        internal class OutfitProcessor : AbstractProcessorThread<TR1OutfitRandomizer>
        {
            private static readonly List<TREntities> _invisibleLaraEntities = new List<TREntities>
            {
                TREntities.Lara, TREntities.LaraPonytail_H_U,
                TREntities.LaraPistolAnim_H, TREntities.LaraShotgunAnim_H, TREntities.LaraMagnumAnim_H,
                TREntities.LaraUziAnimation_H, TREntities.LaraMiscAnim_H, TREntities.CutsceneActor1
            };

            private static readonly List<TREntities> _ponytailEntities = new List<TREntities>
            {
                TREntities.LaraPonytail_H_U
            };

            private static readonly Dictionary<TREntities, Dictionary<EMTextureFaceType, int[]>> _headAmendments = new Dictionary<TREntities, Dictionary<EMTextureFaceType, int[]>>
            {
                [TREntities.Lara] = new Dictionary<EMTextureFaceType, int[]>
                {
                    [EMTextureFaceType.Rectangles] = new int[] { 1 },
                    [EMTextureFaceType.Triangles] = new int[] { 66, 67, 68, 69, 70, 71, 72, 73 }
                },
                [TREntities.LaraUziAnimation_H] = new Dictionary<EMTextureFaceType, int[]>
                {
                    [EMTextureFaceType.Rectangles] = new int[] { 6 },
                    [EMTextureFaceType.Triangles] = new int[] { 56, 57, 58, 59, 60, 61, 62, 63 }
                }
            };

            private readonly List<TR1CombinedLevel> _levels;

            internal override int LevelCount => _levels.Count;

            internal OutfitProcessor(TR1OutfitRandomizer outer)
                : base(outer)
            {
                _levels = new List<TR1CombinedLevel>();
            }

            internal void AddLevel(TR1CombinedLevel level)
            {
                _levels.Add(level);
            }

            protected override void ProcessImpl()
            {
                foreach (TR1CombinedLevel level in _levels)
                {
                    if (_outer.IsBraidLevel(level.Script))
                    {
                        // Only import the braid if Lara is visible. Note that it will automatically replace the model in Lost Valley.
                        // Cutscenes don't currently support the braid in T1M.
                        if (!_outer.IsInvisibleLevel(level.Script))
                        {
                            ImportBraid(level);
                        }
                    }
                    else if (level.Is(TRLevelNames.VALLEY))
                    {
                        // The global setting may be on so we need to hide the OG braid
                        HideEntities(level, _ponytailEntities);
                    }

                    if (_outer.IsInvisibleLevel(level.Script))
                    {
                        HideEntities(level, _invisibleLaraEntities);
                    }
                    else if (_outer.IsGymLevel(level.Script))
                    {
                        ConvertToGymOutfit(level);
                    }                   

                    _outer.SaveLevel(level);

                    if (!_outer.TriggerProgress())
                    {
                        break;
                    }
                }
            }

            private void ImportBraid(TR1CombinedLevel level)
            {
                TR1ModelImporter importer = new TR1ModelImporter
                {
                    Level = level.Data,
                    LevelName = level.Name,
                    ClearUnusedSprites = false,
                    EntitiesToImport = _ponytailEntities,
                    TexturePositionMonitor = _outer.TextureMonitor.CreateMonitor(level.Name, _ponytailEntities),
                    DataFolder = _outer.GetResourcePath(@"TR1\Models")
                };

                string remapPath = _outer.GetResourcePath(@"TR1\Textures\Deduplication\" + level.Name + "-TextureRemap.json");
                if (File.Exists(remapPath))
                {
                    importer.TextureRemapPath = remapPath;
                }

                importer.Import();

                // Find the texture references for the plain parts of imported hair
                TRMesh[] ponytailMeshes = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraPonytail_H_U);

                ushort plainHairQuad = ponytailMeshes[0].TexturedRectangles[0].Texture;
                ushort plainHairTri = ponytailMeshes[5].TexturedTriangles[0].Texture;

                foreach (TREntities laraType in _headAmendments.Keys)
                {
                    TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level.Data, laraType);
                    if (meshes == null || meshes.Length < 15)
                    {
                        continue;
                    }

                    TRMesh headMesh = meshes[14];

                    // Replace the hairband with plain hair - the imported ponytail has its own band so this is much tidier
                    foreach (int face in _headAmendments[laraType][EMTextureFaceType.Rectangles])
                    {
                        headMesh.TexturedRectangles[face].Texture = plainHairQuad;
                    }
                    foreach (int face in _headAmendments[laraType][EMTextureFaceType.Triangles])
                    {
                        headMesh.TexturedTriangles[face].Texture = plainHairTri;
                    }

                    // Move the base of Lara's bun up so the ponytail looks more natural
                    headMesh.Vertices[headMesh.Vertices.Length - 1].Y = 36;
                    headMesh.Vertices[headMesh.Vertices.Length - 2].Y = 38;
                    headMesh.Vertices[headMesh.Vertices.Length - 3].Y = 38;
                }
            }
            
            private void HideEntities(TR1CombinedLevel level, IEnumerable<TREntities> entities)
            {
                MeshEditor editor = new MeshEditor();
                foreach (TREntities ent in entities)
                {
                    TRMesh[] meshes = TRMeshUtilities.GetModelMeshes(level.Data, ent);
                    if (meshes != null)
                    {
                        foreach (TRMesh mesh in meshes)
                        {
                            editor.Mesh = mesh;
                            editor.ClearAllPolygons();
                            editor.WriteToLevel(level.Data);
                        }
                    }
                }

                // Repeat the process if there is a cutscene after this level.
                // Skip Mines because CutsceneActor1 is Natla, not Lara.
                if (level.HasCutScene && !level.CutSceneLevel.Is(TRLevelNames.MINES_CUT))
                {
                    HideEntities(level.CutSceneLevel, entities);
                }
            }

            private void ConvertToGymOutfit(TR1CombinedLevel level)
            {
                if (Array.Find(level.Data.Models, m => m.ID == (uint)TREntities.TRex) != null)
                {
                    return;
                }

                TRMesh[] lara = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.Lara);
                TRMesh[] laraPistol = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraPistolAnim_H);
                TRMesh[] laraShotgun = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraShotgunAnim_H);
                TRMesh[] laraMagnums = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraMagnumAnim_H);
                TRMesh[] laraUzis = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraUziAnimation_H);
                TRMesh[] laraMisc = TRMeshUtilities.GetModelMeshes(level.Data, TREntities.LaraMiscAnim_H);

                // Basic meshes to take from LaraMiscAnim. We don't replace Lara's gloves
                // or thighs (at this stage - handled below with gun swaps).
                int[] basicLaraIndices = new int[] { 0, 2, 3, 5, 6, 7, 8, 9, 11, 12 };
                foreach (int index in basicLaraIndices)
                {
                    TRMeshUtilities.DuplicateMesh(level.Data, lara[index], laraMisc[index]);
                }

                // Copy the guns and holsters from the original models and paste them
                // onto each of Lara's thighs. The ranges are the specific faces we want
                // to copy from each.
                int[] thighs = new int[] { 1, 4 };
                foreach (int thigh in thighs)
                {
                    // Empty holsters
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = lara[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(8, 6)
                    });

                    // Holstered pistols
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraPistol[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 6),
                        TextureFaceCopies = Enumerable.Range(5, 5)
                    });

                    // Holstered magnums
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraMagnums[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 6),
                        TextureFaceCopies = Enumerable.Range(3, 5)
                    });

                    // Holstered uzis
                    CopyMeshParts(level.Data, new MeshCopyData
                    {
                        BaseMesh = laraUzis[thigh],
                        NewMesh = laraMisc[thigh],
                        ColourFaceCopies = Enumerable.Range(4, 7),
                        TextureFaceCopies = Enumerable.Range(3, 19)
                    });
                }

                // Don't forget the shotgun on her back
                CopyMeshParts(level.Data, new MeshCopyData
                {
                    BaseMesh = laraShotgun[7],
                    NewMesh = laraMisc[7],
                    TextureFaceCopies = Enumerable.Range(8, 12)
                });
            }

            private void CopyMeshParts(TRLevel level, MeshCopyData data)
            {
                MeshEditor editor = new MeshEditor();
                TRMeshUtilities.InsertMesh(level, editor.Mesh = editor.CloneMesh(data.NewMesh));

                List<TRFace4> texturedQuads = editor.Mesh.TexturedRectangles.ToList();
                List<TRFace4> colouredQuads = editor.Mesh.ColouredRectangles.ToList();

                List<TRVertex> vertices = editor.Mesh.Vertices.ToList();

                if (data.TextureFaceCopies != null)
                {
                    foreach (int faceIndex in data.TextureFaceCopies)
                    {
                        TRFace4 face = data.BaseMesh.TexturedRectangles[faceIndex];
                        ushort[] vertexPointers = new ushort[4];
                        for (int j = 0; j < vertexPointers.Length; j++)
                        {
                            TRVertex origVertex = data.BaseMesh.Vertices[face.Vertices[j]];
                            int newVertIndex = vertices.FindIndex(v => v.X == origVertex.X && v.Y == origVertex.Y && v.Z == origVertex.Z);
                            if (newVertIndex == -1)
                            {
                                newVertIndex = vertices.Count;
                                vertices.Add(origVertex);
                            }
                            vertexPointers[j] = (ushort)newVertIndex;
                        }

                        texturedQuads.Add(new TRFace4
                        {
                            Texture = face.Texture,
                            Vertices = vertexPointers
                        });
                    }
                }

                if (data.ColourFaceCopies != null)
                {
                    foreach (int faceIndex in data.ColourFaceCopies)
                    {
                        TRFace4 face = data.BaseMesh.ColouredRectangles[faceIndex];
                        ushort[] vertexPointers = new ushort[4];
                        for (int j = 0; j < vertexPointers.Length; j++)
                        {
                            TRVertex origVertex = data.BaseMesh.Vertices[face.Vertices[j]];
                            int newVertIndex = vertices.FindIndex(v => v.X == origVertex.X && v.Y == origVertex.Y && v.Z == origVertex.Z);
                            if (newVertIndex == -1)
                            {
                                newVertIndex = vertices.Count;
                                vertices.Add(origVertex);
                            }
                            vertexPointers[j] = (ushort)newVertIndex;
                        }

                        colouredQuads.Add(new TRFace4
                        {
                            Texture = face.Texture,
                            Vertices = vertexPointers
                        });
                    }
                }

                editor.Mesh.TexturedRectangles = texturedQuads.ToArray();
                editor.Mesh.NumTexturedRectangles = (short)texturedQuads.Count;

                editor.Mesh.ColouredRectangles = colouredQuads.ToArray();
                editor.Mesh.NumColouredRectangles = (short)colouredQuads.Count;

                editor.Mesh.Vertices = vertices.ToArray();
                editor.Mesh.NumVertices = (short)vertices.Count;

                editor.Mesh.Normals = data.BaseMesh.Normals;
                editor.Mesh.NumNormals = data.BaseMesh.NumNormals;

                editor.Mesh.CollRadius = data.BaseMesh.CollRadius;
                editor.WriteToLevel(level);

                TRMeshUtilities.DuplicateMesh(level, data.BaseMesh, editor.Mesh);
            }
        }
    }
}