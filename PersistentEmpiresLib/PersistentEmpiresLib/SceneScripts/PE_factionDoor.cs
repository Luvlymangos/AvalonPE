using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_Faction_Door : UsableMissionObject
    {
        public float Duration = 1f;
        public Vec3 Axis = new Vec3(0, 0, 1);
        public float Angle = 90f;
        public float Delay = 500f;
        public int CastleId = -1;
        public bool Lockpickable = true;

        private bool isOpen = false;
        private long lastOpened = 0;

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
            base.OnUse(userAgent);

            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                if (userAgent.MissionPeer == null)
                {
                    Debug.Print("Agent's mission peer is null");
                    return;
                }
                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - this.lastOpened > this.Delay)
                {
                    SkillObject AdminSkill = MBObjectManager.Instance.GetObject<SkillObject>("Athletics");
                    if (userAgent.Character.GetSkillValue(AdminSkill) > 300)
                    {
                        this.ToggleDoor();

                    }
                    bool canPlayerUse = true;
                    NetworkCommunicator player = userAgent.MissionPeer.GetNetworkPeer();
                    if (player == null) return;
                    if (this.CastleId > -1)
                    {
                        canPlayerUse = false;
                        Faction f = this.GetCastleBanner().GetOwnerFaction();
                        if (f.members.Contains(player)) canPlayerUse = true;
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
