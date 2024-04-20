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
        public static int next_track(int unused, int length)
        {
            track_num++;
            if (track_num >= length)
            {
                track_num = 0;
            }
            return track_num;
        }

        [HarmonyPatch("StartMusic")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> patch_start_music(IEnumerable<CodeInstruction> instructions)
        {
            int a = 0;
            int b = 1;
            int c = b + a;
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; ++i)
            {
                // todo don't hardcode these indices
                if (codes[i].opcode == OpCodes.Ldfld && i == 7)
                {
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i - 1].ToString());
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    codes[i - 1] = new CodeInstruction(OpCodes.Nop);
                    codes[i] = new CodeInstruction(OpCodes.Nop);
                }
                else if (codes[i].Calls(typeof(Random).GetMethod("Next", new Type[] { typeof(Int32), typeof(Int32) })))
                {
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    var classtype = typeof(Boombox_start_music_patch);
                    var funcname = nameof(Boombox_start_music_patch.next_track);
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(classtype, funcname, new Type[] { typeof(Int32), typeof(Int32) }));
                    Ordered_custom_boombox_base.LogInfo("With: " + codes[i].ToString());
                }
            }

            return codes;
        }
    }
}


