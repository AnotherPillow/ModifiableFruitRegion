# ModifiableFruitRegion

A mod to allow Content Patcher mods edit fruit cave spawning region.

## How to use

Patch the FarmCave map with a `FruitSpawningRegion` map property.
It is made up of four numbers (such as `5 8 5 8`). The first two are for the X range and the last two are for the Y range.

These are used in `Game1.random.Next`, so it includes the first number but not the last. 
> A 32-bit signed integer that is greater than or equal to 0, and less than maxValue; that is, the range of return values ordinarily

Additionally, see [the example](\[CP\] MFR Example Mod/content.json).