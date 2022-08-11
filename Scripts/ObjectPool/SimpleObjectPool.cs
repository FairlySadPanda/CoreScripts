using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;

namespace FairlySadProductions.Scripts.ObjectPool
{
    /// <summary>
    /// SimpleObjectPool leverages VRChat's VRCObjectPool system to manage a pool of objects, assigning one object to
    /// each player, and handling re-allocating those objects when players leave. All pooled objects have
    /// UdonSharpBehaviours that inherit from SimplePooledObject. In addition, the pool contains a reference to the
    /// pooled object that was allocated to us, allowing the SimpleObjectPool to provide the object to client
    /// behaviours.
    /// </summary>
    [RequireComponent(typeof(VRCObjectPool)), UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SimpleObjectPool : UdonSharpBehaviour
    {
        private VRCObjectPool pool;
        private SimplePooledObject ourObject;

        public void Start()
        {
            pool = GetComponent<VRCObjectPool>();
        }

        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject))
            {
                Debug.Log("ignoring OnPlayerJoined: not pool owner");
                
                return;
            }
            
            if (player == null || !player.IsValid())
            {
                Debug.Log("ignoring OnPlayerJoined: player invalid");
                
                return;
            }

            GameObject obj = pool.TryToSpawn();

            if (!obj)
            {
                Debug.LogError("cannot allocate object: pool full!");
                return;
            }

            SimplePooledObject simplePooledObject = obj.GetComponent<SimplePooledObject>();
            if (!simplePooledObject)
            {
                Debug.Log($"ignoring {simplePooledObject.gameObject.name}: does not have SPO");
                return;
            }

            Debug.Log($"setting ownerID of {simplePooledObject.gameObject.name} to {player.playerId}");
            simplePooledObject.SetOwnerID(player.playerId);
        }
        
        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            if (!Networking.IsOwner(gameObject))
            {
                return;
            }

            if (player == null || !player.IsValid())
            {
                return;
            }

            foreach (var obj in pool.Pool)
            {
                var simplePooledObject = obj.GetComponent<SimplePooledObject>();
                if (!simplePooledObject)
                {
                    continue;
                }

                if (simplePooledObject.GetOwnerID() == player.playerId)
                {
                    simplePooledObject.SetOwnerID(0);
                    pool.Return(obj.gameObject);

                    return;
                }
            }
        }
        
        /// <summary>
        /// Stores the provided object in the pool to be retrieved by client behaviours. Calling this function again
        /// will do nothing: the first object stored cannot be replaced.
        /// </summary>
        /// <param name="obj">The object to store. The object must be owned by the local player.</param>
        public void SetPooledObject(SimplePooledObject obj)
        {
            if (ourObject)
            {
                Debug.LogError($"[{name}]: cannot set object {obj.name}: object {ourObject.name} is already stored in this pool.");
                return;
            }

            if (!Networking.IsOwner(obj.gameObject))
            {
                Debug.LogError($"[{name}]: cannot set object {obj}: object is not owned by us.");
                return;
            }
            
            ourObject = obj;
        }

        /// <summary>
        /// Retrieves the stored SimplePooledObject.
        /// </summary>
        /// <returns>The local player's SimplePooledObject.</returns>
        public SimplePooledObject GetPooledObject()
        {
            return ourObject;
        }
    }
}