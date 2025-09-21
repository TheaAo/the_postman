using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NPCManager : MonoBehaviour, IClickable
{
    [Header("NPC Info")]
    public string npcName;

    [Header("Dialogue Lines")]
    public DialogueLine[] dialogueLines;
    
    void Start()
    {
        Debug.Log("NPCManager started.");
    }

    public void OnClick()
  {
    Debug.Log("Clicked NPC: " + npcName);

    // Find the DialogueManager in the scene
    DialogueManager manager = FindFirstObjectByType<DialogueManager>();

    if (manager != null)
    {
      manager.StartDialogue(npcName, dialogueLines);
    }
    else
    {
      Debug.LogWarning("DialogueManager not found in scene!");
    }
  }
}
