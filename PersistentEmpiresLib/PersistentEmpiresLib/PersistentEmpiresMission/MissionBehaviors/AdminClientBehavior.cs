using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class AdminClientBehavior : MissionNetwork
    {
        public delegate void AdminPanelClick();
        public event AdminPanelClick OnAdminPanelClick;
        public Dictionary<NetworkCommunicator, string> PlayerStats = new Dictionary<NetworkCommunicator, string>();

        public void HandleAdminPanelClick()
        {
            if (this.OnAdminPanelClick != null)
            {
                this.OnAdminPanelClick();
            }
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if(GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<AuthorizeAsAdmin>(this.HandleAuthorizeAsAdminFromServer);
                networkMessageHandlerRegisterer.Register<SendPlayerStatsToClient>(this.HandleSendPlayerStatsToClient);
            }
        }

        private void HandleSendPlayerStatsToClient(SendPlayerStatsToClient message)
        {
            if (message.joined)
            {
                if (!PlayerStats.ContainsKey(message.peer))
                {
                    PlayerStats.Add(message.peer, message.stats);
                }
                else
                {
                    PlayerStats[message.peer] = message.stats;
                }
            }
            else
            {
                PlayerStats.Remove(message.peer);
            }
        }

        private void HandleAuthorizeAsAdminFromServer(AuthorizeAsAdmin message)
        {
            GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>().IsAdmin = true;
        }
    }
}
