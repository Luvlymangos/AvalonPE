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
    public sealed class SyncCraftingStats : GameNetworkMessage
    {
        public string Skill;
        public int Amount;
        public SyncCraftingStats()
        {

        }

        public SyncCraftingStats( string skill, int amount)
        {
            this.Skill = skill;
            this.Amount = amount;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionObjects;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync Crafting Stats";
        }

        protected override bool OnRead()
        {
            bool result = true;
            this.Skill = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.Amount = GameNetworkMessage.ReadIntFromPacket(new CompressionInfo.Integer(int.MinValue, int.MaxValue, true), ref result);
            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Skill);
            GameNetworkMessage.WriteIntToPacket(this.Amount, new CompressionInfo.Integer(int.MinValue, int.MaxValue, true));
        }
    }
}
