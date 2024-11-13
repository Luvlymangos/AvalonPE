using PersistentEmpiresLib.Factions;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using PersistentEmpires.Views.Views.AdminPanel;
using TaleWorlds.MountAndBlade;
using PersistentEmpiresClient.ViewsVM.AdminPanel.Buttons;

namespace PersistentEmpires.Views.ViewsVM.AdminPanel
{
    public class PEAdminSkillPanelVM : ViewModel
    {
        private MBBindingList<PEAdminPlayerVM> _players;
        private MBBindingList<PEAdminButtonVM> _adminButtons;
        private string _searchedPlayerName;
        private int _count = 1;
        private PEAdminPlayerVM _selectedPlayer;
        private static List<PEAdminButtonVM> _customButtons = new List<PEAdminButtonVM>();




        public PEAdminSkillPanelVM()
        {
            _adminButtons = new MBBindingList<PEAdminButtonVM>()
            {
                new ChangeSkill("Weaving", Count),
                new ChangeSkill("WeaponSmithing", Count),
                new ChangeSkill("ArmourSmithing", Count),
                new ChangeSkill("BlackSmithing", Count),
                new ChangeSkill("Carpentry", Count),
                new ChangeSkill("Cooking", Count),
                new ChangeSkill("Farming", Count),
                new ChangeSkill("Mining", Count),
                new ChangeSkill("Fletching", Count),
                new ChangeSkill("Animals", Count),
            };

            if (_customButtons.Any())
            {
                _customButtons.ForEach(x => _adminButtons.Add(x));
            }

            OnPropertyChangedWithValue(_adminButtons, "AdminButtons");
        }
        public static void RegisterCustomButton(PEAdminButtonVM button)
        {
            if (!_customButtons.Contains(button))
            {
                _customButtons.Add(button);
            }
        }
        public override void RefreshValues()
        {
                base.RefreshValues();
                this.Players = new MBBindingList<PEAdminPlayerVM>();
                foreach (NetworkCommunicator peer in GameNetwork.NetworkPeers)
                {
                    this.Players.Add(new PEAdminPlayerVM(peer, (PEAdminPlayerVM selected) =>
                    {
                        this.SelectedPlayer = selected;
                    }));
                }
                base.OnPropertyChanged("FilteredPlayers");
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminPlayerVM> Players
        {
            get => _players;
            set
            {
                if (value != this._players)
                {
                    this._players = value;
                    base.OnPropertyChangedWithValue(value, "Players");
                }
            }
        }

        [DataSourceProperty]
        public string SearchedPlayerName
        {
            get => this._searchedPlayerName;
            set
            {
                if (value != this._searchedPlayerName)
                {
                    this._searchedPlayerName = value;
                    base.OnPropertyChangedWithValue(value, "SearchedPlayerName");
                    base.OnPropertyChanged("FilteredPlayers");
                }
            }
        }

        [DataSourceProperty]
        public int Count
        {
            get => this._count;
            set
            {
                if (value != this._count)
                {
                    this._count = value;
                    foreach (ChangeSkill button in this.AdminButtons)
                    {
                        button.SetValue(value);
                    }
                    base.OnPropertyChangedWithValue(value, "Count");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminPlayerVM> FilteredPlayers
        {
            get
            {
                List<PEAdminPlayerVM> filtered = this.SearchedPlayerName == null || this.SearchedPlayerName == "" ? this.Players.ToList() : this.Players.Where(p => p.PlayerName.Contains(this.SearchedPlayerName) || p.FactionName.Contains(this.SearchedPlayerName)).ToList();
                MBBindingList<PEAdminPlayerVM> filteredBinding = new MBBindingList<PEAdminPlayerVM>();
                foreach (PEAdminPlayerVM f in filtered)
                {
                    filteredBinding.Add(f);
                }
                return filteredBinding;
            }
        }

        [DataSourceProperty]
        public PEAdminPlayerVM SelectedPlayer
        {
            get => this._selectedPlayer;
            set
            {
                if (value != this._selectedPlayer)
                {
                    if (this._selectedPlayer != null)
                    {
                        this._selectedPlayer.IsSelected = false;
                    }
                    this._selectedPlayer = value;
                    if (value != null)
                    {
                        this._selectedPlayer.IsSelected = true;
                    }

                    _adminButtons.ToList().ForEach(x => x.SetSelectedPlayer(value));
                    base.OnPropertyChangedWithValue(this._selectedPlayer, "SelectedPlayer");
                }
            }
        }

        [DataSourceProperty]
        public MBBindingList<PEAdminButtonVM> AdminButtons
        {
            get => _adminButtons;
            set
            {
                if (value != _adminButtons)
                {
                    this._adminButtons = value;
                    base.OnPropertyChangedWithValue(value, "AdminButtons");
                }
            }
        }
    }
}
