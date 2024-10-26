using Org.BouncyCastle.Asn1.Ocsp;
using PersistentEmpiresLib.Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresGameModels;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using TaleWorlds.PlayerServices;

namespace PersistentEmpiresLib.SceneScripts
{
    public enum GrowingPhase
    {
        Dirt,
        Growing,
        Grown
    }

    public class PE_GrowingPlant : PE_UsableFromDistance
    {
        GameEntity Dirt;
        GameEntity GrowingPlant;
        GameEntity SeedlingEntity;
        GameEntity GrownEntity;
        Dictionary<string,GameEntity> AvaliablePlants = new Dictionary<string, GameEntity>();
        public int GrowTime = 30;
        public int GrowDistance = 1;
        public float MovePerTick = 2.8f;
        private PlantingBehaviour _plantingBehaviour;
        private long _LastTick = 0;
        private GrowingPhase CurrentPhase = GrowingPhase.Dirt;
        private Growables _growable;

        public override ScriptComponentBehavior.TickRequirement GetTickRequirement()
        {
            if (GameNetwork.IsClientOrReplay)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            else if (GameNetwork.IsServer)
            {
                return base.GetTickRequirement() | ScriptComponentBehavior.TickRequirement.Tick | ScriptComponentBehavior.TickRequirement.TickParallel;
            }
            return base.GetTickRequirement();
        }

        protected override void OnInit()
        {
            //Classify Game Entities
            Dirt = base.GameEntity.GetChildren().FirstOrDefault((GameEntity x) => x.HasTag("Dirt"));
            foreach(GameEntity child in base.GameEntity.GetChildren())
            {
                if (child.HasTag("Dirt"))
                {
                    continue;
                }
                AvaliablePlants.Add(child.Tags.First<string>(), child);
            }

            _LastTick = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            this._plantingBehaviour = Mission.Current.GetMissionBehavior<PlantingBehaviour>();
            base.OnInit();
            base.ActionMessage = new TextObject("Plant");
            TextObject descriptionMessage = new TextObject("Press {KEY} To Use");
            descriptionMessage.SetTextVariable("KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            base.DescriptionMessage = descriptionMessage;
        }

        protected override void OnTick(float dt)
        {
            if(CurrentPhase == GrowingPhase.Growing && _LastTick < DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            {
                _LastTick = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                GrowTime -= 1;
                if (GrowTime == 0)
                {
                    CurrentPhase = GrowingPhase.Grown;
                    SeedlingEntity.SetVisibilityExcludeParents(false);
                }

                
                MatrixFrame frame = this.SeedlingEntity.GetFrame();
                frame.Elevate(2.8f * dt);
                if (GrowTime >= 0)
                {
                    Vec3 vec = frame.origin;
                    vec.z = vec.z + GrowDistance;
                    frame.TransformToLocal(vec);
                }
                this.SeedlingEntity.SetFrame(ref frame);

            }
        }

        public void SetNewPlant(string plantName)
        {
            if (CurrentPhase == GrowingPhase.Dirt)
            {
                SeedlingEntity = AvaliablePlants[plantName].GetChildren().FirstOrDefault((GameEntity x) => x.HasTag("Grow"));
                GrownEntity = AvaliablePlants[plantName].GetChildren().FirstOrDefault((GameEntity x) => x.HasTag("Grown"));
                SeedlingEntity.SetVisibilityExcludeParents(true);
                GrowTime = _growable.GrowTime;
                CurrentPhase = GrowingPhase.Growing;
            }
        }

        public override void OnUse(Agent userAgent)
        {
            EquipmentIndex wieldedItemIndex = userAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            MissionWeapon wieldedItem = userAgent.Equipment[wieldedItemIndex];
            NetworkCommunicator networkCommunicator = userAgent.MissionPeer.GetNetworkPeer();
            PersistentEmpireRepresentative persistentEmpireRepresentative = networkCommunicator.GetComponent<PersistentEmpireRepresentative>();


            //Harvesting
            if (CurrentPhase == GrowingPhase.Grown)
            {
                if (wieldedItem.ToString() != _growable.HarvestItem)
                {
                    InformationComponent.Instance.SendMessage($"You need a {MBObjectManager.Instance.GetObject < ItemObject > (_growable.HarvestItem).Name} to harvest this!", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                ItemObject DropsItemObject = MBObjectManager.Instance.GetObject<ItemObject>(_growable.CropName);
                ItemObject DropsSeedObject = MBObjectManager.Instance.GetObject<ItemObject>(_growable.SeedName);
                Inventory playerInventory = persistentEmpireRepresentative.GetInventory();
                playerInventory.AddCountedItemSynced(DropsItemObject, _growable.Yield, ItemHelper.GetMaximumAmmo(DropsItemObject));
                playerInventory.AddCountedItemSynced(DropsSeedObject, _growable.SeedYield, ItemHelper.GetMaximumAmmo(DropsSeedObject));
                persistentEmpireRepresentative.IncreaseSkilllevel("Farming", _growable.SkillYield);
            }


            //Growing
            else if (CurrentPhase == GrowingPhase.Growing)
            {
                InformationComponent.Instance.SendMessage("This Plant is still growing", Colors.Red.ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
            }


            //Planting
            else if (CurrentPhase == GrowingPhase.Dirt)
            {
                if (wieldedItemIndex == EquipmentIndex.None)
                {
                    InformationComponent.Instance.SendMessage("You need a seed to plant this!", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                if (!_plantingBehaviour.IsValidSeed(wieldedItem.Item.StringId))
                {
                    InformationComponent.Instance.SendMessage("This is not a valid seed!", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                _growable = _plantingBehaviour.GetPlant(wieldedItem.Item.StringId);
                if (persistentEmpireRepresentative.GetSkill("Farming").Value < _growable.SkillRequired)
                {
                    InformationComponent.Instance.SendMessage("You need a higher skill to plant this!", new Color(1f, 0, 0).ToUnsignedInteger(), userAgent.MissionPeer.GetNetworkPeer());
                    userAgent.StopUsingGameObjectMT(false);
                    return;
                }
                SetNewPlant(_growable.PlantName);
                userAgent.StopUsingGameObjectMT(false);
                return;
            }






            if (!base.IsUsable(userAgent))
            {
                userAgent.StopUsingGameObjectMT(false);
                return;
            }
            base.OnUse(userAgent);

        }
        public override string GetDescriptionText(GameEntity gameEntity = null)
        {
            return "Use Door";
        }
    }
}
