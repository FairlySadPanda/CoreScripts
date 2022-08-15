using System;
using UdonSharp;
using VRC.SDKBase;

namespace FairlySadProductions.CoreScripts.Scripts.Utilities
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class TrackPlayerPosition : UdonSharpBehaviour
    {
        public VRCPlayerApi playerToTrack;

        public void LateUpdate()
        {
            if (playerToTrack != null && playerToTrack.IsValid())
            {
                transform.position = playerToTrack.GetPosition();
            }
        }
    }
}