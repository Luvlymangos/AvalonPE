using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresMission.MissionBehaviors
{
    public class WorkshopRentalHandler:MissionLogic
    {
        public Dictionary<int, PE_Workshop> Workshops;


        public override void OnBehaviorInitialize()
        {
            if (GameNetwork.IsServer)
            {
                Debug.Print("[Avalon HCRP] Workshop System Initalized", 0, Debug.DebugColor.Purple);
            }
        }

        public override void AfterStart()
        {
            if (GameNetwork.IsServer)
            {
                List<GameEntity> gameEntities = new List<GameEntity>();
                base.Mission.Scene.GetAllEntitiesWithScriptComponent<PE_Workshop>(ref gameEntities);
                foreach (var item in gameEntities)
                {
                    PE_Workshop Workshopitem = item.GetFirstScriptOfType<PE_Workshop>();
                    if (Workshopitem != null)
                    {
                        Workshops.Add(Workshopitem.WorkshopIndex, Workshopitem);
                    }
                }
            }
        }

        public override void OnMissionTick(float dt)
        {
        }
        
        public void RentWorkshop(int workshopIndex, string WorkshopTag)
        {
            PE_Workshop selectedworkshop = null;

            foreach (var item in Workshops)
            {
                if (item.Key == workshopIndex)
                {
                    selectedworkshop = item.Value;
                    break;
                }
            }
            
            if (selectedworkshop == null)
            {
                Debug.Print($"[Avalon HCRP] Workshop not found ({workshopIndex.ToString()})", 0, Debug.DebugColor.Red);
                return;
            }

            if (WorkshopTag == "Carpentry")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else if (WorkshopTag == "Blacksmithing")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else if (WorkshopTag == "Cooking")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else if (WorkshopTag == "Fletching")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else if (WorkshopTag == "Weaving")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else if (WorkshopTag == "Tannery")
            {
                selectedworkshop.SetWorkshopServerSide(WorkshopTag);
            }
            else
            {
                Debug.Print($"[Avalon HCRP] Workshop Tag not found ({WorkshopTag})", 0, Debug.DebugColor.Red);
            }
        }
    }
}
