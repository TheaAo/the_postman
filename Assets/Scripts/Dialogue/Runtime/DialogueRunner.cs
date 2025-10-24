using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Dialogue.Runtime {
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
        public void Clear() { Set.Clear(); _flags.Clear(); }
        public IReadOnlyCollection<string> Snapshot() => _flags.AsReadOnly();
        public void Restore(IEnumerable<string> flags) {
            Clear();
            if (flags == null) return;
            foreach (var f in flags) Add(f);
        }
    }

    [Serializable]
    public class DialogueState {
        public string graphId;
        public string currentNodeId;
        public HashSet<string> visitedNodes = new HashSet<string>();
    }

    public class RuntimeNode {
        public string id;
        public string speaker;
        public string text;
        public string nextId;
        public List<RuntimeOption> options = new List<RuntimeOption>();
        public List<string> requireFlags;
        public List<string> events = new List<string>();
    }

    public class RuntimeOption {
        public string text;
        public string nextId;
        public List<string> setFlags;
        public List<string> requireFlags;
        public int cost = 0;
        public string insufficientText = null;
        public string insufficientNextId = null;
    }

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

    public class DialogueRunner : MonoBehaviour {
        [Header("Dependencies")]
        [SerializeField] private MonoBehaviour sourceComponent;
        [SerializeField] private UnityUIDialogueView viewComponent;

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
            if (_isRunning) return null;
            return StartCoroutine(Run(graphId, startNodeId));
        }

        IEnumerator Run(string graphId, string startNodeId) {
            if (Source == null || View == null) yield break;
            State.graphId = graphId;
            State.currentNodeId = string.IsNullOrEmpty(startNodeId) ? Source.GetStartNodeId(graphId) : startNodeId;
            DialogueEvents.RaiseStarted(graphId);

            while (!string.IsNullOrEmpty(State.currentNodeId)) {
                if (!Source.TryGetNode(State.graphId, State.currentNodeId, out var node)) break;

                if (!PassFlags(node.requireFlags)) break;

                Debug.Log($"当前node：{State.currentNodeId}");

                State.visitedNodes.Add(node.id);
                DialogueEvents.RaiseNode(node.id);

                View.ClearOptions();
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
                    continue;
                }

                int? chosen = null;
                View.ShowOptions(visible.ConvertAll(o => o.text), idx => chosen = idx);
                yield return new WaitUntil(() => chosen.HasValue);

                var opt = visible[chosen.Value];
                DialogueEvents.RaiseOption(node.id, chosen.Value);

                // 金币检查逻辑
                if (opt.cost > 0) {
                    int currentGold = 0;
                    var gm = GameManager.I;
                    if (gm != null) currentGold = gm.GetGold();

                    if (currentGold < opt.cost) {
                        string msg = string.IsNullOrEmpty(opt.insufficientText) ? "This is a lot..." : opt.insufficientText;
                        View.ShowLine(null, msg);
                        yield return View.WaitForConfirm();

                        // 新逻辑：跳转到 insufficientNextId
                        if (!string.IsNullOrEmpty(opt.insufficientNextId)) {
                            State.currentNodeId = opt.insufficientNextId;
                            continue;
                        }

                        // 默认逻辑：回到当前节点
                        continue;
                    }
                    else {
                        if (gm != null) gm.AddGold(-opt.cost);
                    }
                }

                Store.AddMany(opt.setFlags);
                if (opt.setFlags != null && GlobalFlagStore.I != null)
                    foreach (var f in opt.setFlags) GlobalFlagStore.I.Set(f);

                State.currentNodeId = string.IsNullOrEmpty(opt.nextId) ? null : opt.nextId;
            }

            DialogueEvents.RaiseEnded(graphId);
            _isRunning = false;
        }

        List<RuntimeOption> FilterOptions(List<RuntimeOption> options) {
            var list = new List<RuntimeOption>();
            if (options == null) return list;
            foreach (var o in options)
                if (PassFlags(o.requireFlags)) list.Add(o);
            return list;
        }

        bool PassFlags(List<string> requires) {
            if (requires == null || requires.Count == 0) return true;
            foreach (var r in requires) {
                if (GlobalFlagStore.I != null) {
                    if (!GlobalFlagStore.I.Has(r)) return false;
                }
                else {
                    if (!Store.Has(r)) return false;
                }
            }
            return true;
        }
    }
}
