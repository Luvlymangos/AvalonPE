using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpires.Views.ViewsVM;
using PersistentEmpires.Views.ViewsVM.CraftingStation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine.GauntletUI;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ScreenSystem;
using PersistentEmpiresLib;
using PersistentEmpiresLib.ErrorLogging;
using TaleWorlds.Engine;
using PersistentEmpiresMission.AIBehaviours.Data;

namespace PersistentEmpires.Views.Views
{
    public class PECraftingStationScreen : PEBaseInventoryScreen
    {
        private CraftingComponent _craftingComponent;
        private PECraftingStationVM _dataSource;
        private PE_CraftingStation ActiveEntity;
        private long _craftingStartedAt;
        private long _craftingFinishAt;
        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            this._craftingComponent = base.Mission.GetMissionBehavior<CraftingComponent>();
            this._craftingComponent.OnCraftingUse += this.OnOpen;
            this._craftingComponent.OnCraftingStarted += this.OnCraftingStarted;
            this._craftingComponent.OnCraftingCompleted += this.OnCraftingCompleted;
            this._dataSource = new PECraftingStationVM(base.HandleClickItem);
        }

        private void OnCraftingStarted(PE_CraftingStation craftingStation, int craftIndex, NetworkCommunicator player)
        {
            this._craftingStartedAt = DateTimeOffset.Now.ToUnixTimeSeconds();
            this._dataSource.CraftingDuration = craftingStation.Craftables[craftIndex].CraftTime;
            this._craftingFinishAt = this._craftingStartedAt + this._dataSource.CraftingDuration;
            this._dataSource.IsCrafting = true;
            this._dataSource.PastDuration = 0;
        }
        private void OnCraftingCompleted()
        {
            this._dataSource.IsCrafting = false;
            this._dataSource.CraftingDuration = 0;
            this._dataSource.PastDuration = 0;
        }
        public override void OnMissionScreenTick(float dt)
        {
            base.OnMissionScreenTick(dt);
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            
            if (this._dataSource == null) return;
            if (myRepresentative != null)
            {
                try
                {
                    int totalSkills = 0;
                    foreach (int skillValue in myRepresentative.LoadedSkills.Values)
                    {
                        totalSkills += skillValue;
                    }
                    this._dataSource.WeaponLevel = myRepresentative.LoadedSkills["WeaponSmithing"];
                    this._dataSource.WeavingLevel = myRepresentative.LoadedSkills["Weaving"];
                    this._dataSource.ArmourLevel = myRepresentative.LoadedSkills["ArmourSmithing"];
                    this._dataSource.SmithingLevel = myRepresentative.LoadedSkills["BlackSmithing"];
                    this._dataSource.CarpLevel = myRepresentative.LoadedSkills["Carpentry"];
                    this._dataSource.CookingLevel = myRepresentative.LoadedSkills["Cooking"];
                    this._dataSource.FarmingLevel = myRepresentative.LoadedSkills["Farming"];
                    this._dataSource.MiningLevel = myRepresentative.LoadedSkills["Mining"];
                    this._dataSource.FletchingLevel = myRepresentative.LoadedSkills["Fletching"];
                    this._dataSource.AnimalLevel = myRepresentative.LoadedSkills["Animals"];
                    this._dataSource.TestLevel = myRepresentative.LoadedSkills["Animals"];
                    this._dataSource.TotalLevel = totalSkills;
                    this._dataSource.WeaponLock = myRepresentative.LockedSkills["WeaponSmithing"] ? "locked" : "unlocked";
                    this._dataSource.WeavingLock = myRepresentative.LockedSkills["Weaving"] ? "locked" : "unlocked";
                    this._dataSource.ArmourLock = myRepresentative.LockedSkills["ArmourSmithing"] ? "locked" : "unlocked";
                    this._dataSource.SmithingLock = myRepresentative.LockedSkills["BlackSmithing"] ? "locked" : "unlocked";
                    this._dataSource.CarpLock = myRepresentative.LockedSkills["Carpentry"] ? "locked" : "unlocked";
                    this._dataSource.CookingLock = myRepresentative.LockedSkills["Cooking"] ? "locked" : "unlocked";
                    this._dataSource.FarmingLock = myRepresentative.LockedSkills["Farming"] ? "locked" : "unlocked";
                    this._dataSource.MiningLock = myRepresentative.LockedSkills["Mining"] ? "locked" : "unlocked";
                    this._dataSource.FletchingLock = myRepresentative.LockedSkills["Fletching"] ? "locked" : "unlocked";
                    this._dataSource.AnimalLock = myRepresentative.LockedSkills["Animals"] ? "locked" : "unlocked";
                    this._dataSource.TestLock = myRepresentative.LockedSkills["Animals"] ? "locked" : "unlocked";

                }
                catch (Exception e)
                {
                }
            }
            if (this._dataSource.IsCrafting)
            {
                this._dataSource.PastDuration = Convert.ToInt32(DateTimeOffset.Now.ToUnixTimeSeconds() - this._craftingStartedAt);
            }
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            if (myRepresentative != null && myRepresentative.DebugMode)
            {
                MBDebug.ClearRenderObjects();
                foreach (Agent agent in Mission.Current.Agents)
                {   
                    if (agent.MissionPeer != null) continue;
                    if (agent != null)
                    {
                        // Get the forward direction of the agent on the XZ plane
                        Vec3 forwardDirection = agent.LookFrame.rotation.f;
                        forwardDirection.z = 0; // Flatten the direction to the horizontal plane
                        forwardDirection = forwardDirection.NormalizedCopy(); // Normalize to ensure it has unit length

                        // Define the rotation angles (60 degrees in both directions)
                        float angleRadiansPositive = MathF.PI / 3;  // 60 degrees in radians
                        float angleRadiansNegative = -MathF.PI / 3; // -60 degrees in radians

                        // Compute the rotated directions
                        Vec3 rotatedDirectionPositive = new Vec3(
                            forwardDirection.x * MathF.Cos(angleRadiansPositive) - forwardDirection.y * MathF.Sin(angleRadiansPositive),
                            forwardDirection.x * MathF.Sin(angleRadiansPositive) + forwardDirection.y * MathF.Cos(angleRadiansPositive),
                            0 // Keep it flat on the XZ plane
                        );

                        Vec3 rotatedDirectionNegative = new Vec3(
                            forwardDirection.x * MathF.Cos(angleRadiansNegative) - forwardDirection.y * MathF.Sin(angleRadiansNegative),
                            forwardDirection.x * MathF.Sin(angleRadiansNegative) + forwardDirection.y * MathF.Cos(angleRadiansNegative),
                            0 // Keep it flat on the XZ plane
                        ); 
                        Vec3 directionToAgent = GameNetwork.MyPeer.ControlledAgent.Position - agent.Position;
                        // Render debug arrows for both directions
                        MBDebug.RenderDebugLine(agent.Position, rotatedDirectionPositive * 30, 4278190335); // Positive 60 degrees
                        MBDebug.RenderDebugLine(agent.Position, rotatedDirectionNegative * 30, 4294901760);
                        MBDebug.RenderDebugLine(agent.Position, directionToAgent, 4294901760);// Negative 60 degrees
                        //if (agent.IsAIControlled && agent.GetComponent<BanditAgentComponent>() != null)
                        //{
                        //    BanditAgentComponent BAC = agent.GetComponent<BanditAgentComponent>();
                        //    if (BAC != null)
                        //    {
                        //        foreach (var i in BAC.PerceivedAgents)
                        //        {
                        //            MBDebug.RenderDebugLine(agent.Position, , 4294901760);
                        //        }




                        //    }
                        //}
                    }
                }
            }
            if (this._gauntletLayer != null && this.IsActive && (this._gauntletLayer.Input.IsHotKeyReleased("ToggleEscapeMenu") || this._gauntletLayer.Input.IsHotKeyReleased("Exit")))
            {
                this.Close();
            }
        }
        public void Close()
        {
            if (this.IsActive)
            {
                this.CloseAux();
            }
        }
        private void CloseAux()
        {
            this.IsActive = false;
            this._dataSource.Filter = "";
            this._gauntletLayer.InputRestrictions.ResetInputRestrictions();
            base.MissionScreen.RemoveLayer(this._gauntletLayer);
            this._gauntletLayer = null;

        }

 

