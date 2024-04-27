using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;

namespace Ordered_Custom_Boombox.Patches
{
    [HarmonyPatch(typeof(GrabbableObject))]
    internal class Boombox_Grabbable_Item_patch
    {
        [HarmonyPatch("ItemInteractLeftRight")]
        [HarmonyPostfix]
        static void Patch_interact_left_right(ref GrabbableObject __instance, object[] __args)
        {
            if (__instance is BoomboxItem)
            {
                if ((bool)__args[0])
                {
                    ((BoomboxItem)__instance).ItemActivate(false);
                    ((BoomboxItem)__instance).ItemActivate(true);
                }
                else
                {
                    Boombox_start_music_patch.prev_track(((BoomboxItem)__instance).musicAudios.Length);
                    Boombox_start_music_patch.prev_track(((BoomboxItem)__instance).musicAudios.Length);
                    ((BoomboxItem)__instance).ItemActivate(false);
                    ((BoomboxItem)__instance).ItemActivate(true);

                }
            }
        }


        [HarmonyPatch("EquipItem")]
        [HarmonyPostfix]
        static void Patch_equip_item(ref GrabbableObject __instance, ref PlayerControllerB ___playerHeldBy)
        {
            if (__instance is BoomboxItem)
            {
                ___playerHeldBy.equippedUsableItemQE = true;
            }
        }


        [HarmonyPatch("DiscardItem")]
        [HarmonyPrefix]
        static void Patch_discard_item(ref GrabbableObject __instance, ref PlayerControllerB ___playerHeldBy)
        {
            if (__instance is BoomboxItem)
            {
                ___playerHeldBy.equippedUsableItemQE = false;
            }
        }
    }
}
