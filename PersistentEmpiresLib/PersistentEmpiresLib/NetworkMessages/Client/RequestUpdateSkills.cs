using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace PersistentEmpiresLib.NetworkMessages.Client
{
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class RequestUpdateSkills : GameNetworkMessage
    {
        public NetworkCommunicator Id;
        public string Skill;
        public int Value;

        public RequestUpdateSkills() { }
        public RequestUpdateSkills(NetworkCommunicator id, string skill, int value)
        {
            this.Id = id;
            Skill = skill;
            Value = value;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Skill Update Requested";
        }

        protected override bool OnRead()
        {
            bool result = true;
            Id = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            Skill = GameNetworkMessage.ReadStringFromPacket(ref result);
            Value = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(int.MinValue, int.MaxValue, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(Id);
            GameNetworkMessage.WriteStringToPacket(Skill);
            GameNetworkMessage.WriteIntToPacket(Value, new CompressionInfo.Integer(int.MinValue, int.MaxValue, true));
        }
    }
}
