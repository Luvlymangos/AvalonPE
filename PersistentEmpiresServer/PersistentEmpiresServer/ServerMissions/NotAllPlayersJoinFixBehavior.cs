﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.MountAndBlade;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresServer.ServerMissions
{
    public class NotAllPlayersJoinFixBehavior : MissionNetwork
    {
        public CustomBattleServer DedicatedCustomServer { get; private set; }
        protected int _checkTimeInterval = 20;
        protected long _lastCheckedAt = 0;
        public override void OnBehaviorInitialize()
        {
            this.DedicatedCustomServer = DedicatedCustomServerSubModule.Instance.DedicatedCustomGameServer;
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (_lastCheckedAt + _checkTimeInterval > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                return;
            }
            _lastCheckedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            List<PlayerId> requestedPlayerIds = new List<PlayerId>();
            List<PlayerId> customBattlePlayers = new List<PlayerId>();

            requestedPlayerIds = (List<PlayerId>)typeof(CustomBattleServer).GetField("_requestedPlayers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this.DedicatedCustomServer);
            customBattlePlayers = (List<PlayerId>)typeof(CustomBattleServer).GetField("_customBattlePlayers", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(this.DedicatedCustomServer);

            List<PlayerId> inGamePlayerIds = GameNetwork.NetworkPeers.Select(p => p.VirtualPlayer.Id).ToList();

            requestedPlayerIds = requestedPlayerIds.FindAll(pid => inGamePlayerIds.Contains(pid)).ToList();
            customBattlePlayers = customBattlePlayers.FindAll(pid => inGamePlayerIds.Contains(pid)).ToList();

            typeof(CustomBattleServer).GetField("_requestedPlayers", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.DedicatedCustomServer, requestedPlayerIds);
            typeof(CustomBattleServer).GetField("_customBattlePlayers", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(this.DedicatedCustomServer, customBattlePlayers);
        }
    }
}
