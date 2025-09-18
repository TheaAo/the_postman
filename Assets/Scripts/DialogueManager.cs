using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[System.Serializable]
public class DialogueOption {
    public string buttonText;
    public Action onClick;
}

[Serializable]
public class DialogueLine
{
    public string npcText;        // What NPC says
    public DialogueOption[] options; // Player response options
}

public class DialogueManager : MonoBehaviour
{
  public GameObject dialogueBox;
  public TextMeshProUGUI nameText;
  public TextMeshProUGUI dialogueText;
  public Button[] buttons;

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

    // Setup buttons
    // for (int i = 0; i < buttons.Length; i++)
    // {
    //   if (i < line.options.Length)
    //   {
    //     buttons[i].gameObject.SetActive(true);
    //     buttonTexts[i].text = line.options[i].buttonText;
    //     int index = i;
    //     buttons[i].onClick.RemoveAllListeners();
    //     buttons[i].onClick.AddListener(() =>
    //     {
    //       line.options[index]?.onClick.Invoke();
    //       NextLine();
    //     });
    //   }
    //   else
    //   {
    //     buttons[i].gameObject.SetActive(false);
    //   }
    // }
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
      dialogueBox.SetActive(false);
    }
  }
}
