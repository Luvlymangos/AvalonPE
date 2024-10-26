using NetworkMessages.FromServer;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
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
    public class PE_GrowingPoint : PE_DestructableComponent
    {
        private PlantingBehaviour _plantingBehaviour;

        protected override void OnInit()
        {
            if (GameNetwork.IsServer)
            {
                this._plantingBehaviour = Mission.Current.GetMissionBehavior<PlantingBehaviour>();
            }
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackBehavior)
        {
            throw new NotImplementedException();
        }

        public void TriggerOnHit(Agent attackerAgent, int inflictedDamage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior)
        {
            bool flag;
            this.OnHit(attackerAgent, inflictedDamage, impactPosition, impactDirection, weapon, attackerScriptComponentBehavior, out flag);
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            if (GameNetwork.IsServer)
            {
                _plantingBehaviour.SpawnPlant(impactPosition, "Master_Grow_Point");
            }
            reportDamage = false;
            return true;
        }
    }
}
