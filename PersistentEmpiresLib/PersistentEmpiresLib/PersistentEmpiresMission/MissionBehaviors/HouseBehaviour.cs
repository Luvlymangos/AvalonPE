using Database.DBEntities;
using NetworkMessages.FromClient;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class HouseBehviour : MissionNetwork
    {
        public Dictionary<int, House> Houses { get; set; }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.Houses = new Dictionary<int, House>();
            List<GameEntity> gameEntities = new List<GameEntity>();
            base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_PlayerHouse>(ref gameEntities);
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            if (GameNetwork.IsServer)
            {
                IEnumerable<DBHouses> houses = SaveSystemBehavior.HandleGetHouses();
                foreach (DBHouses item in houses)
                {
                    if (!Houses.ContainsKey(item.HouseIndex))
                    {
                        foreach (var entity in gameEntities)
                        {
                            PE_PlayerHouse playerhouse = entity.GetFirstScriptOfType<PE_PlayerHouse>();
                            if (playerhouse != null && playerhouse.HouseIndex == item.HouseIndex)
                            {
                                Houses.Add(item.HouseIndex, new House(item.LordId, item.Marshalls, item.RentEnd, playerhouse, item.IsRented));
                                playerhouse.isOwned = item.IsRented;
                            }
                        }
                    }
                    else
                    {
                        Debug.Print("[HOUSE BEHAVIOUR] House already exists!");
                    }
                }
            }
        }

        public void SyncHouse(House house)
        {
            foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
            {
                Debug.Print("[HOUSE BEHAVIOUR] Syncing house to peer: " + peer.VirtualPlayer.Id);
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new SyncHouse(house));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public override void AfterStart()
        {
            if (GameNetwork.IsServer)
            {
                List<GameEntity> gameEntities = new List<GameEntity>();
                base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_PlayerHouse>(ref gameEntities);
                foreach (var item in gameEntities)
                {
                    PE_PlayerHouse playerhouse = item.GetFirstScriptOfType<PE_PlayerHouse>();
                    if (!Houses.ContainsKey(playerhouse.HouseIndex))
                    {
                        Houses.Add(playerhouse.HouseIndex, new House());
                    }
                }
                foreach (var item in Houses)
                {
                    foreach (var entity in gameEntities)
                    {
                        PE_PlayerHouse playerhouse = entity.GetFirstScriptOfType<PE_PlayerHouse>();
                        if (playerhouse != null && playerhouse.HouseIndex == item.Key)
                        {
                            item.Value.house = playerhouse;
                        }
                    }

                }
            }

        }
        public void SetPlayerHouse(NetworkCommunicator player, int houseindex)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
            if (GameNetwork.IsServer)
            {
                if (persistentEmpireRepresentative == null) return;
                if (Houses.ContainsKey(houseindex))
                {
                    persistentEmpireRepresentative.SetHouse(Houses[houseindex]);
                    Houses[houseindex].lordId = persistentEmpireRepresentative.Peer.Id.ToString();
                    Houses[houseindex].isrented = true;
                    Houses[houseindex].rentEnd = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds();
                    SaveSystemBehavior.HandleCreateOrSaveHouse(Houses[houseindex], houseindex);
                    SyncHouse(Houses[houseindex]);
                }
            }
            if (GameNetwork.IsClient)
            {
                persistentEmpireRepresentative.SetHouse(Houses[houseindex]);
            }
        }

        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerDisconnectedFromServer(networkPeer);
            // Clean up
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegistererContainer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegistererContainer();
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<SyncHouse>(this.HandleHouseUpdate);
            }

            if(GameNetwork.IsServer)
            {
                networkMessageHandlerRegisterer.Register<AddMarshalToHouse>(this.HandleAddMarshalToHouse);
            }
            
        }

        private void HandleHouseUpdate(SyncHouse message)
        {
            if (GameNetwork.IsClient)
            {
                if (message.lordId == GameNetwork.MyPeer.VirtualPlayer.Id.ToString())
                {
                    PersistentEmpireRepresentative myRepr = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
                    if (myRepr != null && myRepr.GetHouse() == null)
                    {
                        myRepr.SetHouse(Houses[message.HouseIndex]);
                    }
                }
                if (Houses.ContainsKey(message.HouseIndex))
                {
                    Houses[message.HouseIndex].lordId = message.lordId;
                    Houses[message.HouseIndex].marshalls = Houses[message.HouseIndex].LoadMarshallsFromSerialized(message.marshalls);
                    Houses[message.HouseIndex].rentEnd = message.rentEnd;
                }
                else
                {
                    List<GameEntity> gameEntities = new List<GameEntity>();
                    base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_PlayerHouse>(ref gameEntities);
                    foreach (var entity in gameEntities)
                    {
                        PE_PlayerHouse playerhouse = entity.GetFirstScriptOfType<PE_PlayerHouse>();
                        if (playerhouse != null && playerhouse.HouseIndex == message.HouseIndex)
                        {
                            if (message.lordId != "") playerhouse.ClientRentHouse();
                            else playerhouse.RentEnd();
                            Houses.Add(message.HouseIndex, new House(message.lordId, message.marshalls, message.rentEnd, playerhouse, message.IsRented));
                        }
                    }
                }
            }
        }

        private void HandleAddMarshalToHouse(AddMarshalToHouse message)
        {
            if (GameNetwork.IsServer)
            {
                if (Houses.ContainsKey(message.HouseIndex))
                {
                    Houses[message.HouseIndex].marshalls.Add(message.TargetPlayer.VirtualPlayer.Id.ToString());
                    SaveSystemBehavior.HandleCreateOrSaveHouse(Houses[message.HouseIndex], message.HouseIndex);
                    SyncHouse(Houses[message.HouseIndex]);
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (GameNetwork.IsServer)
            {
                foreach (var item in Houses)
                {
                    if (item.Value.rentEnd < DateTimeOffset.UtcNow.ToUnixTimeSeconds() && item.Value.isrented == true)
                    {

                        item.Value.lordId = "";
                        item.Value.marshalls = new List<string>();
                        item.Value.rentEnd = 0;
                        item.Value.isrented = false;
                        item.Value.house.RentEnd();
                        SaveSystemBehavior.HandleCreateOrSaveHouse(item.Value, item.Key);
                        SyncHouse(item.Value);

                    }
                }
            }
        }
    }
}
