using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static System.Collections.Specialized.BitVector32;

namespace PersistentEmpiresLib
{
    public class PersistentEmpireRepresentative : MissionRepresentativeBase
    {
        public bool DebugMode = false;
        private House _house;
        private Faction _playerFaction;
        private int _factionIndex = -1;
        private string _classId = "pe_peasant";
        private Inventory playerInventory;
        private int hunger = 0;
        private PE_SpawnFrame nextSpawnFrame = null;
        public Timer SpawnTimer;
        public int LoadedHealth = 100;
        public bool IsAdmin = false;
        public bool IsFirstAgentSpawned = false; // To check if player's initial agent is spawned on connection
        public bool KickedFromFaction = false;
        public long DisconnectedAt = 0;
        public bool LoadFromDb = false;
        public Vec3 LoadedDbPosition;
        public Equipment LoadedSpawnEquipment;
        public bool IsLordClass = false;
        public int SkillCap = 1200;
        public Dictionary<string, int> LoadedSkills = new Dictionary<string, int>();
        public Dictionary<string,bool> LockedSkills = new Dictionary<string, bool>();

        public int[] LoadedAmmo { get; set; }

        public PersistentEmpireRepresentative()
        {
            playerInventory = new Inventory(5, 10, "PlayerInventory");
            hunger = 100;
            this.SpawnTimer = new Timer(Mission.Current.CurrentTime, 3f, false);
        }


        public KeyValuePair<string, int> GetSkill(string skillName)
        {
            foreach (var skill in LoadedSkills)
            {
                if (skill.Key == skillName)
                {
                    return skill;
                }
            }
            return new KeyValuePair<string, int>();
        }

        float GetDiminishingFactor(int skillLevel)
        {
            float maxSkill = (float)SkillCap; // Maximum skill level
            float minFactor = 0.20f;  // Minimum factor (1% of skill gained)

            // Calculate the diminishing factor with a minimum threshold
            float diminishingFactor2 = 1.0f - (float)Math.Pow(skillLevel / maxSkill, 0.5f);

            // Ensure it does not drop below the minimum factor
            return Math.Max(diminishingFactor2, minFactor);
        }

        public KeyValuePair<string, int> GetRandomSkillExcluding(string excludeSkillName)
        {
            // Check if LoadedSkills is empty
            if (LoadedSkills.Count == 0)
            {
                return new KeyValuePair<string, int>("", 0);
            }

            // Filter out the skill that matches the exclusion name, those with a value of 0, and locked skills
            var eligibleSkills = LoadedSkills
                .Where(x => x.Key != excludeSkillName && x.Value > 0 && (!LockedSkills.ContainsKey(x.Key) || !LockedSkills[x.Key]))
                .ToList();

            // Return default if no eligible skill is found
            if (eligibleSkills.Count == 0)
            {
                return new KeyValuePair<string, int>("", 0);
            }

            // Select a random skill from the eligible skills
            Random random = new Random();
            int randomIndex = random.Next(eligibleSkills.Count);
            return eligibleSkills[randomIndex];
        }

