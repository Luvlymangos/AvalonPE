using NetworkMessages.FromServer;
using PersistentEmpiresLib;
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
    public class ForceStart : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            if(Main.IsAdmin(networkPeer))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string Command()
        {
            return "!FS";
        }

        public string Description()
        {
            return "Force Starts The Match";
        }

        public bool Execute(NetworkCommunicator networkPeer, string[] args)
        {
            GameController gameController = Mission.Current.GetMissionBehavior<GameController>();
            if (gameController != null)
            {
                gameController.BeginMatch();
            }

            return true;
        }
    }
}
