using PersistentEmpiresLib.NetworkMessages.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors
{
    public class OfflineProtectionBehaviour : MissionNetwork
    {
        public bool IsOfflineProtectionActive = false;
        public int StartHour = 4;
        public int EndHour = 16;
        public override void OnBehaviorInitialize()
        {
            var timeUtc = DateTime.UtcNow;
            this.StartHour = ConfigManager.GetIntConfig("OfflineStartHour", 4);
            this.EndHour = ConfigManager.GetIntConfig("OfflineEndHour", 16);
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            Debug.Print("[Avalon HCRP] Offline Protection Initalized", 0, Debug.DebugColor.Purple);
            Debug.Print("Current Eastern Time: " + easternTime.Hour.ToString());
            if (easternTime.Hour >= StartHour && easternTime.Hour < EndHour)
            {
                IsOfflineProtectionActive = true;
            }
            else
            {
                IsOfflineProtectionActive = false;
            }
            base.OnBehaviorInitialize();
        }

        public bool IsDifferent(bool test)
        {
            if (IsOfflineProtectionActive != test)
            {
                return true;
            }
            return false;
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
        }
        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            var timeUtc = DateTime.UtcNow;
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime easternTime = TimeZoneInfo.ConvertTimeFromUtc(timeUtc, easternZone);
            if (easternTime.Hour >= StartHour && easternTime.Hour < EndHour)
            {
                if(IsOfflineProtectionActive != true)
                {
                    InformationComponent.Instance.BroadcastAnnouncement("Offline Protection is now activated");
                }
                IsOfflineProtectionActive = true;
            }
            else
            {
                if (IsOfflineProtectionActive != false)
                {
                    InformationComponent.Instance.BroadcastAnnouncement("Offline Protection is now deactivated");
                }
                IsOfflineProtectionActive = false;
            }
        }
    }
}
