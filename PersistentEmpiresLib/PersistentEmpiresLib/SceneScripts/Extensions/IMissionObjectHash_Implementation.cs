﻿using PersistentEmpiresLib.Helpers;
using PersistentEmpiresLib.SceneScripts.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Library;

namespace PersistentEmpiresLib.SceneScripts.Extensions
{
    public static class IMissionObjectHash_Implementation
    {
        public static string GetMissionObjectHash(this IMissionObjectHash missionObjectHash)
        {
            MatrixFrame frame = missionObjectHash.GetMissionObject().GameEntity.GetGlobalFrame();
            float x = frame.origin.X;
            float y = frame.origin.Y;
            float z = frame.origin.Z;

            string toHashed = x + "," + y + "," + z;
            return CryptoHelper.GetHashString(toHashed);
        }
    }
}
