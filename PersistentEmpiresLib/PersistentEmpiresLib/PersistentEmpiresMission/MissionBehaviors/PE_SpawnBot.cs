using Newtonsoft.Json;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.Agent;

namespace PersistentEmpiresMission.MissionBehaviors
{
    public class SpawnBotBehavior : MissionLogic
    {
#if SERVER
        private readonly Dictionary<int, long> willBeSpawnAts = new Dictionary<int, long>();
        private readonly Dictionary<int, long> willBeIdleAts = new Dictionary<int, long>();
        private const int spawnDuration = 40;
        private const int idleDuration = 3;
        private readonly Vec3 botInitialPosition = new Vec3(1374.61f, 1101.65f, 1.41434f, -1f);
        private readonly Random random = new Random();
        private const int botCheckingDuration = 10;
        private long willBotCheckingAt;
        private CastlesBehavior castlesBehavior;
        private readonly List<string> botIndexList = new List<string>
        {
            "event_theoden"
        };
        private readonly List<BotAgent> botAgents = new List<BotAgent>();

        private List<BotConfig> BotConfigList = new List<BotConfig>();

        public override void OnBehaviorInitialize()
        {
            Debug.Print("[Avalon HCRP] Bot System Initalized", 0, Debug.DebugColor.Purple);
            base.OnBehaviorInitialize();
            castlesBehavior = Mission.GetMissionBehavior<CastlesBehavior>();
        }

        protected void MissionSpawnBot(BotConfig botConfig)
        {
            int index = random.Next(botIndexList.Count);
            var factionBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            Faction faction = factionBehavior.Factions[0];
            BasicCharacterObject characterObject = MBObjectManager.Instance.GetObject<BasicCharacterObject>(botIndexList[index]);
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

            float zRotationDegrees = 25.0f;
            float zRotationRadians = MathF.PI * zRotationDegrees / 180.0f;
            Vec3 forward = new Vec3(MathF.Cos(zRotationRadians), MathF.Sin(zRotationRadians), 0);
            var spawnPosition = new Vec3(botConfig.x, botConfig.y, botConfig.z, -1f);
            Mat3 rotationMatrix = Mat3.CreateMat3WithForward(forward);
            MatrixFrame spawnFrame = MatrixFrame.Identity;
            spawnFrame.rotation = rotationMatrix;
            spawnFrame.origin = spawnPosition;

            agentData.InitialPosition(spawnFrame.origin);
            agentData.InitialDirection(spawnFrame.rotation.f.AsVec2);

            Agent agent = Mission.SpawnAgent(agentData, true);
            agent.AgentVisuals.SetVoiceDefinitionIndex(-1, 0.0f);
            agent.AIStateFlags |= AIStateFlag.Guard;
            agent.SetTargetPosition(new Vec2(botConfig.x, botConfig.y));
            agent.BaseHealthLimit = (float)ConfigManager.GetIntConfig("BotHealth", 800);
            agent.HealthLimit = agent.BaseHealthLimit;
            agent.Health = agent.HealthLimit;
            agent.OnAgentHealthChanged += AgentBot_OnAgentHealthChanged;
            botAgents.Add(new BotAgent { Agent = agent, Config = botConfig });
        }

