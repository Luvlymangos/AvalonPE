﻿using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;

namespace PersistentEmpiresClient
{
    public class MissionManager
    {
        [MissionMethod]
        public static void OpenPersistentEmpires(string scene)
        {
            MissionState.OpenNew("PersistentEmpires", new MissionInitializerRecord(scene), delegate (Mission missionController)
            {
                return new MissionBehavior[]
                {
                    MissionLobbyComponent.CreateBehavior(),
                    new InformationComponent(),
                    new FactionsBehavior(),
                    new CastlesBehavior(),
                    new FactionPollComponent(),
                    new WoundingBehavior(),
                    new PersistentEmpireClientBehavior(),
                    new PlayerInventoryComponent(),
                    new ImportExportComponent(),
                    new CraftingComponent(),
                    new MoneyPouchBehavior(),
                    new StockpileMarketComponent(),
                    new ProximityChatComponent(),
                    new MultiplayerTimerComponent(),
                    new MultiplayerMissionAgentVisualSpawnComponent(),
                    new PersistentEmpireSceneSyncBehaviors(),
                    new AgentHungerBehavior(),
                    new MissionLobbyEquipmentNetworkComponent(),
                    new MissionBoundaryPlacer(),
                    new MissionBoundaryCrossingHandler(),
                    new FactionUIComponent(),
                    new LocalChatComponent(),
                    new MissionOptionsComponent(),
                    new AdminClientBehavior(),
                    new BankingComponent(),
                    new PatreonRegistryBehavior(),
                    new TradingCenterBehavior(),
                    new DayNightCycleBehavior(),
                    new InstrumentsBehavior(),
                    new MoneyChestBehavior(),
                    new HouseBehviour(),
                    //new DecapitationBehavior(),
                    new AnimationBehavior(),
                    new PlantingBehaviour(),
                    new AgentCapture()
                };
            },true,true);
        }
    }
}
