using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpires.Views.ViewsVM.AdminPanel;
using PersistentEmpires.Views.ViewsVM.PETabMenu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using PersistentEmpiresClient.ViewsVM.AdminPanel;

namespace PersistentEmpires.Views.Views.AdminPanel
{
    public class PEAdminSkillView : MissionView
    {
        protected AdminClientBehavior _adminBehavior;
        protected GauntletLayer _gauntletLayer;
        private PEAdminSkillPanelVM _dataSource;

        public bool IsActive { get; private set; }

        public override bool OnEscape()
        {
            bool result = base.OnEscape();
            this.CloseManagementMenu();
            return result;
        }
        protected void CloseManagementMenu()
        {
            if (this.IsActive)
            {
                this.IsActive = false;
                this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
                base.MissionScreen.RemoveLayer(this._gauntletLayer);
                this._gauntletLayer = null;
            }
        }
        public void OnOpen()
        {
            this._dataSource.RefreshValues();
            this._gauntletLayer = new GauntletLayer(2);
            this._gauntletLayer.LoadMovie("PEAdminSkillManagment", this._dataSource);
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.Mouse);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            this.IsActive = true;
        }

        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._adminBehavior = base.Mission.GetMissionBehavior<AdminClientBehavior>();
            _dataSource = new PEAdminSkillPanelVM();
        }
    }
}
