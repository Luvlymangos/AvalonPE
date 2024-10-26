using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Database.DBEntities
{
    public class DBHouses
    {
        public int Id { get; set; }
        public int HouseIndex { get; set; }
        public string LordId { get; set; }
        public string Marshalls { get; set; }
        public long RentEnd { get; set; }
        public bool IsRented { get; set; }
    }
}
