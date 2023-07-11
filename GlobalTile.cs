﻿using Terraria.ID;
using Terraria;
using Terraria.DataStructures;
using MagiTronics.Tiles;

namespace MagiTronics
{
    internal class GlobalTile : Terraria.ModLoader.GlobalTile
    {
        public override void HitWire(int i, int j, int type)
        {
            MagicWiring.HitwireChest(i, j);
            Point16 p = new Point16(i, j);
            if(MagitronicsWorld.modedActuators.Contains(p))
            {
                TERedirector.registerTerminal(p);
            }
        }
    }
}
