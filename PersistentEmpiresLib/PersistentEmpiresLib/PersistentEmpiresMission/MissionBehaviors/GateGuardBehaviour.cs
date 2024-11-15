using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.AgentBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.MountAndBlade.Agent;
using Data;

namespace PersistentEmpiresMission.MissionBehaviors
{
    public class GateGuardBehaviour : MissionLogic
    {
#if SERVER
        public Dictionary<Agent,PE_GuardPost> Guards = new Dictionary<Agent, PE_GuardPost>();
        public FactionsBehavior factionbehavior;
        public Agent AgentCurrentlySpeaking;
        public int Delay = 10;
        public List<GuardTypeClass> GuardTypes = new List<GuardTypeClass>();
        public Dictionary<Agent,List<Agent>> GuardsAcknowleding = new Dictionary<Agent, List<Agent>>();
        public long lastcheck = 0;
        public SkillObject medicineSkill;

        public override void OnBehaviorInitialize()
        {
            factionbehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            base.OnBehaviorInitialize();
            Debug.Print("[AvalonHCRP] Gate Guard Behaviour Initialized");
            List<int> LordSoundEvents = new List<int>();
            List<int> FriendSoundEvents = new List<int>();
            List<int> NeutralSoundEvents = new List<int>();
            LordSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_LR1"));
            LordSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_LR2"));
            FriendSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_FR1"));
            FriendSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_FR2"));
            NeutralSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_NU1"));
            NeutralSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_BB_NU2"));
            GuardTypes.Add(new GuardTypeClass(1, "guard", "Gate_Guard",
                new List<int>(LordSoundEvents),
                new List<int>(FriendSoundEvents),
                new List<int>(NeutralSoundEvents)));

            // Second GuardTypeClass
            LordSoundEvents = new List<int>(); // Create new instances
            FriendSoundEvents = new List<int>();
            NeutralSoundEvents = new List<int>();
            
            LordSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_Lord1"));
            LordSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_Lord2"));
            FriendSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_FN_FR1"));
            FriendSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_FN_FR2"));
            NeutralSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_FN_NU1"));
            NeutralSoundEvents.Add(SoundEvent.GetEventIdFromString("Guard_FN_NU2"));

            GuardTypes.Add(new GuardTypeClass(2, "guard", "Finnin_Guard",
                new List<int>(LordSoundEvents),
                new List<int>(FriendSoundEvents),
                new List<int>(NeutralSoundEvents)));


        }

        //public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        //{
        //    base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);
        //    if (Guards.ContainsKey(affectedAgent))
        //    {
        //        affectedAgent.FadeOut(true, true);
        //        PE_GuardPost station = Guards[affectedAgent];
        //        station.Guard = null;
        //        Guards.Remove(affectedAgent);
        //        station.RespawnTime = station.MaxRespawnTime;
        //        station.GuardType = null;
        //    }
        //}
    

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            IEnumerable<GameEntity> GuardPosts = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_GuardPost>();
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastcheck > 1)
            {
                Debug.Print("TimeCheck");
                lastcheck = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                foreach (GameEntity Post in GuardPosts)
                {

                    PE_GuardPost Station = Post.GetFirstScriptOfType<PE_GuardPost>();
                    if (Station.Guard == null && Station.RespawnTime == 0)
                    {
                        SpawnBot(Station);
                    }
                    if (Station.Guard == null && Station.RespawnTime > 0)
                    {
                        Station.RespawnTime -= 1;
                    }
                }
            }

