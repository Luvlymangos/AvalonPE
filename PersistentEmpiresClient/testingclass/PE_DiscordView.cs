using Discord;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.NetworkMessages.Client;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using System.Management;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;
using System.Net.NetworkInformation;

namespace PersistentEmpires.Views.Views
{
    public class PEDiscordView : MissionView
    {
        public long applicationId = 1066379293204172971;

        private Discord.Discord discord;

        public bool DiscordNotWorks = false;
        public PersistentEmpireClientBehavior persistentEmpireClientBehavior;


        public override void OnMissionScreenInitialize()
        {
            base.OnMissionScreenInitialize();
            try
            {
                discord = new Discord.Discord(applicationId, (System.UInt64)Discord.CreateFlags.NoRequireDiscord);
                persistentEmpireClientBehavior = base.Mission.GetMissionBehavior<PersistentEmpireClientBehavior>();
                persistentEmpireClientBehavior.OnSynchronized += this.OnSynchronized;
            }
            catch (Exception e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Unable to connect discord. Please load desktop application of discord for easy communication about complaints and ban reasons", Color.ConvertStringToColor("#d32f2fff")));
                this.DiscordNotWorks = true;
            }
        }

        public override void OnMissionScreenTick(float dt)
        {
            if (this.DiscordNotWorks == false)
            {
                try
                {
                    discord.RunCallbacks();
                }
                catch (Exception e)
                {
                    this.DiscordNotWorks = false;
                }
            }
        }

        private void OnSynchronized()
        {
            if (this.DiscordNotWorks)
            {
                InformationManager.DisplayMessage(new InformationMessage("Unable to connect discord.", Color.ConvertStringToColor("#d32f2fff")));
                return;
            }
            discord.GetUserManager().OnCurrentUserUpdate += this.OnCurrentUserUpdate;
        }
        private string GetCPUID()
        {
            ManagementObjectCollection mbCol = new ManagementClass("Win32_BaseBoard").GetInstances();
            //Enumerating the list
            ManagementObjectCollection.ManagementObjectEnumerator mbEnum = mbCol.GetEnumerator();
            //Move the cursor to the first element of the list (and most probably the only one)
            mbEnum.MoveNext();
            //Getting the serial number of that specific motherboard
            return ((ManagementObject)(mbEnum.Current)).Properties["SerialNumber"].Value.ToString();
        }

        public string GetDefaultMacAddress()
        {
            Dictionary<string, long> macAddresses = new Dictionary<string, long>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] = nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }
            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }
            return mac;
        }

        private void OnCurrentUserUpdate()
        {
            try
            {
                Discord.User user = discord.GetUserManager().GetCurrentUser();
                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new MyDiscordId(user.Id.ToString()));
                GameNetwork.EndModuleEventAsClient();
            }
            catch (Exception e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Unable to get discord user. Reason: " + e.ToString(), Color.ConvertStringToColor("#d32f2fff")));
            }
            try
            {
                string cpuId = GetCPUID();
                if (cpuId == null || cpuId == "Default string" || cpuId == "To be filled by O.E.M." || cpuId == "" || cpuId == "BSS-0123456789")
                {
                    cpuId = GetDefaultMacAddress();
                }

                GameNetwork.BeginModuleEventAsClient();
                GameNetwork.WriteMessage(new MyCPUId(cpuId));
                GameNetwork.EndModuleEventAsClient();
            }
            catch (Exception e)
            {
                // InformationManager.DisplayMessage(new InformationMessage("Unable to get CPUID. Reason: " + e.ToString(), Color.ConvertStringToColor("#d32f2fff")));
            }
        }
    }
}
