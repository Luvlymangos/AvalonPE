using Newtonsoft.Json;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using Websocket.Client;
using System.Text.RegularExpressions;
using TaleWorlds.Engine;
using Database.DBEntities;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class PersistentEmpireBehavior : MissionMultiplayerGameModeBase
    {

        public class ServerTokenValidate
        {
            public string Token { get; set; }
            public int MaxPlayer { get; set; }
            public string ServerName { get; set; }
        }

        public class MasterServerResponse
        {
            public string key { get; set; }
            public string error { get; set; }
        }

        private FactionsBehavior _factionsBehavior;
        private CastlesBehavior _castlesBehavior;
        private HouseBehviour _houseBehavior;
        private PlantingBehaviour _plantingBehaviour;
        private OfflineProtectionBehaviour _protectionBehaviour;
        private long lastPolledAt = 0;
        private bool registered = false;
        public bool agentLabelEnabled = true;
        public int AdminOn = 0;
        public override bool IsGameModeHidingAllAgentVisuals => true;

        public override bool IsGameModeUsingOpposingTeams => false;
        public string ServerSignature;

        // public static string ServerSignature = "";

        public override void OnAgentMount(Agent agent)
        {
            base.OnAgentMount(agent);
            if (agent.IsPlayerControlled && agent.MissionPeer.GetNetworkPeer() != null)
            {
                LoggerHelper.LogAnAction(agent.MissionPeer.GetNetworkPeer(), LogAction.PlayerMountedHorse, null, new object[] { agent });
            }
        }

        public override void OnAgentDismount(Agent agent)
        {
            base.OnAgentDismount(agent);
        }



        public override MultiplayerGameType GetMissionType()
        {
            return MultiplayerGameType.FreeForAll;
        }
        protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            networkPeer.AddComponent<PersistentEmpireRepresentative>();
        }
        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            base.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
            if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled && affectedAgent.IsUsingGameObject)
            {
                // affectedAgent.StopUsingGameObjectMT(false, true, false);
                affectedAgent.StopUsingGameObjectMT(false);
            }

            if (GameNetwork.IsServer && affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsHuman && affectorAgent.IsPlayerControlled && affectorAgent.IsActive())
            {

                NetworkCommunicator issuer = affectorAgent.MissionPeer.GetNetworkPeer();
                NetworkCommunicator affected = null;
                if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.MissionPeer.GetNetworkPeer();
                }
                else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                }
                LoggerHelper.LogAnAction(issuer, LogAction.PlayerHitToAgent, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { affectorWeapon, affectedAgent });

            }
            else if (GameNetwork.IsServer && affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsMount && affectorAgent.RiderAgent != null && affectorAgent.IsActive())
            {
                NetworkCommunicator issuer = affectorAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                NetworkCommunicator affected = null;
                if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.MissionPeer.GetNetworkPeer();
                }
                else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                {
                    affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                }
                LoggerHelper.LogAnAction(issuer, LogAction.PlayerBumpedWithHorse, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { affectorWeapon, affectedAgent });
            }
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow killingBlow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, killingBlow);
            if (GameNetwork.IsServer)
            {
                if ((agentState == AgentState.Killed || agentState == AgentState.Unconscious || agentState == AgentState.Routed) && affectedAgent != null && affectedAgent.IsHuman)
                {
                    if (affectedAgent.MissionPeer != null && !affectedAgent.MissionPeer.GetNetworkPeer().QuitFromMission)
                    {
                        this.OnPlayerDies(affectedAgent.MissionPeer.GetNetworkPeer());
                    }

                    if (affectorAgent != null && affectedAgent != affectorAgent && affectorAgent.IsHuman && affectorAgent.IsPlayerControlled)
                    {
                        NetworkCommunicator affected = null;
                        if (affectedAgent.IsHuman && affectedAgent.IsPlayerControlled)
                        {
                            affected = affectedAgent.MissionPeer.GetNetworkPeer();
                        }
                        else if (affectedAgent.IsMount && affectedAgent.RiderAgent != null && affectedAgent.RiderAgent.IsHuman && affectedAgent.RiderAgent.IsPlayerControlled)
                        {
                            affected = affectedAgent.RiderAgent.MissionPeer.GetNetworkPeer();
                        }
                        MissionWeapon weapon = new MissionWeapon();
                        if (affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand) != EquipmentIndex.None)
                        {
                            weapon = affectorAgent.Equipment[affectorAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand)];
                        }
                        LoggerHelper.LogAnAction(affectorAgent.MissionPeer.GetNetworkPeer(), LogAction.PlayerKilledAnAgent, affected == null ? new AffectedPlayer[] { } : new AffectedPlayer[] { new AffectedPlayer(affected) }, new object[] { weapon, affectedAgent });
                    }
                }
            }
        }
        public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerConnectedToServer(networkPeer);
            networkPeer.QuitFromMission = false;
        }
        protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator peer)
        {
            base.HandleEarlyPlayerDisconnect(peer);
            peer.QuitFromMission = true;
        }
        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
            base.OnPlayerDisconnectedFromServer(networkPeer);
            networkPeer.QuitFromMission = true;

            SaveSystemBehavior saveSystemBehavior = base.Mission.GetMissionBehavior<SaveSystemBehavior>();

            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative != null)
            {
                persistentEmpireRepresentative.DisconnectedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            }
            if (GameNetwork.IsServer)
            {
                LoggerHelper.LogAnAction(networkPeer, LogAction.PlayerDisconnected);
                if (Main.IsPlayerAdmin(networkPeer))
                {
                    this.AdminOn--;
                }
            }

            if (saveSystemBehavior != null && persistentEmpireRepresentative != null && persistentEmpireRepresentative.IsFirstAgentSpawned)
            {
                persistentEmpireRepresentative.IsFirstAgentSpawned = false;
                SaveSystemBehavior.HandleCreateOrSavePlayer(networkPeer);
                SaveSystemBehavior.HandleCreateOrSavePlayerInventory(networkPeer);

            }
            if (networkPeer.ControlledAgent != null && networkPeer.ControlledAgent.MountAgent != null)
            {
                networkPeer.ControlledAgent.FadeOut(true, true);
            }
        }


        private void OnPlayerDies(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            LoggerHelper.LogAnAction(peer, LogAction.PlayerDied);
            persistentEmpireRepresentative.SpawnTimer.Reset(Mission.Current.CurrentTime, (float)MissionLobbyComponent.GetSpawnPeriodDurationForPeer(peer.GetComponent<MissionPeer>()));
            if (persistentEmpireRepresentative.KickedFromFaction)
            {
                persistentEmpireRepresentative.SetClass("pe_serf");
                persistentEmpireRepresentative.KickedFromFaction = false;
            }
        }

        private bool checkAlphaNumeric(String name)
        {
            // ^[a-zA-Z0-9\s,\[,\],\(,\)]*$
            Regex rg = new Regex(@"^[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ.\s,\[,\],\(,\),_,-,\p{IsCJKUnifiedIdeographs}]*$");
            return rg.IsMatch(name);
        }

        protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            base.HandleLateNewClientAfterSynchronized(networkPeer);
            if (networkPeer.IsConnectionActive == false || networkPeer.IsNetworkActive == false) return;
            this._factionsBehavior = base.Mission.GetMissionBehavior<FactionsBehavior>();
            this._castlesBehavior = base.Mission.GetMissionBehavior<CastlesBehavior>();
            this._houseBehavior = base.Mission.GetMissionBehavior<HouseBehviour>();
            this._plantingBehaviour = base.Mission.GetMissionBehavior<PlantingBehaviour>();
            this._protectionBehaviour = base.Mission.GetMissionBehavior<OfflineProtectionBehaviour>();
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkPeer.GetComponent<PersistentEmpireRepresentative>();
            if (Main.IsPlayerAdmin(networkPeer))
            {
                this.AdminOn++;
            }
            if (persistentEmpireRepresentative != null)
            {
                if (persistentEmpireRepresentative.Gold <= 0)
                {
                    persistentEmpireRepresentative.SetGold(ConfigManager.StartingGold);
                }
                else
                {
                    persistentEmpireRepresentative.SetGold(persistentEmpireRepresentative.Gold);
                }
            }
            MissionPeer component = networkPeer.GetComponent<MissionPeer>();

            if (GameNetwork.IsServer)
            {
                InformationComponent.Instance.SendMessage("Your player id is " + networkPeer.VirtualPlayer.Id.ToString(), Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), networkPeer);
                if (_protectionBehaviour.IsOfflineProtectionActive)
                {
                    InformationComponent.Instance.SendMessage("Offline protection is active", Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), networkPeer);
                }
                else
                {
                    InformationComponent.Instance.SendMessage("Offline protection is not active", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), networkPeer);
                }
                if (this.AdminOn == 1)
                {
                    InformationComponent.Instance.SendMessage("1 Admin is online", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), networkPeer);
                }
                else if (this.AdminOn > 1)
                {
                    InformationComponent.Instance.SendMessage($"{this.AdminOn} Admin's are online", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), networkPeer);
                }
                else
                {
                    InformationComponent.Instance.SendMessage("No Admin is online", Color.ConvertStringToColor("#FF0000FF").ToUnsignedInteger(), networkPeer);
                }
                //SYNC FACTIONS
                List<PE_CastleBanner> castleBanners = this._castlesBehavior.castles.Values.ToList();
                for (int i = 0; i < this._factionsBehavior.Factions.Keys.ToList().Count; i++)
                {
                    Faction f = this._factionsBehavior.Factions[i];
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SyncFaction(i, f));
                    GameNetwork.EndModuleEventAsServer();
                    foreach (string s in this._factionsBehavior.Factions[i].marshalls)
                    {
                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new AddMarshallIdToFaction(s, i));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
                
                
                //SYNC BANNERS
                foreach (PE_CastleBanner castleBanner in castleBanners)
                {
                    GameNetwork.BeginModuleEventAsServer(networkPeer);
                    GameNetwork.WriteMessage(new SyncCastleBanner(castleBanner, castleBanner.FactionIndex));
                    GameNetwork.EndModuleEventAsServer();
                }

                //SYNC PLANTS
                //foreach (Growables i in _plantingBehaviour.Plants)
                //{
                //    GameNetwork.BeginModuleEventAsServer(networkPeer);
                //    GameNetwork.WriteMessage(new UpdatePlants(i));
                //    GameNetwork.EndModuleEventAsServer();
                //}

