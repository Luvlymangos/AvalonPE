using Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using System;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.SceneScripts
{
    public class PE_BanditSpawn : SynchedMissionObject
    {
        public string BotType = "bandit";
        public int BanditID;
        public int RespawnTime = 5;
        public int Health = 800;
        public int AggroRadius = 25;
        public Agent Bandit;
        public int MaxChaseDistance = 50;
        public int MaxRespawnTime = 600;
        public BanditType BanditSpawnParams;
    }
}
