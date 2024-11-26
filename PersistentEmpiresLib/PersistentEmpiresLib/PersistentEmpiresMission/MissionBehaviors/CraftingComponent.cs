using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class CraftingComponent : MissionNetwork
    {

        public enum Tier
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary,
            Mythic
        }

        private class CraftingAction
        {
            public CraftingAction(PE_CraftingStation craftingStation, Craftable craftable, long startedAt)
            {
                this.craftingStation = craftingStation;
                this.craftable = craftable;
                this.startedAt = startedAt;
            }
            public PE_CraftingStation craftingStation;
            public Craftable craftable;
            public long startedAt;
        }
        Dictionary<NetworkCommunicator, CraftingAction> craftings;

        public delegate void CraftingStationUseHandler(PE_CraftingStation craftingStation, Inventory playerInventory);
        public event CraftingStationUseHandler OnCraftingUse;

        public delegate void CraftingStartedHandler(PE_CraftingStation craftingStation, int craftIndex, NetworkCommunicator player);
        public event CraftingStartedHandler OnCraftingStarted;

        public delegate void CraftingCompletedHandler();
        public event CraftingCompletedHandler OnCraftingCompleted;
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            this.craftings = new Dictionary<NetworkCommunicator, CraftingAction>();
        }
        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            this.AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        }
        public override void OnMissionTick(float dt)
        {
#if SERVER
            base.OnMissionTick(dt);
            if (GameNetwork.IsClient) return;
            if (this.craftings == null) return;
            foreach (NetworkCommunicator player in craftings.Keys.ToList())
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = player.GetComponent<PersistentEmpireRepresentative>();
                if (!player.IsConnectionActive)
                {
                    this.craftings.Remove(player);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CraftingCompleted(player));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    continue;
                }
                if (persistentEmpireRepresentative == null)
                {
                    this.craftings.Remove(player);
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new CraftingCompleted(player));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    continue;
                }
                CraftingAction craftingAction = this.craftings[player];
                if (player.ControlledAgent != null && craftingAction.craftingStation.Animation != "")
                {
                    if (player.ControlledAgent.GetCurrentAction(0).Name == "act_none")
                    {
                        ActionIndexCache action = ActionIndexCache.Create(craftingAction.craftingStation.Animation);
                        player.ControlledAgent.SetActionChannel(0, action, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
                    }
                }
                if (craftingAction.startedAt + craftingAction.craftable.CraftTime <= DateTimeOffset.Now.ToUnixTimeSeconds())
                {
                    // Crafting complete bra.
                    bool hasEveryItem = craftingAction.craftable.Receipts.All((r) => persistentEmpireRepresentative.GetInventory().IsInventoryIncludes(r.Item, r.NeededCount));
                    if (!hasEveryItem)
                    {
                        InformationComponent.Instance.SendMessage("You don't have all of the items required.", new Color(1f, 0f, 0f).ToUnsignedInteger(), player);
                        this.craftings.Remove(player);
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CraftingCompleted(player));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                        continue;
                    }
                    List<int> updatedSlots;
                    foreach (CraftingReceipt r in craftingAction.craftable.Receipts)
                    {
                        updatedSlots = persistentEmpireRepresentative.GetInventory().RemoveCountedItemSynced(r.Item, r.NeededCount);
                        foreach (int i in updatedSlots)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                    }
                    


                    if (IsWeapon(craftingAction.craftable))
                    {
                        Debug.Print($"{craftingAction.craftable.CraftableItem} Weapon Rarity Craft");
                        ItemObject itemObject = RarityCraft(persistentEmpireRepresentative.GetSkill(craftingAction.craftable.RelevantSkill).Value, craftingAction.craftable);
                        updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(itemObject, craftingAction.craftable.OutputCount, ItemHelper.GetMaximumAmmo(craftingAction.craftable.CraftableItem));
                        foreach (int i in updatedSlots)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CraftingCompleted(player));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                        if (player.ControlledAgent != null)
                        {

                            player.ControlledAgent.StopUsingGameObjectMT(true);
                            // player.ControlledAgent.StopUsingGameObjectMT(true, true, false);
                            ActionIndexCache ac = ActionIndexCache.act_none;
                            player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);

                        }
                        if (persistentEmpireRepresentative.IncreaseSkilllevel(craftingAction.craftable.RelevantSkill, craftingAction.craftable.RewardedSkillLevel))
                        {
                            Debug.Print("[ERROR] UNABLE TO INCREASE PLAYER SKILL LEVEL!");
                        }
                        SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                        this.craftings.Remove(player);
                    }

                    else if (IsArmour(craftingAction.craftable))
                    {
                        Debug.Print("Armour Craft");
                        ItemObject itemObject = RarityCraft(persistentEmpireRepresentative.GetSkill(craftingAction.craftable.RelevantSkill).Value, craftingAction.craftable);
                        updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(itemObject, craftingAction.craftable.OutputCount, ItemHelper.GetMaximumAmmo(craftingAction.craftable.CraftableItem));
                        foreach (int i in updatedSlots)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CraftingCompleted(player));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                        if (player.ControlledAgent != null)
                        {

                            player.ControlledAgent.StopUsingGameObjectMT(true);
                            // player.ControlledAgent.StopUsingGameObjectMT(true, true, false);
                            ActionIndexCache ac = ActionIndexCache.act_none;
                            player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);

                        }
                        if (persistentEmpireRepresentative.IncreaseSkilllevel(craftingAction.craftable.RelevantSkill, craftingAction.craftable.RewardedSkillLevel))
                        {
                            Debug.Print("[ERROR] UNABLE TO INCREASE PLAYER SKILL LEVEL!");
                        }
                        SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                        this.craftings.Remove(player);
                    }

                    else
                    {
                        Debug.Print($"{craftingAction.craftable.CraftableItem} Non-Rarity Craft");
                        updatedSlots = persistentEmpireRepresentative.GetInventory().AddCountedItemSynced(craftingAction.craftable.CraftableItem, craftingAction.craftable.OutputCount, ItemHelper.GetMaximumAmmo(craftingAction.craftable.CraftableItem));
                        foreach (int i in updatedSlots)
                        {
                            GameNetwork.BeginModuleEventAsServer(player);
                            GameNetwork.WriteMessage(new UpdateInventorySlot("PlayerInventory_" + i, persistentEmpireRepresentative.GetInventory().Slots[i].Item, persistentEmpireRepresentative.GetInventory().Slots[i].Count));
                            GameNetwork.EndModuleEventAsServer();
                        }
                        GameNetwork.BeginBroadcastModuleEvent();
                        GameNetwork.WriteMessage(new CraftingCompleted(player));
                        GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                        if (player.ControlledAgent != null)
                        {

                            player.ControlledAgent.StopUsingGameObjectMT(true);
                            // player.ControlledAgent.StopUsingGameObjectMT(true, true, false);
                            ActionIndexCache ac = ActionIndexCache.act_none;
                            player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);

                        }
                        if (persistentEmpireRepresentative.IncreaseSkilllevel(craftingAction.craftable.RelevantSkill, craftingAction.craftable.RewardedSkillLevel))
                        {
                            Debug.Print("[ERROR] UNABLE TO INCREASE PLAYER SKILL LEVEL!");
                        }
                        SaveSystemBehavior.HandleCreateOrSavePlayer(player);
                        this.craftings.Remove(player);
                    }
                }
            }
