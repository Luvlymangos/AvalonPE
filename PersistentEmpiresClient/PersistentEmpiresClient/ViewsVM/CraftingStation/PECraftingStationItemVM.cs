﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace PersistentEmpires.Views.ViewsVM.CraftingStation
{
    public class PECraftingStationItemVM : ViewModel
    {
        public int CurrentTier;
        public ItemObject CraftableItem;
        public int CraftableIndex;
        private ImageIdentifierVM _imageIdentifier;
        private int _outputCount;
        private int _craftingRequirment;
        private string _craftingType;
        private int _tier;
        private MBBindingList<PECraftingReceiptVM> _craftingReceipts;
        private int _craftingDuration;

        private Action<PECraftingStationItemVM> _executeCraft;
        private string _craftableName;
        public PECraftingStationItemVM(ItemObject craftableItem, int outputCount, int required, int myskill, MBBindingList<PECraftingReceiptVM> craftingReceipts, Action<PECraftingStationItemVM> executeCraft, int index, int craftingDuration, string Skill) {
            this.CraftableItem = craftableItem;
            this.ImageIdentifier = new ImageIdentifierVM(craftableItem);
            this.OutputCount = outputCount;
            this.CraftingRequirment = required;
            this.CraftingType = Skill;
            this.Tier = required;
            this.CurrentTier = myskill;
            this.CraftingReceipts = craftingReceipts;
            this.CraftableName = this.CraftableItem.Name.ToString();
            this._executeCraft = executeCraft;
            this.CraftableIndex = index;
            this.CraftingDuration = craftingDuration;
        }
        public void ExecuteCraft()
        {
            this._executeCraft(this);
        }
        public void ExecuteHoverStart()
        {
            if (this.CraftableItem != null)
            {
                InformationManager.ShowTooltip(typeof(ItemObject), new object[] {
                    new EquipmentElement(this.CraftableItem)
                });
            }
        }

        public void ExecuteHoverEnd()
        {
            InformationManager.HideTooltip();
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
        public string CraftableName
        {
            get => this._craftableName;
            set
            {
                if(value != this._craftableName)
                {
                    this._craftableName = value;
                    base.OnPropertyChangedWithValue(value, "CraftableName");
                }
            }
        }
        [DataSourceProperty]
        public bool CanCraft
        {
            get => this.Tier <= this.CurrentTier;
        }
        

        [DataSourceProperty]
        public ImageIdentifierVM ImageIdentifier
        {
            get => this._imageIdentifier;
            set
            {
                if (value != this._imageIdentifier)
                {
                    this._imageIdentifier = value;
                    base.OnPropertyChangedWithValue(value, "ImageIdentifier");
                }
            }
        }
        [DataSourceProperty]
        public int Tier
        {
            get => this._tier;
            set
            {
                if (value != this._tier)
                {
                    this._tier = value;
                    base.OnPropertyChangedWithValue(value, "Tier");
                }
            }
        }
        [DataSourceProperty]
        public int OutputCount {
            get => this._outputCount;
            set
            {
                if(value != this._outputCount)
                {
                    this._outputCount = value;
                    base.OnPropertyChangedWithValue(value, "OutputCount");
                }
            }
        }
        [DataSourceProperty]
        public MBBindingList<PECraftingReceiptVM> CraftingReceipts {
            get => this._craftingReceipts;
            set { 
                if(value != this._craftingReceipts)
                {
                    this._craftingReceipts = value;
                    base.OnPropertyChangedWithValue(value, "CraftingReceipts");
                }
            }
        }
        [DataSourceProperty]
        public int CraftingRequirment
        {
            get => this._craftingRequirment;
            set
            {
                if (value != this._craftingRequirment)
                {
                    this._craftingRequirment = value;
                    base.OnPropertyChangedWithValue(value, "CraftingRequirment");
                }
            }
        }
        [DataSourceProperty]
        public string CraftingType
        {
            get => this._craftingType;
            set
            {
                if (value != this._craftingType)
                {
                    this._craftingType = value;
                    base.OnPropertyChangedWithValue(value, "CraftingType");
                }
            }
        }
    }
}
