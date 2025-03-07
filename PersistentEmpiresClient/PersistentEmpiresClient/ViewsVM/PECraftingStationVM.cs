﻿using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM.CraftingStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core.ViewModelCollection.Selector;
using TaleWorlds.Library;
using PersistentEmpiresLib;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.NetworkMessages.Client;

namespace PersistentEmpires.Views.ViewsVM
{
    public class PECraftingStationVM : ViewModel
    {
        private PEInventoryVM _playerInventory;
        private MBBindingList<PECraftingStationItemVM> _craftingList;
        private Action<PECraftingStationItemVM> _craft;
        private string _filter;
        private MBBindingList<PECraftingStationItemVM> _filteredItemList;
        private bool _isCrafting;
        private int _craftingDuration;
        private int _pastDuration;
        private Action<PEItemVM> _handleClickItem;
        private SelectorVM<SelectorItemVM> _cultureFilter;
        private SelectorVM<SelectorItemVM> _tierFilter;
        private SelectorVM<SelectorItemVM> _itemCategoryFilter;
        private int _weavinglevel;
        private int _weaponlevel;
        private int _armourlevel;
        private int _smithinglevel;
        private int _carplevel;
        private int _cookinglevel;
        private int _farminglevel;
        private int _mininglevel;
        private int _fletchinglevel;
        private int _animallevel;
        private int _totallevel;
        private string _weavinglock;
        private string _weaponlock;
        private string _armourlock;
        private string _smithinglock;
        private string _carplock;
        private string _cookinglock;
        private string _farminglock;
        private string _mininglock;
        private string _fletchinglock;
        private string _animalslock;
        public int TestLevel;
        public string TestLock;
        public PECraftingStationVM(Action<PEItemVM> handleClickItem) {
            this._handleClickItem = handleClickItem;
        }
        public void RefreshValues(PE_CraftingStation craftingStation, Inventory inventory, Action<PECraftingStationItemVM> craft)
        {
            this.CraftingList = new MBBindingList<PECraftingStationItemVM>();
            this.PlayerInventory = new PEInventoryVM(this._handleClickItem);
            this.PlayerInventory.SetItems(inventory);
            List<string> classTypes = new List<string>();
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            for (int i = 0; i < craftingStation.Craftables.Count; i++) {
                Craftable craftable = craftingStation.Craftables[i];
                // craftable.CraftableItem.Type
                MBBindingList<PECraftingReceiptVM> craftingReceipts = new MBBindingList<PECraftingReceiptVM>();
                foreach(CraftingReceipt receipt in craftable.Receipts)
                {
                    craftingReceipts.Add(new PECraftingReceiptVM(receipt.Item, receipt.NeededCount));
                }
                this.CraftingList.Add(new PECraftingStationItemVM(craftable.CraftableItem, craftable.OutputCount, craftable.RequiredSkillLevel, myRepresentative.GetSkill(craftable.RelevantSkill).Value, craftingReceipts, this.ExecuteCraft, i, craftable.CraftTime, craftable.RelevantSkill));
            }
            
            this._craft = craft;

            List<string> cultureFilterList = new List<string>();
            cultureFilterList.Add("All");
            cultureFilterList.Add("Vlandia");
            cultureFilterList.Add("Khuzait");
            cultureFilterList.Add("Aserai");
            cultureFilterList.Add("Sturgia");
            cultureFilterList.Add("Battania");
            cultureFilterList.Add("Empire");
            cultureFilterList.Add("Neutral Culture");
            this.CultureFilter = new SelectorVM<SelectorItemVM>(cultureFilterList, 0, this.OnCultureFilterSelected);

            List<string> tierFilterList = new List<string>();
            tierFilterList.Add("All");
            tierFilterList.Add("Tier 1");
            tierFilterList.Add("Tier 2");
            tierFilterList.Add("Tier 3");
            this.TierFilter = new SelectorVM<SelectorItemVM>(tierFilterList, 0, this.OnTierFilterSelected);


            List<string> itemCategoryList = new List<string>();
            itemCategoryList.Add("All");
            itemCategoryList.Add("Invalid");
            itemCategoryList.Add("Horse");
            itemCategoryList.Add("OneHandedWeapon");
            itemCategoryList.Add("TwoHandedWeapon");
            itemCategoryList.Add("Polearm");
            itemCategoryList.Add("Arrows");
            itemCategoryList.Add("Bolts");
            itemCategoryList.Add("Shield");
            itemCategoryList.Add("Bow");
            itemCategoryList.Add("Crossbow");
            itemCategoryList.Add("Thrown");
            itemCategoryList.Add("Goods");
            itemCategoryList.Add("HeadArmor");
            itemCategoryList.Add("BodyArmor");
            itemCategoryList.Add("LegArmor");
            itemCategoryList.Add("HandArmor");
            itemCategoryList.Add("Pistol");
            itemCategoryList.Add("Musket");
            itemCategoryList.Add("Bullets");
            itemCategoryList.Add("Animal");
            itemCategoryList.Add("Book");
            itemCategoryList.Add("ChestArmor");
            itemCategoryList.Add("Cape");
            itemCategoryList.Add("HorseHarness");
            itemCategoryList.Add("Banner");
            this.ItemCategoryFilter = new SelectorVM<SelectorItemVM>(itemCategoryList, 0, this.OnCategoryFilterSelected);

        }

