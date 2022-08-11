using FairlySadProductions.CoreScripts.Scripts.Base;

namespace FairlySadProductions.CoreScripts.Scripts.Game
{
    public abstract class ActivityManager : NetworkedUdonSharpBehaviour
    {
        public abstract void TryStartActivityWith(int[] players);
    }
}