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
using PersistentEmpiresLib.SceneScripts;

namespace PersistentEmpiresSave.Database.Repositories
{
    public class DBWorkshopRepository
    {
        public static void Initialize()
        {
            SaveSystemBehavior.OnGetWorkshops += GetWorkshops;
            SaveSystemBehavior.OnCreateOrSaveWorkshop += CreateOrSaveWorkshop;
            SaveSystemBehavior.OnGetWorkshop += GetWorkshop;
        }
        public static IEnumerable<DBWorkshops> GetWorkshops()
        {
            return DBConnection.Connection.Query<DBWorkshops>("SELECT * FROM workshops");
        }
        private static DBWorkshops CreateDBWorkshop(PE_Workshop Workshop, int workshopindex)
        {
            return new DBWorkshops
            {
               WorkshopIndex = workshopindex,
               WorkshoptypeIndex = Workshop.CurrentTierIndex
            };
        }
        public static DBWorkshops GetWorkshop(int WorkshopIndex)
        {
            IEnumerable<DBWorkshops> factions = DBConnection.Connection.Query<DBWorkshops>("SELECT * FROM workshops WHERE WorkshopIndex = @WorkshopIndex", new { WorkshopIndex = WorkshopIndex });
            if (factions.Count() == 0) return null;
            return factions.First();
        }
        public static DBWorkshops CreateOrSaveWorkshop(PE_Workshop workshop, int workshopindex)
        {
            if (GetWorkshop(workshopindex) == null)
            {
                return CreateWorkshop(workshop, workshopindex);
            }
            return SaveWorkshop(workshop, workshopindex);
        }
        public static DBWorkshops CreateWorkshop(PE_Workshop workshop, int workshopindex)
        {
            DBWorkshops dbFaction = CreateDBWorkshop(workshop, workshopindex);
            string insertSql = "INSERT INTO workshops (WorkshopIndex, WorkshopTypeIndex) VALUES (@WorkshopIndex, @WorkshoptypeIndex)";
            DBConnection.Connection.Execute(insertSql, dbFaction);
            return dbFaction;
        }
        public static DBWorkshops SaveWorkshop(PE_Workshop workshop, int workshopindex)
        {
            DBWorkshops dbFaction = CreateDBWorkshop(workshop, workshopindex);
            string updateSql = "UPDATE workshops SET WorkshopIndex = @WorkshopIndex, WorkshopTypeIndex = @WorkshopTypeIndex WHERE WorkshopIndex = @WorkshopIndex";
            DBConnection.Connection.Execute(updateSql, dbFaction);
            return dbFaction;
        }
    }
}
