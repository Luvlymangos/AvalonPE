using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public class BanditAgentType
    {
        public int GuardID { get; set; }
        public string GuardType { get; set; }
        public string GuardClass { get; set; }

        public List<int> LordSoundEvents = new List<int>();
        public List<int> FriendSoundEvents = new List<int>();
        public List<int> NeutralSoundEvents = new List<int>();

        public BanditAgentType(int id, string guardType, string guardClass, List<int> lordSoundEvents, List<int> friendSoundEvents, List<int> neutralSoundEvents)
        {
            GuardID = id;
            GuardType = guardType;
            GuardClass = guardClass;
            LordSoundEvents = lordSoundEvents;
            FriendSoundEvents = friendSoundEvents;
            NeutralSoundEvents = neutralSoundEvents;
        }
    }
}
