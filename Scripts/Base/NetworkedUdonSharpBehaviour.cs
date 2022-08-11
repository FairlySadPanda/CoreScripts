using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.CoreScripts.Scripts.Base
{
    /// <summary>
    /// <para>NetworkedUdonSharpBehaviour is a manually-synced UdonSharpBehaviour that overrides a lot of default
    /// VRChat networking code to enhance networking stability and automate common functionality.</para>
    ///
    /// <para>In particular, the code replaces the normal "OnDeserialization" workflow with enforcing the coder to
    /// use a new function called OnNetworkUpdate. This function is only called when the behaviour is confident
    /// it has received new data. This guards against potential VRChat Udon networking regressions where its
    /// guarantee that OnDeserialization is only called for fresh data has not been honoured.</para>
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public abstract class NetworkedUdonSharpBehaviour : UdonSharpBehaviour
    {
        [Tooltip("Allow this object to be transferred between players. If this is disabled, the only time the game" +
                 "object can legally be transferred is when the owner quits the instance.")]
        [SerializeField]
        private bool allowUnforcedOwnershipTransfer = true;
        [Tooltip("Allow this object to be transferred from the current owner by another player. If this is disabled," +
                 "only the current owner of the object can legally transfer the object to another.")]
        [SerializeField]
        private bool allowNonOwnerToRequestOwnership = true;
        
        /// <summary>
        /// The atomic clock that ticks up by one every time we make a sync.
        /// </summary>
        [UdonSynced] private uint clock;

        /// <summary>
        /// The last known good clock value.
        /// </summary>
        private uint oldClock;

        /// <summary>
        /// Helper function that automates setting ownership of the object to the local player.
        /// </summary>
        protected void ClaimOwnership()
        {
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }
        
        public sealed override bool OnOwnershipRequest(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            if (!allowUnforcedOwnershipTransfer)
            {
                return false;
            }

            if (!allowNonOwnerToRequestOwnership && Networking.LocalPlayer.playerId != requestingPlayer.playerId)
            {
                return false;
            }
            
            return IsOwnershipTransferAuthorized(requestingPlayer, requestedOwner);
        }

        /// <summary>
        /// Called after the universal ownership validation checks.
        /// </summary>
        /// <param name="requestingPlayer">The player requesting ownership transfer.</param>
        /// <param name="requestedOwner">The desired target for transfer.</param>
        /// <returns>True; overriding functions should return true if the ownership transfer is authorized, or false if
        /// it is not.</returns>
        protected virtual bool IsOwnershipTransferAuthorized(VRCPlayerApi requestingPlayer, VRCPlayerApi requestedOwner)
        {
            return true;
        }

        public sealed override void RequestSerialization()
        {
            if (!Networking.IsOwner(gameObject))
            {
                Debug.LogWarning($"[{name}] Cannot RequestSerialization: Do not own object. Current owner is: {Networking.GetOwner(gameObject).displayName}");
                return;
            }

            clock++;
            
            Debug.Log($"[{name}] RequestSerialization called, clock is now {clock}.");
            
            base.RequestSerialization();
            SendCustomEventDelayedFrames("_onDeserialization", 1);
        }
        
        public sealed override void OnPreSerialization()
        {
            OnSendNetworkingUpdate();
        }
        
        public sealed override void OnPostSerialization(VRC.Udon.Common.SerializationResult result)
        {
            if (!result.success)
            {
                // Undo clock damage by rolling back to last known good clock
                clock = oldClock;
                
                Debug.LogWarning($"[{name}] Serialization failed for {result.byteCount} bytes.");
                return;
            }
            
            OnSentNetworkingUpdate();
        }
        
        public sealed override void OnDeserialization()
        {
            if (clock <= oldClock)
            {
                Debug.LogError($"[{name}] Rejecting network update: {oldClock} not less than {clock}");
                return;
            }

            OnNetworkUpdate();
            
            oldClock = clock;
        }

        /// <summary>
        /// OnSendNetworkingUpdate is fired when synced data is about to be serialized out to other clients.
        /// </summary>
        protected virtual void OnSendNetworkingUpdate() {}
        
        /// <summary>
        /// OnSentNetworkingUpdate is fired when synced data has been serialized to other clients.
        /// </summary>
        protected virtual void OnSentNetworkingUpdate() {}
        
        /// <summary>
        /// OnNetworkUpdate is fired in two instances:
        /// <code>
        /// 1. A user has triggered a sync of data they own. In this instance, it is called the following frame.
        /// 2. A user has received valid data that is almost certainly newer than what we had prior.
        /// </code>
        /// </summary>
        protected abstract void OnNetworkUpdate();
    }
}