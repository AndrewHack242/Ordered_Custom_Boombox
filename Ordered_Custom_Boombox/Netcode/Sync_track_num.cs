using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Collections;
using Unity.Netcode;
using Ordered_Custom_Boombox;
using Zeekerss.Core.Singletons;

namespace Ordered_Custom_Boombox.Netcode
{

    [HarmonyPatch]
    public static class Sync_track_num
    {
        internal static int get_track_num()
        {
            return Ordered_Custom_Boombox.Patches.Boombox_start_music_patch.track_num;
        }
        internal static void set_track_num(int new_track_num)
        {
            Ordered_Custom_Boombox.Patches.Boombox_start_music_patch.track_num = new_track_num;
        }

        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        public static void Init()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                Ordered_custom_boombox_base.LogInfo("Set up track number sync in server");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Ordered_Custom_Boombox.Receive_server_set_track_num_rpc", Receive_server_set_track_num_rpc);
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                Ordered_custom_boombox_base.LogInfo("Set up track number sync in client");
                NetworkManager.Singleton.CustomMessagingManager.RegisterNamedMessageHandler("Ordered_Custom_Boombox.Receive_client_set_track_num_rpc", Receive_client_set_track_num_rpc);
            }
            Client_send_sync_request_to_server();
        }

        public static void Client_send_sync_request_to_server()
        {
            if (!NetworkManager.Singleton.IsClient)
            {
                Ordered_custom_boombox_base.LogWarning("[Client_send_sync_request_to_server] Not a client?");
                return;
            }
            if (NetworkManager.Singleton.IsServer)
            {
                Server_send_track_num_sync(get_track_num());
                return;
            }
            Ordered_custom_boombox_base.LogInfo("Requesting track num sync from server");
            FastBufferWriter messageStream = new FastBufferWriter(4, Allocator.Temp);
            int value = get_track_num();
            messageStream.WriteValue(in value, default(FastBufferWriter.ForPrimitives));
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessage("Ordered_Custom_Boombox.Receive_server_set_track_num_rpc", 0uL, messageStream);
        }

        private static void Receive_server_set_track_num_rpc(ulong clientId, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }
            Server_send_track_num_sync(get_track_num());
        }

        public static void Server_send_track_num_sync(int server_track_num)
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                Ordered_custom_boombox_base.LogWarning("[Server_send_track_num_sync] Only the server can call this method!");
                return;
            };
            Ordered_custom_boombox_base.LogInfo("Syncing track num of " + server_track_num + " with clients");
            FastBufferWriter messageStream = new FastBufferWriter(4, Allocator.Temp);
            messageStream.WriteValue(in server_track_num, default(FastBufferWriter.ForPrimitives));
            NetworkManager.Singleton.CustomMessagingManager.SendNamedMessageToAll("Ordered_Custom_Boombox.Receive_client_set_track_num_rpc", messageStream);
        }

        private static void Receive_client_set_track_num_rpc(ulong clientId, FastBufferReader reader)
        {
            if (!NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                return;
            }
            reader.ReadValue(out int new_track_num, default(FastBufferWriter.ForPrimitives));
            Ordered_custom_boombox_base.LogInfo("setting track number for client via rpc: " + new_track_num);
            set_track_num(new_track_num);
        }
    }
}