#if SERVER
                //SYNC MARKETS
                IEnumerable<GameEntity> StockpileMarkets = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_StockpileMarket>();
                List<PE_StockpileMarket> SearchedStockpileMarkets = new List<PE_StockpileMarket>();
                HashSet<string> stockpileTags = new HashSet<string>(); // Store unique StockpileTags

                foreach (GameEntity i in StockpileMarkets)
                {
                    PE_StockpileMarket stockpileMarket = i.GetFirstScriptOfType<PE_StockpileMarket>();

                    // Add to the list if the StockpileTag is not already present
                    if (stockpileTags.Add(stockpileMarket.XmlFile))
                    {
                        SearchedStockpileMarkets.Add(stockpileMarket);
                    }
                }

                foreach (PE_StockpileMarket stockpileMarket in SearchedStockpileMarkets)
                {
                    Debug.Print($"Stockpile Market {stockpileMarket.XmlFile}");
                    foreach (MarketItem i in stockpileMarket.MarketItems)
                    {
                        string x = $"{i.Item.StringId}*{i.MinimumPrice}*{i.MaximumPrice}";
                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new Sendmarket(x, stockpileMarket.XmlFile));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }

                //SYNC CRAFTING STATIONS
                IEnumerable<GameEntity> CraftingStations = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_CraftingStation>();
                List<PE_CraftingStation> SearchedStations = new List<PE_CraftingStation>();
                HashSet<string> craftingReceiptTags = new HashSet<string>(); // Store unique CraftingRecieptTags
                
                foreach (GameEntity i in CraftingStations)
                {
                    PE_CraftingStation craftingStation = i.GetFirstScriptOfType<PE_CraftingStation>();

                    // Add to the list if the CraftingRecieptTag is not already present
                    if (craftingReceiptTags.Add(craftingStation.CraftingRecieptTag))
                    {
                        SearchedStations.Add(craftingStation);
                    }
                }

                foreach (PE_CraftingStation craftingStation in SearchedStations)
                {
                    if (craftingStation == null) continue;
                    Debug.Print($"Crafting Station {craftingStation.StationName}");
                    foreach (string i in craftingStation.CraftingList)
                    {
                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new SendCrafting(i ,craftingStation.CraftingRecieptTag));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
#endif
                //SYNC PLAYERS
                foreach (NetworkCommunicator player in GameNetwork.NetworkPeers)
                {
                    PersistentEmpireRepresentative persistentEmpireRepresentative1 = player.GetComponent<PersistentEmpireRepresentative>();
                    if (persistentEmpireRepresentative1 != null)
                    {
                        Faction f = persistentEmpireRepresentative1.GetFaction();

                        GameNetwork.BeginModuleEventAsServer(networkPeer);
                        GameNetwork.WriteMessage(new SyncMember(player, persistentEmpireRepresentative1.GetFactionIndex(), f == null ? false : f.marshalls.Contains(player.VirtualPlayer.Id.ToString())));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }


                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(new AgentLabelConfig(this.agentLabelEnabled));
                GameNetwork.EndModuleEventAsServer();

            }
            SaveSystemBehavior saveSystemBehavior = base.Mission.GetMissionBehavior<SaveSystemBehavior>();
            if (saveSystemBehavior != null)
            {
                //CREATE PLAYER
                bool created;
                SaveSystemBehavior.HandleCreatePlayerNameIfNotExists(networkPeer);
                DBPlayer dbPlayer = SaveSystemBehavior.HandleGetOrCreatePlayer(networkPeer, out created);
                Debug.Print(dbPlayer.FactionIndex.ToString());
                if (!created)
                {
                    //SYNC PLAYER CLASS,GOLD,HUNGER,FACTION
                    persistentEmpireRepresentative.SetClass(dbPlayer.Class);
                    persistentEmpireRepresentative.SetGold(dbPlayer.Money);
                    persistentEmpireRepresentative.SetHunger(dbPlayer.Hunger);
                    this._factionsBehavior.SetPlayerFaction(networkPeer, dbPlayer.FactionIndex, -1);

                    persistentEmpireRepresentative.LoadedDbPosition = new Vec3(dbPlayer.PosX, dbPlayer.PosY, dbPlayer.PosZ);
                    Equipment loadedEquipment = new Equipment();
                    int[] loadedAmmo = new int[4];
                    //SYNC PLAYER EQUIPMENT
                    if (dbPlayer.Equipment_0 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon0] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_0));
                        loadedAmmo[0] = dbPlayer.Ammo_0;
                    }
                    if (dbPlayer.Equipment_1 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon1] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_1));
                        loadedAmmo[1] = dbPlayer.Ammo_1;
                    }
                    if (dbPlayer.Equipment_2 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon2] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_2));
                        loadedAmmo[2] = dbPlayer.Ammo_2;
                    }
                    if (dbPlayer.Equipment_3 != null)
                    {
                        loadedEquipment[EquipmentIndex.Weapon3] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Equipment_3));
                        loadedAmmo[3] = dbPlayer.Ammo_3;
                    }
                    if (dbPlayer.Armor_Head != null)
                    {
                        loadedEquipment[EquipmentIndex.Head] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Head));
                    }
                    if (dbPlayer.Armor_Cape != null)
                    {
                        loadedEquipment[EquipmentIndex.Cape] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Cape));
                    }
                    if (dbPlayer.Armor_Gloves != null)
                    {
                        loadedEquipment[EquipmentIndex.Gloves] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Gloves));
                    }
                    if (dbPlayer.Armor_Body != null)
                    {
                        loadedEquipment[EquipmentIndex.Body] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Body));
                    }
                    if (dbPlayer.Armor_Leg != null)
                    {
                        loadedEquipment[EquipmentIndex.Leg] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Armor_Leg));
                    }
                    if (dbPlayer.Horse != null)
                    {
                        loadedEquipment[EquipmentIndex.Horse] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.Horse));
                    }
                    if (dbPlayer.HorseHarness != null)
                    {
                        loadedEquipment[EquipmentIndex.HorseHarness] = new EquipmentElement(MBObjectManager.Instance.GetObject<ItemObject>(dbPlayer.HorseHarness));
                    }
                    persistentEmpireRepresentative.LoadedSpawnEquipment = loadedEquipment;
                    persistentEmpireRepresentative.LoadedHealth = dbPlayer.Health <= 0 ? 10 : dbPlayer.Health;
                    persistentEmpireRepresentative.LoadFromDb = true;
                    persistentEmpireRepresentative.LoadedAmmo = loadedAmmo;
                    //SYNC SKILLS
