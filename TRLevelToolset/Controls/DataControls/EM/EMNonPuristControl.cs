﻿using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TREnvironmentEditor.Model;
using TRLevelReader.Model;
using TRLevelToolset.Interfaces;
using TRLevelToolset.IOLogic;

namespace TRLevelToolset.Controls.DataControls.EM
{
    internal class EMNonPuristControl : IDrawable
    {
        public EMEditorSet Data { get; set; }
        
        public void Draw()
        {
            ImGui.Text("Non Purist Control");
        }
    }
}