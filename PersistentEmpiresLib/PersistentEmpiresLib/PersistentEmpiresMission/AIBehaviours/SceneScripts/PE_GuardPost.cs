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
    public class PE_GuardPost : SynchedMissionObject
    {
        public int CastleID;
        public int BotID;
        public string BotType = "guard";
        public int RespawnTime = 5;
        public int Health = 800;
        public int AggroRadius = 25;
        public Agent Guard;
        public bool IsDead = true;
        public int MaxChaseDistance = 50;
        public int MaxRespawnTime = 600;
        public GuardTypeClass GuardType;
    }
}