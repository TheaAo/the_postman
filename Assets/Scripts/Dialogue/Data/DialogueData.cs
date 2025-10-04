// 数据层：只存数据，不做任何 UI/流程逻辑

using System;
using System.Collections.Generic;

namespace Game.Dialogue {
    // 一段完整对话（通常对应一个NPC）
    [Serializable]
    public class DialogueGraph {
        public string id;                     // 对话图ID（如：npc_blacksmith）
        public string startNodeId;            // 进入时的起始节点
        public Dictionary<string, DialogueNode> nodes = new();  // 节点表

        public DialogueNode GetNodeOrNull(string nodeId) {
            if (string.IsNullOrEmpty(nodeId)) return null;
            nodes.TryGetValue(nodeId, out var node);
            return node;
        }
    }

    // 单个台词/步骤
    [Serializable]
    public class DialogueNode {
        public string id;                     // 节点ID（如：intro_01）
        public string speaker;                // 说话者显示名
        public string text;                   // 台词文本
        public string portraitKey;            // 头像资源key，可空
        public List<DialogueOption> options = new(); // 选项列表
        public string nextId;                 // 线性下一节点（无选项时使用）
        public List<string> events = new();   // 进入节点时触发的事件key
    }

    // 一个可选分支
    [Serializable]
    public class DialogueOption {
        public string text;                   // 按钮显示文字
        public string nextId;                 // 选择后跳转的节点
        public List<string> requireFlags = new(); // 显示需要满足的flag
        public List<string> setFlags = new();     // 选择后设置的flag
    }

    // 对话内容来源接口（可从JSON/资源等加载）
    public interface IDialogueSource {
        DialogueGraph LoadGraph(string graphId);
        bool HasGraph(string graphId);
    }

    // NPC和对话图的映射表
    [Serializable]
    public class DialogueRegistry {
        public Dictionary<string, string> npcToGraph = new();

        public bool TryGetGraphId(string npcKey, out string graphId) {
            if (string.IsNullOrEmpty(npcKey)) {
                graphId = null;
                return false;
            }
            return npcToGraph.TryGetValue(npcKey, out graphId);
        }
    }

    // 辅助：检查引用是否有效
    public static class DialogueValidation {
        public static bool Validate(DialogueGraph graph, List<string> errors) {
            bool ok = true;
            if (graph == null) {
                errors?.Add("Graph 为 null");
                return false;
            }
            if (string.IsNullOrEmpty(graph.id)) {
                ok = false; errors?.Add("Graph.id 为空");
            }
            if (string.IsNullOrEmpty(graph.startNodeId) || !graph.nodes.ContainsKey(graph.startNodeId)) {
                ok = false; errors?.Add($"起始节点不存在：{graph.startNodeId}");
            }

            foreach (var kv in graph.nodes) {
                var node = kv.Value;
                if (node == null) {
                    ok = false; errors?.Add($"节点 {kv.Key} 为 null");
                    continue;
                }
                if (!string.IsNullOrEmpty(node.nextId) && !graph.nodes.ContainsKey(node.nextId)) {
                    ok = false; errors?.Add($"节点 {node.id} 的 nextId 无效：{node.nextId}");
                }
                if (node.options != null) {
                    for (int i = 0; i < node.options.Count; i++) {
                        var opt = node.options[i];
                        if (opt == null) {
                            ok = false; errors?.Add($"节点 {node.id} 的选项 {i} 为 null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(opt.text)) {
                            ok = false; errors?.Add($"节点 {node.id} 的选项 {i} 文本为空");
                        }
                        if (string.IsNullOrEmpty(opt.nextId) || !graph.nodes.ContainsKey(opt.nextId)) {
                            ok = false; errors?.Add($"节点 {node.id} 的选项 {i} 的 nextId 无效：{opt.nextId}");
                        }
                    }
                }
            }
            return ok;
        }
    }
}