#endif
        }
#if SERVER
        public static ItemObject RarityCraft(int skillLevel, Craftable craftableItem)
        {
            float roll = MBRandom.RandomFloatRanged(1);
            Tier craftedTier;

            if (skillLevel > 900)
            {
                if (roll < 0.001) 
                {
                    InformationComponent.Instance.BroadcastAnnouncement("A Mythic Item has been crafted!");
                    craftedTier = Tier.Mythic; 
                }
                else if (roll < 0.15) craftedTier = Tier.Legendary;
                else if (roll < 0.30) craftedTier = Tier.Epic;
                else if (roll < 0.50) craftedTier = Tier.Rare;
                else if (roll < 0.75) craftedTier = Tier.Uncommon;
                else craftedTier = Tier.Common;
            }
            else if (skillLevel > 700)
            {
                if (roll < 0.05) craftedTier = Tier.Legendary;
                else if (roll < 0.15) craftedTier = Tier.Epic;
                else if (roll < 0.30) craftedTier = Tier.Rare;
                else if (roll < 0.60) craftedTier = Tier.Uncommon;
                else craftedTier = Tier.Common;
            }
            else if (skillLevel > 500)
            {
                if (roll < 0.05) craftedTier = Tier.Epic;
                else if (roll < 0.20) craftedTier = Tier.Rare;
                else if (roll < 0.50) craftedTier = Tier.Uncommon;
                else craftedTier = Tier.Common;
            }
            else if (skillLevel > 300)
            {
                if (roll < 0.10) craftedTier = Tier.Rare;
                else if (roll < 0.40) craftedTier = Tier.Uncommon;
                else craftedTier = Tier.Common;
            }
            else if (skillLevel > 100)
            {
                if (roll < 0.20) craftedTier = Tier.Uncommon;
                else craftedTier = Tier.Common;
            }
            else
            {
                craftedTier = Tier.Common;
            }

            string craftableItemId = $"{craftedTier}_{craftableItem.CraftableItem}";
            ItemObject itemDebug2 = MBObjectManager.Instance.GetObject<ItemObject>(craftableItemId);
            if(itemDebug2 == null)

            {
                Debug.Print($"ERROR IN Crafting {craftableItemId} SERIALIZATION ITEM ID NOT FOUND !!!", 0, Debug.DebugColor.Red);
            }

            return itemDebug2;
        }
