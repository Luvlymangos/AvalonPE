using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class AddPlant : GameNetworkMessage
    {
        public string PrefabName;
        public int X;
        public int Y;
        public int Z;
        public MissionObjectId MissionobjectID;
        public AddPlant()
        {

        }

        public AddPlant(string Prefab, Vec3 position,MissionObjectId ID )
        {
            this.PrefabName = Prefab;
            this.X = (int)position.x;
            this.Y = (int)position.y;
            this.Z = (int)position.z;
            this.MissionobjectID = ID;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "OpenBank";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.PrefabName = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.X = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-100,int.MaxValue, true), ref result);
            this.Y = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-100, int.MaxValue, true), ref result);
            this.Z = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(-100, int.MaxValue, true), ref result);
            this.MissionobjectID = GameNetworkMessage.ReadMissionObjectIdFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.PrefabName);
            GameNetworkMessage.WriteIntToPacket(this.X, new CompressionInfo.Integer(-100, int.MaxValue, true));
            GameNetworkMessage.WriteIntToPacket(this.Y, new CompressionInfo.Integer(-100, int.MaxValue, true));
            GameNetworkMessage.WriteIntToPacket(this.Z, new CompressionInfo.Integer(-100, int.MaxValue, true));
            GameNetworkMessage.WriteMissionObjectIdToPacket(this.MissionobjectID);

        }
    }
}
