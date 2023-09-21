﻿using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace MagiTronics.Tiles
{
    internal class TERedirector : ModTileEntity
    {

        private static TERedirector workingTE;

        public List<Point16> wiredTerminals = new();

        public Point16 Target()
        {
            UpdateTarget();
            Point16 target = Point16.NegativeOne;
            Item item = Main.LocalPlayer.HeldItem;
            if (item.IsAir) return target;
            foreach (Point16 point in wiredTerminals)
            {
                Tile tile = Main.tile[point.X, point.Y];
                ushort type = tile.TileType;
                Tile tileLeft = Main.tile[point.X - 1, point.Y];
                Tile tileRight = Main.tile[point.X + 1, point.Y];
                Tile tileDown = Main.tile[point.X, point.Y + 1];
                Tile tileUp = Main.tile[point.X, point.Y - 1];

                if (Main.tileAxe[type])
                {
                    if (item.axe > 0)
                    {
                        return point;
                    }
                } else if (Main.tileHammer[type])
                {
                    if (item.hammer > 0) { return point; }
                }
                else if (WorldGen.CanKillTile(point.X, point.Y) && item.pick > 0)
                {
                    return point;
                }
                else if(item.hammer > 0 && tile.WallType > 0)
                {
                    return point;
                }
                switch (item.type)
                {
                    case ItemID.LawnMower:
                        if (tile.HasUnactuatedTile && (type == TileID.Grass || type == TileID.HallowedGrass))
                        {
                            return point;
                        }
                        break;
                    case ItemID.StaffofRegrowth:
                    case ItemID.AcornAxe:
                        switch(type)
                        {
                            case TileID.MatureHerbs:
                            case TileID.BloomingHerbs:
                            case TileID.Dirt:
                                return point;
                        }
                        if (tile.HasTile) target = point;
                        break;
                }
                if(item.createWall > -1)
                {
                    if(WallLoader.CanPlace(point.X, point.Y, item.createWall) && Main.tile[point.X, point.Y].WallType != item.createWall)
                    {
                        if (Main.tile[point.X+1, point.Y].HasTile || Main.tile[point.X+1, point.Y].WallType > 0
                            || Main.tile[point.X, point.Y + 1].HasTile || Main.tile[point.X, point.Y + 1].WallType > 0
                            || Main.tile[point.X - 1, point.Y].HasTile || Main.tile[point.X - 1, point.Y].WallType > 0
                            || Main.tile[point.X, point.Y - 1].HasTile || Main.tile[point.X, point.Y - 1].WallType > 0)
                        {
                            if (Main.tile[point.X, point.Y].WallType > 0)
                            {
                                if (Main.LocalPlayer.TileReplacementEnabled
                                    && WorldGen.NearFriendlyWall(point.X, point.Y)
                                    && !(Main.wallDungeon[Main.tile[point.X, point.Y].WallType] && !NPC.downedBoss3)
                                    && !(Main.tile[point.X, point.Y].WallType == WallID.LihzahrdBrickUnsafe && !NPC.downedGolemBoss))
                                {
                                    return point;
                                }
                            }
                            else
                            {
                                return point;
                            }
                        }
                    }
                }
                int createTile = item.createTile;
                if(item.tileWand > -1)
                    createTile = item.tileWand;
                if(createTile > -1 && tile.HasTile && !Main.tileCut[type]
                    && !TileID.Sets.BreakableWhenPlacing[type])
                {
                    if (Main.tileMoss[createTile])
                    {
                        if (type == TileID.Stone || type == TileID.GrayBrick)
                        {
                            return point;
                        }
                        else continue;
                    }
                    switch(createTile)
                    {

                        case TileID.Grass:
                        case TileID.HallowedGrass:
                            if (type == TileID.Dirt)
                                return point; break;
                        case TileID.CorruptGrass:
                        case TileID.CrimsonGrass:
                            if (type == TileID.Grass || type == TileID.Mud)
                                return point;
                            break;
                        case TileID.JungleGrass:
                        case TileID.MushroomGrass:
                        case TileID.CorruptJungleGrass:
                        case TileID.CrimsonJungleGrass:
                            if (type == TileID.Mud)
                                return point;
                            break;
                        default:
                            if(Main.LocalPlayer.TileReplacementEnabled)
                            {
                                Item bestPickaxe = Main.LocalPlayer.GetBestPickaxe();
                                if (bestPickaxe != null
                                    && !WorldGen.WouldTileReplacementBeBlockedByLiquid(point.X, point.Y, LiquidID.Lava)
                                    && ItemID.Sets.SortingPriorityRopes[item.type] == -1
                                    && !Main.tileMoss[createTile]
                                    && !TileID.Sets.DoesntPlaceWithTileReplacement[createTile]
                                    && !TileID.Sets.DoesntGetReplacedWithTileReplacement[type]
                                    && !(TileID.Sets.Falling[createTile]
                                        && !TileID.Sets.Falling[type]
                                        && bestPickaxe.pick < 110 
                                        && tileUp.HasTile
                                        && TileID.Sets.Falling[tileUp.TileType])
                                    && !(type == TileID.CrispyHoneyBlock && Main.getGoodWorld)
                                    && WorldGen.WouldTileReplacementWork((ushort)createTile, point.X, point.Y)
                                    && WorldGen.IsTileReplacable(point.X, point.Y)
                                    && TileLoader.CanReplace(point.X, point.Y, type, createTile)
                                    && TileLoader.CanPlace(point.X, point.Y, createTile)
                                    )
                                {
                                    if (createTile == type)
                                    {
                                        int style = item.placeStyle;
                                        int framey = tile.TileFrameY;
                                        if ((TileID.Sets.Platforms[type] && framey != style * 18)
                                            || (TileID.Sets.Torch[type] && TileID.Sets.Torch[createTile] && framey != style * 22)
                                            || (TileID.Sets.Campfire[type] && TileID.Sets.Campfire[createTile] && framey != style * 54)
                                            || (TileID.Sets.BasicChest[type] && TileID.Sets.BasicChest[createTile] && framey != style * 36)
                                            || (TileID.Sets.BasicDresser[type] && TileID.Sets.BasicDresser[createTile] && framey != style * 54)
                                            )
                                        {
                                            return point;
                                        }
                                    }
                                    else return point;
                                }
                            }
                        break;
                    }
                }
                if (createTile > -1 && 
                    (!tile.HasTile || Main.tileCut[type]
                    || TileID.Sets.BreakableWhenPlacing[type]))
                {
                    

                    if(TileObjectData.CustomPlace(createTile, item.placeStyle))
                    {
                        if(TileObject.CanPlace(point.X, point.Y, createTile, item.placeStyle, Main.LocalPlayer.direction, out TileObject tileObject))
                        {

                            bool canPlace = true;
                            switch (createTile)
                            {
                                case TileID.Pigronata:
                                    for (int i = -2; i < 2; i++)
                                    {
                                        Tile pigTile = Main.tile[Player.tileTargetX + i, Player.tileTargetY];
                                        if (tile.HasTile && tile.TileType == 454)
                                        {
                                            canPlace = false;
                                        }
                                    }
                                    break;
                                case TileID.Pumpkins:
                                    for (int i = -1; i < 1; i++)
                                    {
                                        for (int j = 0; j < 2; j++)
                                        {
                                            if (!WorldGen.CanCutTile(Player.tileTargetX + j, Player.tileTargetY + i, TileCuttingContext.TilePlacement))
                                            {
                                                canPlace = false;
                                            }
                                        }
                                    }
                                    break;
                                case TileID.Coral:
                                case TileID.BeachPiles:
                                    if (tile.HasTile && (Main.tileCut[type] || TileID.Sets.BreakableWhenPlacing[type] || (type >= 373 && type <= 375) || type == 461))
                                    {
                                        canPlace = false;
                                    }
                                    break;

                            }
                            if (canPlace) return point;
                        }
                        continue;
                    }

                    if (TileID.Sets.Torch[createTile] || createTile == TileID.Switches)
                    {
                        

                        if ((!TileID.Sets.AllowLightInWater[Main.LocalPlayer.BiomeTorchHoldStyle(type)] || tile.LiquidType <= 0) &&
                        (tile.WallType > 0 ||
                        (tileLeft.HasTile && (tileLeft.Slope == SlopeType.Solid || tileLeft.Slope == SlopeType.SlopeDownRight || tileLeft.Slope == SlopeType.SlopeUpRight) && ((Main.tileSolid[tileLeft.TileType] && !Main.tileNoAttach[tileLeft.TileType] && !Main.tileSolidTop[tileLeft.TileType] && !TileID.Sets.NotReallySolid[tileLeft.TileType]) || TileID.Sets.IsBeam[tileLeft.TileType] || (WorldGen.IsTreeType(tileLeft.TileType) && WorldGen.IsTreeType(Main.tile[point.X - 1, point.Y - 1].TileType) && WorldGen.IsTreeType(Main.tile[point.X - 1, point.Y + 1].TileType))))
                        || (tileRight.HasTile && (tileRight.Slope == 0 || tileRight.Slope == SlopeType.SlopeDownLeft || tileRight.Slope == SlopeType.SlopeUpLeft) && ((Main.tileSolid[tileRight.TileType] && !Main.tileNoAttach[tileRight.TileType] && !Main.tileSolidTop[tileRight.TileType] && !TileID.Sets.NotReallySolid[tileRight.TileType]) || TileID.Sets.IsBeam[tileRight.TileType] || (WorldGen.IsTreeType(tileRight.TileType) && WorldGen.IsTreeType(Main.tile[point.X + 1, point.Y - 1].TileType) && WorldGen.IsTreeType(Main.tile[point.X + 1, point.Y + 1].TileType))))
                        || (tileDown.HasTile && Main.tileSolid[tileDown.TileType] && !Main.tileNoAttach[tileDown.TileType] && (!Main.tileSolidTop[tileDown.TileType] || (TileID.Sets.Platforms[tileDown.TileType] && tileDown.Slope == 0)) && !TileID.Sets.NotReallySolid[tileDown.TileType] && !tileDown.IsHalfBlock && tileDown.Slope == 0))
                        && !TileID.Sets.Torch[tile.TileType])
                        {
                            Main.NewText("torch spot: "+point);
                            return point;
                        }
                    } else if (TileID.Sets.Platforms[createTile])
                    {
                        for(int i = point.X -1; i <= point.X + 1; i++)
                            for(int j = point.Y -1; j <= point.Y + 1; j++)
                            {
                                if (Main.tile[i, j].HasTile)
                                    return point;
                            }
                    }
                    switch (createTile)
                    {
                        case TileID.Plants:
                            if (WorldGen.IsFitToPlaceFlowerIn(point.X, point.Y, TileID.Plants))
                                return point;
                            break;
                        case TileID.WaterDrip:
                        case TileID.LavaDrip:
                        case TileID.HoneyDrip:
                        case TileID.SandDrip:
                            if (tileUp.HasUnactuatedTile && !Main.tileSolidTop[tileUp.TileType])
                                return point;
                            break;
                        case TileID.SkullLanterns:
                        case TileID.ClayPot:
                        case TileID.Candelabras:
                        case TileID.PlatinumCandelabra:
                        case TileID.PlatinumCandle:
                        case TileID.BeachPiles:
                            if(tileDown.HasUnactuatedTile && (Main.tileSolid[tileDown.TileType] || Main.tileTable[tileDown.TileType]))
                                return point;
                            break;
                        case TileID.LogicGateLamp:
                            if(tileDown.HasTile && (tileDown.TileType == TileID.LogicGateLamp || (item.placeStyle != 2 && tileDown.TileType == TileID.LogicGate)))
                                return point;
                            break;
                        case TileID.Bottles:
                        case TileID.PiggyBank:
                        case TileID.Candles:
                        case TileID.WaterCandle:
                        case TileID.Books:
                        case TileID.Bowls:
                            if(tileDown.HasUnactuatedTile && Main.tileTable[tileDown.TileType])
                                return point;
                            break;
                        case TileID.Cobweb:
                        case TileID.CopperCoinPile:
                        case TileID.SilverCoinPile:
                        case TileID.GoldCoinPile:
                        case TileID.PlatinumCoinPile:
                        case TileID.LivingFire:
                        case TileID.LivingCursedFire:
                        case TileID.LivingDemonFire:
                        case TileID.LivingFrostFire:
                        case TileID.LivingIchor:
                        case TileID.LivingUltrabrightFire:
                        case TileID.Bubble:
                        case TileID.ChimneySmoke:
                            if (tileLeft.HasTile || tileLeft.WallType > 0
                                || tileRight.HasTile || tileRight.WallType > 0
                                || tileUp.HasTile || tileUp.WallType > 0
                                || tileDown.HasTile || tileDown.WallType > 0)
                                return point;
                            break;
                        case TileID.MinecartTrack:
                            for (int i = point.X - 1; i <= point.X + 1; i++)
                                for (int j = point.Y - 1; j <= point.Y + 1; j++)
                                    if (Main.tile[i, j].HasTile || Main.tile[i, j].WallType > 0)
                                        return point;
                            break;
                        default:
                            if (tile.WallType > 0)
                                return point;
                            if (tileRight.HasTile && (Main.tileSolid[tileRight.TileType] || TileID.Sets.IsBeam[tileRight.TileType] || Main.tileRope[tileRight.TileType]) || tileRight.WallType > 0
                                || tileLeft.HasTile && (Main.tileSolid[tileLeft.TileType] || TileID.Sets.IsBeam[tileLeft.TileType] || Main.tileRope[tileLeft.TileType]) || tileLeft.WallType > 0
                                || tileUp.HasTile && (Main.tileSolid[tileUp.TileType] || TileID.Sets.IsBeam[tileUp.TileType] || Main.tileRope[tileUp.TileType]) || tileUp.WallType > 0
                                || tileDown.HasTile && (Main.tileSolid[tileDown.TileType] || TileID.Sets.IsBeam[tileDown.TileType] || Main.tileRope[tileDown.TileType]) || tileDown.WallType > 0)
                                return point;
                            break;
                    
                    }
                }
            }
            return target;
        }

        private void UpdateTarget()
        {
            workingTE = this;
            wiredTerminals.Clear();
            TerminalChecker.TripWire(Position.X, Position.Y, 2, 2);
            workingTE = null;
        }

        public static void registerTerminal(Point16 p)
        {
            workingTE?.wiredTerminals.Add(p);
        }


        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<Redirector>();
        }


        public override int Hook_AfterPlacement(int x, int y, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {

                // Sync the placement of the tile entity with other clients
                // The "type" parameter refers to the tile type which placed the tile entity, so "Type" (the type of the tile entity) needs to be used here instead
                NetMessage.SendData(MessageID.TileEntityPlacement, number: x, number2: y, number3: Type);
            }

            // ModTileEntity.Place() handles checking if the entity can be placed, then places it for you
            // Set "tileOrigin" to the same value you set TileObjectData.newTile.Origin to in the ModTile
            Point16 tileOrigin = new Point16(0, 1);
            int placedEntity = Place(x - tileOrigin.X, y - tileOrigin.Y);
            return placedEntity;
        }

        public override void OnNetPlace()
        {
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.TileEntitySharing, number: ID, number2: Position.X, number3: Position.Y);
            }
        }

        public static bool IsResting => workingTE is null;


    }
}
