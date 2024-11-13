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
    public sealed class RequestSkillLocks : GameNetworkMessage
    {
        public string Skillname { get; set; }
        public RequestSkillLocks() { }

        public RequestSkillLocks(string skillname)
        {
            Skillname = skillname;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.AgentsDetailed;
        }

        protected override string OnGetLogFormat()
        {
            return "Request SkillLock";
        }

        protected override bool OnRead()
        {
            bool result = false;
            Skillname = GameNetworkMessage.ReadStringFromPacket(ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(Skillname);
        }
    }
}
