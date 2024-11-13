using PersistentEmpiresLib;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminPlayerVM : ViewModel
    {
        private NetworkCommunicator playerPeer;
        private AdminClientBehavior _adminbehaviour;
        public Dictionary<string, int> LoadedSkills;
        private string _playerName;
        private string _factionName;
        private bool _isSelected;
        private Action<PEAdminPlayerVM> _executeSelect;
        private int _weavinglevel;
        private int _weaponlevel;
        private int _armourlevel;
        private int _smithinglevel;
        private int _carplevel;
        private int _cookinglevel;
        private int _farminglevel;
        private int _mininglevel;
        private int _fletchinglevel;
        private int _animalslevel;


        public PEAdminPlayerVM(NetworkCommunicator playerPeer, Action<PEAdminPlayerVM> executeSelect)
        {
            this.playerPeer = playerPeer;
            this.PlayerName = this.playerPeer.UserName;
            _adminbehaviour = Mission.Current.GetMissionBehavior<AdminClientBehavior>();
            PersistentEmpireRepresentative persistentEmpireRepresentative = this.playerPeer.GetComponent<PersistentEmpireRepresentative>();
            try
            {
                LoadedSkills = DeserializeCraftingStats(_adminbehaviour.PlayerStats[playerPeer]);
                this.WeavingLevel = LoadedSkills["Weaving"];
                this.WeaponLevel = LoadedSkills["WeaponSmithing"];
                this.ArmourLevel = LoadedSkills["ArmourSmithing"];
                this.SmithingLevel = LoadedSkills["BlackSmithing"];
                this.CarpLevel = LoadedSkills["Carpentry"];
                this.CookingLevel = LoadedSkills["Cooking"];
                this.FarmingLevel = LoadedSkills["Farming"];
                this.MiningLevel = LoadedSkills["Mining"];
                this.FletchingLevel = LoadedSkills["Fletching"];
                this.AnimalsLevel = LoadedSkills["Animals"];
            }
            catch (Exception e)
            {
            }
            this.FactionName = persistentEmpireRepresentative == null || persistentEmpireRepresentative.GetFaction() == null ? "Unknown" : persistentEmpireRepresentative.GetFaction().name;
            this._executeSelect = executeSelect;
        }

        public NetworkCommunicator GetPeer()
        {
            return this.playerPeer;
        }

        [DataSourceProperty]
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                if(value != this._isSelected)
                {
                    this._isSelected = value;
                    base.OnPropertyChangedWithValue(value, "IsSelected");
                }
            }
        }

        [DataSourceProperty]
        public string FactionName
        {
            get => this._factionName;
            set
            {
                if(value != this._factionName)
                {
                    this._factionName = value;
                    base.OnPropertyChangedWithValue(value, "FactionName");
                }
            }
        }

        [DataSourceProperty]
        public string PlayerName
        {
            get => this._playerName;
            set
            {
                if(value != this._playerName)
                {
                    this._playerName = value;
                    base.OnPropertyChangedWithValue(value, "PlayerName");
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
        public int AnimalsLevel
        {
            get => this._animalslevel;
            set
            {
                if (value != this._animalslevel)
                {
                    this._animalslevel = value;
                    base.OnPropertyChangedWithValue(value, "AnimalsLevel");
                }
            }
        }

        public Dictionary<string, int> DeserializeCraftingStats(string serializedData)
        {
            Dictionary<string, int> deserializedSkills = new Dictionary<string, int>();

            // Split by '=' to get each skill entry
            var entries = serializedData.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in entries)
            {
                // Split each entry by '*' to get the skill key and value
                var parts = entry.Split('*');

                if (parts.Length == 2 && int.TryParse(parts[1], out int skillValue))
                {
                    string skillKey = parts[0];
                    deserializedSkills[skillKey] = skillValue;
                }
            }

            return deserializedSkills;
        }

        public void ExecuteSelect()
        {
            this._executeSelect(this);
        }
    }
}
