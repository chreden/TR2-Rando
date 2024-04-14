﻿namespace TRRandomizerCore;

public enum TRRandomizerType
{
    All,

    // Top-level options
    LevelSequence,
    Unarmed,
    Ammoless,
    Sunset,
    NightMode,
    Secret,
    SecretReward,
    Item,
    Enemy,
    Texture,
    StartPosition,
    Audio,
    Outfit,
    Text,
    Environment,
    Health, // Distinct from ammoless in TR1X
    Weather,

    // Individual settings
    DisableDemos = 100,
    OutfitDagger,
    SFX,
    GlobeDisplay,
    RewardRooms,
    VFX,
    DragonSpawn,
    BirdMonsterBehaviour,
    SecretAudio,
    Mediless,
    KeyItems,
    GlitchedSecrets,
    HardSecrets,
    SecretCount,
    Ladders,
    KeyItemTextures,
    SecretTextures,
    Braid,
    WaterColour,
    ExtraPickups,
    AmbientTracks,
    AtlanteanEggBehaviour,
    GymOutfit,
    HiddenEnemies,
    ItemSprite,
    SecretModels,
    LarsonBehaviour,
    DynamicTextures,
    MeshSwaps,
    HardEnvironment,
    Traps,
    ChallengeRooms,
    DynamicEnemyTextures,
    ClonedEnemies,
    ReturnPaths,
    GeneralBugFixes,
    ShortcutFixes,
    KeyContinuity,
    GameMode,
    ItemDrops,
    LevelCount,
}
