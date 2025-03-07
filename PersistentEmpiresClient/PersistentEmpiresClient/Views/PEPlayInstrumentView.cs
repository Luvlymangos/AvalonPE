﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade.View.MissionViews;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using TaleWorlds.InputSystem;

namespace PersistentEmpires.Views.Views
{
    public class PEPlayInstrumentView : MissionView
    {
        public bool RequestedStartPlaying = false;
        private InstrumentsBehavior _instrumentsBehavior;

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._instrumentsBehavior = base.Mission.GetMissionBehavior<InstrumentsBehavior>();

        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            GameKey defendClick = HotKeyManager.GetCategory("CombatHotKeyCategory").GetGameKey("Defend");
            if (base.MissionScreen.SceneLayer.Input.IsGameKeyPressed(defendClick.Id))
            {
                this.RequestedStartPlaying = this._instrumentsBehavior.RequestStartPlaying();
            }
            else if (base.MissionScreen.SceneLayer.Input.IsGameKeyReleased(defendClick.Id) && this.RequestedStartPlaying)
            {
                this._instrumentsBehavior.RequestStopEat();
                this.RequestedStartPlaying = false;
            }
        }
    }
}
