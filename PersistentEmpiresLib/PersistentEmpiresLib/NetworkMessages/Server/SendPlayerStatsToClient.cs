using PersistentEmpiresLib.ErrorLogging;
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
    public sealed class SendPlayerStatsToClient : GameNetworkMessage
    {
        public string stats;
        public NetworkCommunicator peer;
        public bool joined;
        
        public SendPlayerStatsToClient() { }
        public SendPlayerStatsToClient(string Stats, NetworkCommunicator Peer, bool joined)
        {
            this.stats = Stats;
            this.peer = Peer;
            this.joined = joined;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.None;
        }

        protected override string OnGetLogFormat()
        {
            return "Crafting set";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.stats = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.peer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.joined = GameNetworkMessage.ReadBoolFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.stats);
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.peer);
            GameNetworkMessage.WriteBoolToPacket(this.joined);
        }
    }
}
