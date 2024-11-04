using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersistentEmpiresAPI.DTO
{
    public class EditSkillsDTO
    {
        public string PlayerId { get; set; }
        public string Skillname { get; set; }
        public int SkillValue { get; set; }
    }
}
