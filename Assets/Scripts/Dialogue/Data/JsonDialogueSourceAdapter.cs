// Parses Resources/Dialogues/*.json into a RuntimeNode/RuntimeOption for use by the DialogueRunner.

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Dialogue.Runtime {
    public class JsonDialogueSourceAdapter : MonoBehaviour, IRuntimeDialogueSource {
        [Header("Resources folder name")]
        [SerializeField] private string resourcesFolder = "Dialogues";

        // graphId -> run-time cache
        private readonly Dictionary<string, GraphCache> _cache = new();

        private class GraphCache {
            public string startNodeId;
            public readonly Dictionary<string, RuntimeNode> nodes = new();
        }

        // ¡ª¡ª IRuntimeDialogueSource ¡ª¡ª //
        public string GetStartNodeId(string graphId) {
            var g = GetOrBuild(graphId);
            return g?.startNodeId;
        }

        public bool TryGetNode(string graphId, string nodeId, out RuntimeNode node) {
            node = null;
            var g = GetOrBuild(graphId);
            if (g == null) return false;
            return g.nodes.TryGetValue(nodeId, out node);
        }

        // ¡ª¡ª Build or fetch cache ¡ª¡ª //
        GraphCache GetOrBuild(string graphId) {
            if (string.IsNullOrEmpty(graphId)) return null;
            if (_cache.TryGetValue(graphId, out var gc)) return gc;

            // 1) first£ºFile name == graphId
            var path = string.IsNullOrEmpty(resourcesFolder) ? graphId : $"{resourcesFolder}/{graphId}";
            var ta = Resources.Load<TextAsset>(path);

            // 2) Optional: Traverse the folder, matching by the JSON "id" field.
            if (ta == null) {
                foreach (var candidate in Resources.LoadAll<TextAsset>(resourcesFolder)) {
                    try {
                        var jid = JObject.Parse(candidate.text)?["id"]?.Value<string>();
                        if (!string.IsNullOrEmpty(jid) && string.Equals(jid, graphId, StringComparison.Ordinal)) {
                            ta = candidate;
                            break;
                        }
                    }
                    catch { }
                }
            }

            if (ta == null) {
                Debug.LogError($"[JsonDialogueSourceAdapter] No graphical resources found£ºgraphId={graphId}");
                return null;
            }

            try {
                var root = JObject.Parse(ta.text);

                var startNodeId = root["startNodeId"]?.Value<string>();
                var nodesToken = root["nodes"] as JObject;
                if (nodesToken == null) {
                    Debug.LogError($"[JsonDialogueSourceAdapter] JSON lacks nodes£º{ta.name}");
                    return null;
                }

                gc = new GraphCache { startNodeId = startNodeId };

                foreach (var kv in nodesToken) {
                    var nObj = kv.Value as JObject;
                    if (nObj == null) continue;

                    var rn = new RuntimeNode {
                        id = nObj["id"]?.Value<string>() ?? kv.Key,
                        speaker = nObj["speaker"]?.Value<string>(),
                        text = nObj["text"]?.Value<string>(),
                        nextId = nObj["nextId"]?.Type == JTokenType.Null ? null : nObj["nextId"]?.Value<string>(),
                        requireFlags = ReadStringList(nObj["requireFlags"]),
                        options = new List<RuntimeOption>(),
                        events = ReadStringList(nObj["events"]),
                    };

                    var opts = nObj["options"] as JArray;
                    if (opts != null) {
                        foreach (var oTok in opts) {
                            var oObj = oTok as JObject;
                            if (oObj == null) continue;

                            rn.options.Add(new RuntimeOption {
                                text = oObj["text"]?.Value<string>(),
                                nextId = oObj["nextId"]?.Type == JTokenType.Null ? null : oObj["nextId"]?.Value<string>(),
                                setFlags = ReadStringList(oObj["setFlags"]),
                                requireFlags = ReadStringList(oObj["requireFlags"]),

                                cost = oObj["cost"] != null ? oObj["cost"].Value<int>() : 0,
                                insufficientText = oObj["insufficientText"]?.Value<string>(),
                                insufficientNextId = oObj["insufficientNextId"]?.Value<string>()
                            });
                        }
                    }

                    gc.nodes[rn.id] = rn;
                }

                _cache[graphId] = gc;
                return gc;
            }
            catch (Exception e) {
                Debug.LogError($"[JsonDialogueSourceAdapter] parsing anomaly£¨{ta.name}£©£º\n{e}");
                return null;
            }
        }

        static List<string> ReadStringList(JToken token) {
            var list = new List<string>();
            if (token is JArray arr) {
                foreach (var t in arr) {
                    var s = t?.Value<string>();
                    if (!string.IsNullOrEmpty(s)) list.Add(s);
                }
            }
            return list;
        }
    }
}
