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
    public sealed class SyncSkillLocks : GameNetworkMessage
    {
        public string Skill;
        public bool Locked;
        public SyncSkillLocks() { }
        public SyncSkillLocks(string skill, bool locked)
        {
            this.Skill = skill;
            this.Locked = locked;  
        }
        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.General;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync SkillLocks";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Skill = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Locked = GameNetworkMessage.ReadBoolFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Skill);
            GameNetworkMessage.WriteBoolToPacket(this.Locked);
        }
    }
}
