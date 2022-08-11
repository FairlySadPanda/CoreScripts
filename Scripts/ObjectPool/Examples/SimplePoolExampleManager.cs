using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace FairlySadProductions.Scripts.ObjectPool
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class SimplePoolExampleManager : UdonSharpBehaviour
    {
        [SerializeField] private TextMeshProUGUI text;
        [HideInInspector] public SimplePooledObjectExample ourExample;
        [SerializeField] private SimpleObjectPool pool;

        public void Start()
        {
            WriteText($"Pool started: owner is {Networking.GetOwner(pool.gameObject).displayName}");
        }

        public void WriteText(string input)
        {
            text.text += $"\n{input}";
        }

        public void Interact()
        {
            if (ourExample)
            {
                ourExample.IncrementCounter();
            }
        }
    }
}