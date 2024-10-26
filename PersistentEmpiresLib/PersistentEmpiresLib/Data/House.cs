using PersistentEmpiresLib.SceneScripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace PersistentEmpiresLib.Factions
{
    public class House
    {   // virtual player id
        public string lordId { get; set; }
        public long rentEnd { get; set; }
        // virtual player id's
        public bool isrented { get; set; }
        public List<string> marshalls { get; set; }
        public int HouseIndex { get; set; }

        public PE_PlayerHouse house { get; set; }


        public string SerializeMarshalls()
        {
            return string.Join("|", this.marshalls);
        }

        public List<string> LoadMarshallsFromSerialized(string serialized)
        {
            if (serialized == null) return new List<string>();
            else return serialized.Split('|').ToList<string>();
        }


        public House(string Lord, string Marshalls, long End, PE_PlayerHouse house, bool isrented)
        {
            this.lordId = Lord;
            this.marshalls = LoadMarshallsFromSerialized(Marshalls);
            this.rentEnd = End;
            this.house = house;
            this.isrented = isrented;
        }

        public House()
        { this.lordId = "0"; this.marshalls = new List<string>(); this.rentEnd = 0; this.isrented = false; }
    }
}
