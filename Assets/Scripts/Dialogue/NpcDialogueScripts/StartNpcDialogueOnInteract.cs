using UnityEngine;
using Game.Dialogue.Runtime;
using UnityEngine.InputSystem;
public class StartNpcDialogueOnInteract : MonoBehaviour {
    public DialogueRunner runner;
    public string graphId;
    public string npcKey;  
    public ConversationDirector director; 
    bool inRange;
    bool isTalking; // 防重入

    void OnEnable() {
        Game.Dialogue.Runtime.DialogueEvents.OnEnded += HandleEnded;
    }
    void OnDisable() {
        Game.Dialogue.Runtime.DialogueEvents.OnEnded -= HandleEnded;
    }
    void HandleEnded(string g) { isTalking = false; }

    void OnTriggerEnter2D(Collider2D other) { if (other.CompareTag("Player")) inRange = true; }
    void OnTriggerExit2D(Collider2D other) { if (other.CompareTag("Player")) inRange = false; }

    void Update() {
        if (!inRange || isTalking) return;
        var kb = Keyboard.current;
        if (kb != null && kb.fKey.wasPressedThisFrame) {
            if (runner != null) {
                isTalking = true;
                // 向 Director 查询这次要开的图
                var finalGraph = (director != null)
                    ? director.GetGraphForNpc(runner.Store, npcKey)
                    : null;
                if (string.IsNullOrEmpty(finalGraph)) {
                    Debug.LogWarning($"[StartDialogue] Director 未给出图，回落到 registry 默认或手填 graphId。");
                }
                // 回落逻辑：如果无 director，可直接用旧的 graphId
                var g = string.IsNullOrEmpty(finalGraph) ? /* 旧字段 */ graphId : finalGraph;
                runner.StartDialogue(g);
            }
        }
    }
}
