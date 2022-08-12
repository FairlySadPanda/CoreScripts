using UdonSharp;
using VRC.Udon;

namespace FairlySadProductions.CoreScripts.Scripts.Utilities
{
    /// <summary>
    /// Timer is a behaviour that uses SendCustomEventDelayedSeconds as a pseudo-callback. To use this behaviour:
    /// <code>1. Use a prebuilt prefab or create a GameObject with this behaviour assigned to it.
    /// 2. Ensure that the object is in the heirarchy. UdonBehaviours need to be in the scene to properly be cloned.
    /// 3. Use Instantiate() to duplicate the timer. Retrieve this component off the spawned object, and call
    /// StartTimer(float, UdonBehaviour, string) to start the timer.
    /// 4. To trigger the timer early, call EndTimer(). To cancel the timer, Delete() the timer's GameObject.</code>
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class Timer : UdonSharpBehaviour
    {
        private UdonBehaviour behaviour;
        private string eventName;
        
        public void StartTimer(float newTimeRemaining, UdonBehaviour callbackBehaviour, string callbackEvent)
        {
            behaviour = callbackBehaviour;
            eventName = callbackEvent;
            
            SendCustomEventDelayedSeconds(nameof(EndTimer), newTimeRemaining);
        }

        public void EndTimer()
        {
            behaviour.SendCustomEvent(eventName);
            Destroy(gameObject);
        }
    }
}