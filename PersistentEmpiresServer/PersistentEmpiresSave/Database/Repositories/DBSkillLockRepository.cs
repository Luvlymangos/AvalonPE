using Dapper;
using PersistentEmpiresLib;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.PersistentEmpiresMission;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBSkillLockRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnCreateOrSaveSkillLock += CreateOrSaveLocks;
            SaveSystemBehavior.OnCreateOrGetSkillLock += CreateOrGetLocks;
        }

        private static DBSkillLocks CreateDBLock(NetworkCommunicator peer)
        {
            PersistentEmpireRepresentative persistentEmpireRepresentative = peer.GetComponent<PersistentEmpireRepresentative>();
            Debug.Print("[Save Module] CREATING DBPlayerLock FOR PLAYER " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " IS CONTROLLEDAGENT NULL ? " + (peer.ControlledAgent == null) + " IS REPRESENTATIVE NULL ? " + (persistentEmpireRepresentative == null));
            if (persistentEmpireRepresentative == null)
            {
                return new DBSkillLocks
                {
                    Id = peer.VirtualPlayer.Id.ToString(),
                    Weaving = false,
                    WeaponSmithing = false,
                    ArmourSmithing = false,
                    BlackSmithing = false,
                    Carpentry = false,
                    Cooking = false,
                    Farming = false,
                    Mining = false,
                    Fletching = false,
                    Animals = false
                };
            }
            DBSkillLocks dbskilllock = new DBSkillLocks
            {
                Id = peer.VirtualPlayer.Id.ToString(),
                Weaving = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Weaving") ? false : persistentEmpireRepresentative.LockedSkills["Weaving"],
                WeaponSmithing = !persistentEmpireRepresentative.LockedSkills.ContainsKey("WeaponSmithing") ? false : persistentEmpireRepresentative.LockedSkills["WeaponSmithing"],
                ArmourSmithing = !persistentEmpireRepresentative.LockedSkills.ContainsKey("ArmourSmithing") ? false : persistentEmpireRepresentative.LockedSkills["ArmourSmithing"],
                BlackSmithing = !persistentEmpireRepresentative.LockedSkills.ContainsKey("BlackSmithing") ? false : persistentEmpireRepresentative.LockedSkills["BlackSmithing"],
                Carpentry = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Carpentry") ? false : persistentEmpireRepresentative.LockedSkills["Carpentry"],
                Cooking = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Cooking") ? false : persistentEmpireRepresentative.LockedSkills["Cooking"],
                Farming = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Farming") ? false : persistentEmpireRepresentative.LockedSkills["Farming"],
                Mining = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Mining") ? false : persistentEmpireRepresentative.LockedSkills["Mining"],
                Fletching = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Fletching") ? false : persistentEmpireRepresentative.LockedSkills["Fletching"],
                Animals = !persistentEmpireRepresentative.LockedSkills.ContainsKey("Animals") ? false : persistentEmpireRepresentative.LockedSkills["Animals"]
            };
            return dbskilllock;
        }

        public static IEnumerable<DBSkillLocks> GetLock(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] LOAD PLAYERLock FROM DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            IEnumerable<DBSkillLocks> result = DBConnection.Connection.Query<DBSkillLocks>("SELECT * FROM skillslocks WHERE Id = @Id", new { Id = peer.VirtualPlayer.Id.ToString() });
            Debug.Print("[Save Module] LOAD PLAYERLock FROM DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!") + " RESULT COUNT : " + result.Count());
            return result;
        }

        public static DBSkillLocks CreateOrGetLocks(NetworkCommunicator peer)
        {
            // DBConnection.Connection.Query<DBPlayer>();
            IEnumerable<DBSkillLocks> getQuery = DBSkillLockRepository.GetLock(peer);
            if (getQuery.Count() == 0)
            {
                Debug.Print("Creating DBLOCK");
                CreateLock(peer);
                getQuery = GetLock(peer);
            }
            return getQuery.First();
        }

        public static DBSkillLocks CreateOrSaveLocks(NetworkCommunicator peer)
        {
            if (GetLock(peer).Count() > 0)
            {
                return SaveLock(peer);
            }
            else
            {
                return CreateLock(peer);
            }
        }
        public static DBSkillLocks SaveLock(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] SAVING PLAYERLock TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));


            string updateQuery = "UPDATE skillslocks SET Weaving = @Weaving, WeaponSmithing = @WeaponSmithing, ArmourSmithing = @ArmourSmithing, BlackSmithing = @BlackSmithing, Carpentry = @Carpentry, Cooking = @Cooking, Farming = @Farming, Mining = @Mining, Fletching = @Fletching, Animals = @Animals WHERE Id = @Id";
            DBSkillLocks player = CreateDBLock(peer);
            DBConnection.Connection.Execute(updateQuery, player);
            Debug.Print("[Save Module] SAVED PLAYERLock TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            return player;
        }
        public static DBSkillLocks CreateLock(NetworkCommunicator peer)
        {
            Debug.Print("[Save Module] CREATING PLAYERLock TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));
            Debug.Print("Creating DBPlayerLock for " + peer.UserName);
            string insertQuery = "INSERT INTO skillslocks (Id, Weaving, WeaponSmithing, ArmourSmithing, BlackSmithing, Carpentry, Cooking, Farming, Mining, Fletching, Animals) VALUES (@Id, @Weaving, @ArmourSmithing, @WeaponSmithing, @BlackSmithing, @Carpentry, @Cooking, @Farming, @Mining, @Fletching, @Animals)";
            DBSkillLocks player = CreateDBLock(peer);
            DBConnection.Connection.Execute(insertQuery, player);
            Debug.Print("[Save Module] CREATED PLAYERLock TO DB " + (peer != null ? peer.UserName : "NETWORK COMMUNICATOR IS NULL !!!!"));

            return player;
        }
    }
}
