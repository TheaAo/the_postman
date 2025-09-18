using UnityEngine;

public class NPCManager : MonoBehaviour, IClickable
{
    public string npcName;
    public DialogueLine[] dialogueLines;

    public void OnClick()
    {
        Debug.Log("NPCManager: NPC Clicked.");
        DialogueManager manager = FindFirstObjectByType<DialogueManager>();
        Debug.Log("Manager found: " + (manager != null));
        if (manager != null)
        {
          Debug.Log("NPCManager: Starting dialogue with " + npcName);
          manager.StartDialogue(npcName, dialogueLines);
        }
    }
}