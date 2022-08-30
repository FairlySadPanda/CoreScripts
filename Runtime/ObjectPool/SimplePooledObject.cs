using FairlySadProductions.CoreScripts.Scripts.Base;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.CoreScripts.Scripts.ObjectPool
{
    /// <summary>
    /// The base class for objects that are pooled in a SimpleObjectPool. This base class handles interoperability with
    /// the owning SimpleObjectPool and enforces rules to ensure the pool does not behave unexpectedly. Implementing
    /// classes must implement ClearSyncedData and HandleNewSyncedData.
    /// </summary>
    public abstract class SimplePooledObject : NetworkedUdonSharpBehaviour
    {
        [SerializeField] private SimpleObjectPool pool;
        [UdonSynced] protected int ownerID;

        /// <summary>
        /// Get the ownerID of this object. The ownerID is set by the pool owner. This ownerID will not drift when the
        /// owner does.
        /// </summary>
        /// <returns>The playerID of the owner of this object.</returns>
        public int GetOwnerID()
        {
            return ownerID;
        }
        
        /// <summary>
        /// Sets the OwnerID. This can only be done by the owner of the pool.
        /// </summary>
        /// <param name="newID"></param>
        public void SetOwnerID(int newID)
        {
            if (!Networking.IsOwner(pool.gameObject))
            {
                Debug.LogError($"[{name}] Cannot set owner of object {name}: not owner of pool");
                
                return;
            }
            
            ownerID = newID;
            
            Debug.Log($"[{name}] ID set to {newID}");
            
            SendCustomEventDelayedSeconds(nameof(EnforceSync), pool.GetWaitTime());
        }

        // Explanation of this weird code:- ownerID is allocated after we've *spawned* the object but real network
        // sync is only enabled some time later: we're assuming about 3 seconds. For some reason. It's agony.
        // So we have to do ownerID when we can and then wait for OnEnable to resolve. :|
        
        // TODO: When the promised network ownership and sync issues are fixed, revisit this workaround.
        
        public void EnforceSync()
        {
            var owner = ownerID > 0 ? $"{VRCPlayerApi.GetPlayerById(ownerID).displayName}" : "nobody";
            Debug.Log($"pooled object {name} enabled: owner is {owner}: {ownerID}");
            
            if (!Networking.IsOwner(pool.gameObject))
            {
                return;
            }
            
            ClaimOwnership();
            ClearSyncedData();
            RequestSerialization();
        }

        protected sealed override bool IsOwnershipTransferAuthorized(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return Networking.LocalPlayer.playerId == ownerID || Networking.IsOwner(pool.gameObject);
        }

        protected sealed override void OnNetworkUpdate()
        {
            if (Networking.LocalPlayer.playerId == ownerID)
            {
                Debug.Log($"[{name}: Claiming ownership of this pooled object: we own it");
                
                ClaimOwnership();
                pool.SetPooledObject(this);
            } else if (ownerID == 0 && Networking.IsOwner(Networking.LocalPlayer, pool.gameObject))
            {
                Debug.Log($"[{name}: Claiming ownership of this object: it's not owned by anyone and we own the pool");
                
                ClaimOwnership();
            }
            
            HandleNewSyncedData();
        }

        /// <summary>
        /// ClearSyncedData is called when the pool needs to reset a pooled object back to its initial state. It must
        /// set all synced data in the behaviour back to default (0, 0.0, "", etc).
        /// </summary>
        protected abstract void ClearSyncedData();
        
        /// <summary>
        /// HandleNewSyncedData is called when new synced data has been received.
        /// </summary>
        protected abstract void HandleNewSyncedData();
    }
}