#if SERVER
                    try
                    {
                        persistentEmpireRepresentative.LoadedSkills.Add("Weaving", dbPlayer.Weaving);
                        persistentEmpireRepresentative.LoadedSkills.Add("WeaponSmithing", dbPlayer.WeaponSmithing);
                        persistentEmpireRepresentative.LoadedSkills.Add("ArmourSmithing", dbPlayer.ArmourSmithing);
                        persistentEmpireRepresentative.LoadedSkills.Add("BlackSmithing", dbPlayer.BlackSmithing);
                        persistentEmpireRepresentative.LoadedSkills.Add("Carpentry", dbPlayer.Carpentry);
                        persistentEmpireRepresentative.LoadedSkills.Add("Cooking", dbPlayer.Cooking);
                        persistentEmpireRepresentative.LoadedSkills.Add("Farming", dbPlayer.Farming);
                        persistentEmpireRepresentative.LoadedSkills.Add("Mining", dbPlayer.Mining);
                        persistentEmpireRepresentative.LoadedSkills.Add("Fletching", dbPlayer.Fletching);
                        persistentEmpireRepresentative.LoadedSkills.Add("Animals", dbPlayer.Animals);
                        foreach (KeyValuePair<string, int> skill in persistentEmpireRepresentative.LoadedSkills)
                        {
                            persistentEmpireRepresentative.SyncCraftingStats(skill.Key);
                        }
                    }
                    catch {
                        foreach (KeyValuePair<string, int> skill in persistentEmpireRepresentative.LoadedSkills)
                        {
                            persistentEmpireRepresentative.SyncCraftingStats(skill.Key);
                        }
                    }
                    //SYNC HOUSE
                    foreach (var item in _houseBehavior.Houses)
                    {
                        _houseBehavior.SyncHouse(item.Value);
                        if (item.Value.lordId == networkPeer.VirtualPlayer.Id.ToString())
                        {
                            persistentEmpireRepresentative.SetHouse(item.Value);
                        }
                    }
