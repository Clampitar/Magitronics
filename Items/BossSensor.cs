﻿using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace MagiTronics.Items
{
    internal class BossSensor : ModItem
    {
        public override void SetDefaults()
        {
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.BossSensor>();
            Item.mech = true;
            Item.ResearchUnlockCount = 5;
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.SoulofFright, 1);
            recipe.AddRecipeGroup(RecipeGroupID.IronBar, 1);
            recipe.AddIngredient(ItemID.Wire);
            recipe.AddTile(TileID.MythrilAnvil);
            recipe.Register();
        }
    }
}
