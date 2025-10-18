using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Dialogue.Runtime {
    /// <summary>
    /// 负责“这次与某 NPC 对话，应该启动哪个 graphId？”
    /// - 读取 Resources 下的 registry.json
    /// - 按 routes 里 whenAllFlags 匹配；命中则返回 nextGraph；否则回落 npcToGraph 的默认
    /// - 监听 DialogueEvents.OnEnded 写入 NPC.{Npc}.Visited
    /// </summary>
    public class ConversationDirector : MonoBehaviour {
        [Header("Resources path")]
        [SerializeField] private string registryPath = "Dialogues";

        [Header("调试")]
        public bool verbose = true;

        // 数据
        private readonly Dictionary<string, string> _npcToDefault = new();  // npc -> defaultGraph
        private readonly List<Route> _routes = new();                      // 路由规则
        private readonly Dictionary<string, string> _graphToNpc = new();    // 反向索引：graph -> npc（用于写 Visited）

        [Serializable]
        class Route {
            public string npc;
            public List<string> whenAllFlags = new();
            public string nextGraph;
        }

        void Awake() {
            LoadRegistry();
            DialogueEvents.OnEnded += OnDialogueEnded;
        }

        void OnDestroy() {
            DialogueEvents.OnEnded -= OnDialogueEnded;
        }

        void LoadRegistry() {
            _npcToDefault.Clear();
            _routes.Clear();
            _graphToNpc.Clear();

            var ta = Resources.Load<TextAsset>(registryPath);
            if (ta == null) {
                Debug.LogWarning($"[ConversationDirector] 未找到 {registryPath}.json，使用空注册表。");
                return;
            }

            var root = JObject.Parse(ta.text);
            var npcToGraph = root["npcToGraph"] as JObject;
            if (npcToGraph != null) {
                foreach (var kv in npcToGraph) {
                    var npc = kv.Key;
                    var g = kv.Value?.Value<string>();
                    if (!string.IsNullOrEmpty(npc) && !string.IsNullOrEmpty(g)) {
                        _npcToDefault[npc] = g;
                        _graphToNpc[g] = npc; // 反向索引：默认图
                    }
                }
            }

            var routes = root["routes"] as JArray;
            if (routes != null) {
                foreach (var t in routes) {
                    var o = t as JObject;
                    if (o == null) continue;
                    var r = new Route {
                        npc = o["npc"]?.Value<string>(),
                        nextGraph = o["nextGraph"]?.Value<string>(),
                        whenAllFlags = ReadStringList(o["whenAllFlags"])
                    };
                    if (string.IsNullOrEmpty(r.npc) || string.IsNullOrEmpty(r.nextGraph)) continue;
                    _routes.Add(r);
                    _graphToNpc[r.nextGraph] = r.npc; // 反向索引：路由图
                }
            }

            if (verbose) {
                Debug.Log($"[ConversationDirector] 载入 {_npcToDefault.Count} 个默认图，{_routes.Count} 条路由。");
            }
        }

        static List<string> ReadStringList(JToken tok) {
            var list = new List<string>();
            if (tok is JArray arr) foreach (var e in arr) {
                    var s = e?.Value<string>();
                    if (!string.IsNullOrEmpty(s)) list.Add(s);
                }
            return list;
        }

        /// <summary> 入口：给 NPC 取这次要开的图。 </summary>
        public string GetGraphForNpc(VariableStore store, string npc) {
            if (string.IsNullOrEmpty(npc)) return null;
            // 1) 路由优先：顺序匹配 whenAllFlags
            foreach (var r in _routes.Where(r => string.Equals(r.npc, npc, StringComparison.Ordinal))) {
                if (PassAll(store, r.whenAllFlags)) {
                    if (verbose) Debug.Log($"[ConversationDirector] 命中路由 npc={npc} -> {r.nextGraph} (flags: {string.Join(",", r.whenAllFlags)})");
                    return r.nextGraph;
                }
            }
            // 2) 默认
            if (_npcToDefault.TryGetValue(npc, out var g)) {
                if (verbose) Debug.Log($"[ConversationDirector] 未命中路由，使用默认 npc={npc} -> {g}");
                return g;
            }
            Debug.LogWarning($"[ConversationDirector] 未找到 NPC={npc} 的默认图，返回 null");
            return null;
        }

        static bool PassAll(VariableStore store, List<string> flags) {
            if (flags == null || flags.Count == 0) return false; // 约束：必须有条件才算路由
            foreach (var f in flags) if (!store.Has(f)) return false;
            return true;
        }

        void OnDialogueEnded(string graphId) {
            if (string.IsNullOrEmpty(graphId)) return;
            if (_graphToNpc.TryGetValue(graphId, out var npc)) {
                // 写入通用“聊过”标记
                var flag = $"NPC.{npc}.Visited";
                var runner = FindAnyObjectByType<DialogueRunner>();
                if (runner != null) {
                    runner.Store.Add(flag);
                    if (verbose) Debug.Log($"[ConversationDirector] 对话结束：标记 {flag}=true");
                }
            }
        }
    }
}
