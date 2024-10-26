using PersistentEmpiresLib.ErrorLogging;
using PersistentEmpiresLib.SceneScripts;
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
    public sealed class Sendmarket : GameNetworkMessage
    {
        public string Market;
        public int Index;
        public string MarketType;
        public int MaxIndex;
        public Sendmarket() { }
        public Sendmarket(string market, string markettype)
        {
            this.Market = market;
            this.MarketType = markettype;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Market set";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Market = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.MarketType = GameNetworkMessage.ReadStringFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Market);
            GameNetworkMessage.WriteStringToPacket(this.MarketType);
        }
    }
}
