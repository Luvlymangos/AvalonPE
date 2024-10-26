using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts.Extensions;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class MarketItem
    {
        public ItemObject Item;
        public int Stock;
        public int MaximumPrice;
        public float CurrentPrice;
        public int MinimumPrice;
        public int Constant;
        public int Stability;
        public int Tier;
        // X is stock Y is price x^(1/stability) * y = k
        public MarketItem(string itemId, int maximumPrice, int minimumPrice, int stability, int tier)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.MaximumPrice = maximumPrice;
            this.MinimumPrice = minimumPrice;
            this.Constant = MaximumPrice;
            this.Stability = stability;
            this.CurrentPrice = (MaximumPrice + MinimumPrice) / 2;
            this.Tier = tier;
        }
        public void UpdateReserve(int newStock)
        {
            if (newStock > 500) newStock = 500;
            if (newStock < 0) newStock = 0;
            this.Stock = newStock;
        }
        public int BuyPrice()
        {
            if (MinimumPrice <= 20) return MaximumPrice;
            int price = (int)CurrentPrice;
            if (Stock > 500)
            {
                price = MinimumPrice;
            }
            else if (Stock < 1)
            {
                price = MaximumPrice;
            }
            else
            {
                int priceRange = MaximumPrice - MinimumPrice;
                int stockRange = 500;
                int stockFactor = Stock;
                price = MinimumPrice + (priceRange * (stockRange - stockFactor) / stockRange);
            }

            price = (int)Math.Ceiling(price + (price * 0.075));

            // Ensure price stays within bounds
            if (price < MinimumPrice + (MinimumPrice * 0.075))
            {
                price = (int)Math.Ceiling(MinimumPrice + (MinimumPrice * 0.075));
            }
            else if (price > MaximumPrice + (MaximumPrice * 0.075))
            {
                price = (int)Math.Ceiling(MaximumPrice + (MaximumPrice * 0.075));
            }

            return price;
        }
        public int SellPrice()
        {
            int price = (int)CurrentPrice;
            if (MinimumPrice <= 20) return MinimumPrice;
            if (Stock > 500)
            {
                price = MinimumPrice;
            }
            else if (Stock < 1)
            {
                price = MaximumPrice;
            }
            else
            {
                int priceRange = MaximumPrice - MinimumPrice;
                int stockRange = 500;
                int stockFactor = Stock;
                price = MinimumPrice + (priceRange * (stockRange - stockFactor) / stockRange);
            }

            price = (int)Math.Floor(price - (price * 0.075));

            // Ensure price stays within bounds
            if (price < MinimumPrice - (MinimumPrice * 0.075))
            {
                price = (int)Math.Floor(MinimumPrice - (MinimumPrice * 0.075));
            }
            else if (price > MaximumPrice - (MaximumPrice * 0.075))
            {
                price = (int)Math.Floor(MaximumPrice - (MaximumPrice * 0.075));
            }

            return price;
        }
    }
    public class CraftingBox
    {
        public ItemObject BoxItem;
        public int MinTierLevel;
        public int MaxTierLevel;
        public CraftingBox(string itemId, string minTierLevel, string maxTierLevel)
        {
            this.BoxItem = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.MinTierLevel = int.Parse(minTierLevel);
            this.MaxTierLevel = int.Parse(maxTierLevel);
        }
    }
    public class PE_StockpileMarket : PE_UsableFromDistance, IMissionObjectHash
    {

        public static int MAX_STOCK_COUNT = 1000;
        public string XmlFile = "examplemarket"; // itemId*minimum*maximum,itemId*minimum*maximum
        public string ModuleFolder = "PersistentEmpires";
        protected override bool LockUserFrames
        {
            get
            {
                return false;
            }
        }
        protected override bool LockUserPositions
        {
            get
            {
                return false;
            }
        }

        public List<MarketItem> MarketItems { get; private set; }
        public List<CraftingBox> CraftingBoxes { get; private set; }
        public StockpileMarketComponent stockpileMarketComponent { get; private set; }
        protected void LoadMarketItems(string innerText, int tier)
        {

            if (innerText == "") return;
            foreach (string marketItemStr in innerText.Trim().Split('|'))
            {
                string[] values = marketItemStr.Split('*');
                string itemId = values[0];
                int minPrice = int.Parse(values[1]);
                int maxPrice = int.Parse(values[2]);
                int stability = values.Length > 4 ? int.Parse(values[3]) : 10;

                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
                if (item == null)
                {
                    Debug.Print(" ERROR IN MARKET SERIALIZATION " + this.XmlFile + " ITEM ID " + itemId + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
                }
                this.MarketItems.Add(new MarketItem(itemId, maxPrice, minPrice, stability, tier));
            }
        }

        protected void LoadMarketItem(string innerText)
        {

            if (innerText == "") return;
            string[] values = innerText.Split('*');
            string itemId = values[0];
            int minPrice = int.Parse(values[1]);
            int maxPrice = int.Parse(values[2]);
            int stability = values.Length > 4 ? int.Parse(values[3]) : 10;

            ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            if (item == null)
            {
                Debug.Print(" ERROR IN MARKET SERIALIZATION " + this.XmlFile + " ITEM ID " + itemId + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
            }
            this.MarketItems.Add(new MarketItem(itemId, maxPrice, minPrice, stability, 1));
            
        }
        protected void LoadCraftingBoxes(string innerText)
        {
            this.CraftingBoxes = new List<CraftingBox>();
            if (innerText == "") return;
            foreach (string craftingBoxStr in innerText.Trim().Split('|'))
            {
                string[] values = craftingBoxStr.Split('*');
                string itemId = values[0];
                string minTierLevel = values[1];
                string maxTierLevel = values[2];
                this.CraftingBoxes.Add(new CraftingBox(itemId, minTierLevel, maxTierLevel));
            }
        }

        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<Sendmarket>(this.HandleMarketFromServer);
            }
        }

        public void HandleMarketFromServer(Sendmarket message)
        {
            if (message.MarketType != this.XmlFile) return;
            this.LoadMarketItem(message.Market);
        }

        protected override void OnInit()
        {
            if (GameNetwork.IsClient)
            {
                this.MarketItems = new List<MarketItem>();
                AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            }
#if SERVER
            if (GameNetwork.IsServer)
            {
                base.OnInit();
                TextObject actionMessage = new TextObject("Browse the Market");
                base.ActionMessage = actionMessage;
                TextObject descriptionMessage = new TextObject("Press {KEY} To Browse");
                descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
                base.DescriptionMessage = descriptionMessage;
                this.stockpileMarketComponent = Mission.Current.GetMissionBehavior<StockpileMarketComponent>();
                Debug.Print("[Feudal Kingdoms] Initiating Stockpile Market With " + this.ModuleFolder + " Module", 0, Debug.DebugColor.DarkCyan);
                string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "Markets/" + this.XmlFile);
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlPath);
                this.MarketItems = new List<MarketItem>();
                this.LoadMarketItems(xmlDocument.SelectSingleNode("/Market/Items").InnerText, 1);
            }
