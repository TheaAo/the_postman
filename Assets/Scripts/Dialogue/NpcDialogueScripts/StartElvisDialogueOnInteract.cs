using UnityEngine;
using Game.Dialogue.Runtime;
using UnityEngine.InputSystem;
public class StartElvisDialogueOnInteract : MonoBehaviour {
    public DialogueRunner runner;
    public string graphId;
    bool inRange;
    bool isTalking; // ∑¿÷ÿ»Î

    void OnEnable() {
        Game.Dialogue.Runtime.DialogueEvents.OnEnded += HandleEnded;
    }
    void OnDisable() {
        Game.Dialogue.Runtime.DialogueEvents.OnEnded -= HandleEnded;
    }
    void HandleEnded(string g) { if (g == graphId) isTalking = false; }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) inRange = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) inRange = false; }

    void Update() {
        if (!inRange || isTalking) return;
        var kb = Keyboard.current;
        if (kb != null && kb.fKey.wasPressedThisFrame) { 
            if (runner != null) {
                isTalking = true;
                runner.StartDialogue(graphId);
            }
        }
    }
}
