using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine.Rendering;

namespace Ordered_Custom_Boombox.Patches
{
    [HarmonyPatch(typeof(BoomboxItem))]
    internal class Boombox_start_music_patch
    {

        internal static Dictionary<ulong, int> track_num_map = new Dictionary<ulong, int>();
        public static int seek_track(ulong bbox_id, int seek_amount, int length)
        {
            if (!track_num_map.ContainsKey(bbox_id))
            {
                track_num_map.Add(bbox_id, 10000000);
            }
            track_num_map[bbox_id] += seek_amount;
            if (track_num_map[bbox_id] >= length)
            {
                track_num_map[bbox_id] = 0;
            }
            if (track_num_map[bbox_id] < 0)
            {
                track_num_map[bbox_id] = length - 1;
            }
            return track_num_map[bbox_id];
        }

        public static IEnumerable<ulong> get_all_bbox_ids()
        {
            foreach (var key in track_num_map.Keys)
            {
                Ordered_custom_boombox_base.LogInfo("key: " + key);
            }
            return track_num_map.Keys;
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
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i+1].ToString());
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Property(typeof(NetworkBehaviour), nameof(NetworkBehaviour.NetworkObjectId)).GetGetMethod());
                    codes[i+1] = new CodeInstruction(OpCodes.Ldc_I4_1);
                    Ordered_custom_boombox_base.LogInfo("With: " + codes[i].ToString());
                    Ordered_custom_boombox_base.LogInfo("With: " + codes[i+1].ToString());
                }
                else if (codes[i].Calls(typeof(Random).GetMethod("Next", new Type[] { typeof(Int32), typeof(Int32) })))
                {
                    Ordered_custom_boombox_base.LogInfo("Replacing " + codes[i].ToString());
                    var classtype = typeof(Boombox_start_music_patch);
                    var funcname = nameof(Boombox_start_music_patch.seek_track);
                    codes[i] = new CodeInstruction(OpCodes.Call, AccessTools.Method(classtype, funcname, new Type[] { typeof(ulong), typeof(Int32), typeof(Int32) }));
                    Ordered_custom_boombox_base.LogInfo("With: " + codes[i].ToString());
                }
            }

            return codes;
        }
    }
}


