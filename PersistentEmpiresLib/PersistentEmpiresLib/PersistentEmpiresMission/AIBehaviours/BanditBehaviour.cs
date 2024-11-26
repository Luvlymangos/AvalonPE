using PersistentEmpiresLib;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.MountAndBlade.Agent;
using static TaleWorlds.Library.Debug;
using System.Xml;
using TaleWorlds.ModuleManager;
using System.IO;
using Data;
using PersistentEmpiresMission.AIBehaviours.Data;

namespace PersistentEmpiresMission.MissionBehaviors
{
    public class BanditBehaviour : MissionLogic
    {
#if SERVER
        public FactionsBehavior FactionsBehavior;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Debug.Print("[AvalonHCRP] Bandit Behaviour Initialized", 0, DebugColor.Purple);
            FactionsBehavior = Mission.Current.GetMissionBehavior<FactionsBehavior>();
            string GuardPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Bandits");
            Debug.Print("[AvalonHCRP] Trying Loading " + GuardPath, 0, DebugColor.Purple);
            if (File.Exists(GuardPath) == false) return;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(GuardPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/Bandits/Bandit"))
            {
                int GuardID = int.Parse(node["GuardID"].InnerText);
                string GuardType = node["GuardType"].InnerText;
                string GuardClass = node["GuardClass"].InnerText;
                List<int> LordSoundEventsRaw = ParseGuardsSounds(node["LordSoundEvents"].InnerText);
                List<int> FriendlySoundEventsRaw = ParseGuardsSounds(node["FriendlySoundEvents"].InnerText);
                List<int> EnemySoundEventsRaw = ParseGuardsSounds(node["EnemySoundEvents"].InnerText);
                List<int> NeutralSoundEventsRaw = ParseGuardsSounds(node["NeutralSoundEvents"].InnerText);
            }
        }




        public List<int> ParseGuardsSounds(string soundstring)
        {
            List<int> sounds = new List<int>();
            string[] soundarray = soundstring.Split('|');
            foreach (string sound in soundarray)
            {
                Debug.Print("Sound: " + sound);
                sounds.Add(SoundEvent.GetEventIdFromString(sound));
            }
            return sounds;
        }



        public override void OnMissionTick(float dt)
        {

        }




        public void SpawnBot(BanditAgentType typeclass, PE_BanditSpawnZone station)
        {
            try
            {

                Faction faction = FactionsBehavior.Factions[1];
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
                station.BanditAgentParams = typeclass;
                station.BotID = typeclass.GuardID;
                station.Bandit = agent;
                agent.OnAgentHealthChanged += new OnAgentHealthChangedDelegate(AgentBot_OnAgentHealthChanged);
                Debug.Print("Bot Spawned");
            }
            catch (Exception e)
            {
                Debug.Print("Error in SpawnBot: " + e.Message);
            }
        }

        //        private void handleAgentAcnowledgment(Agent agent, PE_GuardPost station)
        //        {

        //        }



        private void AgentBot_OnAgentHealthChanged(Agent agent, float oldHealth, float newHealth)
        {
            return;
        }

        //        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        //        {

        //        }

        //        public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled, AttackCollisionData collisionData)
        //        {
        //            base.OnMissileHit(attacker, victim, isCanceled, collisionData);
        //            return;
        //        }


        //        private void HandleAgentTargeting(Agent agent, PE_GuardPost station)
        //        {

        //        }
#endif
    }

}
