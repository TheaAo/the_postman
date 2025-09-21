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
  public TextMeshProUGUI nameText;
  public TextMeshProUGUI dialogueText;
  public Button[] buttons;   // Pre-create some buttons (e.g. 2–3)

  private DialogueLine[] dialogueLines;
  private int currentLineIndex = 0;

  public void StartDialogue(string npcName, DialogueLine[] lines)
  {
    if (dialogueBox.activeSelf)
      return; // Dialogue already active

    dialogueBox.SetActive(true);
    nameText.text = npcName;
    dialogueLines = lines;
    currentLineIndex = 0;

    ShowLine(dialogueLines[currentLineIndex]);
  }

  void ShowLine(DialogueLine line)
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
        option.onClick.Invoke();
        NextLine();
      });
    }
  }

  void NextLine()
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


