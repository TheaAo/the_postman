using UnityEngine;

namespace Game.Dialogue.Runtime {
    /// <summary>
    /// 场景开始时自动启动一个对话，用于快速测试。
    /// 把它挂到任意物体上，在 Inspector 填好 graphId 即可。
    /// </summary>
    public class DialoguePlayOnStart : MonoBehaviour {
        [Header("要启动的对话")]
        public DialogueRunner runner;
        public string graphId = "flag_bm_gate_angelo"; // 你的 JSON / graphId
        public string startNodeId = null;               // 可留空使用 JSON 的 startNodeId

        void Reset() {
            // 尝试在场景里找一个 Runner
            if (runner == null) runner = Object.FindAnyObjectByType<DialogueRunner>();
        }

        void Start() {
            if (runner == null) {
                Debug.LogError("[DialoguePlayOnStart] 未找到 DialogueRunner。");
                return;
            }
            runner.StartDialogue(graphId, string.IsNullOrEmpty(startNodeId) ? null : startNodeId);
        }
    }
}
