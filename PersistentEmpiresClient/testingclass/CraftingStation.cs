using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
    public struct CraftingReceipt
    {
        public ItemObject Item;
        public int NeededCount;
        public CraftingReceipt(String itemId, int neededCount)
        {
            this.Item = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.NeededCount = neededCount;
        }
    }
    public struct Craftable
    {
        public List<CraftingReceipt> Receipts;
        public int OutputCount;
        public ItemObject CraftableItem;
        public int RewardedSkillLevel;
        public int RequiredSkillLevel;
        public int CraftTime;
        public string RelevantSkill;
        public Craftable(List<CraftingReceipt> receipts, String itemId, int outputCount, int craftTime, string relevantSkill, int rewardedskilllevel, int requiredskilllevel)
        {
            this.Receipts = receipts;
            this.OutputCount = outputCount;
            this.CraftableItem = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
            this.RewardedSkillLevel = rewardedskilllevel;
            this.RequiredSkillLevel = requiredskilllevel;
            this.CraftTime = craftTime;
            this.RelevantSkill = relevantSkill;
        }
    }
    public class PE_CraftingStation : PE_UsableFromDistance
    {
        public string StationName = "Carpenter Bench";
        public string ModuleFolder = "PersistentEmpires";
        public CraftingComponent craftingComponent { get; private set; }
        public PE_UpgradeableBuildings upgradeableBuilding { get; private set; }
        private PlayerInventoryComponent playerInventoryComponent;
        public List<Craftable> Craftables { get; private set; }
        public string Animation = "";
        public string CraftingRecieptTag = "";
        private string Craftings = "";

        public List<Craftable> ParseStringToCraftables(string allCraftableReceipt)
        {
            List<Craftable> craftables = new List<Craftable>();

            if (string.IsNullOrEmpty(allCraftableReceipt)) return craftables;

            foreach (string recipie in allCraftableReceipt.Split('|'))
            {
                if (string.IsNullOrWhiteSpace(recipie)) continue;

                string[] parts = recipie.Split('=');
                string ResultSide = parts[0];
                string RecipieSide = parts[1];
                string SkillSide = parts[2];

                if (ResultSide == null || RecipieSide == null || SkillSide == null)
                {
                    Debug.Print($"ERROR IN Crafting {CraftingRecieptTag} SERIALIZATION INVALID !!!", 0, Debug.DebugColor.Red);
                }


                // Input Side
                List<CraftingReceipt> CraftingRecipie = new List<CraftingReceipt>();
                foreach (string r in RecipieSide.Split(','))
                {
                    string[] rParts = r.Split('*');
                    string itemId = rParts[0];
                    int count = int.Parse(rParts[1]);

                    CraftingRecipie.Add(new CraftingReceipt(itemId, count));
                    ItemObject itemDebug = MBObjectManager.Instance.GetObject<ItemObject>(itemId);
                    if (itemDebug == null)
                    {
                        Debug.Print($"ERROR IN Crafting {CraftingRecieptTag} SERIALIZATION ITEM ID {itemId} NOT FOUND !!!", 0, Debug.DebugColor.Red);
                    }
                }
                // OutputSide
                string[] leftParts = ResultSide.Split('*');
                int craftTime = int.Parse(leftParts[0]);
                string craftableItemId = leftParts[1];

                ItemObject itemDebug2 = MBObjectManager.Instance.GetObject<ItemObject>(craftableItemId);
                if (itemDebug2 == null)
                {
                    Debug.Print($"ERROR IN Crafting {CraftingRecieptTag} SERIALIZATION ITEM ID {craftableItemId} NOT FOUND !!!", 0, Debug.DebugColor.Red);
                }

                int outputAmount = int.Parse(leftParts[2]);


                //SkillSide
                string[] skillParts = SkillSide.Split('*');
                string skillId = skillParts[1];
                int skillLevel = int.Parse(skillParts[0]);
                int skillXP = int.Parse(skillParts[2]);
                if (skillId == null)
                {
                    Debug.Print($"ERROR IN Crafting {CraftingRecieptTag} {craftableItemId} Skill ID Invalid !!!", 0, Debug.DebugColor.Red);
                }

                Craftable craftable = new Craftable(CraftingRecipie, craftableItemId, outputAmount, craftTime, skillId, skillXP, skillLevel);
                craftables.Add(craftable);
            }
            return craftables;
        }


        public void LoadCraftables()
        {
            List<Craftable> tier1Crafts = this.ParseStringToCraftables(this.Craftings);
            this.Craftables = new List<Craftable>();
            this.Craftables.AddRange(tier1Crafts);
        }
        protected override void OnInit()
        {
            base.OnInit();
            // Set OnHover Variables
            Debug.Print("[Feudal Kingdoms] Initiating Crafting Station With " + this.ModuleFolder + " Module", 0, Debug.DebugColor.DarkCyan);
            TextObject actionMessage = new TextObject("Use {Station} To Craft");
            actionMessage.SetTextVariable("Station", this.StationName);
            base.ActionMessage = actionMessage;
            TextObject descriptionMessage = new TextObject("Press {KEY} To Interact");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;

            // Load Craftables
            string xmlPath = ModuleHelper.GetXmlPath(this.ModuleFolder, "CraftingRecipies/" + this.CraftingRecieptTag);
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.Load(xmlPath);
            foreach (XmlNode node in xmlDocument.DocumentElement.ChildNodes)
            {
                if (node.Name == "Craftings") this.Craftings = node.InnerText.Trim();
            }


            this.playerInventoryComponent = Mission.Current.GetMissionBehavior<PlayerInventoryComponent>();
            this.craftingComponent = Mission.Current.GetMissionBehavior<CraftingComponent>();
            this.LoadCraftables();
        }

        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Crafting Station Named As " + this.StationName;
        }



        public override void OnUse(Agent userAgent)
        {
            Debug.Print("[USING LOG] AGENT USE " + this.GetType().Name);
            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);
            userAgent.StopUsingGameObjectMT(true);
            if (GameNetwork.IsServer)
            {
                this.craftingComponent.AgentRequestCrafting(userAgent, this);
            }

        }
    }
}
