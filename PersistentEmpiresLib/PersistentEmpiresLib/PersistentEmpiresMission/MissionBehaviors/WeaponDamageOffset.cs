using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class WeaponDamageOffset : MissionLogic
    {
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Debug.Print("[Avalon HCRP] Weapon Damage Offset Initalized", 0, Debug.DebugColor.Purple);
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (blow.InflictedDamage == 0) return;
            if (!affectorAgent.IsHuman) return;
            if (affectorAgent == null) return;
            if (affectorWeapon.Item == null) return;

            if (affectorWeapon.Item.StringId.StartsWith("Uncommon_"))
            {
                AddNewDamage(blow.InflictedDamage, .1, affectedAgent, affectorAgent);
            }
            if (affectorWeapon.Item.StringId.StartsWith("Rare_"))
            {
                AddNewDamage(blow.InflictedDamage, .2, affectedAgent, affectorAgent);
            }
            if (affectorWeapon.Item.StringId.StartsWith("Epic_"))
            {
                AddNewDamage(blow.InflictedDamage, .3, affectedAgent, affectorAgent);
            }
            if (affectorWeapon.Item.StringId.StartsWith("Legendary_"))
            {
                AddNewDamage(blow.InflictedDamage, .4, affectedAgent, affectorAgent);
            }
            if (affectorWeapon.Item.StringId.StartsWith("Mythic_"))
            {
                AddNewDamage(blow.InflictedDamage, .5, affectedAgent, affectorAgent);
            }
            if (affectorWeapon.Item.Type == ItemObject.ItemTypeEnum.Crossbow)
            {
                AddNewDamage(blow.InflictedDamage, .2, affectedAgent, affectorAgent);
            }
        }
        public void AddNewDamage(int BaseDamage, double multiplier, Agent affectedAgent, Agent affectorAgent)
        {
            try
            {
                NetworkCommunicator peer = affectedAgent.MissionPeer.GetNetworkPeer();
                NetworkCommunicator peer2 = affectorAgent.MissionPeer.GetNetworkPeer();
                Blow blow2 = new Blow(affectedAgent.Index);
                blow2.DamageType = TaleWorlds.Core.DamageTypes.Pierce;
                blow2.BoneIndex = affectedAgent.Monster.HeadLookDirectionBoneIndex;
                blow2.GlobalPosition = affectedAgent.Position;
                blow2.GlobalPosition.z = blow2.GlobalPosition.z + affectedAgent.GetEyeGlobalHeight();
                blow2.BaseMagnitude = 40;
                blow2.WeaponRecord.FillAsMeleeBlow(null, null, -1, -1);
                blow2.InflictedDamage = (int)(BaseDamage * multiplier);
                blow2.SwingDirection = affectedAgent.LookDirection;
                MatrixFrame frame = affectedAgent.Frame;
                blow2.SwingDirection = frame.rotation.TransformToParent(new Vec3(-1f, 0f, 0f, -1f));
                blow2.SwingDirection.Normalize();
                blow2.Direction = blow2.SwingDirection;
                blow2.DamageCalculated = true;
                sbyte mainHandItemBoneIndex = affectedAgent.Monster.MainHandItemBoneIndex;
                AttackCollisionData attackCollisionDataForDebugPurpose = AttackCollisionData.GetAttackCollisionDataForDebugPurpose(false, false, false, true, false, false, false, false, false, false, false, false, CombatCollisionResult.StrikeAgent, -1, 0, 2, blow2.BoneIndex, BoneBodyPartType.Head, mainHandItemBoneIndex, Agent.UsageDirection.AttackLeft, -1, CombatHitResultFlags.NormalHit, 0.5f, 1f, 0f, 0f, 0f, 0f, 0f, 0f, Vec3.Up, blow2.Direction, blow2.GlobalPosition, Vec3.Zero, Vec3.Zero, affectedAgent.Velocity, Vec3.Up);
                affectedAgent.RegisterBlow(blow2, attackCollisionDataForDebugPurpose);
                InformationComponent.Instance.SendMessage($"You have been hit by a weapon with a damage increase of " + blow2.InflictedDamage + "!", Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), peer);
                InformationComponent.Instance.SendMessage($"You have hit with a weapon with a damage increase of " + blow2.InflictedDamage + "!", Color.ConvertStringToColor("#008000FF").ToUnsignedInteger(), peer2);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}