        public void DisplaySkillmessage(string skillName, int amount, bool increase)
        {
            if (increase)
            {
                InformationComponent.Instance.SendMessage($"{skillName}: {LoadedSkills[skillName]} + {amount}", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());
            }
            else
            {
                InformationComponent.Instance.SendMessage($"{skillName}: {LoadedSkills[skillName]} - {amount}", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
            }
        }

        public bool IncreaseSkilllevel(string skillName, int amount)
        {
            if (!this.LoadedSkills.ContainsKey(skillName)) return false;
            KeyValuePair<string, int> skill = GetSkill(skillName);
            float diminishingFactor = GetDiminishingFactor(skill.Value);
            int GainAmount = (int)(amount * diminishingFactor);
            if (GainAmount <= 0) GainAmount = 1;
            int totalSkills = 0;

            foreach (int skillValue in LoadedSkills.Values)
            {
                totalSkills += skillValue;
            }

            if (LoadedSkills[skillName] >= 1000)
            {
                LoadedSkills[skillName] = 1000;
                this.SyncCraftingStats(skillName);
            }

            totalSkills = 0;
            foreach (int skillValue in LoadedSkills.Values)
            {
                totalSkills += skillValue;
            }

            //Correct for Previous Bad Code
            if (totalSkills > SkillCap)
            {
                InformationComponent.Instance.SendMessage($"You are over the skill cap, the system will now correct this issue!", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());
                while (totalSkills > SkillCap)
                {
                    totalSkills = 0;
                    foreach (int skillValue in LoadedSkills.Values)
                    {
                        totalSkills += skillValue;
                    }
                    KeyValuePair<string, int>  SkillToDrop = GetRandomSkillExcluding(skillName);
                    if (SkillToDrop.Key == "") break;
                    int AmountToDrop = totalSkills - SkillCap;
                    if (AmountToDrop > LoadedSkills[SkillToDrop.Key])
                    {
                        LoadedSkills[SkillToDrop.Key] = SkillToDrop.Value - SkillToDrop.Value;
                        this.SyncCraftingStats(SkillToDrop.Key);
                        DisplaySkillmessage(SkillToDrop.Key, SkillToDrop.Value, false);
                    }
                    else
                    {
                        LoadedSkills[SkillToDrop.Key] = SkillToDrop.Value - AmountToDrop;
                        this.SyncCraftingStats(SkillToDrop.Key);
                        DisplaySkillmessage(SkillToDrop.Key, AmountToDrop, false);
                    }

                }
            }

            //Increase Skill Level
            if (LockedSkills.ContainsKey(skillName) && LockedSkills[skillName]) return false;
            if (LoadedSkills[skillName] == 1000) return false;
            totalSkills = 0;
            foreach (int skillValue in LoadedSkills.Values)
            {
                totalSkills += skillValue;
            }
            if (totalSkills + GainAmount > SkillCap)
            {
                if (GetRandomSkillExcluding(skillName).Key == "")
                {
                    InformationComponent.Instance.SendMessage("All Skills are locked or at 0, Unable to increase skill points!", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
                    return false;
                }
                LoadedSkills[skillName] = LoadedSkills[skillName] + GainAmount;
                this.SyncCraftingStats(skillName);
                DisplaySkillmessage(skillName, GainAmount, true);
                int DropAmount = GainAmount;

                while (DropAmount > 0)
                {
                    KeyValuePair<string, int> SkillToDrop = GetRandomSkillExcluding(skillName);
                    if (SkillToDrop.Key == "") break;
                    
                    if (LoadedSkills[SkillToDrop.Key] < DropAmount)
                    {
                        DropAmount = DropAmount - LoadedSkills[SkillToDrop.Key];
                        DisplaySkillmessage(SkillToDrop.Key, LoadedSkills[SkillToDrop.Key], false);
                        LoadedSkills[SkillToDrop.Key] = 0;
                        this.SyncCraftingStats(SkillToDrop.Key);
                    }
                    else
                    {
                        LoadedSkills[SkillToDrop.Key] = LoadedSkills[SkillToDrop.Key] - DropAmount;
                        DisplaySkillmessage(SkillToDrop.Key, DropAmount, false);
                        this.SyncCraftingStats(SkillToDrop.Key);
                        DropAmount = 0;
                    }
                }
                return true;
                


            }
            else
            {
                LoadedSkills[skillName] = LoadedSkills[skillName] + GainAmount;
                this.SyncCraftingStats(skillName);
                DisplaySkillmessage(skillName, GainAmount, true);
                return true;
            }




        }


        public bool IncreaseSkilllevel2(string skillName, int amount)
        {
            if (this.LoadedSkills.ContainsKey(skillName))
            {
                try
                {
                    KeyValuePair<string, int> skill = GetSkill(skillName);
                    int currentSkill = skill.Value;

                    // Apply diminishing returns
                    float diminishingFactor = GetDiminishingFactor(currentSkill);
                    amount = (int)(amount * diminishingFactor);
                    if (amount <= 0)
                    {
                        amount = 1;
                    }
                    int totalSkills = 0;

                    foreach (int skillValue in LoadedSkills.Values)
                    {
                        totalSkills += skillValue;
                    }
                    int newSkill = currentSkill + amount;



                    // Ensure the skill level does not exceed SkillCap
                    if (totalSkills + amount <= SkillCap)
                    {
                        LoadedSkills[skillName] = newSkill;
                        this.SyncCraftingStats(skillName);
                        InformationComponent.Instance.SendMessage($"{skillName}: {LoadedSkills[skillName]} + {amount}", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());
                        return true;
                    }
                    else
                    {
                        KeyValuePair<string, int> droppedSkill = GetRandomSkillExcluding(skillName);
                        LoadedSkills[skillName] = newSkill;
                        this.SyncCraftingStats(skillName);
                        InformationComponent.Instance.SendMessage($"{skillName}: {LoadedSkills[skillName]} + {amount}", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());
                        LoadedSkills[droppedSkill.Key] = droppedSkill.Value - amount;
                        if (LoadedSkills[droppedSkill.Key] < 0) LoadedSkills[droppedSkill.Key] = 0;
                        this.SyncCraftingStats(droppedSkill.Key);
                        if (droppedSkill.Value > 0)
                            InformationComponent.Instance.SendMessage($"{droppedSkill.Key}: {LoadedSkills[droppedSkill.Key]} - {amount}", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
                        else
                            InformationComponent.Instance.SendMessage($"{droppedSkill.Key}: {LoadedSkills[droppedSkill.Key]}", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());


                        totalSkills = 0;
                        foreach (int skillValue in LoadedSkills.Values)
                        {
                            totalSkills += skillValue;
                        }

                        if (totalSkills > SkillCap)
                        {
                            InformationComponent.Instance.SendMessage($"Unfortunatly, your skills have exceeded the Maximum, the system will now correct them!", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
                            KeyValuePair<string, int> droppedSkill2 = GetRandomSkillExcluding(skillName);
                            int Amounttodrop = totalSkills - SkillCap;
                            if (Amounttodrop > LoadedSkills[droppedSkill2.Key])
                            {
                                LoadedSkills[droppedSkill2.Key] = droppedSkill2.Value - droppedSkill2.Value;
                                this.SyncCraftingStats(droppedSkill2.Key);
                                InformationComponent.Instance.SendMessage($"{droppedSkill2.Key}: {LoadedSkills[droppedSkill2.Key]} - {droppedSkill2.Value}", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
                            }
                            else
                            {
                                LoadedSkills[droppedSkill2.Key] = droppedSkill2.Value - Amounttodrop;
                                this.SyncCraftingStats(droppedSkill2.Key);
                                InformationComponent.Instance.SendMessage($"{droppedSkill2.Key}: {LoadedSkills[droppedSkill2.Key]} - {Amounttodrop}", Color.ConvertStringToColor("#FF5733FF").ToUnsignedInteger(), this.GetNetworkPeer());
                            }

                        }


                        return true;
                    }

                }
                catch (Exception e)
                {
                    Debug.Print("Error with crafting skill gain");
                }
            }
            return false;
        }



        public PE_SpawnFrame GetNextSpawnFrame()
        {
            return this.nextSpawnFrame;
        }

        public List<PE_SpawnFrame> GetSpawnableCastleFrames()
        {
            List<PE_SpawnFrame> liste = new List<PE_SpawnFrame>();
            if (this.GetFaction() != null)
            {
                //Faction f = this.GetFaction();
                List<PE_CastleBanner> castleBanners;
                castleBanners = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_CastleBanner>().Select(g => g.GetFirstScriptOfType<PE_CastleBanner>()).Where(c => c.FactionIndex == this.GetFactionIndex()).ToList();
                foreach (PE_CastleBanner castleBanner in castleBanners)
                {
                    List<PE_SpawnFrame> spawnFrame = Mission.Current.GetActiveEntitiesWithScriptComponentOfType<PE_SpawnFrame>().Select(g => g.GetFirstScriptOfType<PE_SpawnFrame>()).Where(frame => frame.CastleIndex == castleBanner.CastleIndex).ToList();

                    liste.AddRange(spawnFrame);
                }
            }
            return liste;
        }

        public void SetSpawnFrame(PE_SpawnFrame frame)
        {
            this.nextSpawnFrame = frame;

        }

        public House GetHouse()
        {
            return this._house;
        }

        public bool SetHouse(House house)
        {
            if (_house == null)
            {
                this._house = house;
                this._house.lordId = this.GetNetworkPeer().VirtualPlayer.Id.ToString();
                return true;
            }
            else
            {
                InformationComponent.Instance.SendMessage("You already have a house", Color.ConvertStringToColor("#F44336FF").ToUnsignedInteger(), this.GetNetworkPeer());
                return false;
            }
        }

        public int GetHunger()
        {
            return this.hunger;
        }
        public void SetHunger(int hunger)
        {
            this.hunger = hunger;
            if (this.hunger < 0) this.hunger = 0;
            if (this.hunger > 100) this.hunger = 100;
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new SetHunger(this.hunger));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public Inventory GetInventory()
        {
            return this.playerInventory;
        }
        public void SetInventory(Inventory inventory)
        {
            this.playerInventory = inventory;
        }

        public Faction GetFaction()
        {
            return this._playerFaction;
        }
        public int GetFactionIndex()
        {
            return this._factionIndex;
        }
        public string GetClassId()
        {
            return this._classId;
        }
        public void SetClass(string classId)
        {
            this._classId = classId;
        }
        public void SetFaction(Faction f, int factionIndex)
        {
            this._playerFaction = f;
            this._factionIndex = factionIndex;
            if (this._playerFaction != null)
            {
                this.MissionPeer.Team = this._playerFaction.team;
            }
        }

        public string SerializeCraftingStats()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var skill in LoadedSkills)
            {
                sb.Append(skill.Key + "*" + skill.Value + "=");
            }
            return sb.ToString();
        }

        public void SyncCraftingStats(string skill)
        {
            if (GameNetwork.IsServer)
            {
                foreach(NetworkCommunicator peer in GameNetwork.NetworkPeers)
                {
                    if (Main.IsPlayerAdmin(peer))
                    {
                        GameNetwork.BeginModuleEventAsServer(peer);
                        GameNetwork.WriteMessage(new SendPlayerStatsToClient(this.SerializeCraftingStats(), this.GetNetworkPeer(), true));
                        GameNetwork.EndModuleEventAsServer();
                    }
                }
                int amount = LoadedSkills[skill];
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new SyncCraftingStats(skill, amount));
                GameNetwork.EndModuleEventAsServer();
            }
        }
        public void SetGold(int newGold)
        {
            this.UpdateGold(newGold);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new SyncGold(newGold));
                GameNetwork.EndModuleEventAsServer();
            }
        }

