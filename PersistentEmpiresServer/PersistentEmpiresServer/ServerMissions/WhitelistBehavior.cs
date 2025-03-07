﻿using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib;

namespace PersistentEmpiresServer.ServerMissions
{
    public class WhitelistBehavior : MissionNetwork
    {
        public bool IsEnabled { get; private set; }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.IsEnabled = ConfigManager.GetBoolConfig("WhitelistEnabled", false);

            if(this.IsEnabled)
            {
                Debug.Print("** PERSISTENT EMPIRES ** Whitelist Enabled.",0,Debug.DebugColor.DarkYellow);
            }
        }
        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator player)
        {
            if (!IsEnabled) return;
            base.HandleLateNewClientAfterSynchronized(player);
            bool isWhitelisted = SaveSystemBehavior.HandleIsPlayerWhitelisted(player);
            if (!isWhitelisted)
            {
                InformationComponent.Instance.SendMessage("You are not whitelisted. Your player id is: " + player.VirtualPlayer.Id.ToString(), Colors.Red.ToUnsignedInteger(), player);
                DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer.KickPlayer(player.VirtualPlayer.Id, false);
            }
        }
       
    }
}
