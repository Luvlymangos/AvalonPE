using PersistentEmpiresLib.PersistentEmpiresMission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace PersistentEmpiresLib.Database.DBEntities
{
    public class DBSkillLocks
    {
        public string Id { get; set; }
        public bool Weaving { get; set; }
        public bool WeaponSmithing { get; set; }
        public bool ArmourSmithing { get; set; }
        public bool BlackSmithing { get; set; }
        public bool Carpentry { get; set; }
        public bool Cooking { get; set; }
        public bool Farming { get; set; }
        public bool Mining { get; set; }
        public bool Fletching { get; set; }
        public bool Animals { get; set; }

    }
}
