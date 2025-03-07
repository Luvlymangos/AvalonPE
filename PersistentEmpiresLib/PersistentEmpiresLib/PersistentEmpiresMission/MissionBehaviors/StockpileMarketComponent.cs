﻿using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class StockpileMarketComponent : MissionNetwork
    {
        public long LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
        public long SaveDuration = 600;

        public delegate void StockpileMarketOpenHandler(PE_StockpileMarket stockpileMarket, Inventory playerInventory);
        public event StockpileMarketOpenHandler OnStockpileMarketOpen;

        public delegate void StockpileMarketUpdateHandler(PE_StockpileMarket stockpileMarket, int itemIndex, int newStock);
        public event StockpileMarketUpdateHandler OnStockpileMarketUpdate;

        public delegate void StockpileMarketUpdateMultiHandler(PE_StockpileMarket stockpileMarket, List<int> indexes, List<int> stocks);
        public event StockpileMarketUpdateMultiHandler OnStockpileMarketUpdateMultiHandler;

        Dictionary<MissionObject, List<NetworkCommunicator>> openedInventories;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            openedInventories = new Dictionary<MissionObject, List<NetworkCommunicator>>();
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenStockpileMarket>(this.HandleOpenStockpileMarketFromServer);
                networkMessageHandlerRegisterer.Register<UpdateStockpileStock>(this.HandleUpdateStockpileStockFromServer);
                networkMessageHandlerRegisterer.Register<UpdateStockpileMultiStock>(this.HandleUpdateStockpileMultiStock);
            }
            else
            {
                networkMessageHandlerRegisterer.Register<RequestBuyItem>(this.HandleRequestBuyItemFromClient);
                networkMessageHandlerRegisterer.Register<RequestSellItem>(this.HandleRequestSellItemFromClient);
                networkMessageHandlerRegisterer.Register<RequestCloseStockpileMarket>(this.HandleRequestCloseStockpileMarketFromClient);
                networkMessageHandlerRegisterer.Register<StockpileUnpackBox>(this.HandleStockpileUnpackBoxFromClient);
            }
        }

        private bool HandleStockpileUnpackBoxFromClient(NetworkCommunicator peer, StockpileUnpackBox message)
        {
            PersistentEmpireRepresentative representative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (representative == null) return false;
            PE_StockpileMarket market = message.StockpileMarket;
            Inventory playerInventory = representative.GetInventory();
            if (message.SlotId >= playerInventory.Slots.Count) return false;
            InventorySlot slot = playerInventory.Slots[message.SlotId];

            CraftingBox box = market.CraftingBoxes.Where(c => c.BoxItem.StringId.Equals(slot.Item.StringId)).FirstOrDefault();
            if (box == null) return false;

            int count = slot.Count;

            for (int i = 0; i < message.StockpileMarket.MarketItems.Count; i++)
            {
                Debug.Print((int)message.StockpileMarket.MarketItems[i].Item.Tier + " ITEM min:" + box.MinTierLevel + " max:" + box.MaxTierLevel);

            }

            var updatedMarketItems = message.StockpileMarket.MarketItems.Select((value, index) => new { index, value }).Where(m => m.value.Tier >= box.MinTierLevel && (int)m.value.Tier <= box.MaxTierLevel).ToList();
            int craftingReward = 0;
            foreach (var marketItem in updatedMarketItems)
            {
                Debug.Print(marketItem.index.ToString());
                marketItem.value.UpdateReserve(marketItem.value.Stock + count);
                this.UpdateStockForPeers(message.StockpileMarket, marketItem.index);
                craftingReward += marketItem.value.SellPrice();
            }
            if (updatedMarketItems.Count > 0)
            {
                craftingReward = ((craftingReward / updatedMarketItems.Count) * 6) * count;
                InformationComponent.Instance.SendMessage("Crafting box imported your reward is " + craftingReward + " denar.", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), peer);
                representative.GoldGain(craftingReward);
                List<int> updatedSlots = playerInventory.RemoveCountedItemSynced(box.BoxItem, count);
                foreach (int i in updatedSlots)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, playerInventory.Slots[i].Item, playerInventory.Slots[i].Count));
                    GameNetwork.EndModuleEventAsServer();
                }
            }

            return true;
        }

        private void HandleUpdateStockpileMultiStock(UpdateStockpileMultiStock message)
        {
            PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)message.Stockpile;
            for (int i = 0; i < message.Indexes.Count; i++)
            {
                int index = message.Indexes[i];
                int stock = message.Stocks[i];
                stockpileMarket.MarketItems[index].Stock = stock;
            }
            if (OnStockpileMarketUpdateMultiHandler != null)
            {
                OnStockpileMarketUpdateMultiHandler(stockpileMarket, message.Indexes, message.Stocks);
            }
            /*stockpileMarket.MarketItems[message.ItemIndex].UpdateReserve(message.NewStock);
            if (this.OnStockpileMarketUpdate != null)
            {
                this.OnStockpileMarketUpdate(stockpileMarket, message.ItemIndex, message.NewStock);
            }*/
        }

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (DateTimeOffset.Now.ToUnixTimeSeconds() > this.LastSaveAt + this.SaveDuration)
            {
                //this.AutoSaveAllMarkets();
                this.LastSaveAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            }
        }



        private void HandleUpdateStockpileStockFromServer(UpdateStockpileStock message)
        {
            PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)message.StockpileMarket;
            stockpileMarket.MarketItems[message.ItemIndex].UpdateReserve(message.NewStock);
            if (this.OnStockpileMarketUpdate != null)
            {
                this.OnStockpileMarketUpdate(stockpileMarket, message.ItemIndex, message.NewStock);
            }
            SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
        }


        private void RemoveFromUpdateList(NetworkCommunicator peer, MissionObject StockpileMarketEntity = null)
        {
            if (StockpileMarketEntity != null && this.openedInventories.ContainsKey(StockpileMarketEntity) && this.openedInventories[StockpileMarketEntity].Contains(peer))
            {
                this.openedInventories[StockpileMarketEntity].Remove(peer);
                return;
            }


            foreach (MissionObject mObject in this.openedInventories.Keys.ToList())
            {
                if (this.openedInventories[mObject].Contains(peer))
                {
                    
                    this.openedInventories[mObject].Remove(peer);
                }
            }
        }


        public override void OnPlayerDisconnectedFromServer(NetworkCommunicator player)
        {
            if (GameNetwork.IsServer)
            {
                this.RemoveFromUpdateList(player);
            }
        }

        private bool HandleRequestCloseStockpileMarketFromClient(NetworkCommunicator peer, RequestCloseStockpileMarket message)
        {
            this.RemoveFromUpdateList(peer, message.StockpileMarketEntity);
            return true;
        }

        private bool HandleRequestBuyItemFromClient(NetworkCommunicator peer, RequestBuyItem message)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)message.StockpileMarket;
            PE_TaxHandler taxHandler = stockpileMarket.GameEntity.GetFirstScriptOfType<PE_TaxHandler>();

            if (peer.ControlledAgent == null || peer.ControlledAgent.IsActive() == false) return false;
            if (peer.ControlledAgent.Position.Distance(stockpileMarket.GameEntity.GlobalPosition) > stockpileMarket.Distance) return false;

            MarketItem marketItem = stockpileMarket.MarketItems[message.ItemIndex];
            if (marketItem.Stock == 0)
            {
                InformationComponent.Instance.SendMessage("There is no stocks", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            if (persistentEmpireRepresentative.GetInventory().HasEnoughRoomFor(marketItem.Item, 1) == false)
            {
                InformationComponent.Instance.SendMessage("Inventory is full", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }
            if (!persistentEmpireRepresentative.ReduceIfHaveEnoughGold(marketItem.BuyPrice()))
            {
                InformationComponent.Instance.SendMessage("You don't have enough gold", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(marketItem.Item, 1, ItemHelper.GetMaximumAmmo(marketItem.Item));
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            
            if (taxHandler != null && taxHandler.CastleId != -1) taxHandler.AddTaxFeeToMoneyChest((marketItem.BuyPrice() * taxHandler.TaxPercentage) / 100);
            marketItem.UpdateReserve(marketItem.Stock - 1);
            // SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            this.UpdateStockForPeers(stockpileMarket, message.ItemIndex);
            // Send a message to update ui
            SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            return true;
        }

        public void AutoSaveAllMarkets()
        {
            List<PE_StockpileMarket> markets = base.Mission.GetActiveEntitiesWithScriptComponentOfType<PE_StockpileMarket>().Select(g => g.GetFirstScriptOfType<PE_StockpileMarket>()).ToList();
            foreach (PE_StockpileMarket market in markets)
            {
                SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(market);
            }
        }

        private void UpdateStockForPeers(PE_StockpileMarket stockpileMarket, int itemIndex)
        {
            MarketItem marketItem = stockpileMarket.MarketItems[itemIndex];
            if (this.openedInventories.ContainsKey(stockpileMarket))
            {
                foreach (NetworkCommunicator toPeer in this.openedInventories[stockpileMarket].ToArray())
                {
                    if (toPeer.IsConnectionActive)
                    {
                        GameNetwork.BeginModuleEventAsServer(toPeer);
                        GameNetwork.WriteMessage(new UpdateStockpileStock(stockpileMarket, marketItem.Stock, itemIndex));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
            }
        }

        public ItemObject QualityCheck(string ItemId, PersistentEmpireRepresentative perp)
        {
            Inventory inventory = perp.GetInventory();
            if (inventory.IsInventoryIncludesString($"Common_{ItemId}",1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Common_{ItemId}"); ;
            }
            if (inventory.IsInventoryIncludesString($"Uncommon_{ItemId}", 1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Uncommon_{ItemId}"); ;
            }
            if (inventory.IsInventoryIncludesString($"Rare_{ItemId}", 1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Rare_{ItemId}"); ;
            }
            if (inventory.IsInventoryIncludesString($"Epic_{ItemId}", 1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Epic_{ItemId}"); ;
            }
            if (inventory.IsInventoryIncludesString($"Legendary_{ItemId}", 1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Legendary_{ItemId}"); ;
            }
            if (inventory.IsInventoryIncludesString($"Mythic_{ItemId}", 1))
            {
                return MBObjectManager.Instance.GetObject<ItemObject>($"Mythic_{ItemId}"); ;
            }
            return null;

        }

        private bool HandleRequestSellItemFromClient(NetworkCommunicator peer, RequestSellItem message)
        {
            
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return false;
            PE_StockpileMarket stockpileMarket = (PE_StockpileMarket)message.StockpileMarket;
            if (peer.ControlledAgent == null || peer.ControlledAgent.IsActive() == false) return false;
            if (peer.ControlledAgent.Position.Distance(stockpileMarket.GameEntity.GlobalPosition) > stockpileMarket.Distance) return false;
            MarketItem marketItem = stockpileMarket.MarketItems[message.ItemIndex];
            if (marketItem.Stock >= marketItem.MaxStock)
            {
                InformationComponent.Instance.SendMessage("This Stockpile is full of that item!", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            if (marketItem.Item.StringId.StartsWith("Common_"))
            {
                string itemStringId = marketItem.Item.StringId.Replace("Common_", "");
                ItemObject newitem = QualityCheck(itemStringId, persistentEmpireRepresentative);
                if (newitem == null)
                {
                    InformationComponent.Instance.SendMessage("You don't have that item in your inventory", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                    return false;
                }
                List<int> updatedSlots2 = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(newitem, 1);
                foreach (int i in updatedSlots2)
                {
                    GameNetwork.BeginModuleEventAsServer(peer);
                    GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                    GameNetwork.EndModuleEventAsServer();
                }
                persistentEmpireRepresentative.GoldGain(marketItem.SellPrice());
                // if (marketItem.Stock > 1000) marketItem.Stock = 1000;
                // LoggerHelper.LogAnAction(peer, LogAction.PlayerSellsStockpile, null, new object[] {
                //     marketItem
                // });
                marketItem.UpdateReserve(marketItem.Stock + 1);
                // SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
                this.UpdateStockForPeers(stockpileMarket, message.ItemIndex);
                SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);

                return true;
            }
            else if (!persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(marketItem.Item, 1))
            {
                InformationComponent.Instance.SendMessage("You don't have that item in your inventory", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            bool arrowCheckFlag = true;
            if (marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Arrows ||
               marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bolts ||
               marketItem.Item.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bullets
                )
            {
                foreach (InventorySlot slot in persistentEmpireRepresentative.GetInventory().Slots)
                {
                    if (slot.Item != null && slot.Item.StringId == marketItem.Item.StringId)
                    {
                        int maxAmmo = ItemHelper.GetMaximumAmmo(marketItem.Item);
                        arrowCheckFlag = maxAmmo == slot.Ammo;
                        if (!arrowCheckFlag) break;
                    }
                }
            }

            if (arrowCheckFlag == false)
            {
                InformationComponent.Instance.SendMessage("You can't sell not filled ammo packs", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            List<int> updatedSlots = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(marketItem.Item, 1);
            foreach (int i in updatedSlots)
            {
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                GameNetwork.EndModuleEventAsServer();
            }
            persistentEmpireRepresentative.GoldGain(marketItem.SellPrice());
            // if (marketItem.Stock > 1000) marketItem.Stock = 1000;
            // LoggerHelper.LogAnAction(peer, LogAction.PlayerSellsStockpile, null, new object[] {
            //     marketItem
            // });
            marketItem.UpdateReserve(marketItem.Stock + 1);
            // SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            this.UpdateStockForPeers(stockpileMarket, message.ItemIndex);

            SaveSystemBehavior.HandleCreateOrSaveStockpileMarket(stockpileMarket);
            return true;
        }

        private void HandleOpenStockpileMarketFromServer(OpenStockpileMarket message)
        {
            PE_StockpileMarket market = (PE_StockpileMarket)message.StockpileMarketEntity;
            /*foreach (int stock in message.Stocks)
            {
                for (int i = 0; i < market.MarketItems.Count; i++)
                {
                    market.MarketItems[i].Stock = stock;
                }
            }*/
            if (this.OnStockpileMarketOpen != null)
            {
                this.OnStockpileMarketOpen(market, message.PlayerInventory);
            }
        }

        private static List<List<T>> ChunkBy<T>(List<T> source, int chunkSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / chunkSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }

        public void OpenStockpileMarketForPeer(PE_StockpileMarket entity, NetworkCommunicator networkCommunicator)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (persistentEmpireRepresentative == null) return;
            if (!this.openedInventories.ContainsKey(entity))
            {
                this.openedInventories[entity] = new List<NetworkCommunicator>();
            }
            this.openedInventories[entity].Add(networkCommunicator);

            List<int> indexes = new List<int>();
            for (int i = 0; i < entity.MarketItems.Count; i++)
            {

                MarketItem marketItem = entity.MarketItems[i];
                if (marketItem.Stock <= 0) continue;
                indexes.Add(i);
            }

            List<List<int>> chunks = ChunkBy<int>(indexes, 100);
            foreach (List<int> chunkIndex in chunks)
            {
                List<int> stocks = new List<int>();
                for (int i = 0; i < chunkIndex.Count; i++)
                {
                    int index = chunkIndex[i];
                    stocks.Add(entity.MarketItems[index].Stock);
                }

                GameNetwork.BeginModuleEventAsServer(networkCommunicator);
                GameNetwork.WriteMessage(new UpdateStockpileMultiStock(entity, chunkIndex, stocks));
                GameNetwork.EndModuleEventAsServer();
            }

            GameNetwork.BeginModuleEventAsServer(networkCommunicator);
            GameNetwork.WriteMessage(new OpenStockpileMarket(entity, persistentEmpireRepresentative.GetInventory()));
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
