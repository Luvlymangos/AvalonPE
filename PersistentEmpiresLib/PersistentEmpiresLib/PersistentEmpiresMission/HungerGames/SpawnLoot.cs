using Data;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using SceneScripts.HungerGames;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.ModuleManager;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;

namespace PersistentEmpiresMission.HungerGames
{
    public class SpawnLoot : MissionLogic
    {
        public Dictionary<int, List<ItemObject>> SpawnabeLoot = new Dictionary<int, List<ItemObject>>();

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            Debug.Print("SpawnLoot initialized");
            string GuardPath = ModuleHelper.GetXmlPath("PersistentEmpires", "Loot");
            Debug.Print("[PE] Trying Loading " + GuardPath, 0, DebugColor.Cyan);
            if (File.Exists(GuardPath) == false) return;

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(GuardPath);
            foreach (XmlNode node in xmlDocument.SelectNodes("/Loots/Loot"))
            {
                int Rarity = int.Parse(node["RarityLevel"].InnerText);
                List<ItemObject> Items = ParseLoot(node["Items"].InnerText);
                SpawnabeLoot.Add(Rarity, Items);
            }

        }

        public override void AfterStart()
        {
            base.AfterStart();
            Debug.Print("SpawnLoot started");
            foreach(GameEntity ge in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<LootBox>())
            {
                LootBox lb = ge.GetFirstScriptOfType<LootBox>();
                PE_InventoryEntity ie = ge.GetFirstScriptOfType<PE_InventoryEntity>();
                if (ie == null) continue;
                if (SpawnabeLoot.ContainsKey(lb.RarityLevel))
                {
                    Inventory someCustomChest = Mission.GetMissionBehavior<PlayerInventoryComponent>().CustomInventories[ie.InventoryId];

                    List<int> updated = someCustomChest.EmptyInventorySynced();
                    foreach (int y in updated)
                    {
                        foreach (NetworkCommunicator player in someCustomChest.CurrentlyOpenedBy)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot(someCustomChest.InventoryId + "_" + y, someCustomChest.Slots[y].Item, someCustomChest.Slots[y].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                    }
                    for (int i = 0; i < lb.ItemsToSpawn; i++)
                    {

                        List<int> updated2 = someCustomChest.AddCountedItemSynced(SpawnabeLoot[lb.RarityLevel].GetRandomElement(), 1, ItemHelper.GetMaximumAmmo(SpawnabeLoot[lb.RarityLevel].GetRandomElement()));
                        foreach (int x in updated2)
                        {
                            foreach (NetworkCommunicator player in someCustomChest.CurrentlyOpenedBy)
                            {
                                GameNetwork.BeginModuleEventAsServer(player);
                                GameNetwork.WriteMessage(new UpdateInventorySlot(someCustomChest.InventoryId + "_" + x, someCustomChest.Slots[x].Item, someCustomChest.Slots[x].Count));
                                GameNetwork.EndModuleEventAsServer();
                            }
                        }
                    }
                    
                }
            }
        }

        public List<ItemObject> ParseLoot(string Loot)
        {
            List<ItemObject> LootList = new List<ItemObject>();
            List<String> splitloot = Loot.Split('|').ToList();
            foreach (string item in splitloot)
            {
                LootList.Add(MBObjectManager.Instance.GetObject<ItemObject>(item));
            }
            return LootList;
        }

        public void NewRound()
        {

            foreach (GameEntity ge in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<LootBox>())
            {
                LootBox lb = ge.GetFirstScriptOfType<LootBox>();
                PE_InventoryEntity ie = ge.GetFirstScriptOfType<PE_InventoryEntity>();
                if (ie == null) continue;
                if (SpawnabeLoot.ContainsKey(lb.RarityLevel))
                {
                    Inventory someCustomChest = Mission.GetMissionBehavior<PlayerInventoryComponent>().CustomInventories[ie.InventoryId];

                    List<int> updated = someCustomChest.EmptyInventorySynced();
                    foreach (int y in updated)
                    {
                        foreach (NetworkCommunicator player in someCustomChest.CurrentlyOpenedBy)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot(someCustomChest.InventoryId + "_" + y, someCustomChest.Slots[y].Item, someCustomChest.Slots[y].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                    }
                    for (int i = 0; i < lb.ItemsToSpawn; i++)
                    {

                        List<int> updated2 = someCustomChest.AddCountedItemSynced(SpawnabeLoot[lb.RarityLevel].GetRandomElement(), 1, ItemHelper.GetMaximumAmmo(SpawnabeLoot[lb.RarityLevel].GetRandomElement()));
                        foreach (int x in updated2)
                        {
                            foreach (NetworkCommunicator player in someCustomChest.CurrentlyOpenedBy)
                            {
                                GameNetwork.BeginModuleEventAsServer(player);
                                GameNetwork.WriteMessage(new UpdateInventorySlot(someCustomChest.InventoryId + "_" + x, someCustomChest.Slots[x].Item, someCustomChest.Slots[x].Count));
                                GameNetwork.EndModuleEventAsServer();
                            }
                        }
                    }

                }
            }
        }
    }

}
