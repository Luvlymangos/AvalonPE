﻿using PersistentEmpiresLib.PersistentEmpiresMission;
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
    public class DBPlayer
    {
        public int Id { get; set; }
        public string PlayerId { get; set; }
        public string DiscordId { get; set; }
        public string Name { get; set; }
        public int BankAmount { get; set; }
        public int Hunger { get; set; }
        public int Health { get; set; }
        public int Money { get; set; }
        public int FactionIndex { get; set; }
        public string Class { get; set; }
        public string Horse { get; set; }
        public string HorseHarness { get; set; }
        public string Equipment_0 { get; set; }
        public string Equipment_1 { get; set; }
        public string Equipment_2 { get; set; }
        public string Equipment_3 { get; set; }

        public int Ammo_0 { get; set; }
        public int Ammo_1 { get; set; }
        public int Ammo_2 { get; set; }
        public int Ammo_3 { get; set; }
        public string Armor_Head { get; set; }
        public string Armor_Body { get; set; }
        public string Armor_Leg { get; set; }
        public string Armor_Gloves { get; set; }
        public string Armor_Cape { get; set; }
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        public string CustomName { get; set; }
        public int Weaving { get; set; }
        public int WeaponSmithing { get; set; }
        public int ArmourSmithing { get; set; }
        public int BlackSmithing { get; set; }
        public int Carpentry { get; set; }
        public int Cooking { get; set; }
        public int Farming { get; set; }
        public int Mining { get; set; }
        public int Fletching { get; set; }
        public int Animals { get; set; }

    }
}
