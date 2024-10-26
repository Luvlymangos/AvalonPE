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
    public sealed class AddMarshalToHouse : GameNetworkMessage
    {
        public NetworkCommunicator TargetPlayer;
        public int HouseIndex;

        public AddMarshalToHouse()
        {

        }
        public AddMarshalToHouse(NetworkCommunicator networkCommunicator, int HouseIndex)
        {
            this.TargetPlayer = networkCommunicator;
            this.HouseIndex = HouseIndex;
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Faction Assign Marshall";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.TargetPlayer = GameNetworkMessage.ReadNetworkPeerReferenceFromPacket(ref result);
            this.HouseIndex = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(0, 100, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteNetworkPeerReferenceToPacket(this.TargetPlayer);
            GameNetworkMessage.WriteIntToPacket(this.HouseIndex, new CompressionInfo.Integer(0, 100, true));
        }
    }
}
