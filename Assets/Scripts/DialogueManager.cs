using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using System;

[System.Serializable]
public class DialogueOption {
    public string buttonText;
    public UnityEvent onClick;
}

[Serializable]
public class DialogueLine
{
    [TextArea(2, 5)]
    public string npcText;        // What NPC says
    public DialogueOption[] options; // Player response options
}

public class DialogueManager : MonoBehaviour
{
  [Header("UI References")]
  public GameObject dialogueBox;
  public TextMeshProUGUI dialogueText;
  public Button[] buttons;   // Pre-create some buttons (e.g. 2–3)

  private DialogueLine[] dialogueLines;
  private int currentLineIndex = 0;
  
  void Start()
  {
    dialogueBox.SetActive(false);
  }

  public void StartDialogue(DialogueLine[] lines)
  {
    Debug.Log("Starting dialogue..." + dialogueBox.activeSelf);

    if (dialogueBox.activeSelf)
    {
      Debug.Log("Dialogue already active.");
      return; // Dialogue already active
    }

    dialogueBox.SetActive(true);
    // nameText.text = npcName;
    dialogueLines = lines;
    currentLineIndex = 0;

    ShowLine(dialogueLines[currentLineIndex]);
  }

  public void ShowLine(DialogueLine line)
  {
    dialogueText.text = line.npcText;

    // Hide all buttons first
    foreach (var btn in buttons)
      btn.gameObject.SetActive(false);

    // Show options for this line
    for (int i = 0; i < line.options.Length && i < buttons.Length; i++)
    {
      int optionIndex = i; // Fix closure issue
      var option = line.options[optionIndex];

      buttons[optionIndex].gameObject.SetActive(true);
      buttons[optionIndex].GetComponentInChildren<TextMeshProUGUI>().text = option.buttonText;

      buttons[optionIndex].onClick.RemoveAllListeners();

      buttons[optionIndex].onClick.AddListener(() =>
      {
        Debug.Log($"Option chosen: {option.buttonText}");
        if (option.onClick != null)
        {
          option.onClick.Invoke();
        }
        else
        {
          NextLine();
        }
      });
    }
  }

  public void NextLine()
  {
    currentLineIndex++;
    if (currentLineIndex < dialogueLines.Length)
    {
      ShowLine(dialogueLines[currentLineIndex]);
    }
    else
    {
      EndDialogue();
    }
  }
    
  public void EndDialogue()
  {
    dialogueBox.SetActive(false);
  }
}


