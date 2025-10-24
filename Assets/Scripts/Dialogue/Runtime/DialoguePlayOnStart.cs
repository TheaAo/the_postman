using UnityEngine;

namespace Game.Dialogue.Runtime {
    /// Automatically start a dialogue at the beginning of the scene for quick testing.
    /// Hook it up to any object and fill in the graphId in the Inspector.

    public class DialoguePlayOnStart : MonoBehaviour {
        [Header("Dialogues to be initiated")]
        public DialogueRunner runner;
        public string graphId = "flag_bm_gate_angelo"; 
        public string startNodeId = null;             

        void Reset() {
            // Try to find a Runner in the scene
            if (runner == null) runner = Object.FindAnyObjectByType<DialogueRunner>();
        }

        void Start() {
            if (runner == null) {
                Debug.LogError("[DialoguePlayOnStart] did not find DialogueRunner¡£");
                return;
            }
            runner.StartDialogue(graphId, string.IsNullOrEmpty(startNodeId) ? null : startNodeId);
        }
    }
}
