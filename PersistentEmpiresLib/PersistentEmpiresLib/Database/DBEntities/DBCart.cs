using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Database.DBEntities
{
    public class DBCart
    {
        public string Id { get; set; }
        public int PosX { get; set; }
        public int PosY { get; set; }
        public int PosZ { get; set; }
        public string Prefab { get; set; }
        public string InventoryID { get; set; }

    }
}