        private void OnOpen(PE_CraftingStation craftingStation, Inventory playerInventory)
        {
            if (this.IsActive) return;
            this.ActiveEntity = craftingStation;
            this._dataSource.RefreshValues(craftingStation, playerInventory, this.ExecuteCraft);
            this._dataSource.PlayerInventory.SetEquipmentSlots(AgentHelpers.GetCurrentAgentEquipment(GameNetwork.MyPeer.ControlledAgent));
            this._gauntletLayer = new GauntletLayer(50);
            this._gauntletLayer.IsFocusLayer = true;
            this._gauntletLayer.InputRestrictions.SetInputRestrictions(true, InputUsageMask.All);
            this._gauntletLayer.Input.RegisterHotKeyCategory(HotKeyManager.GetCategory("GenericPanelGameKeyCategory"));
            this._gauntletLayer.LoadMovie("PECraftingStation", this._dataSource);
            base.MissionScreen.AddLayer(this._gauntletLayer);
            PersistentEmpireRepresentative myRepresentative = GameNetwork.MyPeer.GetComponent<PersistentEmpireRepresentative>();
            try
            {
                int totalSkills = 0;
                foreach (int skillValue in myRepresentative.LoadedSkills.Values)
                {
                    totalSkills += skillValue;
                }
                this._dataSource.WeaponLevel = myRepresentative.LoadedSkills["WeaponSmithing"];
                this._dataSource.WeavingLevel = myRepresentative.LoadedSkills["Weaving"];
                this._dataSource.ArmourLevel = myRepresentative.LoadedSkills["ArmourSmithing"];
                this._dataSource.SmithingLevel = myRepresentative.LoadedSkills["BlackSmithing"];
                this._dataSource.CarpLevel = myRepresentative.LoadedSkills["Carpentry"];
                this._dataSource.CookingLevel = myRepresentative.LoadedSkills["Cooking"];
                this._dataSource.FarmingLevel = myRepresentative.LoadedSkills["Farming"];
                this._dataSource.MiningLevel = myRepresentative.LoadedSkills["Mining"];
                this._dataSource.FletchingLevel = myRepresentative.LoadedSkills["Fletching"];
                this._dataSource.AnimalLevel = myRepresentative.LoadedSkills["Animals"];
                this._dataSource.TotalLevel = totalSkills;
                this._dataSource.WeaponLock = myRepresentative.LockedSkills["WeaponSmithing"] ? "locked" : "unlocked";
                this._dataSource.WeavingLock = myRepresentative.LockedSkills["Weaving"] ? "locked" : "unlocked";
                this._dataSource.ArmourLock = myRepresentative.LockedSkills["ArmourSmithing"] ? "locked" : "unlocked";
                this._dataSource.SmithingLock = myRepresentative.LockedSkills["BlackSmithing"] ? "locked" : "unlocked";
                this._dataSource.CarpLock = myRepresentative.LockedSkills["Carpentry"] ? "locked" : "unlocked";
                this._dataSource.CookingLock = myRepresentative.LockedSkills["Cooking"] ? "locked" : "unlocked";
                this._dataSource.FarmingLock = myRepresentative.LockedSkills["Farming"] ? "locked" : "unlocked";
                this._dataSource.MiningLock = myRepresentative.LockedSkills["Mining"] ? "locked" : "unlocked";
                this._dataSource.FletchingLock = myRepresentative.LockedSkills["Fletching"] ? "locked" : "unlocked";
                this._dataSource.AnimalLock = myRepresentative.LockedSkills["Animals"] ? "locked" : "unlocked";
            }
            catch (Exception e)
            {
                Debug.PrintError("Error in PECraftingStationScreen: " + e.Message);
                Debug.Print("Error in PECraftingStationScreen: " + e.Message);
            }
            ScreenManager.TrySetFocus(this._gauntletLayer);
            this.IsActive = true;
        }

        public void ExecuteCraft(PECraftingStationItemVM selectedCraft) {
            if(this._dataSource.IsCrafting)
            {
                InformationManager.DisplayMessage(new InformationMessage("You're already crafting. You need to wait.", new Color(1f, 0, 0)));
                return;
            }
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new RequestExecuteCraft(this.ActiveEntity, selectedCraft.CraftableIndex));
            GameNetwork.EndModuleEventAsClient();
        }

        protected override PEInventoryVM GetInventoryVM()
        {
            return this._dataSource.PlayerInventory;
        }
    }
}
