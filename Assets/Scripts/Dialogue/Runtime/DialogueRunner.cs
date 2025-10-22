using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue.Runtime {
    // =========================
    // 1) 变量黑板（只做开关型 Flag）
    // =========================
    [Serializable]
    public class VariableStore {
        [SerializeField] private List<string> _flags = new List<string>();
        private HashSet<string> _set;

        HashSet<string> Set {
            get {
                if (_set == null) _set = new HashSet<string>(_flags);
                return _set;
            }
        }

        public bool Has(string flag) => !string.IsNullOrEmpty(flag) && Set.Contains(flag);
        public void Add(string flag) {
            if (string.IsNullOrEmpty(flag)) return;
            if (Set.Add(flag)) _flags.Add(flag);
        }
        public void AddMany(IEnumerable<string> flags) {
            if (flags == null) return;
            foreach (var f in flags) Add(f);
        }
        public void Clear() {
            Set.Clear();
            _flags.Clear();
        }
        public IReadOnlyCollection<string> Snapshot() => _flags.AsReadOnly();
        public void Restore(IEnumerable<string> flags) {
            Clear();
            if (flags == null) return;
            foreach (var f in flags) Add(f);
        }
    }

    // =========================
    // 2) 对话运行时状态
    // =========================
    [Serializable]
    public class DialogueState {
        public string graphId;
        public string currentNodeId;
        public HashSet<string> visitedNodes = new HashSet<string>();
    }

    // =========================
    // 3) 运行需要的数据“形状”
    // =========================
    public class RuntimeNode {
        public string id;
        public string speaker;            // 可选
        public string text;               // 节点正文
        public string nextId;             // 无选项时的跳转（留空则结束）
        public List<RuntimeOption> options = new List<RuntimeOption>();
        public List<string> requireFlags; // 进入节点所需（可空）
        public List<string> events = new List<string>();
    }

    public class RuntimeOption {
        public string text;
        public string nextId;             // 点击后的跳转（可空表示结束）
        public List<string> setFlags;     // 点击后写入的 flags（可空）
        public List<string> requireFlags; // 该选项自身出现条件（可空）

        public int cost = 0;                     // 需要金币，0 表示不收费
        public string insufficientText = null;   // 不足时吐槽文本；null 用默认
    }

    // =========================
    // 4) 运行时接口（与数据层解耦）
    // =========================
    public interface IRuntimeDialogueSource {
        string GetStartNodeId(string graphId);
        bool TryGetNode(string graphId, string nodeId, out RuntimeNode node);
    }

    public interface IRuntimeDialogueView {
        void ShowLine(string speaker, string text);
        IEnumerator WaitForConfirm();
        void ShowOptions(IReadOnlyList<string> options, Action<int> onChosen);
        void ClearOptions();
    }

    // =========================
    // 5) 事件
    // =========================
    public static class DialogueEvents {
        public static event Action<string> OnStarted;
        public static event Action<string> OnNodeEntered;
        public static event Action<string, int> OnOptionChosen;
        public static event Action<string> OnEnded;

        internal static void RaiseStarted(string graphId) => OnStarted?.Invoke(graphId);
        internal static void RaiseNode(string nodeId) => OnNodeEntered?.Invoke(nodeId);
        internal static void RaiseOption(string nodeId, int index) => OnOptionChosen?.Invoke(nodeId, index);
        internal static void RaiseEnded(string graphId) => OnEnded?.Invoke(graphId);
    }

    // =========================
    // 6) Runner：推进状态机
    // =========================
    public class DialogueRunner : MonoBehaviour {
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour sourceComponent; // IRuntimeDialogueSource
        [SerializeField] private UnityUIDialogueView viewComponent;   // IRuntimeDialogueView

        public VariableStore Store = new VariableStore();
        public DialogueState State = new DialogueState();
        public bool verboseLogging = true;

        IRuntimeDialogueSource Source;
        IRuntimeDialogueView View;

        bool _isRunning;

        void Awake() {
            Source = sourceComponent as IRuntimeDialogueSource;
            View = viewComponent;

            if (Source == null) Debug.LogError("[DialogueRunner] 数据源未设置或未实现 IRuntimeDialogueSource");
            if (View == null) Debug.LogError("[DialogueRunner] 视图未设置或未实现 IRuntimeDialogueView");
        }

        public Coroutine StartDialogue(string graphId, string startNodeId = null) {
            if (_isRunning) return null;          // 忽略重复启动
            return StartCoroutine(Run(graphId, startNodeId));
        }

        IEnumerator Run(string graphId, string startNodeId) {
            if (Source == null || View == null) yield break;
            Debug.Log("已进入DialogueRunner.run");
            State.graphId = graphId;
            State.currentNodeId = string.IsNullOrEmpty(startNodeId) ? Source.GetStartNodeId(graphId) : startNodeId;
            DialogueEvents.RaiseStarted(graphId);
            

            while (!string.IsNullOrEmpty(State.currentNodeId)) {
                if (Source == null || View == null) yield break;
                _isRunning = true;
                Debug.Log("已进入DialogueRunner.run");

                if (!Source.TryGetNode(State.graphId, State.currentNodeId, out var node)) {
                    Debug.LogWarning($"[DialogueRunner] 节点不存在：{State.currentNodeId}");
                    break;
                }

                if (!PassFlags(node.requireFlags)) {
                    Debug.Log($"[DialogueRunner] 条件不满足，跳过节点：{node.id}");
                    break; // 简单策略：结束（也可以改为跳 nextId）
                }

                State.visitedNodes.Add(node.id);
                DialogueEvents.RaiseNode(node.id);

                View.ClearOptions();
                // [Added] process node.events as flags (data-driven): set into both local store and global store
                if (node.events != null) {
                    foreach (var ev in node.events) {
                        if (!string.IsNullOrEmpty(ev)) {
                            Store.Add(ev);
                            if (GlobalFlagStore.I != null) GlobalFlagStore.I.Set(ev);
                        }
                    }
                }
                View.ShowLine(node.speaker, node.text);
                yield return View.WaitForConfirm();

                var visible = FilterOptions(node.options);
                if (visible.Count == 0) {
                    State.currentNodeId = string.IsNullOrEmpty(node.nextId) ? null : node.nextId;
                    if (string.IsNullOrEmpty(State.currentNodeId)) break;
                    continue;
                }

                int? chosen = null;
                View.ShowOptions(visible.ConvertAll(o => o.text), idx => chosen = idx);
                yield return new WaitUntil(() => chosen.HasValue);

                var opt = visible[chosen.Value];
                DialogueEvents.RaiseOption(node.id, chosen.Value);

                Debug.LogWarning($"已经取到opt，value为{opt.cost}。");

                // == 付费校验 ==
                if (opt.cost > 0) {
                    int currentGold = 0;
                    var gm = GameManager.I; // 你的单例

                    if (gm == null) {
                        Debug.LogWarning($"[Runner][COST] GameManager.I == null，无法读取/扣除金币。当前按 0 金币处理，cost={opt.cost}。");
                    }
                    else {
                        currentGold = gm.GetGold();
                    }

                    Debug.Log($"[Runner][COST] 选择 \"{opt.text}\": need={opt.cost}, have={currentGold}");

                    if (currentGold < opt.cost) {
                        string msg = string.IsNullOrEmpty(opt.insufficientText) ? "This is a lot..." : opt.insufficientText;
                        Debug.Log($"[Runner][COST] 不足，提示并留在当前节点。");
                        View.ShowLine(null, msg);
                        yield return View.WaitForConfirm();
                        // 回到当前节点（重新渲染）；不写 flag，不跳转
                        continue;
                    }
                    else {
                        if (gm != null) {
                            gm.AddGold(-opt.cost);
                            Debug.Log($"[Runner][COST] 扣费 {opt.cost} 成功，新金币={gm.GetGold()}");
                        }
                        else {
                            Debug.LogWarning($"[Runner][COST] 预期扣费 {opt.cost}，但 GameManager 不存在，实际未扣。");
                        }
                    }
                }


                Store.AddMany(opt.setFlags);
                if (opt.setFlags != null && GlobalFlagStore.I != null) {
                    foreach (var f in opt.setFlags) GlobalFlagStore.I.Set(f);
                }
                State.currentNodeId = string.IsNullOrEmpty(opt.nextId) ? null : opt.nextId;
                if (string.IsNullOrEmpty(State.currentNodeId)) break;
            }

            DialogueEvents.RaiseEnded(graphId);
            _isRunning = false;
        }

        List<RuntimeOption> FilterOptions(List<RuntimeOption> options) {
            var list = new List<RuntimeOption>();
            if (options == null || options.Count == 0) {
                if (verboseLogging) Debug.Log("[Runner] node has no options array.");
                return list;
            }

            for (int i = 0; i < options.Count; i++) {
                var o = options[i];
                bool pass = PassFlags(o.requireFlags);
                if (pass) list.Add(o);
                if (verboseLogging) {
                    string req = o.requireFlags != null && o.requireFlags.Count > 0
                        ? string.Join(",", o.requireFlags)
                        : "(none)";
                    Debug.Log($"[Runner] option[{i}] \"{o.text}\" requireFlags={req} -> {(pass ? "VISIBLE" : "HIDDEN")}");
                }
            }

            if (verboseLogging) Debug.Log($"[Runner] visible options count = {list.Count}");
            return list;
        }

        bool PassFlags(List<string> requires) {
            if (requires == null || requires.Count == 0) return true;
            foreach (var r in requires) {
                // Prefer global store if present (cross-scene), fallback to local Store for backward-compat
                if (GlobalFlagStore.I != null) {
                    if (!GlobalFlagStore.I.Has(r)) return false;
                } else {
                    if (!Store.Has(r)) return false;
                }
            }
            return true;
        }
    }

    // =========================
    // 7) 存档桥
    // =========================
    [Serializable]
    public class DialogueSnapshot {
        public DialogueState state = new DialogueState();
        public List<string> flags = new List<string>();
    }

    public static class SaveBridge {
        public static DialogueSnapshot Capture(DialogueState state, VariableStore store) {
            var snap = new DialogueSnapshot {
                state = new DialogueState {
                    graphId = state.graphId,
                    currentNodeId = state.currentNodeId,
                    visitedNodes = new HashSet<string>(state.visitedNodes)
                },
                flags = new List<string>(store.Snapshot())
            };
            return snap;
        }

        public static void Restore(DialogueSnapshot snap, DialogueState state, VariableStore store) {
            if (snap == null) return;
            state.graphId = snap.state.graphId;
            state.currentNodeId = snap.state.currentNodeId;
            state.visitedNodes = new HashSet<string>(snap.state.visitedNodes ?? new HashSet<string>());
            store.Restore(snap.flags);
        }
    }
}
