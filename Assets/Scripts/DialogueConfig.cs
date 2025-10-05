using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueConfig : MonoBehaviour, IClickable
{
    // [Header("NPC Info")]
    // public string npcName;

    [Header("Dialogue Lines")]
    public DialogueLine[] dialogueLines;

    void Start()
    {
        Debug.Log("DialogueConfig started.");
    }

    public void OnClick()
  {
    Debug.Log("DialogueConfig OnClick");

    // Find the DialogueManager in the scene
    DialogueManager manager = FindFirstObjectByType<DialogueManager>();

    if (manager != null)
    {
      Debug.Log("Dialogue manager exists");
      manager.StartDialogue(dialogueLines);
    }
    else
    {
      Debug.LogWarning("DialogueManager not found in scene!");
    }
  }
}
