using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class Growables
    {
        public string PlantName;
        public string SeedName;
        public string CropName;
        public string PrefabName;
        public int GrowTime;
        public int Yield;
        public int SeedYield;
        public int SkillRequired;
        public int SkillYield;
        public string HarvestItem;
        public Growables(string plantname, string seedName, string cropName, string prefabName, int growTime, int yield, int Seedyield, int skillRequired, int skillYield, string HarvestItem)
        {
            this.PlantName = plantname;
            this.SeedName = seedName;
            this.CropName = cropName;
            this.PrefabName = prefabName;
            this.GrowTime = growTime;
            this.Yield = yield;
            this.SeedYield = Seedyield;
            this.SkillRequired = skillRequired;
            this.SkillYield = skillYield;
            this.HarvestItem = HarvestItem;

        }
    }
    public class PlantingBehaviour : MissionLogic
    {
        public List<Growables> Plants = new List<Growables>();
        public string ModuleFolder = "PersistentEmpires";
        public override void OnBehaviorInitialize()
        {
            Debug.Print("[Avalon HCRP] Planting System Initalized", 0, Debug.DebugColor.Purple);
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            ParsePlants();
        }

        public void SpawnPlant(Vec3 Location, string plant)
        {   
            if (GameNetwork.IsServer)
            {
                Vec3 terrainNormal = Mission.Scene.GetNormalAt(new Vec2(Location.X, Location.Y));
                float TerrainHeight = Mission.Scene.GetTerrainHeight(new Vec2(Location.X, Location.Y));
                // Ensure the normal is normalized
                terrainNormal.Normalize();

                // Define an arbitrary forward vector (initial guess, avoiding parallelism with the normal)
                Vec3 forward = new Vec3(1, 0, 0);

                // If forward vector is parallel to the normal, choose a different initial vector
                if (Math.Abs(Vec3.DotProduct(forward, terrainNormal)) > 0.99f)
                {
                    forward = new Vec3(0, 1, 0); // Use Y axis instead if parallel
                }

                // Calculate the right vector using cross product
                Vec3 right = Vec3.CrossProduct(forward, terrainNormal);
                right.Normalize();

                // Recalculate the forward vector to ensure orthogonality
                forward = Vec3.CrossProduct(terrainNormal, right);
                forward.Normalize();

                // Create the matrix frame
                MatrixFrame matrixFrame = new MatrixFrame();
                matrixFrame.origin = new Vec3(Location.X, Location.Y, TerrainHeight + 0.1f);

                // Set the rotation matrix correctly (right, forward, up)
                matrixFrame.rotation = new Mat3(right, forward, terrainNormal);

                // Instantiate the entity at the aligned position
                GameEntity brokenState2 = GameEntity.Instantiate(Mission.Scene, plant, matrixFrame);

                Debug.Print(brokenState2.GetFirstScriptOfType<PE_GrowingPlant>().Id.ToString());
                foreach (NetworkCommunicator networkCommunicator in GameNetwork.NetworkPeers)
                {
                    GameNetwork.BeginModuleEventAsServer(networkCommunicator);
                    GameNetwork.WriteMessage(new AddPlant(plant, Location, brokenState2.GetFirstScriptOfType<PE_GrowingPlant>().Id));
                    GameNetwork.EndModuleEventAsServer();
                }
            }
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<AddPlant>(HandlePlantCreate);
                networkMessageHandlerRegisterer.Register<UpdatePlants>(ParsePlantsForClient);
            }
        }

        public void HandlePlantCreate(AddPlant message)
        {
            if (GameNetwork.IsClient)
            {
                Vec3 terrainNormal = Mission.Scene.GetNormalAt(new Vec2(message.X, message.Y));
                float TerrainHeight = Mission.Scene.GetTerrainHeight(new Vec2(message.X, message.Y));
                // Ensure the normal is normalized
                terrainNormal.Normalize();

                // Define an arbitrary forward vector (initial guess, avoiding parallelism with the normal)
                Vec3 forward = new Vec3(1, 0, 0);

                // If forward vector is parallel to the normal, choose a different initial vector
                if (Math.Abs(Vec3.DotProduct(forward, terrainNormal)) > 0.99f)
                {
                    forward = new Vec3(0, 1, 0); // Use Y axis instead if parallel
                }

                // Calculate the right vector using cross product
                Vec3 right = Vec3.CrossProduct(forward, terrainNormal);
                right.Normalize();

                // Recalculate the forward vector to ensure orthogonality
                forward = Vec3.CrossProduct(terrainNormal, right);
                forward.Normalize();

                // Create the matrix frame
                MatrixFrame matrixFrame = new MatrixFrame();
                matrixFrame.origin = new Vec3(message.X, message.Y, TerrainHeight + 0.1f);

                // Set the rotation matrix correctly (right, forward, up)
                matrixFrame.rotation = new Mat3(right, forward, terrainNormal);

                // Instantiate the entity at the aligned position
                GameEntity brokenState = GameEntity.Instantiate(Mission.Scene, message.PrefabName, matrixFrame);
                brokenState.GetFirstScriptOfType<PE_GrowingPlant>().Id = message.MissionobjectID;

            }
        }

        public void ParsePlantsForClient(UpdatePlants message)
        {
            if (GameNetwork.IsClient)
            {
                this.Plants.Add(new Growables(
                message.PlantName,
                message.SeedName,
                message.CropName,
                message.PrefabName,
                message.GrowTime,
                message.Yield,
                message.SeedYield,
                message.SkillRequired,
                message.SkillYield,
                message.HarvestItem));
            }
        }

        public Growables GetPlant(string seedName)
        {
            return Plants.FirstOrDefault(x => x.SeedName == seedName);
        }

        public bool IsValidSeed(string seedName)
        {
            return Plants.Any(x => x.SeedName == seedName);
        }

        private void ParsePlants()
        {
            if (GameNetwork.IsServer)
            {

                // Load Craftables
                string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "Plants/Plant");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                foreach (XmlNode node in xmlDocument.SelectNodes("/Growables/Plant"))
                {

                    ItemObject itemDebug = MBObjectManager.Instance.GetObject<ItemObject>(node["SeedName"].InnerText);
                    if (itemDebug == null)

                    {
                        Debug.Print($"ERROR IN Plants SEED {node["SeedName"].InnerText} SERIALIZATION ITEM ID NOT FOUND !!!", 0, Debug.DebugColor.Red);
                    }

                    ItemObject itemDebug2 = MBObjectManager.Instance.GetObject<ItemObject>(node["CropName"].InnerText);
                    if (itemDebug2 == null)

                    {
                        Debug.Print($"ERROR IN Plants CROP {node["CropName"].InnerText} SERIALIZATION ITEM ID NOT FOUND !!!", 0, Debug.DebugColor.Red);
                    }

                    this.Plants.Add(new Growables(
                        node["PlantName"].InnerText,
                        node["SeedName"].InnerText,
                        node["CropName"].InnerText,
                        node["PrefabName"].InnerText,
                        int.Parse(node["GrowTime"].InnerText),
                        int.Parse(node["Yield"].InnerText),
                        int.Parse(node["SeedYield"].InnerText),
                        int.Parse(node["SkillRequired"].InnerText),
                        int.Parse(node["SkillYield"].InnerText),
                        node["HarvestItem"].InnerText));

                }
            }
        }

    }

}
