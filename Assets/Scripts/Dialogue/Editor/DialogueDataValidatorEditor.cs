// Assets/Editor/DialogueDataValidatorEditor.cs
// 目的：离线校验 Resources/Dialogues 下的 JSON 是否符合数据层预期结构；不依赖你的运行时或注册器类。
// 用法：菜单 Tools/Dialogue/Validate All Graphs
// 依赖：Newtonsoft.Json（Unity 6 自带）

#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

public static class DialogueDataValidatorEditor {
    [MenuItem("Tools/Dialogue/Validate All Graphs")]
    public static void ValidateAll() {
        var assets = Resources.LoadAll<TextAsset>("Dialogues");
        if (assets == null || assets.Length == 0) {
            Debug.LogWarning("未在 Assets/Resources/Dialogues/ 下找到任何 .json（确保放在 Resources 里，且文件扩展名为 .json）。");
            return;
        }

        int ok = 0, fail = 0;
        foreach (var ta in assets) {
            var errors = new List<string>();
            var warns = new List<string>();

            try {
                var root = JObject.Parse(ta.text);

                // ---- 1) 顶层字段 ----
                var id = ReadString(root, "id");
                var startNodeId = ReadString(root, "startNodeId");
                var nodesObj = root["nodes"] as JObject;
                if (string.IsNullOrEmpty(id)) errors.Add("缺少或为空：id");
                if (string.IsNullOrEmpty(startNodeId)) errors.Add("缺少或为空：startNodeId");
                if (nodesObj == null) errors.Add("缺少：nodes 对象");

                // 没有 nodes 就不继续深入
                int nodeCount = 0, optionCount = 0;

                // ---- 2) 节点级校验 ----
                var nodeIds = new HashSet<string>(StringComparer.Ordinal);
                var referIds = new HashSet<string>(StringComparer.Ordinal); // nextId 引用

                if (nodesObj != null) {
                    foreach (var kv in nodesObj) {
                        nodeCount++;
                        var n = kv.Value as JObject;
                        var nid = ReadString(n, "id") ?? kv.Key;
                        if (string.IsNullOrEmpty(nid))
                            errors.Add($"节点缺少 id（key: {kv.Key}）");
                        else
                            nodeIds.Add(nid);

                        var text = ReadString(n, "text");
                        if (string.IsNullOrEmpty(text))
                            warns.Add($"节点[{nid}] 文本 text 为空（允许但建议填写）");

                        // nextId（可空）
                        var nextId = ReadStringAllowNull(n, "nextId");
                        if (!string.IsNullOrEmpty(nextId))
                            referIds.Add(nextId);

                        // requireFlags（可选，必须是字符串数组）
                        CheckStringArray(n, "requireFlags", warns, $"节点[{nid}]");

                        // 选项
                        var opts = n["options"] as JArray;
                        if (opts != null) {
                            foreach (var ot in opts) {
                                optionCount++;
                                var o = ot as JObject;
                                var otext = ReadString(o, "text");
                                if (string.IsNullOrEmpty(otext))
                                    errors.Add($"节点[{nid}] 存在选项但 text 为空");

                                var onext = ReadStringAllowNull(o, "nextId");
                                if (!string.IsNullOrEmpty(onext))
                                    referIds.Add(onext);

                                CheckStringArray(o, "setFlags", warns, $"节点[{nid}] 的选项");
                                CheckStringArray(o, "requireFlags", warns, $"节点[{nid}] 的选项");
                            }
                        }
                    }
                }

                // ---- 3) 关系一致性 ----
                if (nodesObj != null) {
                    if (!nodeIds.Contains(startNodeId))
                        errors.Add($"startNodeId 指向不存在的节点：{startNodeId}");

                    foreach (var rid in referIds)
                        if (!nodeIds.Contains(rid))
                            errors.Add($"有 nextId 引用未知节点：{rid}");
                }

                // ---- 4) 结果输出 ----
                if (errors.Count == 0) {
                    ok++;
                    Debug.Log($"✔ {ta.name}  (id:{id})  节点:{nodeCount}  选项:{optionCount}  起始:{startNodeId}"
                              + (warns.Count > 0 ? $"   [警告 {warns.Count} 条，展开 Console 查看]" : ""));
                    foreach (var w in warns) Debug.LogWarning("  - " + w);
                }
                else {
                    fail++;
                    Debug.LogWarning($"✘ {ta.name}  (id:{id ?? "null"})  发现 {errors.Count} 个问题：");
                    foreach (var e in errors) Debug.LogError("  - " + e);
                    foreach (var w in warns) Debug.LogWarning("  - " + w);
                }
            }
            catch (Exception ex) {
                fail++;
                Debug.LogError($"✘ {ta.name}  JSON 解析失败：{ex.Message}");
            }
        }

        Debug.Log($"校验完成：通过 {ok} 个，失败 {fail} 个（目录：Resources/Dialogues）。");
    }

    // ===== Helper =====
    static string ReadString(JObject obj, string key) {
        return obj?[key]?.Type == JTokenType.String ? obj[key]!.Value<string>() : null;
    }
    static string ReadStringAllowNull(JObject obj, string key) {
        if (obj == null || !obj.ContainsKey(key)) return null;
        var tok = obj[key];
        if (tok == null || tok.Type == JTokenType.Null) return null;
        return tok.Type == JTokenType.String ? tok.Value<string>() : null;
    }
    static void CheckStringArray(JObject obj, string key, List<string> warns, string ctx) {
        if (obj == null || obj[key] == null) return;
        if (obj[key] is JArray arr) {
            foreach (var t in arr)
                if (t.Type != JTokenType.String)
                    warns.Add($"{ctx} 的 {key} 含非字符串元素");
        }
        else {
            warns.Add($"{ctx} 的 {key} 不是数组");
        }
    }
}
#endif
