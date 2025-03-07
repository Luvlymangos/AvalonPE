﻿using PersistentEmpiresLib.Factions;
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
    public class PE_Gate : PE_UsableFromDistance
    {
        public float Duration = 1f;
        public Vec3 Axis = new Vec3(0, 0, 1);
        public float Angle = 90f;
        public float Delay = 500f;
        public int CastleId = -1;
        public bool Lockpickable = true;
        public bool PlayerHouse = false;
        public int HouseIndex = -1;

        protected bool isOpen = false;
        protected long lastOpened = 0;

        private MatrixFrame openFrame;
        private MatrixFrame closedFrame;

        protected override void OnInit()
        {
            base.OnInit();
            base.ActionMessage = new TextObject("Door");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Use");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;

            this.closedFrame = base.GameEntity.GetFrame();
            MatrixFrame tempFrame = base.GameEntity.GetFrame();
            tempFrame.Rotate(MBMath.ToRadians(this.Angle), this.Axis);
            this.openFrame = tempFrame;
        }

        public PE_CastleBanner GetCastleBanner()
        {
            // FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            if (castleBehaviors.castles.ContainsKey(this.CastleId))
            {
                return castleBehaviors.castles[this.CastleId];
            }
            return null;
        }

        public override bool IsDisabledForAgent(Agent agent)
        {
            return this.IsDeactivated || (this.IsDisabledForPlayers && !agent.IsAIControlled) || !agent.IsOnLand();
        }
        public override void OnUse(Agent userAgent)
        {
            if (userAgent.MissionPeer == null)
            {
                Debug.Print("Agent's mission peer is null");
                return;
            }
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this.lastOpened > this.Delay)
                {

                    SkillObject AdminSkill = MBObjectManager.Instance.GetObject<SkillObject>("Athletics");
                    if (userAgent.Character.GetSkillValue(AdminSkill) > 300)
                    {
                        this.ToggleDoor();
                        return;
                    }
                    
                    if (this.PlayerHouse)
                    {
                        HouseBehviour house = Mission.Current.GetMissionBehavior<HouseBehviour>();
                        if (userAgent.MissionPeer.GetComponent<PersistentEmpireRepresentative>().GetHouse() == house.Houses[this.HouseIndex] || house.Houses[this.HouseIndex].marshalls.Contains(userAgent.MissionPeer.Peer.Id.ToString()))
                        {
                            NetworkCommunicator player2 = userAgent.MissionPeer.GetNetworkPeer();
                            if (player2 == null) return;
                            InformationComponent.Instance.SendMessage("Welcome Home", 0x0606c2d9, player2);
                            this.ToggleDoor();
                            return;
                        }
                        else
                        {
                            NetworkCommunicator player2 = userAgent.MissionPeer.GetNetworkPeer();
                            if (player2 == null) return;
                            InformationComponent.Instance.SendMessage("This door is locked", 0x0606c2d9, player2);
                            return;
                        }
                    }
                    bool canPlayerUse = true;
                    NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                    if (player == null) return;
                    if (this.CastleId > -1)
                    {
                        canPlayerUse = false;
                        Faction f = this.GetCastleBanner().GetOwnerFaction();
                        if (f.doorManagers.Contains(player.VirtualPlayer.Id.ToString()) || f.marshalls.Contains(player.VirtualPlayer.Id.ToString()) || f.lordId == player.VirtualPlayer.Id.ToString()) canPlayerUse = true;
                        PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                        if (destructComponent != null && destructComponent.IsBroken) canPlayerUse = true;
                    }
                    if (canPlayerUse)
                    {
                        this.ToggleDoor();
                    }
                    else
                    {
                        Faction f = this.GetCastleBanner().GetOwnerFaction();
                        InformationComponent.Instance.SendMessage("This door is locked by " + f.name, 0x0606c2d9, player);
                        Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_close"), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
                    }
                }
            }
        }

        public void ToggleDoor()
        {
            this.lastOpened = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (this.isOpen)
            {
                PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                if (destructComponent == null || destructComponent.IsBroken == false)
                {
                    this.CloseDoor();
                }
            }
            else this.OpenDoor();
        }
        public void OpenDoor()
        {
            base.SetFrameSynchedOverTime(ref this.openFrame, this.Duration);
            this.isOpen = true;
            Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_open"), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);
        }
        public void CloseDoor()
        {
            base.SetFrameSynchedOverTime(ref this.closedFrame, this.Duration);
            this.isOpen = false;
            Mission.Current.MakeSound(SoundEvent.GetEventIdFromString("event:/mission/movement/foley/door_close"), base.GameEntity.GetGlobalFrame().origin, false, true, -1, -1);

        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            OfflineProtectionBehaviour offlineProtectionBehaviour = Mission.Current.GetMissionBehavior<OfflineProtectionBehaviour>();
#if SERVER
            if (GameNetwork.IsServer)
            {
                if (attackerAgent.MissionPeer == null)
                {
                    Debug.Print("Agent's mission peer is null");
                    return false;
                }
                if (offlineProtectionBehaviour.IsOfflineProtectionActive && offlineProtectionBehaviour != null)
                {
                    NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
                    if (player == null) return false;
                    InformationComponent.Instance.SendMessage("Offline protection is active", 0x02ab89d9, attackerAgent.MissionPeer.GetNetworkPeer());
                    PE_RepairableDestructableComponent destructComponent = base.GameEntity.GetFirstScriptOfType<PE_RepairableDestructableComponent>();
                    if (destructComponent != null)
                    {
                        destructComponent.SetHitPoint(destructComponent.HitPoint + damage, impactDirection, attackerScriptComponentBehavior);
                        return false;
                    }
                }
                
            }
#endif
            if (this.Lockpickable == false) return false;
            if (this.CastleId == -1) return false;

            bool lockPickSuccess = LockpickingBehavior.Instance.Lockpick(attackerAgent, weapon);
            if (lockPickSuccess) this.ToggleDoor();

            return false;
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Use Door";
        }
    }
}
