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
    public sealed class LocalMessageServer : GameNetworkMessage
    {
        public String Message;
        public NetworkCommunicator Sender;
        public string Prefix;

        public LocalMessageServer() { }
        public LocalMessageServer(String message, NetworkCommunicator sender, string prefix)
        {
            this.Sender = sender;
            this.Message = message;
            Prefix = prefix;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Local message received";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Sender = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.Message = GameNetworkMessage.ReadStringFromPacket(ref result);
            Prefix = GameNetworkMessage.ReadStringFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.Sender);
            GameNetworkMessage.WriteStringToPacket(this.Message);
            GameNetworkMessage.WriteStringToPacket(Prefix);
        }
    }
}
