// 作用：把 Resources/Dialogues/*.json 解析为 RuntimeNode/RuntimeOption，提供给 DialogueRunner 使用。
// 依赖：Newtonsoft.Json（Unity 6 自带）

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Game.Dialogue.Runtime {
    public class JsonDialogueSourceAdapter : MonoBehaviour, IRuntimeDialogueSource {
        [Header("Resources 文件夹名")]
        [SerializeField] private string resourcesFolder = "Dialogues";

        // graphId -> 运行时缓存
        private readonly Dictionary<string, GraphCache> _cache = new();

        private class GraphCache {
            public string startNodeId;
            public readonly Dictionary<string, RuntimeNode> nodes = new();
        }

        // —— IRuntimeDialogueSource —— //
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

        // —— 内部：构建或取缓存 —— //
        GraphCache GetOrBuild(string graphId) {
            if (string.IsNullOrEmpty(graphId)) return null;
            if (_cache.TryGetValue(graphId, out var gc)) return gc;

            // 1) 优先：文件名 == graphId
            var path = string.IsNullOrEmpty(resourcesFolder) ? graphId : $"{resourcesFolder}/{graphId}";
            var ta = Resources.Load<TextAsset>(path);

            // 2) 备选：遍历文件夹，按 JSON 的 "id" 字段匹配
            if (ta == null) {
                foreach (var candidate in Resources.LoadAll<TextAsset>(resourcesFolder)) {
                    try {
                        var jid = JObject.Parse(candidate.text)?["id"]?.Value<string>();
                        if (!string.IsNullOrEmpty(jid) && string.Equals(jid, graphId, StringComparison.Ordinal)) {
                            ta = candidate;
                            break;
                        }
                    }
                    catch { /* 忽略坏文件 */ }
                }
            }

            if (ta == null) {
                Debug.LogError($"[JsonDialogueSourceAdapter] 未找到图资源：graphId={graphId}");
                return null;
            }

            try {
                var root = JObject.Parse(ta.text);

                var startNodeId = root["startNodeId"]?.Value<string>();
                var nodesToken = root["nodes"] as JObject;
                if (nodesToken == null) {
                    Debug.LogError($"[JsonDialogueSourceAdapter] JSON 缺少 nodes：{ta.name}");
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
                Debug.LogError($"[JsonDialogueSourceAdapter] 解析异常（{ta.name}）：\n{e}");
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