        private string RandomString(int length)
        {
            return new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789", length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private void AgentBot_OnAgentHealthChanged(Agent agent, float oldHealth, float newHealth)
        {
            if (agent != null && newHealth < 20.0)
            {
                agent.AIStateFlags |= AIStateFlag.Guard;
            }
        }

        private float GetRadian(Vec3 v1, Vec3 v2)
        {
            return (float)Math.Acos(Vec3.DotProduct(v1, v2) / (v1.Length * v2.Length));
        }

        private void BotIdleMoving(int id, Agent agentBot)
        {
            if (willBeIdleAts[id] < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                var botAgent = botAgents.FirstOrDefault(ba => ba.Agent.Equals(agentBot));
                if (botAgent != null)
                {
                    Vec3 idlePosition = new Vec3(botAgent.Config.x, botAgent.Config.y, botAgent.Config.z, -1f);
                    Vec3 botPosition = agentBot.Position;

                    if (botPosition.Distance(idlePosition) > 10.0)
                    {
                        agentBot.ResetEnemyCaches();
                        agentBot.SetTargetPosition(idlePosition.AsVec2);
                    }
                    else
                    {
                        int xOffset = random.Next((int)-botAgent.Config.IdleMovement, (int)botAgent.Config.IdleMovement);
                        int yOffset = random.Next((int)-botAgent.Config.IdleMovement, (int)botAgent.Config.IdleMovement);
                        agentBot.SetTargetPosition(new Vec2(idlePosition.X + xOffset, idlePosition.Y + yOffset));
                    }
                    willBeIdleAts[id] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + idleDuration;
                }
            }
        }

        public override void OnAfterMissionCreated()
        {
            base.OnAfterMissionCreated();
            Debug.Print("Opening Bots.xml");
            
            string FoodPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Bots");
            Debug.Print("[PE] Trying Loading " + FoodPath, 0, DebugColor.Cyan);
            if (File.Exists(FoodPath) == false) Debug.Print("Bots Path Does not Exist");

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(FoodPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/Bots/Bot"))
            {
                BotConfig cfg = new BotConfig
                {
                    Id = int.Parse(node["Id"].InnerText),
                    CastleId = int.Parse(node["CastleId"].InnerText),
                    x = float.Parse(node["X"].InnerText),
                    y = float.Parse(node["Y"].InnerText),
                    z = float.Parse(node["Z"].InnerText),
                    Range = float.Parse(node["Range"].InnerText),
                    IdleMovement = float.Parse(node["IdleMovement"].InnerText)
                };
                this.BotConfigList.Add(cfg);
            }
        }

        private void ResetBotCheckingDuration()
        {
            willBotCheckingAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + botCheckingDuration;
        }

        private bool IsBotCheckingAble()
        {
            return willBotCheckingAt < DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        private void SpawnBots(BotConfig botConfig)
        {
            if (botAgents.All(b => b.Config.Id != botConfig.Id))
            {
                if (!willBeSpawnAts.ContainsKey(botConfig.Id)) willBeSpawnAts[botConfig.Id] = 0;

                if (willBeSpawnAts[botConfig.Id] < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    willBeSpawnAts[botConfig.Id] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + spawnDuration;
                    willBeIdleAts[botConfig.Id] = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + idleDuration;
                    MissionSpawnBot(botConfig);
                    Debug.Print("Spawn Bot", 0, Debug.DebugColor.Magenta, 17592186044416UL);
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
                if (IsBotCheckingAble())
                {
                    foreach (var botConfig in BotConfigList)
                    {
                        if (botConfig.CastleId > 0)
                        {
                            if (castlesBehavior.castles.TryGetValue(botConfig.CastleId, out var castle) && castle.GetOwnerFaction().lordId == "0")
                            {
                                SpawnBots(botConfig);
                            }
                        }
                        else
                        {
                            SpawnBots(botConfig);
                        }
                    }
                    ResetBotCheckingDuration();
                }

                foreach (var botAgent in botAgents)
                {
                    var agent = botAgent?.Agent;
                    if (agent != null && !agent.IsPaused && !agent.IsInBeingStruckAction && agent.GetTargetAgent() == null)
                    {
                        BotIdleMoving(botAgent.Config.Id, agent);

                    }
                    HandleAgentTargeting(agent, botAgent.Config);
                    
                }
            
        }

        private void HandleAgentTargeting(Agent agent, BotConfig botConfig)
        {
            try
            {
                // If agent already has a target and is within range, no need to update targeting
                Agent currentTarget = agent.GetTargetAgent();
                if (currentTarget != null)
                {
                    float distanceToTarget = agent.Position.Distance(currentTarget.Position);
                    if (distanceToTarget <= botConfig.Range)
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
                // Check for nearby valid targets within the specified range
                Agent nearestTarget = Mission.Current.AllAgents
                        .Where(a => a.IsHuman && a.IsActive() && a != agent && a.Team != agent.Team)
                        .OrderBy(a => a.Position.Distance(agent.Position))
                        .FirstOrDefault(a => a.Position.Distance(agent.Position) < botConfig.Range);

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

        // Rest of the code continues as needed
    }
}
