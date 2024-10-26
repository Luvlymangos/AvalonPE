using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresGameModels;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresLib.SceneScripts
{
    public class WorkshopButton : PE_UsableFromDistance
    {
        public int WorkshipIndex = 0;
        public string Tag = "Carpentry";
        public int Cost = 1000;

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject($"Build {Tag} Worskop");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Use \nCost: {Cost}");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            descriptionMessage.SetTextVariable("Cost", Cost);
            base.DescriptionMessage = descriptionMessage;
        }

        public override bool IsDisabledForAgent(Agent agent)
        {
            return this.IsDeactivated || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
        }
        public override void OnUse(Agent userAgent)
        {
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);

            if (GameNetwork.IsServer)
            {
            }

            userAgent.StopUsingGameObjectMT(true);
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            return false;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Use Door";
        }
    }
}
