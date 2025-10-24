// Data layer: data only, no UI/process logic

using System;
using System.Collections.Generic;

namespace Game.Dialogue {
    // A complete dialogue (usually corresponding to an NPC)
    [Serializable]
    public class DialogueGraph {
        public string id;                     // Dialogue map ID
        public string startNodeId;            // Starting node on entry
        public Dictionary<string, DialogueNode> nodes = new();  // list of nodes

        public DialogueNode GetNodeOrNull(string nodeId) {
            if (string.IsNullOrEmpty(nodeId)) return null;
            nodes.TryGetValue(nodeId, out var node);
            return node;
        }
    }

    // Individual dialogue
    [Serializable]
    public class DialogueNode {
        public string id;                     // Node ID (e.g., intro_01)
        public string speaker;                // Speaker display name
        public string text;                   // Text of the lines
        public string portraitKey;            // Avatar resource key, nullable
        public List<DialogueOption> options = new(); // Options list
        public string nextId;                 // Linear next node (used when there is no option)
        public List<string> events = new();   // Event key triggered when entering the node
    }

    // An option
    [Serializable]
    public class DialogueOption {
        public string text;                   // Button display text
        public string nextId;                 // Node to jump to after selection
        public List<string> requireFlags = new(); // Show flags that need to be met
        public List<string> setFlags = new();     // Flag set after selection
    }

    // Dialogue content source interface
    public interface IDialogueSource {
        DialogueGraph LoadGraph(string graphId);
        bool HasGraph(string graphId);
    }

    // Mapping table of NPCs and dialogue graphs
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

    // util: check if the reference is valid
    public static class DialogueValidation {
        public static bool Validate(DialogueGraph graph, List<string> errors) {
            bool ok = true;
            if (graph == null) {
                errors?.Add("Graph is null");
                return false;
            }
            if (string.IsNullOrEmpty(graph.id)) {
                ok = false; errors?.Add("Graph.id is empty");
            }
            if (string.IsNullOrEmpty(graph.startNodeId) || !graph.nodes.ContainsKey(graph.startNodeId)) {
                ok = false; errors?.Add($"Starting node does not exist£º{graph.startNodeId}");
            }

            foreach (var kv in graph.nodes) {
                var node = kv.Value;
                if (node == null) {
                    ok = false; errors?.Add($"node {kv.Key} is null");
                    continue;
                }
                if (!string.IsNullOrEmpty(node.nextId) && !graph.nodes.ContainsKey(node.nextId)) {
                    ok = false; errors?.Add($"node {node.id} has invalid nextId £º{node.nextId}");
                }
                if (node.options != null) {
                    for (int i = 0; i < node.options.Count; i++) {
                        var opt = node.options[i];
                        if (opt == null) {
                            ok = false; errors?.Add($"node {node.id}'s option{i} is null");
                            continue;
                        }
                        if (string.IsNullOrEmpty(opt.text)) {
                            ok = false; errors?.Add($"node {node.id}'s option{i} has empty text");
                        }
                        if (string.IsNullOrEmpty(opt.nextId) || !graph.nodes.ContainsKey(opt.nextId)) {
                            ok = false; errors?.Add($"node {node.id}'s option{i} has invalid nextId£º{opt.nextId}");
                        }
                    }
                }
            }
            return ok;
        }
    }
}
