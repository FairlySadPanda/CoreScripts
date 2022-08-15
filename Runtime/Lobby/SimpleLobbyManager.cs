using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.CoreScripts.Scripts.Lobby
{
    public class SimpleLobbyManager : LobbyManager
    {
        protected override void UpdatePlayersView()
        {
            foreach (var player in players)
            {
                VRCPlayerApi api = VRCPlayerApi.GetPlayerById(player);
                if (api == null)
                {
                    break;
                }
                
                Debug.Log($"Lobby member: {api.displayName}");
            }
        }

        public override void TryToStart()
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }
            
            var playerApis = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            playerApis = VRCPlayerApi.GetPlayers(playerApis);
            players = new int[playerApis.Length];

            for (int i = 0; i < players.Length; i++)
            {
                players[i] = playerApis[i].playerId;
            }
            
            base.TryToStart();
            
            RequestSerialization();
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player != null)
            {
                RemovePlayer(player.playerId);
            }
        }
    }
}