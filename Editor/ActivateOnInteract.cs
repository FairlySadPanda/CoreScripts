using System.Linq;
using UdonSharpEditor;
using UnityEditor;
using UnityEngine;
using VRC.Udon;

namespace FairlySadProductions.CoreScripts.Scripts.Utilities.Editor
{
    [CustomEditor(typeof(CoreScripts.Scripts.Utilities.ActivateOnInteract))]
    public class ActivateOnInteract : UnityEditor.Editor
    {
        private CoreScripts.Scripts.Utilities.ActivateOnInteract activateOnInteract;
        private int selectedIndex;
        private SerializedProperty propBehaviour;
        private SerializedProperty propIsNetworked;
        private SerializedProperty propOwnerOnly;
        
        private bool isUpdateable => activateOnInteract != null && activateOnInteract.behaviour != null;

        private void OnEnable()
        {
            activateOnInteract  = target as CoreScripts.Scripts.Utilities.ActivateOnInteract;
            propBehaviour = serializedObject.FindProperty(nameof(CoreScripts.Scripts.Utilities.ActivateOnInteract.behaviour));
            propIsNetworked = serializedObject.FindProperty(nameof(CoreScripts.Scripts.Utilities.ActivateOnInteract.isNetworked));
            propOwnerOnly = serializedObject.FindProperty(nameof(CoreScripts.Scripts.Utilities.ActivateOnInteract.ownerOnly));
            
            if (!isUpdateable)
            {
                return;
            }
            
            selectedIndex = GetSelectedIndex();
        }
        
        public override void OnInspectorGUI()
        {
            // If not an UdonBehaviour return
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            {
                return;
            }

            UpdateSelectedIndexIfChanged();
            UpdateEventNameIfDropDownSelectionChanged();
            UpdateNetworkingBools();

            // Commit changes made
            serializedObject.ApplyModifiedProperties();
        }
        
        private void UpdateSelectedIndexIfChanged()
        {
            // Has the behaviour changed?
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(propBehaviour);
            
            // Record changes
            if (EditorGUI.EndChangeCheck())
            {
                selectedIndex = GetSelectedIndex();
            }
        }

        private void UpdateEventNameIfDropDownSelectionChanged()
        {
            if (!isUpdateable)
            {
                return;
            }
            
            EditorGUI.BeginChangeCheck();

            UdonBehaviour behaviour = activateOnInteract.behaviour;
            string[] eventsArray = behaviour.programSource.SerializedProgramAsset.RetrieveProgram().EntryPoints
                .GetExportedSymbols().ToArray();
            selectedIndex = EditorGUILayout.Popup("Event Name", selectedIndex, eventsArray);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(activateOnInteract, "Updated event name for ActivateOnInteract script");
                activateOnInteract.eventName = eventsArray[selectedIndex];
            }
        }

        private void UpdateNetworkingBools()
        {
            EditorGUILayout.PropertyField(propIsNetworked);
            if (propIsNetworked.boolValue)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(propOwnerOnly, new GUIContent("Send to owner only"));
                EditorGUI.indentLevel--;
            }
        }
        
        private int GetSelectedIndex()
        {
            if (!isUpdateable)
            {
                return 0;
            }
            
            string[] eventsArray = activateOnInteract.behaviour.programSource.SerializedProgramAsset.RetrieveProgram()
                .EntryPoints.GetExportedSymbols().ToArray();

            for (int i = 0; i < eventsArray.Length; i++)
            {
                if (eventsArray[i] == activateOnInteract.eventName)
                {
                    return i;
                }
            }

            return 0;
        }
    }
}