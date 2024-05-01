/* Auto-Stack Ammo
 *
 * SMAPI mod that automatically adds any new ammo to slingshots that
 * have the same type of ammo attached.
 * 
 * Copyright (C) 2024 Jonathan Feenstra
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Inventories;
using StardewValley.Tools;
using Object = StardewValley.Object;

namespace AutoStackAmmo;

internal sealed class ModEntry : Mod
{
    public override void Entry(IModHelper helper)
    {
        helper.Events.Player.InventoryChanged += OnInventoryChanged;
        helper.Events.World.ChestInventoryChanged += OnChestInventoryChanged;
    }

    private static void OnInventoryChanged(object? sender, InventoryChangedEventArgs e)
    {
        foreach (var item in e.Added)
        {
            if (IsSlingshotAmmo(item))
            {
                AddAmmoToSlingshots(Game1.player.Items, item);
            }
        }
    }
    
    private static void OnChestInventoryChanged(object? sender, ChestInventoryChangedEventArgs e)
    {
        foreach (var item in e.Added)
        {
            if (IsSlingshotAmmo(item))
            {
                AddAmmoToSlingshots(e.Chest.Items, item);
            }
        }
    }

    private static bool IsSlingshotAmmo(Item item)
    {
        switch (item.QualifiedItemId)
        {
            case "(O)378":
            case "(O)380":
            case "(O)382":
            case "(O)384":
            case "(O)386":
            case "(O)388":
            case "(O)390":
            case "(O)441":
                return true;
            default:
                if (item is not Object obj || obj.bigCraftable.Value)
                {
                    return false;
                }

                return item.Category is -5 or -79 or -75;
        }
    }

    private static void AddAmmoToSlingshots(Inventory inventory, Item ammo)
    {
        foreach (var item in inventory)
        {
            if (item is not Slingshot slingshot) continue;
            var currentAmmo = slingshot.attachments[0];
            if (currentAmmo?.ItemId != ammo.ItemId) continue;
            var newStackSize = currentAmmo.Stack + ammo.Stack;
            var maxSize = currentAmmo.maximumStackSize();
            if (newStackSize > maxSize)
            {
                currentAmmo.Stack = maxSize;
                ammo.Stack = newStackSize - maxSize;
            }
            else
            {
                currentAmmo.Stack = newStackSize;
                inventory.RemoveButKeepEmptySlot(ammo);
                return;
            }
        }
    }
}