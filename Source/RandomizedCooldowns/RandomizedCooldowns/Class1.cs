using Verse;
using HarmonyLib;

namespace RandomCooldowns
{
    // Add a random value to ranged attack cooldowns.
    // Cooldown decreases (bonuses) are more common than increases, but also smaller.
    // Average (mean) cooldowns are the same as vanilla.
    // The change is not a multiplier, so it's most noticeable with faster weapons.
    // Works with most modded weapons, but does nothing if cooldowns are already very short.

    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            var harmony = new Harmony("iron_xides.random_cooldowns");
            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(Verb), "TryCastNextBurstShot")]
    class Patch
    {
        static void Postfix(Verb __instance)
        {
            // A lot of things that aren't ranged attacks use TryCastNextBurstShot.
            // Also, don't alter cooldown between burst shots.
            if (__instance.CasterIsPawn && !__instance.verbProps.nonInterruptingSelfCast 
                && __instance.verbProps.LaunchesProjectile && !__instance.Bursting)
            {
                Stance_Cooldown stance = __instance.CasterPawn.stances.curStance as Stance_Cooldown;
                // Don't modify extremely short cooldowns.
                if (stance != null && stance.ticksLeft > 29)
                {
                    int random_ticks = (int)RandomCooldownTicksCurve.Evaluate(Rand.Value);
                    stance.ticksLeft += random_ticks;
                }
            }
        }

        private static readonly SimpleCurve RandomCooldownTicksCurve = new SimpleCurve
        {
            // 70% of -21/2 + 30% of +49/2 = 0. Range doesn't include -22 or 50.
            {
                new CurvePoint(0.0f, -22),
                true
            },
            {
                new CurvePoint(0.7f, 0),
                true
            },
            {
                new CurvePoint(1.0f, 50),
                true
            }
        };

    }

}
