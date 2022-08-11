using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.Scripts.ObjectPool
{
    public class SimplePooledObjectExample : SimplePooledObject
    {
        [UdonSynced] private int counter;

        [SerializeField] private SimplePoolExampleManager manager;

        public void IncrementCounter()
        {
            if (ownerID != Networking.LocalPlayer.playerId || !Networking.IsOwner(gameObject))
            {
                Debug.Log("IncrementCounter failed");
                return;
            }

            Debug.Log("IncrementCounter activated");
            counter++;
            RequestSerialization();
        }

        private void OnDisable()
        {
            manager.WriteText($"{name} removed from {ownerID}; returned to pool owner");
        }

        protected override void ClearSyncedData()
        {
            counter = 0;
            
            manager.WriteText($"{name} cleared of data");
        }

        protected override void HandleNewSyncedData()
        {
            if (ownerID == Networking.LocalPlayer.playerId)
            {
                if (manager.ourExample == null)
                {
                    manager.ourExample = this;
                }
            }
            
            manager.WriteText($"{name} got new synced data: counter is {counter}");
        }
    }
}