        private void OnCategoryFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            base.OnPropertyChanged("FilteredItemList");
        }

        private void OnTierFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            base.OnPropertyChanged("FilteredItemList");
        }

        private void OnCultureFilterSelected(SelectorVM<SelectorItemVM> obj)
        {
            base.OnPropertyChanged("FilteredItemList");
        }

        private void ExecuteCraft(PECraftingStationItemVM obj)
        {
            this._craft(obj); 
        }

        public void ExecuteWeaving()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Weaving"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteWeapon()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("WeaponSmithing"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteArmour()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("ArmourSmithing"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteSmithing()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("BlackSmithing"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteCarpentry()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Carpentry"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteCooking()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Cooking"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteFarming()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Farming"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteMining()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Mining"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteFletching()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Fletching"));
            GameNetwork.EndModuleEventAsClient();
        }

        public void ExecuteAnimals()
        {
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestSkillLocks("Animals"));
            GameNetwork.EndModuleEventAsClient();
        }







        [DataSourceProperty]
        public int PastDuration
        {
            get => this._pastDuration;
            set
            {
                if(value != this._pastDuration)
                {
                    if(value == 0)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Crafting completed !", new Color(0,1f,0)));
                    } 
                    this._pastDuration = value;
                    base.OnPropertyChangedWithValue(value, "PastDuration");
                }
            }
        }
        [DataSourceProperty]
        public int CraftingDuration
        {
            get => this._craftingDuration;
            set
            {
                if(value != this._craftingDuration)
                {
                    this._craftingDuration = value;
                    base.OnPropertyChangedWithValue(value, "CraftingDuration");
                }
            }
        }
        [DataSourceProperty]
        public bool IsCrafting {
            get => this._isCrafting;
            set
            {
                if(value != this._isCrafting)
                {
                    this._isCrafting = value;
                    base.OnPropertyChangedWithValue(value, "IsCrafting");
                }
            }
        }

        [DataSourceProperty]
        public int WeavingLevel
        {
            get => this._weavinglevel;
            set
            {
                if (value != this._weavinglevel)
                {
                    this._weavinglevel = value;
                    base.OnPropertyChangedWithValue(value, "WeavingLevel");
                }
            }
        }

        [DataSourceProperty]
        public int WeaponLevel
        {
            get => this._weaponlevel;
            set
            {
                if (value != this._weaponlevel)
                {
                    this._weaponlevel = value;
                    base.OnPropertyChangedWithValue(value, "WeaponLevel");
                }
            }
        }

        [DataSourceProperty]
        public int ArmourLevel
        {
            get => this._armourlevel;
            set
            {
                if (value != this._armourlevel)
                {
                    this._armourlevel = value;
                    base.OnPropertyChangedWithValue(value, "ArmourLevel");
                }
            }
        }

        [DataSourceProperty]
        public int SmithingLevel
        {
            get => this._smithinglevel;
            set
            {
                if (value != this._smithinglevel)
                {
                    this._smithinglevel = value;
                    base.OnPropertyChangedWithValue(value, "SmithingLevel");
                }
            }
        }

        [DataSourceProperty]
        public int CarpLevel
        {
            get => this._carplevel;
            set
            {
                if (value != this._carplevel)
                {
                    this._carplevel = value;
                    base.OnPropertyChangedWithValue(value, "CarpLevel");
                }
            }
        }

        [DataSourceProperty]
        public int TotalLevel
        {
            get => this._totallevel;
            set
            {
                if (value != this._totallevel)
                {
                    this._totallevel = value;
                    base.OnPropertyChangedWithValue(value, "TotalLevel");
                }
            }
        }
        

        [DataSourceProperty]
        public int CookingLevel
        {
            get => this._cookinglevel;
            set
            {
                if (value != this._cookinglevel)
                {
                    this._cookinglevel = value;
                    base.OnPropertyChangedWithValue(value, "CookingLevel");
                }
            }
        }

        [DataSourceProperty]
        public int FarmingLevel
        {
            get => this._farminglevel;
            set
            {
                if (value != this._farminglevel)
                {
                    this._farminglevel = value;
                    base.OnPropertyChangedWithValue(value, "FarmingLevel");
                }
            }
        }

        [DataSourceProperty]
        public int MiningLevel
        {
            get => this._mininglevel;
            set
            {
                if (value != this._mininglevel)
                {
                    this._mininglevel = value;
                    base.OnPropertyChangedWithValue(value, "MiningLevel");
                }
            }
        }

        [DataSourceProperty]
        public int FletchingLevel
        {
            get => this._fletchinglevel;
            set
            {
                if (value != this._fletchinglevel)
                {
                    this._fletchinglevel = value;
                    base.OnPropertyChangedWithValue(value, "FletchingLevel");
                }
            }
        }

        [DataSourceProperty]
        public int AnimalLevel
        {
            get => this._animallevel;
            set
            {
                if (value != this._animallevel)
                {
                    this._animallevel = value;
                    base.OnPropertyChangedWithValue(value, "AnimalsLevel");
                }
            }
        }

        [DataSourceProperty]
        public string WeaponLock
        {
            get => this._weaponlock;
            set
            {
                if (value != this._weaponlock)
                {
                    this._weaponlock = value;
                    base.OnPropertyChangedWithValue(value, "WeaponLock");
                }
            }
        }

        [DataSourceProperty]
        public string WeavingLock
        {
            get => this._weavinglock;
            set
            {
                if (value != this._weavinglock)
                {
                    this._weavinglock = value;
                    base.OnPropertyChangedWithValue(value, "WeavingLock");
                }
            }
        }

        public string ArmourLock
        {
            get => this._armourlock;
            set
            {
                if (value != this._armourlock)
                {
                    this._armourlock = value;
                    base.OnPropertyChangedWithValue(value, "ArmourLock");
                }
            }
        }

        public string SmithingLock
        {
            get => this._smithinglock;
            set
            {
                if (value != this._smithinglock)
                {
                    this._smithinglock = value;
                    base.OnPropertyChangedWithValue(value, "SmithingLock");
                }
            }
        }

        public string CarpLock
        {
            get => this._carplock;
            set
            {
                if (value != this._carplock)
                {
                    this._carplock = value;
                    base.OnPropertyChangedWithValue(value, "CarpLock");
                }
            }
        }

        public string CookingLock
        {
            get => this._cookinglock;
            set
            {
                if (value != this._cookinglock)
                {
                    this._cookinglock = value;
                    base.OnPropertyChangedWithValue(value, "CookingLock");
                }
            }
        }

        public string FarmingLock
        {
            get => this._farminglock;
            set
            {
                if (value != this._farminglock)
                {
                    this._farminglock = value;
                    base.OnPropertyChangedWithValue(value, "FarmingLock");
                }
            }
        }

        public string MiningLock
        {
            get => this._mininglock;
            set
            {
                if (value != this._mininglock)
                {
                    this._mininglock = value;
                    base.OnPropertyChangedWithValue(value, "MiningLock");
                }
            }
        }

        public string FletchingLock
        {
            get => this._fletchinglock;
            set
            {
                if (value != this._fletchinglock)
                {
                    this._fletchinglock = value;
                    base.OnPropertyChangedWithValue(value, "FletchingLock");
                }
            }
        }

        public string AnimalLock
        {
            get => this._animalslock;
            set
            {
                if (value != this._animalslock)
                {
                    this._animalslock = value;
                    base.OnPropertyChangedWithValue(value, "AnimalsLock");
                }
            }
        }





        [DataSourceProperty]
        public string Filter
        {
            get => this._filter;
            set
            {
                if (value != this._filter)
                {
                    this._filter = value;
                    base.OnPropertyChangedWithValue(value, "Filter");
                    if (value != null || value != "")
                    {
                        this._filteredItemList = new MBBindingList<PECraftingStationItemVM>();
                        foreach (PECraftingStationItemVM i in this._craftingList)
                        {
                            if (i.CraftableName.ToLower().Contains(this.Filter.ToLower())) this._filteredItemList.Add(i);
                        }
                        base.OnPropertyChanged("FilteredItemList");
                    }

                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<PECraftingStationItemVM> FilteredItemList
        {
            get
            {
                List<PECraftingStationItemVM> filteredItems = new List<PECraftingStationItemVM>();
                foreach (PECraftingStationItemVM i in this.CraftingList)
                {
                    filteredItems.Add(i);
                }
                if (this.CultureFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "vlandia");
                }
                else if (this.CultureFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "khuzait");
                }
                else if (this.CultureFilter.SelectedIndex == 3)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "aserai");
                }
                else if (this.CultureFilter.SelectedIndex == 4)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "sturgia");
                }
                else if (this.CultureFilter.SelectedIndex == 5)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "battania");
                }
                else if (this.CultureFilter.SelectedIndex == 6)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "empire");
                }
                else if (this.CultureFilter.SelectedIndex == 7)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableItem.Culture.StringId == "neutral_culture");
                }

                if(this.ItemCategoryFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Invalid);
                }else if (this.ItemCategoryFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Horse);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 3)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.OneHandedWeapon);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 4)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.TwoHandedWeapon);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 5)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Polearm);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 6)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Arrows);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 7)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bolts);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 8)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Shield);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 9)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bow);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 10)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Crossbow);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 11)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Thrown);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 12)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Goods);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 13)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HeadArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 14)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.BodyArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 15)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.LegArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 16)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HandArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 17)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Pistol);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 18)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Musket);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 19)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Bullets);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 20)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Animal);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 21)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Book);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 22)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.ChestArmor);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 23)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Cape);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 24)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.HorseHarness);
                }
                else if (this.ItemCategoryFilter.SelectedIndex == 25)
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Type == TaleWorlds.Core.ItemObject.ItemTypeEnum.Banner);
                }

                if(this.TierFilter.SelectedIndex == 1)
                {
                    filteredItems = filteredItems.FindAll(p => p.Tier == 1);
                }else if (this.TierFilter.SelectedIndex == 2)
                {
                    filteredItems = filteredItems.FindAll(p => p.Tier == 2);
                }else if (this.TierFilter.SelectedIndex == 3)
                {
                    filteredItems = filteredItems.FindAll(p => p.Tier == 3);
                }

                if (this.Filter != null && this.Filter != "")
                {
                    filteredItems = filteredItems.FindAll(p => p.CraftableItem.Culture != null && p.CraftableName.ToLower().Contains(this.Filter.ToLower()));
                }
                MBBindingList<PECraftingStationItemVM> _filter = new MBBindingList<PECraftingStationItemVM>();
                foreach (PECraftingStationItemVM p in filteredItems) _filter.Add(p);
                return _filter;



            }
        }



        [DataSourceProperty]
        public PEInventoryVM PlayerInventory
        {
            get => this._playerInventory;
            set
            {
                if(value != this._playerInventory)
                {
                    this._playerInventory = value;
                    base.OnPropertyChangedWithValue(value, "PlayerInventory");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PECraftingStationItemVM> CraftingList
        {
            get => this._craftingList;
            set
            {
                if (value != this._craftingList)
                {
                    this._craftingList = value;
                    base.OnPropertyChangedWithValue(value, "CraftingList");
                }
            }
        }


        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> TierFilter
        {
            get => this._tierFilter;
            set
            {
                if (this._tierFilter != value)
                {
                    this._tierFilter = value;
                    base.OnPropertyChangedWithValue(value, "TierFilter");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> ItemCategoryFilter
        {
            get => this._itemCategoryFilter;
            set
            {
                if (this._itemCategoryFilter != value)
                {
                    this._itemCategoryFilter = value;
                    base.OnPropertyChangedWithValue(value, "ItemCategoryFilter");
                }
            }
        }

        [DataSourceProperty]
        public SelectorVM<SelectorItemVM> CultureFilter { 
            get => this._cultureFilter; 
            set
            {
                if(value != this._cultureFilter)
                {
                    this._cultureFilter = value;
                    base.OnPropertyChangedWithValue(value, "CultureFilter");
                }
            }
        }
    }
}
