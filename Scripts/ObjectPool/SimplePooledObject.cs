﻿using FairlySadProductions.CoreScripts.Scripts.Base;
using FairlySadProductions.Scripts.ObjectPool;
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
                Debug.LogError($"cannot set owner of object {name}: not owner of pool");
                
                return;
            }
            
            ownerID = newID;
            
            Debug.Log($"{name} SetOwnerID: ID set to {newID}");
        }

        // Explanation of this weird code:- ownerID is allocated after we've *spawned* the object but real network
        // sync is only enabled after OnEnable is called. For some reason. It's agony.
        // So we have to do ownerID when we can and then wait for OnEnable to resolve. :|
        
        public void OnEnable()
        {
            var owner = ownerID == 0 ? "nobody" : "VRCPlayerApi.GetPlayerById(ownerID).displayName";
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
                Debug.Log($"Claiming ownership of pooled object {name}");
                
                ClaimOwnership();
                pool.SetPooledObject(this);
            } else if (ownerID == 0 && Networking.IsOwner(Networking.LocalPlayer, pool.gameObject))
            {
                Debug.Log($"Claiming ownership of pooled object {name} because it's not owned by anyone and we own the pool");
                
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