using System;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Locations;
using xTile;
using xTile.ObjectModel;
using SObject = StardewValley.Object;

namespace ModifiableFruitRegion
{
    internal sealed class ModEntry : Mod
    {
        private static IMonitor _monitor = null!;
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony(this.ModManifest.UniqueID);

            // example patch, you'll need to edit this for your patch
            harmony.Patch(
               original: AccessTools.Method(typeof(FarmCave), nameof(FarmCave.DayUpdate)),
               prefix: new HarmonyMethod(typeof(ModEntry), nameof(DayUpdate_Prefix))
            );

            ModEntry._monitor = Monitor;

        }

        internal static bool DayUpdate_Prefix(int dayOfMonth, StardewValley.Locations.FarmCave __instance)
        {
            try
            {
                /*
                 * Adapted from https://github.com/janxious/BT-WeaponRealizer/blob/89defeb47e9d45f144dc9013d98f31f879ae76ba/NumberOfShotsEnabler.cs#L24 (MIT LIcensed)
                 * Modified with help with Selph and Button from the stardew discord.
                 */
                var method = typeof(GameLocation).GetMethod("DayUpdate", AccessTools.all);
                if (method is null)
                {
                    _monitor.Log($"Failed to find base method in {nameof(DayUpdate_Prefix)}. Falling back to original code.", LogLevel.Error);
                    return true;
                }
                var dm = new DynamicMethod("GameLocationUpdate", typeof(void), new Type[] { typeof(FarmCave), typeof(int) }, typeof(FarmCave));
                var gen = dm.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Call, method);
                gen.Emit(OpCodes.Ret);

                var GameLocationUpdate = (Action<FarmCave, int>)dm.CreateDelegate(typeof(Action<FarmCave, int>));
                GameLocationUpdate(__instance, dayOfMonth);
                if (Game1.MasterPlayer.caveChoice.Value == 1)
                {
                    while (Game1.random.NextDouble() < 0.66)
                    {
                        string fruitId = Game1.random.Next(5) switch
                        {
                            0 => "296",
                            1 => "396",
                            2 => "406",
                            3 => "410",
                            _ => (Game1.random.NextDouble() < 0.1) ? "613" : Game1.random.Next(634, 639).ToString(),
                        };

                        PropertyValue customLocation = null!;
                        __instance.map.Properties.TryGetValue("FruitSpawningRegion", out customLocation!);

                        Vector2 v = new Vector2(
                            Game1.random.Next(1, // random between 1 & map width minus 1 - so theres 1 tile gap on each side
                                __instance.map.Layers[0].LayerWidth - 1),
                            Game1.random.Next(1, //random between 1 & map height minus 4 - so theres a 1 tile gap on the top and a 4 tile gap on the bottom
                                __instance.map.Layers[0].LayerHeight - 4));
                        if (customLocation != null)
                        {
                            string[] parts = customLocation.ToString().Split(" ");
                            if (parts.Length != 4)
                            {
                                _monitor.Log("Invalid FruitSpawningRegion value. Skipping fruit.", LogLevel.Error);
                                __instance.UpdateReadyFlag();
                                return false;
                            }

                            int[] numbers = parts.Select(int.Parse).ToArray();

                            v = new Vector2(
                                Game1.random.Next(numbers[0], numbers[1]), // x min & x max
                                Game1.random.Next(numbers[2], numbers[3]) // y min & y max
                            );

                            _monitor.Log(v.ToString(), LogLevel.Info);

                        }
                        SObject fruit = ItemRegistry.Create<SObject>("(O)" + fruitId);
                        fruit.IsSpawnedObject = true;
                        if (__instance.CanItemBePlacedHere(v)) // Confirm the area is placeable
                        {
                            __instance.setObject(v, fruit);
                        }
                    }
                }
                __instance.UpdateReadyFlag();
                return false;
            }
            catch (Exception ex)
            {
                _monitor.Log($"Failed in {nameof(DayUpdate_Prefix)}:\n{ex}", LogLevel.Error);
                return true;

            }
        }
    }
}