using UdonSharp;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace FairlySadProductions.CoreScripts.Scripts.Utilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class ActivateOnInteract : UdonSharpBehaviour
    {
        public UdonBehaviour behaviour;
        public string eventName;
        public bool isNetworked;
        public bool ownerOnly;

        public override void Interact()
        {
            if (!behaviour)
            {
                return;
            }
            
            if (isNetworked)
            {
                behaviour.SendCustomNetworkEvent(ownerOnly ? NetworkEventTarget.Owner : NetworkEventTarget.All, eventName);
            }
            else
            {
                behaviour.SendCustomEvent(eventName);
            }
        }
    }
}