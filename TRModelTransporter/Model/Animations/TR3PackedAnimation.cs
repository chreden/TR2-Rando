﻿using TRLevelControl.Model;

namespace TRModelTransporter.Model.Animations;

public class TR3PackedAnimation
{
    public TRAnimation Animation { get; set; }
    public Dictionary<int, TRAnimDispatch> AnimationDispatches { get; set; }
    public Dictionary<int, TR1PackedAnimationCommand> Commands { get; set; }

    public List<TRStateChange> StateChanges { get; set; }

    public TR3PackedAnimation()
    {
        AnimationDispatches = new Dictionary<int, TRAnimDispatch>();
        Commands = new Dictionary<int, TR1PackedAnimationCommand>();
        StateChanges = new List<TRStateChange>();
    }
}
