using UnityEngine;
using Game.Dialogue.Runtime;
using UnityEngine.InputSystem;

public class StartNpcDialogueOnInteract : MonoBehaviour {
    public DialogueRunner runner;
    public string npcKey;
    public ConversationDirector director;
    public string graphId;

    [Header("∑«¥•∑¢ºÏ≤‚…Ë÷√")]
    public float interactRadius = 1.6f;
    public LayerMask playerMask;
    public Transform player;

    bool isTalking;

    void Awake() {
        if (player == null) {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
        if (runner == null) runner = FindAnyObjectByType<Game.Dialogue.Runtime.DialogueRunner>();
        if (director == null) director = FindAnyObjectByType<Game.Dialogue.Runtime.ConversationDirector>();
    }

    void OnEnable() { DialogueEvents.OnEnded += HandleEnded; }
    void OnDisable() { DialogueEvents.OnEnded -= HandleEnded; }
    void HandleEnded(string _) { isTalking = false; }

    void Update() {
        if (isTalking || runner == null) return;

        bool inRange = Physics2D.OverlapCircle(transform.position, interactRadius, playerMask);
        var kb = Keyboard.current;

        if (inRange && kb != null && kb.fKey.wasPressedThisFrame) {
            isTalking = true;
            string finalGraph = director ? director.GetGraphForNpc(runner.Store, npcKey) : null;
            if (string.IsNullOrEmpty(finalGraph))
                finalGraph = graphId;
            runner.StartDialogue(finalGraph);
        }
    }

    void OnDrawGizmosSelected() {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}
