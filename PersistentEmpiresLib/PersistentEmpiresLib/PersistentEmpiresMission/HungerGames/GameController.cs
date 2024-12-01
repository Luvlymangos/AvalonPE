using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresMission.HungerGames;
using SceneScripts.HungerGames;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresMission.MissionBehaviors
{
    public class GameController : MissionLogic
    {
        public int PlayersAlive = 0;
        public int PlayersDead = 0;
        public int PlayersTotal = 0;
        public int PlayersToStart = 24;
        public bool GameStarted = false;
        public bool StartCompleted = false;
        public double startrequested = 0;
        public int LastSpawned = 0;
        public List<NetworkCommunicator> votes = new List<NetworkCommunicator>();
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Debug.Print("GameController initialized");
        }

        public void NewRound()
        {
            SpawnLoot sl = Mission.Current.GetMissionBehavior<SpawnLoot>();
            if (sl != null)
            {
                sl.NewRound();
            }
            KillZone kz = Mission.Current.GetMissionBehavior<KillZone>();
            if (kz != null)
            {
                kz.NewRound();
            }
            startrequested = 0;
        }

        public void BeginMatch()
        {
            foreach (GameEntity ge in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_InventoryEntity>())
            {
                if (ge.GetPrefabName() == "pe_loot")
                {
                    ge.Remove(0);
                }
            }
                startrequested = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            InformationComponent.Instance.BroadcastAnnouncement("Game Starting in 10 seconds");
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (GameNetwork.IsServer)
            {
                try
                {
                    if(GameNetwork.NetworkPeers.Count() == 0 && GameStarted)
                    {
                        GameStarted = false;
                        PlayersAlive = 0;
                        PlayersTotal = 0;
                        PlayersDead = 0;
                        PlayersToStart = 24;
                        startrequested = 0;
                        StartCompleted = false;
                        votes.Clear();
                        LastSpawned = 0;
                    }
                    if (!GameStarted)
                    {
                        foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
                        {
                            PersistentEmpireRepresentative perp = peer.GetComponent<PersistentEmpireRepresentative>();
                            if (perp != null && perp.BRSpawnFrame != null)
                            {
                                peer.ControlledAgent.TeleportToPosition(perp.BRSpawnFrame.GameEntity.GetGlobalFrame().origin);
                            }
                        }
                    }


                    if (votes.Count >= GameNetwork.NetworkPeerCount / 2 && GameNetwork.NetworkPeerCount >= 4 && !GameStarted && startrequested == 0)
                    {
                        BeginMatch();
                    }
                    if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - startrequested > 10 && !GameStarted && startrequested != 0)
                    {
                        GameStarted = true;
                    }
                    if (GameStarted && !StartCompleted)
                    {
                        StartCompleted = true;
                        InformationComponent.Instance.BroadcastAnnouncement("Let The Games Begin!");
                        foreach (Agent agent in Mission.Current.Agents)
                        {
                            agent.ClearTargetFrame();
                        }
                    }
                    if (!GameStarted)
                    {
                        foreach (Agent agent in Mission.Current.Agents)
                        {
                            Vec2 vec = agent.Position.AsVec2;
                            agent.SetTargetPositionSynched(ref vec);
                        }
                    }
                }

                catch (Exception e)
                {
                    Debug.Print(e.ToString());
                }
            }
        }
    }
}
