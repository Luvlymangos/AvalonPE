using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using PersistentEmpiresLib.SceneScripts;
using PersistentEmpiresLib.SceneScripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBMoneyChestRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetAllMoneyChests += GetAllMoneyChests;
            SaveSystemBehavior.OnGetMoneyChest += GetMoneyChest;
            SaveSystemBehavior.OnCreateOrSaveMoneyChest += CreateOrSaveMoneyChest;
        }

        private static DBMoneyChest CreateDBMoneyChest(PE_MoneyChest moneychest)
        {
            Debug.Print("[Save Module] CREATE DB MONEY CHEST (" + moneychest != null ? " " + moneychest.GetMissionObjectHash() : "MONEY CHEST IS NULL !)");
            return new DBMoneyChest
            {
                MissionObjectHash = moneychest.GetMissionObjectHash(),
                Money = (int)moneychest.Gold
            };
        }
        public static IEnumerable<DBMoneyChest> GetAllMoneyChests()
        {
            Debug.Print("[Save Module] LOADING ALL MoneyChests FROM DB");
            return DBConnection.Connection.Query<DBMoneyChest>("SELECT * FROM moneychest");
        }
        public static DBMoneyChest GetMoneyChest(PE_MoneyChest moneychest)
        {
            Debug.Print("[Save Module] LOAD MONEY CHEST FROM DB (" + moneychest.GetMissionObjectHash() + ")");
            IEnumerable<DBMoneyChest> result = DBConnection.Connection.Query<DBMoneyChest>("SELECT * FROM moneychest WHERE MissionObjectHash = @MissionObjectHash", new { MissionObjectHash = moneychest.GetMissionObjectHash() });
            Debug.Print("[Save Module] LOAD MONEY CHEST FROM DB (" + moneychest.GetMissionObjectHash() + ") RESULT COUNT " + result.Count());
            if (result.Count() == 0) return null;
            return result.First();
        }
        public static DBMoneyChest CreateOrSaveMoneyChest(PE_MoneyChest moneychest)
        {

            if (GetMoneyChest(moneychest) == null)
            {
                return CreateMoneyChest(moneychest);
            }
            return SaveMoneyChest(moneychest);
        }
        public static DBMoneyChest CreateMoneyChest(PE_MoneyChest moneychest)
        {
            Debug.Print("[Save Module] CREATE MONEY CHEST TO DB (" + moneychest != null ? " " + moneychest.GetMissionObjectHash() : "MONEY CHEST IS NULL !)");
            DBMoneyChest dbmoneychest = CreateDBMoneyChest(moneychest);
            string insertQuery = "INSERT INTO moneychest (MissionObjectHash, Money) VALUES (@MissionObjectHash, @Money)";
            DBConnection.Connection.Execute(insertQuery, dbmoneychest);
            Debug.Print("[Save Module] CREATED MONEY CHEST TO DB (" + moneychest != null ? " " + moneychest.GetMissionObjectHash() : "MONEY CHEST IS NULL !)");
            return dbmoneychest;
        }

        public static DBMoneyChest SaveMoneyChest(PE_MoneyChest moneychest)
        {
            Debug.Print("[Save Module] UPDATING MONEY CHEST TO DB (" + moneychest != null ? " " + moneychest.GetMissionObjectHash() : "MONEY CHEST IS NULL !)");
            DBMoneyChest dbmoneychest = CreateDBMoneyChest(moneychest);
            string insertQuery = "UPDATE moneychest SET Money = @Money WHERE MissionObjectHash = @MissionObjectHash";
            DBConnection.Connection.Execute(insertQuery, dbmoneychest);
            Debug.Print("[Save Module] UPDATED MONEY CHEST TO DB (" + moneychest != null ? " " + moneychest.GetMissionObjectHash() : "MONEY CHEST IS NULL !)");
            return dbmoneychest;
        }

    }
}
