using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.Rendering;

namespace Ordered_Custom_Boombox.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class Boombox_start_music_patch
    {

        internal static int track_num = 1000000000;
        public static int next_track(int length)
        {
            track_num++;
            if (track_num >= length)
            {
                track_num = 0;
            }
            if (track_num < 0)
            {
                track_num = length - 1;
            }
            return track_num;
        }
        public static int prev_track(int length)
        {
            track_num--;
            if (track_num < 0)
            {
                track_num = length - 1;
            }
            if (track_num >= length)
            {
                track_num = 0;
            }
            return track_num;
        }

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        static void patch_boombox_start(ref BoomboxItem __instance)
        {
            __instance.itemProperties.syncInteractLRFunction = true;
            string[] new_toolTips = new string[__instance.itemProperties.toolTips.Length + 2];
            for (int i = 0; i < __instance.itemProperties.toolTips.Length; ++i)
            {
                new_toolTips[i] = __instance.itemProperties.toolTips[i];
            }
            new_toolTips[__instance.itemProperties.toolTips.Length] = "Prev Song: [Q]";
            new_toolTips[__instance.itemProperties.toolTips.Length + 1] = "Next Song: [E]";
            __instance.itemProperties.toolTips = new_toolTips;
        }



        [HarmonyPatch("StartMusic")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> patch_start_music(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; ++i)
            {
                if (codes[i].opcode == OpCodes.Ldfld && (System.Reflection.FieldInfo)codes[i].operand == AccessTools.Field(typeof(BoomboxItem), "musicRandomizer"))
                {
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i - 1].ToString());
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i+1].ToString());
                    codes[i - 1] = new CodeInstruction(OpCodes.Nop);
                    codes[i] = new CodeInstruction(OpCodes.Nop);
                    codes[i+1] = new CodeInstruction(OpCodes.Nop);
                }
                else if (codes[i].Calls(typeof(Random).GetMethod("Next", new Type[] { typeof(Int32), typeof(Int32) })))
                {
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    var classtype = typeof(Boombox_start_music_patch);
                    var funcname = nameof(Boombox_start_music_patch.next_track);
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(classtype, funcname, new Type[] { typeof(Int32) }));
                    Ordered_custom_boombox_base.LogInfo("With: " + codes[i].ToString());
                }
            }

            return codes;
        }
    }
}


