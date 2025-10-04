using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


public class OptionManager : MonoBehaviour
{
  DialogueLine[] GenerateNewLinesFromText(string[] texts)
  {
    DialogueLine[] lines = new DialogueLine[texts.Length];
    for (int i = 0; i < texts.Length; i++)
    {
      lines[i] = new DialogueLine
      {
        npcText = texts[i],
        options = new DialogueOption[0] // No options for these lines
      };
    }
    return lines;
  }

  void AddNewLinesFromTexts(string[] text)
  {
    DialogueLine[] newLines = GenerateNewLinesFromText(text);
    AddNewLines(newLines);
  }

  void AddNewLines(DialogueLine[] newLines)
  {
    DialogueManager manager = FindFirstObjectByType<DialogueManager>();
    if (manager != null)
    {
      manager.AddLines(newLines);
      manager.NextLine();
      Debug.Log("Added new lines to dialogue manager.");
    }
    else
    {
      Debug.LogWarning("DialogueManager not found in scene!");
    }
  }

  public void ElvisOption1()
  {
    SceneManager.LoadScene("GameScene2");
  }

  public void NelsonOption1()
  {
    DialogueLine[] lines = {
      new DialogueLine
      {
        npcText = "Nelson: You do know the rules. Go to 'TBD' and say Rabbit recommands you. ",
        options = new DialogueOption[]
        {
          new DialogueOption
          {
            buttonText = "Thanks.",
            onClick = new UnityEvent()
          }
        }
      }
    };
    lines[0].options[0].onClick.AddListener(() =>
    {
      SceneManager.LoadScene("GameScene3");
    });
    AddNewLines(lines);
  }

  public void NelsonOption2()
  {
    string[] lineTexts = new string[]
    {
      "Nelson: And you know I am a police. Get off my face before I arrest you for assaulting a police.",
    };
    AddNewLinesFromTexts(lineTexts);
  }

  public void AngeloOption1()
  {
    DialogueLine leaveLine = new DialogueLine
    {
      npcText = "Tom: (That's my last money... Damn it)",
      options = new DialogueOption[]
      {
        new DialogueOption
        {
          buttonText = "Leave",
          onClick = new UnityEvent()
        }
      }
    };

    leaveLine.options[0].onClick.AddListener(() =>
    {
      SceneManager.LoadScene("GameScene4");
    });

    DialogueLine[] lines = {
      new DialogueLine
      {
        npcText = "Angelo: Aha that Nelson, you should told me earlier. You can go in now. ",
        options = new DialogueOption[0]
      },
      leaveLine
    };
    AddNewLines(lines);
  }

  public void AngeloOption2()
  {
    string[] lineText = new string[]
    {
      "Angelo: I don't care about your problems. Go away."
    };
    AddNewLinesFromTexts(lineText);
  }

  public void BrainTeaserOption1()
  {
    string[] lineText = new string[]
    {
      "The Boss: Ummm... interesting answer.",
      "The Boss: Acturally, that does make sense.",
      "The Boss: You make me happy. You can have the flower now.",
      "The Boss: The one who gave me the flower also left a letter here. Do you want it?"
    };
    DialogueLine[] newLines = GenerateNewLinesFromText(lineText);
    newLines[newLines.Length - 1].options = new DialogueOption[]
    {
      new DialogueOption
      {
        buttonText = "Read the letter",
        onClick = new UnityEvent()
      }
    };
    AddNewLines(newLines);
  }

  public void BrainTeaserOption2()
  {
    string[] lineText = new string[]
    {
      "The Boss: Nice try, but I don't think so.",
      "The Boss: You see, I don't get my answer. So you can't get your flower. It's fair enough, right?"
    };
    AddNewLinesFromTexts(lineText);
  }
}
