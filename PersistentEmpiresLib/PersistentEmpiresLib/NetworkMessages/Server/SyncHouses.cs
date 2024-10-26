using PersistentEmpiresLib.Factions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Server
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class SyncHouse : GameNetworkMessage
    {
        public string lordId;
        public long rentEnd;
        public string marshalls;
        public int HouseIndex;
        public bool IsRented;

        public SyncHouse() { }
        public SyncHouse(House house)
        {
            this.lordId = house.lordId;
            this.rentEnd = house.rentEnd;
            this.marshalls = house.SerializeMarshalls();
            this.HouseIndex = house.HouseIndex;
            this.IsRented = house.isrented;

        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Mission;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync Houses";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.lordId = ReadStringFromPacket(ref result);
            this.rentEnd = ReadLongFromPacket(new CompressionInfo.LongInteger(0, long.MaxValue, true), ref result);
            this.marshalls = ReadStringFromPacket(ref result);
            this.HouseIndex = ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            this.IsRented = ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            WriteStringToPacket(this.lordId);
            WriteLongToPacket(this.rentEnd, new CompressionInfo.LongInteger(0, long.MaxValue, true));
            WriteStringToPacket(this.marshalls);
            WriteIntToPacket(this.HouseIndex, new CompressionInfo.Integer(0, 100, true));
            WriteBoolToPacket(this.IsRented);
        }
    }
}
