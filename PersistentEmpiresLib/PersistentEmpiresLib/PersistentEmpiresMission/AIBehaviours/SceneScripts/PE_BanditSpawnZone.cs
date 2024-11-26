using Data;
using PersistentEmpiresLib.Factions;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresMission.AIBehaviours.Data;
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
    public class PE_BanditSpawnZone : SynchedMissionObject
    {
        public int BotID;
        public string BotType = "Bandit";
        public int RespawnTime = 5;
        public int Health = 800;
        public int AggroRadius = 25;
        public Agent Bandit;
        public bool IsDead = true;
        public int MaxChaseDistance = 50;
        public int MaxRespawnTime = 600;
        public BanditAgentType BanditAgentParams;
    }
}