#endif
                }
                else
                {
                    Debug.Print("Player is created");
                    this._factionsBehavior.SetPlayerFaction(networkPeer, 0, -1);
                    foreach (KeyValuePair<string, int> skill in persistentEmpireRepresentative.LoadedSkills)
                    {
                        persistentEmpireRepresentative.SyncCraftingStats(skill.Key);
                    }
                    foreach (var item in _houseBehavior.Houses)
                    {
                        _houseBehavior.SyncHouse(item.Value);
                        if (item.Value.lordId == networkPeer.VirtualPlayer.Id.ToString())
                        {
                            persistentEmpireRepresentative.SetHouse(item.Value);
                        }
                    }
                }

                //SYNC INVENTORY
                DBInventory dbInventory = SaveSystemBehavior.HandleGetOrCreatePlayerInventory(networkPeer, out created);
                if (!created)
                {
                    persistentEmpireRepresentative.SetInventory(Inventory.Deserialize(dbInventory.InventorySerialized, "PlayerInventory", null));
                }
                
            }
        }
     
        

        
        public override void AfterStart()
        {
            if (GameNetwork.IsClient) {
                foreach (var item in _houseBehavior.Houses)
                {
                    if (item.Value.lordId == GameNetwork.MyPeer.VirtualPlayer.Id.ToString())
                    {
                        PersistentEmpireRepresentative persistentEmpireRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
                        persistentEmpireRepresentative.SetHouse(item.Value);
                    }
                }
            }
            Mission.Current.SetMissionCorpseFadeOutTimeInSeconds(60);
            if(GameNetwork.IsServer)
            {
                ConfigManager.Initialize();
                this.agentLabelEnabled = ConfigManager.GetBoolConfig("AgentLabelEnabled", true);
            }
            
            int maxPlayer = MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);
            string serverName = MultiplayerOptions.OptionType.ServerName.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions);

           
            IEnumerable<DBUpgradeableBuilding> dbUpgradeables = SaveSystemBehavior.HandleGetAllUpgradeableBuildings();
            List<PE_UpgradeableBuildings> upgradeables = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_UpgradeableBuildings>().Select(g => g.GetFirstScriptOfType<PE_UpgradeableBuildings>()).ToList();

            Dictionary<string, DBUpgradeableBuilding> savedUpgrades = new Dictionary<string, DBUpgradeableBuilding>();

            foreach (DBUpgradeableBuilding building in dbUpgradeables)
            {
                savedUpgrades[building.MissionObjectHash] = building;
            }
            foreach (PE_UpgradeableBuildings upgradeable in upgradeables)
            {
                Debug.Print("Upgradeable id is : " + upgradeable.GetMissionObjectHash());
                if (savedUpgrades.ContainsKey(upgradeable.GetMissionObjectHash()))
                {
                    upgradeable.SetTier(savedUpgrades[upgradeable.GetMissionObjectHash()].CurrentTier);
                    upgradeable.SetIsUpgrading(savedUpgrades[upgradeable.GetMissionObjectHash()].IsUpgrading);
                }
            }

            // LOAD Carts from DB
            IEnumerable<DBCart> dbCarts = SaveSystemBehavior.HandleGetAllCarts();
            if (dbCarts == null)
            {
                Debug.Print("DBCarts is null");
            }
            else
            {
                foreach (DBCart dbcart in dbCarts)
                {
                    MatrixFrame spawnFrame = new MatrixFrame(new Mat3(), new Vec3(dbcart.PosX, dbcart.PosY, dbcart.PosZ));
                    MissionObject mObject = Mission.Current.CreateMissionObjectFromPrefab(dbcart.Prefab, spawnFrame);
                    PE_AttachToAgent cart = mObject.GameEntity.GetFirstScriptOfType<PE_AttachToAgent>();
                    cart.CreateFromServer(dbcart.Id, dbcart.InventoryID);
                }
            }

            // LOAD StockpileMarkets from DB
            IEnumerable<DBStockpileMarket> dbStockpileMarkets = SaveSystemBehavior.HandleGetAllStockpileMarkets();
            List<PE_StockpileMarket> stockpileMarkets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_StockpileMarket>().Select(g => g.GetFirstScriptOfType<PE_StockpileMarket>()).ToList();
            Dictionary<string, DBStockpileMarket> savedStockpile = new Dictionary<string, DBStockpileMarket>();
            foreach (DBStockpileMarket dbStockpileMarket in dbStockpileMarkets)
            {
                savedStockpile[dbStockpileMarket.MissionObjectHash] = dbStockpileMarket;
            }
            foreach (PE_StockpileMarket stockpileMarket in stockpileMarkets)
            {
                if (savedStockpile.ContainsKey(stockpileMarket.GetMissionObjectHash()))
                {
                    stockpileMarket.DeserializeStocks(savedStockpile[stockpileMarket.GetMissionObjectHash()].MarketItemsSerialized);
                }
            }
            // LOAD HorseMarkets from DB
            IEnumerable<DBHorseMarket> dBHorseMarkets = SaveSystemBehavior.HandleGetAllHorseMarkets();
            List<PE_HorseMarket> horseMarkets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_HorseMarket>().Select(g => g.GetFirstScriptOfType<PE_HorseMarket>()).ToList();
            Dictionary<string, DBHorseMarket> savedHorseMarket = new Dictionary<string, DBHorseMarket>();
            foreach (DBHorseMarket dBHorseMarket in dBHorseMarkets)
            {
                savedHorseMarket[dBHorseMarket.MissionObjectHash] = dBHorseMarket;
            }
            foreach (PE_HorseMarket horseMarket in horseMarkets)
            {
                if (savedHorseMarket.ContainsKey(horseMarket.GetMissionObjectHash()))
                {
                    horseMarket.UpdateReserve(savedHorseMarket[horseMarket.GetMissionObjectHash()].Stock);
                }
            }
        }
    }
}
