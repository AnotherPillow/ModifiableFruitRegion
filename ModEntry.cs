using System;
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
                __instance.DayUpdate(dayOfMonth);
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
                        __instance.map.Properties.TryGetValue("FruitArea", out customLocation!);

                        Vector2 v = null!;
                        if (customLocation is null)
                        {
                            v = new Vector2(Game1.random.Next(1, __instance.map.Layers[0].LayerWidth - 1), Game1.random.Next(1, __instance.map.Layers[0].LayerHeight - 4));
                        } else
                        {

                        }
                        SObject fruit = ItemRegistry.Create<SObject>("(O)" + fruitId);
                        fruit.IsSpawnedObject = true;
                        if (__instance.CanItemBePlacedHere(v))
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