using FairlySadProductions.CoreScripts.Scripts.Base;

namespace FairlySadProductions.CoreScripts.Scripts.Game
{
    public abstract class GameManager : NetworkedUdonSharpBehaviour
    {
        public abstract void TryStartGameWith(int[] players);
    }
}