using UnityEngine;
using UnityEngine.SceneManagement;


public class OptionManager : MonoBehaviour
{
  DialogueLine GenerateNewLineFromText(string text)
  {
    return new DialogueLine
    {
      npcText = text,
      options = new DialogueOption[0]
    };
  }

  void AddNewLines(string lineText)
  {
    DialogueLine[] newLines = { GenerateNewLineFromText(lineText) };
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
    string lineText = "Nelson: You do know the rules. Go to 'TBD' and say Rabbit recommands you. ";
    AddNewLines(lineText);
  }

  public void NelsonOption2()
  {
    string lineText = "Nelson: And you know I am a police. Get off my face before I arrest you for assaulting a police.";
    AddNewLines(lineText);
  }
}