#endif
        public bool IsWeapon(Craftable craftable)
        {
            return (craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.OneHandedWeapon ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.TwoHandedWeapon ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Polearm ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Bow ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Musket ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Pistol ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Arrows ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Bolts ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Crossbow ||
                craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Shield) &&
                craftable.CraftableItem.StringId != "AvalonHCRP_Grabber" &&
                craftable.CraftableItem.StringId != "pe_banner" &&
                craftable.CraftableItem.StringId != "pe_lockpick";
        }

        public bool IsArmour(Craftable craftable)
        {
            return craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.BodyArmor ||
                   craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.LegArmor ||
                   craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.HeadArmor ||
                   craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.HandArmor ||
                   craftable.CraftableItem.ItemType == ItemObject.ItemTypeEnum.Cape;
        }

        public void OnUsedCrafting(Inventory playerInventory, PE_CraftingStation craftingStation)
        {
            if (this.OnCraftingUse != null)
            {
                this.OnCraftingUse(craftingStation, playerInventory);
            }
        }
        public void AgentRequestCrafting(Agent agent, PE_CraftingStation craftingStation)
        {
            if (!agent.IsPlayerControlled) return;
            NetworkCommunicator peer = agent.MissionPeer?.GetNetworkPeer();
            if (peer == null || !peer.IsConnectionActive)
            {
                Debug.Print("Agent's mission peer is null or disconnected");
                return;
            }
            if (GameNetwork.IsServer)
            {
                PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
                GameNetwork.BeginModuleEventAsServer(peer);
                GameNetwork.WriteMessage(new OpenCraftingStation(craftingStation, persistentEmpireRepresentative.GetInventory()));
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public void AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer networkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient)
            {
                networkMessageHandlerRegisterer.Register<OpenCraftingStation>(this.HandleOpenCraftingStationFromServer);
                networkMessageHandlerRegisterer.Register<CraftingStarted>(this.HandleCraftingStartedFromServer);
                networkMessageHandlerRegisterer.Register<CraftingCompleted>(this.HandleCraftingCompletedFromServer);
                // networkMessageHandlerRegisterer.Register<UpdateCastle>(this.HandleUpdateCastle);
            }
#if SERVER
            else
            {
                networkMessageHandlerRegisterer.Register<RequestSkillLocks>(this.HandleRequestSkillLocks);
                networkMessageHandlerRegisterer.Register<RequestExecuteCraft>(this.HandleRequestExecuteCraftFromClient);

            }
#endif
        }

        private bool HandleRequestSkillLocks(NetworkCommunicator player, RequestSkillLocks message)
        {
            PersistentEmpireRepresentative perp = player.GetComponent<PersistentEmpireRepresentative>();
            if (perp != null)
            {
                perp.LockedSkills[message.Skillname] = !perp.LockedSkills[message.Skillname];
                SaveSystemBehavior.HandleCreateOrSaveSkillLock(player);
                GameNetwork.BeginModuleEventAsServer(player);
                GameNetwork.WriteMessage(new SyncSkillLocks(message.Skillname, perp.LockedSkills[message.Skillname]));
                GameNetwork.EndModuleEventAsServer();
                return true;
            }
            return false;
        }
        private void HandleCraftingCompletedFromServer(CraftingCompleted message)
        {
            // Stop the animation here
            if (message.Player.ControlledAgent != null)
            {
                message.Player.ControlledAgent.StopUsingGameObjectMT(true);
                ActionIndexCache ac = ActionIndexCache.act_none;
                message.Player.ControlledAgent.SetActionChannel(0, ac, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, 0, false, -0.2f, 0, true);
            }
            if (message.Player.IsMine)
            {
                if (this.OnCraftingCompleted != null)
                {
                    this.OnCraftingCompleted();
                }
            }
        }

        private void HandleCraftingStartedFromServer(CraftingStarted message)
        {
            // Start an animation here
            PE_CraftingStation craftingStation = (PE_CraftingStation)message.CraftingStation;
            if (message.Player.IsMine)
            {
                if (this.OnCraftingStarted != null)
                {
                    this.OnCraftingStarted((PE_CraftingStation)message.CraftingStation, message.CraftIndex, message.Player);
                }
            }
        }
