using NetworkMessages.FromServer;
using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresServer.ServerMissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    public class Start : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!Start";
        }

        public string Description()
        {
            return "Votes to Start The Match";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            GameController gameController = Mission.Current.GetMissionBehavior<GameController>();
            if (gameController != null)
            {
                gameController.votes.Add(networkPeer);
                if (GameNetwork.NetworkPeerCount < 4)
                {
                    InformationComponent.Instance.SendMessage($"{gameController.votes.Count()}/4 To start the match!", TaleWorlds.Library.Color.White.ToUnsignedInteger(), networkPeer);
                    return true;
                }
                InformationComponent.Instance.SendMessage($"{gameController.votes.Count()}/{GameNetwork.NetworkPeerCount / 2} To start the match!", TaleWorlds.Library.Color.White.ToUnsignedInteger(), networkPeer);
            }

            return true;
        }
    }
}
