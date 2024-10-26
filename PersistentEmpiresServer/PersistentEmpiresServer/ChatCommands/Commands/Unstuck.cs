using PersistentEmpiresLib;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresServer.ChatCommands.Commands
{
    internal class Unstuck : Command
    {
        public bool CanUse(NetworkCommunicator networkPeer)
        {
            return true;
        }

        public string Command()
        {
            return "!unstuck";
        }

        public string Description()
        {
            return "/unstuck makes you not stuck";
        }

        public bool Execute(NetworkCommunicator player, string[] args)
        {
            /*if (player.ControlledAgent == null) return false;
            player.ControlledAgent.TeleportToPosition(GetLocation());
            if (GameNetwork.IsServer) {
                LoggerHelper.LogAnAction(player, LogAction.PlayerUnstuck, null, null);
            }*/
            InformationComponent.Instance.SendMessage("This command is disabled because of abuse",TaleWorlds.Library.Color.White.ToUnsignedInteger(), player);
            return true;
        }
        public Vec3 GetLocation()
        {
            List<GameEntity> gameEntities = new List<GameEntity>();
            Mission.Current.Scene.GetAllEntitiesWithScriptComponent<PE_SpawnFrame>(ref gameEntities);
            foreach (GameEntity gameEntity in gameEntities)
            {
                PE_SpawnFrame spawnFrame = gameEntity.GetFirstScriptOfType<PE_SpawnFrame>();
                if (spawnFrame != null)
                {
                    if (spawnFrame.FactionIndex == 0 && spawnFrame.SpawnFromCastle == false)
                    {
                        return spawnFrame.GameEntity.GetGlobalFrame().origin;
                    }
                }
            }
            return new Vec3(100, 100, 100);
        }
    }
}