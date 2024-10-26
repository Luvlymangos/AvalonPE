using Dapper;
using PersistentEmpiresLib.Database.DBEntities;
using PersistentEmpiresLib.PersistentEmpiresMission.MissionBehaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersistentEmpiresLib.Factions;
using Database.DBEntities;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBHouseRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetHouses += GetHouses;
            SaveSystemBehavior.OnCreateOrSaveHouse += CreateOrSaveHouse;
            SaveSystemBehavior.OnGetHouse += GetHouse;
        }
        public static IEnumerable<DBHouses> GetHouses()
        {
            return DBConnection.Connection.Query<DBHouses>("SELECT * FROM houses");
        }
        private static DBHouses CreateDBHouse(House house, int houseindex)
        {
            return new DBHouses
            {
                HouseIndex = houseindex,
                LordId = house.lordId,
                Marshalls = house.SerializeMarshalls(),
                RentEnd = house.rentEnd,
                IsRented = house.isrented
            };
        }
        public static DBHouses GetHouse(int HouseIndex)
        {
            IEnumerable<DBHouses> factions = DBConnection.Connection.Query<DBHouses>("SELECT * FROM houses WHERE HouseIndex = @HouseIndex", new { HouseIndex = HouseIndex });
            if (factions.Count() == 0) return null;
            return factions.First();
        }
        public static DBHouses CreateOrSaveHouse(House house, int houseIndex)
        {
            if (GetHouse(houseIndex) == null)
            {
                return CreateHouse(house, houseIndex);
            }
            return SaveHouse(house, houseIndex);
        }
        public static DBHouses CreateHouse(House house, int houseindex)
        {
            DBHouses dbFaction = CreateDBHouse(house, houseindex);
            string insertSql = "INSERT INTO houses (HouseIndex, LordId, Marshalls, RentEnd, IsRented) VALUES (@HouseIndex, @LordID, @Marshalls, @RentEnd, @IsRented)";
            DBConnection.Connection.Execute(insertSql, dbFaction);
            return dbFaction;
        }
        public static DBHouses SaveHouse(House house, int houseindex)
        {
            DBHouses dbFaction = CreateDBHouse(house, houseindex);
            string updateSql = "UPDATE houses SET HouseIndex = @HouseIndex, LordId = @LordId, Marshalls = @Marshalls, RentEnd = @RentEnd, IsRented = @IsRented WHERE HouseIndex = @HouseIndex";
            DBConnection.Connection.Execute(updateSql, dbFaction);
            return dbFaction;
        }
    }
}
