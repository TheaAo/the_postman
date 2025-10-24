using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Dialogue.Runtime {
    // - Responsible for "Which graphId should be started for this conversation with an NPC?"
    // - Reads registry.json under Resources.
    // - Match whenAllFlags in routes; if it hits, return nextGraph; otherwise fall back to the npcToGraph default.
    // - Listen to DialogueEvents.OnEnded write to NPC.{Npc}.Visited
    public class ConversationDirector : MonoBehaviour {
        [Header("Resources path")]
        [SerializeField] private string registryPath = "Dialogues";

        [Header("debug")]
        public bool verbose = true;

        // data
        private readonly Dictionary<string, string> _npcToDefault = new();  // npc -> defaultGraph
        private readonly List<Route> _routes = new();                      // Routing Rules
        private readonly Dictionary<string, string> _graphToNpc = new();    // Reverse index: graph -> npc (for writing Visited)

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
                Debug.LogWarning($"[ConversationDirector] did not find {registryPath}.json, use an empty registry.");
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
                        _graphToNpc[g] = npc; // Reverse index: default map
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
                    _graphToNpc[r.nextGraph] = r.npc; // Reverse Index: Route Map
                }
            }

            if (verbose) {
                Debug.Log($"[ConversationDirector] loaded {_npcToDefault.Count} default graphes£¬{_routes.Count} routes¡£");
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

        /// Give the NPC the map you want to open this time.
        public string GetGraphForNpc(VariableStore store, string npc) {
            if (string.IsNullOrEmpty(npc)) return null;
            // 1) Routing priority: sequential matching whenAllFlags
            foreach (var r in _routes.Where(r => string.Equals(r.npc, npc, StringComparison.Ordinal))) {
                if (PassAll(store, r.whenAllFlags)) {
                    if (verbose) Debug.Log($"[ConversationDirector] hit route npc={npc} -> {r.nextGraph} (flags: {string.Join(",", r.whenAllFlags)})");
                    return r.nextGraph;
                }
            }
            // 2) Default
            if (_npcToDefault.TryGetValue(npc, out var g)) {
                if (verbose) Debug.Log($"[ConversationDirector] No route hit, use default npc={npc} -> {g}");
                return g;
            }
            Debug.LogWarning($"[ConversationDirector] did not found {npc}'s default map, return null");
            return null;
        }

        static bool PassAll(VariableStore store, List<string> flags) {
            if (flags == null || flags.Count == 0) return false; // Constraints: must be conditional to be routed
            foreach (var f in flags) if (!store.Has(f)) return false;
            return true;
        }

        void OnDialogueEnded(string graphId) {
            if (string.IsNullOrEmpty(graphId)) return;
            if (_graphToNpc.TryGetValue(graphId, out var npc)) {
                // Write generic "visited" tags
                var flag = $"NPC.{npc}.Visited";
                var runner = FindAnyObjectByType<DialogueRunner>();
                if (runner != null) {
                    runner.Store.Add(flag);
                    if (verbose) Debug.Log($"[ConversationDirector] End of dialogue: marking {flag}=true");
                }
            }
        }
    }
}
