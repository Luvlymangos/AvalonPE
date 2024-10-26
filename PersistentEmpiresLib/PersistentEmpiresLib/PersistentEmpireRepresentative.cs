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
        public int SkillCap = 1000;
        public Dictionary<string, int> LoadedSkills = new Dictionary<string, int>();


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
        //TODO: Crashes Server
        public KeyValuePair<string, int> GetNextHighestSkill()
        {
            // Check if LoadedSkills is empty
            if (LoadedSkills.Count == 0)
            {
                // Return a default value or throw an exception based on your needs
                return new KeyValuePair<string, int>("", 0);
            }

            // Find the maximum skill
            var maximum = LoadedSkills.Aggregate((l, r) => l.Value > r.Value ? l : r);

            // Find the second maximum skill safely
            var secondMaximum = LoadedSkills
                .Where(x => x.Value < maximum.Value)
                .DefaultIfEmpty(new KeyValuePair<string, int>("", 0)) // Provide a default if no second max is found
                .Aggregate((l, r) => l.Value > r.Value ? l : r);

            return secondMaximum;
        }

        public bool IncreaseSkilllevel(string skillName, int amount)
        {
            if (this.LoadedSkills.ContainsKey(skillName))
            {
                KeyValuePair<string, int> skill = GetSkill(skillName);
                int currentSkill = skill.Value;
                // Define the diminishing returns function
                float GetDiminishingFactor(int skillLevel)
                {
                    float maxSkill = 1000.0f; // Maximum skill level
                    float minFactor = 0.20f;  // Minimum factor (1% of skill gained)

                    // Calculate the diminishing factor with a minimum threshold
                    float diminishingFactor2 = 1.0f - (float)Math.Pow(skillLevel / maxSkill, 0.5f);

                    // Ensure it does not drop below the minimum factor
                    return Math.Max(diminishingFactor2, minFactor);
                }

                // Apply diminishing returns
                float diminishingFactor = GetDiminishingFactor(currentSkill);
                amount = (int)(amount * diminishingFactor);
                if (amount<= 0)
                {
                    amount = 1;
                }
                int newSkill = currentSkill + amount;

                // Ensure the skill level does not exceed SkillCap
                if (newSkill <= SkillCap)
                {
                    LoadedSkills[skillName] = newSkill;
                    this.SyncCraftingStats(skillName);
                    InformationComponent.Instance.SendMessage($"{skillName}: {LoadedSkills[skillName]} + {amount}", Color.ConvertStringToColor("#4CAF50FF").ToUnsignedInteger(), this.GetNetworkPeer());
                    return true;
                }
                else
                {
                    KeyValuePair<string, int> droppedSkill = GetNextHighestSkill();
                    {
                        newSkill = SkillCap;
                    }
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
                    return true;
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

        public void SyncCraftingStats(string skill)
        {
            if (GameNetwork.IsServer)
            {
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
