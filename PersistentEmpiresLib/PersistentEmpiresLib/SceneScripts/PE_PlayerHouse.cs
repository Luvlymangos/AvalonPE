using PersistentEmpiresLib.NetworkMessages.Server;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_PlayerHouse : UsableMissionObject
    {
        public int price = 1500;
        public int HouseIndex;
        public bool isOwned = false;
        public PersistentEmpireRepresentative representative;
        protected override void OnInit()
        {  
            base.OnInit();
            TextObject actionMessage = new TextObject("House");
            base.ActionMessage = actionMessage;
            if (!isOwned)
            {
                TextObject descriptionMessage = new TextObject("Press {KEY} To Rent For: " + price + " For 7 Days.");
                descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
                base.DescriptionMessage = descriptionMessage;
            }
            else
            {
                TextObject descriptionMessage = new TextObject($"{representative.Peer.GetComponent<MissionPeer>().DisplayedName}'s House");
                base.DescriptionMessage = descriptionMessage;
            }
        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Bank";
        }


        public void ClientRentHouse()
        {
            isOwned = true;
            TextObject descriptionMessage = new TextObject($"Rented House");
            base.DescriptionMessage = descriptionMessage;
        }

        public override void OnUse(Agent userAgent)
        {

            base.OnUse(userAgent);
            if (GameNetwork.IsServer)
            {
                if (!isOwned)
                {
                    NetworkCommunicator networkCommunicator = userAgent.MissionPeer.GetNetworkPeer();
                    RentHouse(networkCommunicator);
                }
                else
                {
                    InformationComponent.Instance.SendMessage($"house already rented", 0x02ab89d9, userAgent.MissionPeer.GetNetworkPeer());
                }
            }
            userAgent.StopUsingGameObjectMT(true);
        }

        public void RentHouse(NetworkCommunicator networkCommunicator)
        {
            PersistentEmpireRepresentative representative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();
            if (representative != null && representative.GetHouse() == null)
            {
                representative.GoldLost(price);
            }
            isOwned = true;
            TextObject descriptionMessage = new TextObject($"{representative.Peer.GetComponent<MissionPeer>().DisplayedName}'s House");
            base.DescriptionMessage = descriptionMessage;
            Mission.Current.GetMissionBehavior<HouseBehviour>().SetPlayerHouse(networkCommunicator, HouseIndex);
        }
        
        public void RentEnd()
        {
            TextObject descriptionMessage = new TextObject("Press {KEY} To Rent For: " + price + " For 7 Days.");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
            isOwned = false;
        }
    }
}