#if SERVER
        private bool HandleRequestExecuteCraftFromClient(NetworkCommunicator peer, RequestExecuteCraft message)
        {
            if (peer.ControlledAgent == null) return false;
            PE_CraftingStation craftingStation = (PE_CraftingStation)message.CraftingStation;
            Craftable requestedCraft = craftingStation.Craftables[message.CraftIndex];
            PersistentEmpireRepresentative PEREP = peer.GetComponent<PersistentEmpireRepresentative>();
            if (PEREP == null) return false;
            if (!PEREP.LoadedSkills.ContainsKey(requestedCraft.RelevantSkill))
            {
                Debug.Print($"Skill not valid - {requestedCraft.RelevantSkill}");
            }
            KeyValuePair<string, int> skill = PEREP.GetSkill(requestedCraft.RelevantSkill);
            if (this.craftings.ContainsKey(peer))
            {
                InformationComponent.Instance.SendMessage("You already crafting", new Color(1f, 0, 0).ToUnsignedInteger(), peer);
                return false;
            }

            if (requestedCraft.RequiredSkillLevel > skill.Value)
            {
                InformationComponent.Instance.SendMessage("You are not qualified enough.", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                InformationComponent.Instance.SendMessage($"Required - {requestedCraft.RequiredSkillLevel} You - {skill.Value}", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }

            if (!PEREP.GetInventory().HasEnoughRoomFor(requestedCraft.CraftableItem, requestedCraft.OutputCount))
            {
                InformationComponent.Instance.SendMessage("You have not enough room", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }

            bool hasEveryItem = requestedCraft.Receipts.All((r) => PEREP.GetInventory().IsInventoryIncludes(r.Item, r.NeededCount));
            if (!hasEveryItem)
            {
                InformationComponent.Instance.SendMessage("You don't have all of the items required.", new Color(1f, 0f, 0f).ToUnsignedInteger(), peer);
                return false;
            }
            // Start crafting...
            this.craftings[peer] = new CraftingAction(craftingStation, requestedCraft, DateTimeOffset.Now.ToUnixTimeSeconds());
            if (peer.ControlledAgent != null && craftingStation.Animation != "")
            {
                ActionIndexCache action = ActionIndexCache.Create(craftingStation.Animation);
                peer.ControlledAgent.SetActionChannel(0, action, true, 0UL, 0.0f, 1f, -0.2f, 0.4f, MBRandom.RandomFloatRanged(1f), false, -0.2f, 0, true);
            }
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new CraftingStarted(craftingStation, peer, message.CraftIndex));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            return true;
        }
#endif
        private void HandleOpenCraftingStationFromServer(OpenCraftingStation message)
        {
            this.OnUsedCrafting(message.PlayerInventory, (PE_CraftingStation)message.Station);
        }
    }
}