            foreach (var guard in Guards)
            {
                if (guard.Key.State == AgentState.Killed)
                {
                    guard.Key.FadeOut(false, true);
                    MoneyPouchBehavior moneyPouchBehavior = Mission.Current.GetMissionBehavior<MoneyPouchBehavior>();
                    MatrixFrame frame = guard.Key.Frame;
                    frame = frame.Advance(1);
                    moneyPouchBehavior.DropMoney(frame, 50);
                    guard.Value.Guard = null;
                    guard.Value.RespawnTime = guard.Value.MaxRespawnTime;
                    guard.Value.GuardType = null;
                    Guards.Remove(guard.Key);
                }
                if (guard.Key.GetTargetAgent() == null)
                {
                    
                    guard.Key.SetTargetPosition(guard.Value.GameEntity.GetGlobalFrame().origin.AsVec2);
                    guard.Key.SetMovementDirection(guard.Value.GameEntity.GetGlobalFrame().rotation.f.AsVec2);
                    handleAgentAcnowledgment(guard.Key, guard.Value);
                }
                HandleAgentTargeting(guard.Key, guard.Value);
            }
        }

        public PE_CastleBanner GetCastleBanner(PE_GuardPost station)
        {
            // FactionsBehavior factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            CastlesBehavior castleBehaviors = Mission.Current.GetMissionBehavior<CastlesBehavior>();
            if (castleBehaviors.castles.ContainsKey(station.CastleID))
            {
                return castleBehaviors.castles[station.CastleID];
            }
            return null;
        }



            public void SpawnBot(PE_GuardPost station)
        {
            try
            {
                GuardTypeClass typeclass = GuardTypes.GetRandomElement();
                if (this.GetCastleBanner(station) == null)
                {
                    return;
                }
                Faction faction = this.GetCastleBanner(station).GetOwnerFaction();
                BasicCharacterObject newchar = new BasicCharacterObject();

                BasicCharacterObject characterObject = MBObjectManager.Instance.GetObject<BasicCharacterObject>(typeclass.GuardClass);
                Banner banner = faction.banner;
                AgentBuildData agentData = new AgentBuildData(characterObject)
                    .Team(faction.team)
                    .TroopOrigin(new BasicBattleAgentOrigin(characterObject))
                    .ClothingColor1(banner.GetPrimaryColor())
                    .ClothingColor2(banner.GetFirstIconColor())
                    .IsFemale(characterObject.IsFemale)
                    .VisualsIndex(0);

                // Now, set Equipment and BodyProperties separately
                agentData = agentData.Equipment(Equipment.GetRandomEquipmentElements(characterObject, !GameNetwork.IsMultiplayer, false, agentData.AgentEquipmentSeed));
                agentData = agentData.BodyProperties(BodyProperties.GetRandomBodyProperties(
                    agentData.AgentRace,

                    agentData.AgentIsFemale,
                    characterObject.GetBodyPropertiesMin(false),
                    characterObject.GetBodyPropertiesMax(),
                    (int)agentData.AgentOverridenSpawnEquipment.HairCoverType,
                    agentData.AgentEquipmentSeed,
                    characterObject.HairTags,
                    characterObject.BeardTags,
                    characterObject.TattooTags
                ));
                float zRotationDegrees = station.GameEntity.GetGlobalFrame().origin.RotationZ;
                float zRotationRadians = MathF.PI * zRotationDegrees / 180.0f;
                Vec3 forward = new Vec3(MathF.Cos(zRotationRadians), MathF.Sin(zRotationRadians), 0);
                Mat3 rotationMatrix = Mat3.CreateMat3WithForward(forward);
                MatrixFrame spawnFrame = MatrixFrame.Identity;
                spawnFrame.rotation = rotationMatrix;

                agentData.InitialPosition(station.GameEntity.GetGlobalFrame().origin);
                agentData.InitialDirection(spawnFrame.rotation.f.AsVec2);

                Agent agent = Mission.SpawnAgent(agentData, true);
                agent.AgentVisuals.SetVoiceDefinitionIndex(-1, 0.0f);
                agent.AIStateFlags |= AIStateFlag.Guard;
                agent.SetTargetPosition(station.GameEntity.GetGlobalFrame().origin.AsVec2);
                agent.BaseHealthLimit = station.Health;
                agent.HealthLimit = agent.BaseHealthLimit;
                agent.Health = agent.HealthLimit;
                station.GuardType = typeclass;
                station.BotID = typeclass.GuardID;
                Guards.Add(agent, station);
                station.Guard = agent;
                GuardsAcknowleding.Add(agent, new List<Agent>());
                agent.OnAgentHealthChanged += new OnAgentHealthChangedDelegate(AgentBot_OnAgentHealthChanged);
                Debug.Print("Bot Spawned");
            }
            catch (Exception e)
            {
                Debug.Print("Error in SpawnBot: " + e.Message);
            }
        }

        private void handleAgentAcnowledgment(Agent agent, PE_GuardPost station)
        {
            GuardTypeClass typeclass = null;
            foreach (GuardTypeClass guardType in GuardTypes)
            {
                if (station.BotID == guardType.GuardID)
                {
                    typeclass = guardType;
                    break;
                }
            }
            if (typeclass == null) typeclass = GuardTypes.GetRandomElement();
            foreach (Agent a in Mission.Current.AllAgents)
            {
                if (a.MissionPeer == null)
                {
                    continue;
                }
                if (agent.Position.Distance(a.Position) > 25 && GuardsAcknowleding[agent].Contains(a))
                {
                    GuardsAcknowleding[agent].Remove(a);
                }
                if (agent.Position.Distance(a.Position) < 5 && !GuardsAcknowleding[agent].Contains(a))
                {
                    Faction faction = this.GetCastleBanner(station).GetOwnerFaction();
                    if (faction.lordId == a.MissionPeer.GetPeer().Id.ToString())
                    {
                        int speakchance = MBRandom.RandomInt(0, 100);
                        if (speakchance > 50)
                        {
                            GuardsAcknowleding[agent].Add(a);
                            base.Mission.MakeSound(typeclass.LordSoundEvents.GetRandomElement(), agent.Position, false, true, -1, -1);
                        }
                        else
                        {
                            GuardsAcknowleding[agent].Add(a);
                        }
                    }
                    else if (faction.members.Contains(a.MissionPeer.GetNetworkPeer()))
                    {
                        int speakchance = MBRandom.RandomInt(0, 100);
                        if (speakchance > 50)
                        {
                            base.Mission.MakeSound(typeclass.FriendSoundEvents.GetRandomElement(), agent.Position, false, true, -1, -1);
                            GuardsAcknowleding[agent].Add(a);
                        }
                        else
                        {
                            GuardsAcknowleding[agent].Add(a);
                        }
                    }
                    else
                    {
                        int speakchance = MBRandom.RandomInt(0, 100);
                        if (speakchance > 50)
                        {
                            MBMusicManager.Create();
                            base.Mission.MakeSound(typeclass.NeutralSoundEvents.GetRandomElement(), agent.Position, false, true, -1, -1);
                            GuardsAcknowleding[agent].Add(a);
                        }
                        GuardsAcknowleding[agent].Add(a);
                    }
                }
            }
        }
        private void AgentBot_OnAgentHealthChanged(Agent agent, float oldHealth, float newHealth)
        {
            return;
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            Debug.Print("Agent Hit");
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (!Guards.ContainsKey(affectedAgent)) return;
            if (Guards.ContainsKey(affectorAgent)) return;
            affectedAgent.SetTargetAgent(affectorAgent);
            affectedAgent.AIStateFlags |= AIStateFlag.Alarmed;
        }

        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        {
            base.OnMissileHit(attacker, victim, isCanceled, collisionData);
            if (!Guards.ContainsKey(victim)) return;
            if (Guards.ContainsKey(attacker)) return;
            victim.SetTargetAgent(attacker);
            victim.AIStateFlags |= AIStateFlag.Alarmed;
        }
    

        private void HandleAgentTargeting(Agent agent, PE_GuardPost station)
        {
            try
            {
                // If agent already has a target and is within range, no need to update targeting
                Agent currentTarget = agent.GetTargetAgent();
                if (currentTarget != null)
                {
                    float distanceToTarget = agent.Position.Distance(currentTarget.Position);
                    if (distanceToTarget <= station.AggroRadius && distanceToTarget > 1)
                    {
                        //Vec2 directionToPlayer = (currentTarget.Position.AsVec2 - agent.Position.AsVec2).Normalized();
                        //agent.SetMovementDirection(directionToPlayer);
                        agent.SetLookAgent(currentTarget);
                        agent.SetTargetPosition(currentTarget.Position.AsVec2);
                        return; // Keep the current target if within range
                    }
                }
            }
            catch (Exception e)
            {
                Debug.Print("Error in HandleAgentTargeting Agent Already Has Target: " + e.Message);
            }
            try
            {
                if (agent.Position.Distance(station.GameEntity.GlobalPosition) > station.MaxChaseDistance)
                {
                    agent.SetTargetPosition(station.GameEntity.GetGlobalFrame().origin.AsVec2);
                    agent.SetMovementDirection(station.GameEntity.GetGlobalFrame().rotation.f.AsVec2);
                    return;
                }
                Faction faction = this.GetCastleBanner(station).GetOwnerFaction();
                int FactionIndex = factionbehavior.GetIndexByValue(faction);
                // Check for nearby valid targets within the specified range

                    Agent nearestTarget = Mission.Current.AllAgents
                .Where(a => a != null && a.IsHuman && a.IsActive() && a != agent)
                .Where(a =>
                    a.MissionPeer != null &&
                    a.MissionPeer.GetComponent<PersistentEmpireRepresentative>() != null &&
                    (
                        a.MissionPeer.GetComponent<PersistentEmpireRepresentative>().GetFaction().warDeclaredTo.Contains(FactionIndex) ||
                        faction.warDeclaredTo.Contains(a.MissionPeer.GetComponent<PersistentEmpireRepresentative>().GetFactionIndex()) || a.MissionPeer.GetComponent<PersistentEmpireRepresentative>().GetFactionIndex() == 1
                    )
                )
                .OrderBy(a => a.Position.Distance(agent.Position))
                .FirstOrDefault(a => a.Position.Distance(agent.Position) < station.AggroRadius);



                // Set new target if found within range
                if (nearestTarget != null)
                    {
                        agent.SetTargetAgent(nearestTarget);
                        agent.AIStateFlags |= AIStateFlag.Alarmed;
                    }
                    else
                    {
                        // Clear the target if no valid targets are within range
                        agent.SetTargetAgent(null);
                        agent.AIStateFlags &= ~AIStateFlag.Alarmed;
                    }
            }
            catch (Exception e)
            {
                Debug.Print("Error in HandleAgentTargeting No Nearby Valid Targets: " + e.Message);
            }
        }
#endif
    }

}
