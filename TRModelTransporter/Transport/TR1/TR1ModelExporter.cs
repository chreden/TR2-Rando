﻿using TRLevelControl;
using TRLevelControl.Helpers;
using TRLevelControl.Model;
using TRModelTransporter.Data;
using TRModelTransporter.Handlers;
using TRModelTransporter.Handlers.Textures;
using TRModelTransporter.Model.Definitions;

namespace TRModelTransporter.Transport;

public class TR1ModelExporter : AbstractTRModelExporter<TR1Type, TR1Level, TR1ModelDefinition>
{
    public TR1ModelExporter()
    {
        Data = new TR1DefaultDataProvider();
    }

    protected override AbstractTextureExportHandler<TR1Type, TR1Level, TR1ModelDefinition> CreateTextureHandler()
    {
        return new TR1TextureExportHandler();
    }

    protected override TR1ModelDefinition CreateModelDefinition(TR1Level level, TR1Type modelEntity)
    {
        TR1ModelDefinition definition = new()
        {
            Alias = modelEntity
        };

        if (Data.IsAlias(modelEntity))
        {
            modelEntity = Data.TranslateAlias(modelEntity);
        }

        ModelTransportHandler.Export(level, definition, modelEntity);
        ColourTransportHandler.Export(level, definition);
        _textureHandler.Export(level, definition, TextureClassifier, Data.GetSpriteDependencies(modelEntity), Data.GetIgnorableTextureIndices(modelEntity, LevelName));
        CinematicTransportHandler.Export(level, definition, Data.GetCinematicEntities());
        SoundTransportHandler.Export(level, definition, Data.GetHardcodedSounds(definition.Alias));

        return definition;
    }

    protected override void PreDefinitionCreation(TR1Level level, TR1Type modelEntity)
    {
        switch (modelEntity)
        {
            case TR1Type.Pierre:
                AmendPierreGunshot(level);
                AmendPierreDeath(level);
                break;
            case TR1Type.Larson:
                AmendLarsonDeath(level);
                break;
            case TR1Type.SkateboardKid:
                AmendSkaterBoyDeath(level);
                break;
            case TR1Type.Natla:
                AmendNatlaDeath(level);
                break;
            case TR1Type.MovingBlock:
                AddMovingBlockSFX(level);
                break;
        }
    }

    protected override void ModelExportReady(TR1ModelDefinition definition)
    {
        switch (definition.Alias)
        {
            case TR1Type.Kold:
                if (definition.Colours.ContainsKey(123))
                {
                    // Incorrect orange colouring on head and hands
                    definition.Colours[123].Red = 28;
                    definition.Colours[123].Green = 18;
                    definition.Colours[123].Blue = 4;
                }
                break;
            case TR1Type.SkateboardKid:
                if (definition.Colours.ContainsKey(182))
                {
                    // Incorrect yellow colouring on his arm
                    definition.Colours[182].Red = 51;
                    definition.Colours[182].Green = 33;
                    definition.Colours[182].Blue = 22;
                }
                break;
            case TR1Type.CowboyHeadless:
                AmendDXtre3DTextures(definition);
                break;
            default:
                break;
        }
    }

    public static void AmendPierreGunshot(TR1Level level)
    {
        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.Pierre);
        // Get his shooting animation
        TRAnimation anim = model.Animations[10];

        // On the 2nd frame, play SFX 44 (magnums)
        anim.Commands.Add(new()
        {
            Type = TRAnimCommandType.PlaySound,
            Params = new()
            {
                1,
                (short)TR1SFX.LaraMagnums,
            }
        });
    }

    public static void AmendPierreDeath(TR1Level level)
    {
        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.Pierre);
        // Get his death animation
        TRAnimation anim = model.Animations[12];

        // On the 61st frame, play SFX 159 (death)
        anim.Commands.Add(new()
        {
            Type = TRAnimCommandType.PlaySound,
            Params = new()
            {
                60,
                (short)TR1SFX.PierreDeath,
            }
        });
    }

    public static void AmendLarsonDeath(TR1Level level)
    {
        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.Larson);
        // Get his death animation
        TRAnimation anim = model.Animations[15];

        // On the 2nd frame, play SFX 158 (death)
        anim.Commands.Add(new()
        {
            Type = TRAnimCommandType.PlaySound,
            Params = new()
            {
                1,
                (short)TR1SFX.LarsonDeath,
            }
        });
    }

    public static void AmendSkaterBoyDeath(TR1Level level)
    {
        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.SkateboardKid);
        // Get his death animation
        TRAnimation anim = model.Animations[13];
        // Play the death sound on the 2nd frame (doesn't work on the 1st, which is OG).
        anim.Commands[2].Params[0]++;
    }

    public static void AmendNatlaDeath(TR1Level level)
    {
        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.Natla);
        // Get her death animation
        TRAnimation anim = model.Animations[13];

        // On the 5th frame, play SFX 160 (death)
        anim.Commands.Add(new()
        {
            Type = TRAnimCommandType.PlaySound,
            Params = new()
            {
                4,
                (short)TR1SFX.NatlaDeath,
            }
        });
    }

    public static void AddMovingBlockSFX(TR1Level level)
    {
        // ToQ moving blocks are silent but we want them to scrape along the floor when they move.
        // Import the trapdoor closing SFX from Vilcabamba and adjust the animations accordingly.

        if (!level.SoundEffects.ContainsKey(TR1SFX.TrapdoorClose))
        {
            TR1Level vilcabamba = new TR1LevelControl().Read(TR1LevelNames.VILCABAMBA);
            level.SoundEffects[TR1SFX.TrapdoorClose] = vilcabamba.SoundEffects[TR1SFX.TrapdoorClose];
        }

        TRModel model = level.Models.Find(m => m.ID == (uint)TR1Type.MovingBlock);
        for (int i = 2; i < 4; i++)
        {
            TRAnimation anim = model.Animations[i];

            // On the 1st frame, play SFX 162
            anim.Commands.Add(new()
            {
                Type = TRAnimCommandType.PlaySound,
                Params = new()
                {
                    0,
                    (short)TR1SFX.TrapdoorClose,
                }
            });
        }
    }
}
