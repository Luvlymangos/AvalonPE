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
    public sealed class SendCrafting : GameNetworkMessage
    {
        public string Crafting;
        public int Index;
        public string CraftingType;
        public int MaxIndex;
        public SendCrafting() { }
        public SendCrafting(string crafting, string craftingtype)
        {
            this.Crafting = crafting;
            this.CraftingType = craftingtype;
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
            this.Crafting = GameNetworkMessage.ReadStringFromPacket(ref result);
            this.CraftingType = GameNetworkMessage.ReadStringFromPacket(ref result);

            return result;
        }

        protected override void OnWrite()
        {
            GameNetworkMessage.WriteStringToPacket(this.Crafting);
            GameNetworkMessage.WriteStringToPacket(this.CraftingType);
        }
    }
}
