using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AgentCapture : MissionLogic
    {
#if SERVER
        public Dictionary<Agent, Agent> CapturedDict = new Dictionary<Agent, Agent>();
        public int RequiredSkillForGrabbing = 5;

        public string ItemId = "AvalonHCRP_Grabber";
        public override void OnBehaviorInitialize()
        {
            if (GameNetwork.IsServer)
            {
                Debug.Print("[Avalon HCRP] Capture System Initalized", 0, Debug.DebugColor.Purple);
            }
        }
        //This is the method that is called when the agent is hit by the weapon
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {

            if (!affectedAgent.IsHuman) return;
            if (affectedAgent.IsAIControlled) return;
            if (blow.InflictedDamage == 0) return;
            if (!affectorAgent.IsHuman) return;
            if (affectorAgent.MountAgent != null) return;
            if (affectedAgent.MountAgent != null) return;
            if (affectorAgent == null) return;
            if (affectorWeapon.Item == null) return;
            if (affectorWeapon.Item.StringId != this.ItemId) return;
            if (blow.AttackType != AgentAttackType.Standard) return;
            if (affectedAgent.MissionPeer == null) return;
            NetworkCommunicator peer = affectedAgent.MissionPeer.GetNetworkPeer();
            if (peer == null)
            {
                Debug.Print("Agent's mission peer is null");
                return;
            }
            if (affectedAgent.Health > affectedAgent.HealthLimit * 0.8) return;
            if (peer == null) return;
            foreach (var Value in CapturedDict)
            {
                if (Value.Key == affectedAgent)
                {
                    return;
                }
                if (Value.Value == affectedAgent & Value.Key != affectorAgent)
                {
                    return;
                }

                if (Value.Value == affectedAgent & Value.Key == affectorAgent)
                {
                    ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
                    affectedAgent.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    CapturedDict.Remove(Value.Key);
                    return;
                }
            }
            try
            {
                CapturedDict.Add(affectorAgent, affectedAgent);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message, 0, Debug.DebugColor.Red);
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
            if (GameNetwork.IsClient)
            {
                return;
            }

            if (
                affectorAgent == null ||
                agentState != AgentState.Killed
             ) return;
            foreach (var prisoner in CapturedDict)
            {
                if (prisoner.Value == affectedAgent)
                {
                    CapturedDict.Remove(prisoner.Key);
                }
                if (prisoner.Key == affectedAgent)
                {
                    CapturedDict.Remove(prisoner.Key);
                }
            }
        }
        public override void OnAgentMount(Agent agent)
        {
            base.OnAgentMount(agent);
            if (GameNetwork.IsClient)
            {
                return;
            }
            foreach (var prisoner in CapturedDict)
            {
                if (prisoner.Value == agent)
                {
                    CapturedDict.Remove(prisoner.Key);
                    ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
                    prisoner.Value.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
                if (prisoner.Key == agent)
                {
                    CapturedDict.Remove(prisoner.Key);
                    ActionIndexCache actionIndexCache = ActionIndexCache.act_none;
                    prisoner.Value.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                }
            }
        }
        public override void OnMissionTick(float dt)
        {
            if (GameNetwork.IsServer)
            {
                SkillObject AdminSkill = MBObjectManager.Instance.GetObject<SkillObject>("Athletics");
                foreach (var agent in CapturedDict)
                {
                    Vec3 TeleportPos = GetLocation(agent.Key);
                    agent.Value.TeleportToPosition(TeleportPos);
                    string Animation = "act_main_story_become_king_crowd_06";
                    ActionIndexCache actionIndexCache = ActionIndexCache.Create(Animation);
                    agent.Value.SetActionChannel(0, actionIndexCache, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0f, false, -0.2f, 0, true);
                    if (agent.Value.Character.GetSkillValue(AdminSkill) > 300 | agent.Key.Character.GetSkillValue(AdminSkill) > 300)
                    {
                        CapturedDict.Remove(agent.Key);
                    }

                }
                base.OnMissionTick(dt);
            }
        }

        public Vec3 GetLocation(Agent affectedAgent)
        {
            Vec3 AgentPos = affectedAgent.Position + affectedAgent.LookFrame.rotation.f * 2;
            return AgentPos;
        }
#endif
    }
}