#endif
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Stockpile Market";
        }

        public override void OnUse(Agent userAgent)
        {
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);

            if (GameNetwork.IsServer)
            {
                this.stockpileMarketComponent.OpenStockpileMarketForPeer(this, userAgent.MissionPeer.GetNetworkPeer());
                userAgent.StopUsingGameObjectMT(true);
            }
        }
        public void DeserializeStocks(string serialized)
        {
            Debug.Print("Deserializing Stocks From Market:" + this.GetMissionObjectHash(), 0, Debug.DebugColor.DarkCyan);
            string[] elements = serialized.Split('|');
            foreach (string s in elements)
            {
                ItemObject item = MBObjectManager.Instance.GetObject<ItemObject>(s.Split('*')[0]);
                if (item == null)
                {
                    Debug.Print(" ERROR IN MARKET SERIALIZATION " + this.XmlFile + " ITEM ID " + s.Split('*')[0] + " NOT FOUND !!! ", 0, Debug.DebugColor.Red);
                }
                int stock = int.Parse(s.Split('*')[1]);
                MarketItems.Find(m => m.Item.StringId == item.StringId).UpdateReserve(stock);
            }
        }
        public string SerializeStocks()
        {
            return string.Join("|", MarketItems.Select(s => s.Item.StringId + "*" + s.Stock));
        }

        public MissionObject GetMissionObject()
        {
            return this;
        }

        protected override bool OnHit(Agent attackerAgent, int damage, Vec3 impactPosition, Vec3 impactDirection, in MissionWeapon weapon, ScriptComponentBehavior attackerScriptComponentBehavior, out bool reportDamage)
        {
            reportDamage = false;
            if (attackerAgent == null) return false;
            NetworkCommunicator player = attackerAgent.MissionPeer.GetNetworkPeer();
            bool isAdmin = Main.IsPlayerAdmin(player);
            if (isAdmin && weapon.Item != null && weapon.Item.StringId == "pe_adminstockfiller")
            {
                foreach (MarketItem marketItem in this.MarketItems)
                {
                    var currentStock = marketItem.Stock;
                    if (currentStock + 10 < 900)
                    {
                        marketItem.UpdateReserve(currentStock + 10);
                    }
                }
                InformationComponent.Instance.SendMessage("Stocks updated", Colors.Blue.ToUnsignedInteger(), player);
            }
            return true;
        }
    }
}