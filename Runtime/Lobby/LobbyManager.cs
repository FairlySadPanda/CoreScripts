using FairlySadProductions.CoreScripts.Scripts.Base;
using FairlySadProductions.CoreScripts.Scripts.Game;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.CoreScripts.Scripts.Lobby
{
    /// <summary>
    /// LobbyManager manages a player lobby: a collection of players that have signed up to do something.
    /// </summary>
    public abstract class LobbyManager : NetworkedUdonSharpBehaviour
    {
        [SerializeField, Range(1,100)] private int maxLobbySize;
        [SerializeField] private GameManager manager;
        [UdonSynced] protected int[] players;
        private int numberOfSignedUpPlayers;

        [UdonSynced] private bool isLocked;

        public void Start()
        {
            _Reset();
        }

        public void AddPlayer(int playerID)
        {
            if (isLocked)
            {
                return;
            }
            
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }
            
            var firstEmptyIndex = 0;
            
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == playerID)
                {
                    return;
                }
                
                if (players[i] <= 0)
                {
                    firstEmptyIndex = i;
                }
            }

            players[firstEmptyIndex] = playerID;
            
            RequestSerialization();
        }

        public void RemovePlayer(int playerID)
        {
            if (isLocked)
            {
                return;
            }
            
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }
            
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] == playerID)
                {
                    players[i] = 0;
                    RequestSerialization();
                    
                    return;
                }
            }
        }

        public virtual void TryToStart()
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }

            var gamePlayers = new int[numberOfSignedUpPlayers];
            var i = 0;
            foreach (var id in players)
            {
                if (id > 0)
                {
                    Debug.Log($"{id} is playing the game!");
                    
                    gamePlayers[i] = id;
                    i++;
                }

                if (i >= gamePlayers.Length)
                {
                    break;
                }
            }
            
            Debug.Log($"Trying to start game with {gamePlayers.Length} players");
            manager.TryStartGameWith(gamePlayers);
        }

        public void _Lock()
        {
            if (Networking.IsOwner(gameObject) && !isLocked)
            {
                isLocked = true;
                RequestSerialization();
            }
        }

        public void _Unlock()
        {
            if (Networking.IsOwner(gameObject) && isLocked)
            {
                isLocked = false;
                RequestSerialization();
            }
        }

        public void _Reset()
        {
            players = new int[maxLobbySize];
            
            if (Networking.IsOwner(gameObject))
            {
                RequestSerialization();
            }
        }

        protected sealed override void OnNetworkUpdate()
        {
            numberOfSignedUpPlayers = 0;
            foreach (var id in players)
            {
                if (id > 0)
                {
                    numberOfSignedUpPlayers++;
                }
            }
            
            Debug.Log($"{numberOfSignedUpPlayers} are signed up");
            UpdatePlayersView();
        }

        protected abstract void UpdatePlayersView();

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (player != null)
            {
                RemovePlayer(player.playerId);
            }
        }
    }
}