using PersistentEmpiresLib.ErrorLogging;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class UpdatePlants : GameNetworkMessage
    {
        public string PlantName { get; set; }
        public string SeedName { get; set; }
        public string CropName { get; set; }
        public string PrefabName { get; set; }
        public int GrowTime { get; set; }
        public int Yield { get; set; }
        public int SeedYield { get; set; }
        public int SkillRequired { get; set; }
        public int SkillYield { get; set; }
        public string HarvestItem { get; set; }
        
        
        public UpdatePlants()
        {
        }

        public UpdatePlants(Growables plant)
        {
            this.PlantName = plant.PlantName;
            this.SeedName = plant.SeedName;
            this.CropName = plant.CropName;
            this.PrefabName = plant.PrefabName;
            this.GrowTime = plant.GrowTime;
            this.Yield = plant.Yield;
            this.SeedYield = plant.SeedYield;
            this.SkillRequired = plant.SkillRequired;
            this.SkillYield = plant.SkillYield;
            this.HarvestItem = plant.HarvestItem;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Plants Updated";
        }

        protected override bool OnRead()
        {
            bool result = true;

            this.PlantName = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.SeedName = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.CropName = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.PrefabName = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.GrowTime = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 3600),ref result);
            this.Yield = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 3600),ref result);
            this.SeedYield = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 3600), ref result);
            this.SkillRequired = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 1000), ref result);
            this.SkillYield = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 3600), ref result);
            this.HarvestItem = GameNetworkMessage.ReadStringFromPacket(ref result);


            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.PlantName);
            GameNetworkMessage.WriteStringToPacket(this.SeedName);
            GameNetworkMessage.WriteStringToPacket(this.CropName);
            GameNetworkMessage.WriteStringToPacket(this.PrefabName);
            GameNetworkMessage.WriteIntToPacket(this.GrowTime, new CompressionInfo.Integer(0, 3600));
            GameNetworkMessage.WriteIntToPacket(this.Yield, new CompressionInfo.Integer(0, 3600));
            GameNetworkMessage.WriteIntToPacket(this.SeedYield, new CompressionInfo.Integer(0, 3600));
            GameNetworkMessage.WriteIntToPacket(this.SkillRequired, new CompressionInfo.Integer(0, 1000));
            GameNetworkMessage.WriteIntToPacket(this.SkillYield, new CompressionInfo.Integer(0, 3600));
            GameNetworkMessage.WriteStringToPacket(this.HarvestItem);
        }
    }
}
