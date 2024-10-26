using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_Workshop : PE_DestructableComponent, IMissionObjectHash
    {
        public int HouseIndex = 0;
        public int WorkshopIndex = 0;
        public string CarpentryTag;
        public string BlacksmithTag;
        public string CookingTag;
        public string FletchingTag;
        public string TanneryTag;
        public string WeavingTag;

        public int CurrentTierIndex = 0;

        private GameEntity _currentTierState;
        private GameEntity _CarpentryState;
        private GameEntity _BlacksmithState;
        private GameEntity _CookingState;
        private GameEntity _FletchingState;
        private GameEntity _TanneryState;
        private GameEntity _WeavingState;

        private GameEntity _CarpentryButton;
        private GameEntity _BlacksmithButton;
        private GameEntity _CookingButton;
        private GameEntity _FletchingButton;
        private GameEntity _TanneryButton;
        private GameEntity _WeavingButton;



        protected bool ValidateValues()
        {
            if (this.CarpentryTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CarpentryTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " CarpentryTag not found in childrens. " + this.CarpentryTag + " not found in childrens");
                return false;
            }
            if (this.BlacksmithTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BlacksmithTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " BlacksmithTag not found in childrens. " + this.BlacksmithTag + " not found in childrens");
                return false;
            }
            if (this.CookingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CookingTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " CookingTag not found in childrens. " + this.CookingTag + " not found in childrens");
                return false;
            }
            if (this.FletchingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.FletchingTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " FletchingTag not found in childrens. " + this.FletchingTag + " not found in childrens");
                return false;
            }
            if (this.TanneryTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.TanneryTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " TanneryTag not found in childrens. " + this.TanneryTag + " not found in childrens");
                return false;
            }
            if (this.WeavingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.WeavingTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " WeavingTag not found in childrens. " + this.WeavingTag + " not found in childrens");
                return false;
            }
            if (this.CarpentryTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CarpentryTag) && g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " CarpentryTag Button not found in childrens. " + this.CarpentryTag + " not found in childrens");
                return false;
            }
            if (this.BlacksmithTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BlacksmithTag) && g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " BlacksmithTag Button not found in childrens. " + this.BlacksmithTag + " not found in childrens");
                return false;
            }
            if (this.CookingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CookingTag) && g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " CookingTag Button not found in childrens. " + this.CookingTag + " not found in childrens");
                return false;
            }
            if (this.FletchingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.FletchingTag) && !g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " FletchingTag Button not found in childrens. " + this.FletchingTag + " not found in childrens");
                return false;
            }
            if (this.TanneryTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.TanneryTag) && g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " TanneryTag Button not found in childrens. " + this.TanneryTag + " not found in childrens");
                return false;
            }
            if (this.WeavingTag != "" && base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.WeavingTag) && g.HasScriptOfType<WorkshopButton>()) == null)
            {
                MBEditor.AddEntityWarning(base.GameEntity, base.GameEntity.GetPrefabName() + " WeavingTag Button not found in childrens. " + this.WeavingTag + " not found in childrens");
                return false;
            }
            return true;
        }
        protected override void OnSceneSave(string saveFolder)
        {
            this.ValidateValues();
        }
        protected override bool OnCheckForProblems()
        {
            return this.ValidateValues();
        }

        protected override void OnInit()
        {
            this._CarpentryState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CarpentryTag) && !g.HasScriptOfType<WorkshopButton>());
            this._BlacksmithState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BlacksmithTag) && !g.HasScriptOfType<WorkshopButton>());
            this._CookingState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CookingTag) && !g.HasScriptOfType<WorkshopButton>());
            this._FletchingState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.FletchingTag) && !g.HasScriptOfType<WorkshopButton>());
            this._TanneryState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.TanneryTag) && !g.HasScriptOfType<WorkshopButton>());
            this._WeavingState = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.WeavingTag) && !g.HasScriptOfType<WorkshopButton>());

            this._CarpentryButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CarpentryTag) && g.HasScriptOfType<WorkshopButton>());
            this._BlacksmithButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.BlacksmithTag) && g.HasScriptOfType<WorkshopButton>());
            this._CookingButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.CookingTag) && g.HasScriptOfType<WorkshopButton>());
            this._FletchingButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.FletchingTag) && g.HasScriptOfType<WorkshopButton>());
            this._TanneryButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.TanneryTag) && g.HasScriptOfType<WorkshopButton>());
            this._WeavingButton = base.GameEntity.GetChildren().FirstOrDefault((g) => g.Tags.Contains(this.WeavingTag) && g.HasScriptOfType<WorkshopButton>());


            _CarpentryState.SetVisibilityExcludeParents(false);
            _BlacksmithState.SetVisibilityExcludeParents(false);
            _CookingState.SetVisibilityExcludeParents(false);
            _FletchingState.SetVisibilityExcludeParents(false);
            _TanneryState.SetVisibilityExcludeParents(false);
            _WeavingState.SetVisibilityExcludeParents(false);
        }

        public GameEntity GetEntityFromTier(int tier)
        {
            if (tier == 0) return null;
            if (tier == 1) return this._CarpentryState;
            if (tier == 2) return this._BlacksmithState;
            if (tier == 3) return this._CookingState;
            if (tier == 4) return this._FletchingState;
            if (tier == 5) return this._TanneryState;
            if (tier == 6) return this._WeavingState;
            return null;
        }

        public void SetWorkshopServerSide(string WorkshopTag)
        {

        }

        public void SetTier(int tier)
        {
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackBehavior)
        {
            hitPoint = 100;
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            return true;
        }

        public MissionObject GetMissionObject()
        {
            return this;
        }
    }
}
