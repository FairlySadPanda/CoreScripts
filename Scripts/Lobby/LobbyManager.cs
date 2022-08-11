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
        [SerializeField] private int maxLobbySize;
        [SerializeField] private GameManager manager;
        [UdonSynced] protected int[] players;

        public void Start()
        {
            _Reset();
        }

        public void AddPlayer(int playerID)
        {
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

            Debug.Log($"Trying to start game with {players.Length}");
            manager.TryStartGameWith(players);
        }

        public void _Reset()
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }
            
            players = new int[maxLobbySize];
            
            RequestSerialization();
        }

        protected sealed override void OnNetworkUpdate()
        {
            UpdatePlayersView();
        }

        protected abstract void UpdatePlayersView();
    }
}