        public bool ReduceIfHaveEnoughGold(int requiredGold)
        {
            if (requiredGold > this.Gold) return false;
            this.GoldLost(requiredGold);
            return true;
        }
        public bool HaveEnoughGold(int requiredGold)
        {
            if (this.Gold < requiredGold) return false;
            return true;
        }
        public void GoldGain(int goldGain)
        {
            this.UpdateGold(this.Gold + goldGain);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new PEGoldGain(goldGain));
                GameNetwork.EndModuleEventAsServer();
            }
            if (GameNetwork.IsClient)
            {
                SoundEvent.CreateEventFromString("event:/ui/notification/coins_positive", Mission.Current.Scene).Play();
            }
        }
        public void GoldLost(int goldLost)
        {
            int newGold = this.Gold - goldLost;

            this.UpdateGold(newGold > 0 ? newGold : 0);
            if (GameNetwork.IsServer)
            {
                GameNetwork.BeginModuleEventAsServer(this.GetNetworkPeer());
                GameNetwork.WriteMessage(new PEGoldLost(goldLost));
                GameNetwork.EndModuleEventAsServer();
            }
            if (GameNetwork.IsClient)
            {
                SoundEvent.CreateEventFromString("event:/ui/notification/coins_negative", Mission.Current.Scene).Play();
            }
        }
    }

}
