﻿using ImGuiNET;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.TR;

internal class TRANimationControl : IDrawable
{
    public void Draw()
    {
        if (ImGui.TreeNodeEx("Animations Data", ImGuiTreeNodeFlags.OpenOnArrow))
        {
            ImGui.Text("Animations count: " + IOManager.CurrentLevelAsTR1?.Animations.Count);
            ImGui.Text("State change count: " + IOManager.CurrentLevelAsTR1?.StateChanges.Count);
            ImGui.Text("Animation dispatch count: " + IOManager.CurrentLevelAsTR1?.Animations.Count);
            ImGui.Text("Animation command count: " + IOManager.CurrentLevelAsTR1?.AnimCommands.Count);
            ImGui.Text("Mesh tree count: " + IOManager.CurrentLevelAsTR1?.MeshTrees.Count);
            ImGui.Text("Total frames count: " + IOManager.CurrentLevelAsTR1?.Frames.Count);

            ImGui.TreePop();
        }
    }
}
