﻿using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{

    public class PE_BlocShip : PE_MoveableMachine
    {
        // Credits to fucking Bloc
        public bool isPlayerUsing = false;
        public string Animation = "";
        public int StrayDurationSeconds = 7200;

        public string RidingSkillId = "";
        public int RidingSkillRequired = 0;

        public string RepairingSkillId = "";
        public int RepairingSkillRequired = 0;

        public string RepairItemRecipies = "pe_hardwood*2,pe_wooden_stick*1";
        public int RepairDamage = 20;
        public string RepairItem = "pe_buildhammer";
        public string ParticleEffectOnDestroy = "";
        public string SoundEffectOnDestroy = "";
        public string ParticleEffectOnRepair = "";
        public string SoundEffectOnRepair = "";
        public bool DestroyedByStoneOnly = false;

        private long WillBeDeletedAt = 0;
        private SkillObject RidingSkill;
        private SkillObject RepairSkill;

        private List<RepairReceipt> receipt = new List<RepairReceipt>();
        private bool _landed;
        private bool destroyed = false;
        public override ScriptComponentBehavior.TickRequirement GetTickRequirement() => !this.GameEntity.IsVisibleIncludeParents() ? base.GetTickRequirement() : ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;

        private void ParseRepairReceipts()
        {
            string[] repairReceipt = this.RepairItemRecipies.Split(',');
            foreach (string receipt in repairReceipt)
            {
                string[] inflictedReceipt = receipt.Split('*');
                string receiptId = inflictedReceipt[0];
                int count = int.Parse(inflictedReceipt[1]);
                this.receipt.Add(new RepairReceipt(receiptId, count));
            }
        }


        private void CheckIfLanded(MatrixFrame oldFrame)
        {
            // if (this._landed) return;

            if (base.GameEntity == null) return;
            if (oldFrame == null) return;
            float heightUnder = Mission.Current.Scene.GetTerrainHeight(this.GameEntity.GlobalPosition.AsVec2, true);
            if (base.GameEntity.GlobalPosition.Z - heightUnder <= 1f)
            {
                if (base.IsMovingBackward) this.StopMovingBackward();
                if (base.IsMovingDown) this.StopMovingDown();
                if (base.IsMovingForward) this.StopMovingForward();
                if (base.IsMovingUp) this.StopMovingUp();
                if (base.IsTurningLeft) this.StopTurningLeft();
                if (base.IsTurningRight) this.StopTurningRight();
                base.GameEntity.SetFrame(ref oldFrame);
                // this._landed = true;
                Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/siege/merlon/wood_destroy"), this.GameEntity.GlobalPosition, false, true, -1, -1);

                this.SetHitPoint(this.HitPoint - 10, new Vec3(0, 0, 0));
                // this.Disable();
            }
        }
        protected override void OnTick(float dt)
        {
            if (base.GameEntity == null) return;
            MatrixFrame oldFrame = base.GameEntity.GetFrame();
            base.OnTick(dt);
            if (GameNetwork.IsServer)
            {
                if (this.PilotAgent != null)
                {
                    if (this.RidingSkill != null)
                    {
                        int skillValue = this.PilotAgent.Character.GetSkillValue(this.RidingSkill);
                        if (skillValue < this.RidingSkillRequired)
                        {
                            this.PilotAgent.StopUsingGameObjectMT(false);
                            return;
                        }
                    }
                    this.ResetStrayDuration();
                }
            }

            if (GameNetwork.IsClient)
            {
                if (Agent.Main != null && this.PilotAgent == Agent.Main)
                {
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.W))
                    {
                        this.RequestMovingForward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.W))
                    {
                        this.RequestStopMovingForward();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.S))
                    {
                        this.RequestMovingBackward();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.S))
                    {
                        this.RequestStopMovingBackward();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.A))
                    {
                        this.RequestTurningLeft();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.A))
                    {
                        this.RequestStopTurningLeft();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.D))
                    {
                        this.RequestTurningRight();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.D))
                    {
                        this.RequestStopTurningRight();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.Space))
                    {
                        this.RequestMovingUp();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.Space))
                    {
                        this.RequestStopMovingUp();
                    }
                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.LeftShift))
                    {
                        this.RequestMovingDown();
                    }
                    else if (Mission.Current.InputManager.IsKeyReleased(InputKey.LeftShift))
                    {
                        this.RequestStopMovingDown();
                    }

                    if (Mission.Current.InputManager.IsKeyPressed(InputKey.F))
                    {
                        GameNetwork.MyPeer.ControlledAgent.HandleStopUsingAction();
                        this.isPlayerUsing = false;
                        ActionIndexCache ac = ActionIndexCache.act_none;
                        this.PilotAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);
                    }
                }
            }

            if (this.PilotAgent == null)
            {
                if (base.IsMovingBackward) this.StopMovingBackward();
                if (base.IsMovingDown) this.StopMovingDown();
                if (base.IsMovingForward) this.StopMovingForward();
                if (base.IsMovingUp) this.StopMovingUp();
                if (base.IsTurningLeft) this.StopTurningLeft();
                if (base.IsTurningRight) this.StopTurningRight();
            }
            if (GameNetwork.IsServer)
            {
                this.CheckIfLanded(oldFrame);
            }
            if(destroyed)
            {
                base.GameEntity.Remove(0);
                destroyed = false;
            }
        }


        protected override void OnTickParallel(float dt)
        {
            base.OnTickParallel(dt);
            if (!base.GameEntity.IsVisibleIncludeParents())
            {
                return;
            }
        }
        protected override void OnInit()
        {
            base.OnInit();
            foreach (StandingPoint standingPoint in this.StandingPoints)
            {
                standingPoint.AutoSheathWeapons = true;
            }
            if (this.RidingSkillId != "")
            {
                this.RidingSkill = MBObjectManager.Instance.GetObject<SkillObject>(this.RidingSkillId);
            }
            if (this.RepairingSkillId != "")
            {
                this.RepairSkill = MBObjectManager.Instance.GetObject<SkillObject>(this.RepairingSkillId);
            }
            this.ParseRepairReceipts();
            this.ResetStrayDuration();
            this.HitPoint = this.MaxHitPoint;
        }
        public bool IsAgentFullyUsing(Agent usingAgent)
        {
            return this.PilotAgent == usingAgent;
        }
        public override TextObject GetActionTextForStandingPoint(UsableMissionObject usableGameObject)
        {
            TextObject forStandingPoint = new TextObject(this.IsAgentFullyUsing(GameNetwork.MyPeer.ControlledAgent) ? "{=QGdaakYW}{KEY} Stop Using" : "{=bl2aRW8f}{KEY} Command Ship");
            forStandingPoint.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            return forStandingPoint;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return new TextObject("{=}Bloc's Ship").ToString();
        }

        public override bool IsStray()
        {
            if (this.PilotAgent != null) return false;
            return this.WillBeDeletedAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        public override void ResetStrayDuration()
        {
            this.WillBeDeletedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + this.StrayDurationSeconds;
        }

        public override void SetHitPoint(float hitPoint, Vec3 impactDirection)
        {
            this.HitPoint = hitPoint;
            MatrixFrame globalFrame = base.GameEntity.GetGlobalFrame();
            if (this.HitPoint > this.MaxHitPoint) this.HitPoint = this.MaxHitPoint;
            if (this.HitPoint < 0) this.HitPoint = 0;

            if (this.HitPoint == 0)
            {
                if (this.PilotAgent != null)
                {
                    this.PilotAgent.StopUsingGameObjectMT(false);
                }
                if (this.ParticleEffectOnDestroy != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnDestroy), globalFrame);
                }
                if (this.SoundEffectOnDestroy != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnDestroy), globalFrame.origin, false, true, -1, -1);
                }
                destroyed = true;
                // base.GameEntity.Remove(0);
            }
            if (this.HitPoint == this.MaxHitPoint)
            {
                if (this.ParticleEffectOnRepair != "")
                {
                    Mission.Current.Scene.CreateBurstParticle(ParticleSystemManager.GetRuntimeIdByName(this.ParticleEffectOnRepair), globalFrame);
                }
                if (this.SoundEffectOnRepair != "")
                {
                    Mission.Current.MakeSound(SoundEvent.GetEventIdFromString(this.SoundEffectOnRepair), globalFrame.origin, false, true, -1, -1);
                }
            }
        }


        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = true;
            MissionWeapon missionWeapon = weapon;
            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (
                attackerAgent != null &&
                this.RepairSkill != null &&
                attackerAgent.Character.GetSkillValue(this.RepairSkill) >= this.RepairingSkillRequired &&
                missionWeapon.Item != null &&
                missionWeapon.Item.StringId == this.RepairItem &&
                attackerAgent.IsHuman &&
                attackerAgent.IsPlayerControlled &&
                this.HitPoint != this.MaxHitPoint
                )
            {
                reportDamage = false;
                if (attackerAgent.MissionPeer == null)
                {
                    Debug.Print("Agent's mission peer is null");
                    return false;
                }
                NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
                if (player == null)
                {
                    Debug.Print("Agent's mission peer is null");
                    return false;
                }
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (persistentEmpireRepresentative == null) return false;
                bool playerHasAllItems = this.receipt.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.RepairItem, r.NeededCount));
                if (!playerHasAllItems)
                {
                    //TODO: Inform player
                    InformationComponent.Instance.SendMessage("Required Items:", 0x02ab89d9, player);
                    foreach (RepairReceipt r in this.receipt)
                    {
                        InformationComponent.Instance.SendMessage(r.NeededCount + " * " + r.RepairItem.Name.ToString(), 0x02ab89d9, player);
                    }
                    return false;
                }
                foreach (RepairReceipt r in this.receipt)
                {
                    persistentEmpireRepresentative.GetInventory().RemoveCountedItem(r.RepairItem, r.NeededCount);
                }
                InformationComponent.Instance.SendMessage((this.HitPoint + this.RepairDamage).ToString() + "/" + this.MaxHitPoint + ", repaired", 0x02ab89d9, player);
                this.SetHitPoint(this.HitPoint + this.RepairDamage, impactDirection);
            }
            else
            {
                if (this.DestroyedByStoneOnly)
                {
                    if (currentUsageItem == null || (currentUsageItem.WeaponClass != WeaponClass.Stone && currentUsageItem.WeaponClass != WeaponClass.Boulder) || !currentUsageItem.WeaponFlags.HasAnyFlag(WeaponFlags.NotUsableWithOneHand))
                    {
                        damage = 0;
                    }
                }
                if (impactDirection == null) impactDirection = Vec3.Zero;
                this.SetHitPoint(this.HitPoint - damage, impactDirection);
                if (GameNetwork.IsServer)
                {
                    LoggerHelper.LogAnAction(attackerAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerHitToDestructable, null, new object[] { this.GetType().Name });
                }
            }
            return false;
        }
    }
}
