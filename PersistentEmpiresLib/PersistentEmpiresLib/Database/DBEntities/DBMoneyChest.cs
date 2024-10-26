using PersistentEmpiresLib.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBMoneyChest
    {
        public int Id { get; set; }
        public string MissionObjectHash { get; set; }
        public int Money { get; set; }
    }
}
