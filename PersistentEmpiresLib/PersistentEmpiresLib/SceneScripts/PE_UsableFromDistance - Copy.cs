using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.SceneScripts
{
    abstract public class PE_UsableFromDistanceDamage : UsableMissionObject
    {
        public float Distance = 3f;

        public bool IsUsable(Agent user)
        {
            float distance = base.GameEntity.GetGlobalFrame().origin.Distance(user.Position);
            return distance <= this.Distance;
        }
        public abstract void SetHitPoint(float hitPoint, Vec3 impactDirection, ScriptComponentBehavior attackBehavior);
        protected override abstract bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage);

    }
}
