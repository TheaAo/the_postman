using UnityEngine;

public class OptionManager : MonoBehaviour
{
  
  DialogueLine ConfigNextLine(string text)
  {
    return new DialogueLine
    {
      npcText = text,
      options = new DialogueOption[0] // 没有选项
    };
  }

  void ConfigOptionLine(string text)
  {
    Debug.Log("ConfigOptionLine: " + text);
    DialogueLine line = ConfigNextLine(text);
    DialogueManager manager = FindFirstObjectByType<DialogueManager>();
    if (manager != null)
    {
      Debug.Log("Dialogue manager exists");
      manager.ShowLine(line);
    }
    else
    {
      Debug.LogWarning("DialogueManager not found in scene!");
    }

  }
  public void NelsonOption1()
  {
    ConfigOptionLine("Nelson: You do know the rules. Go to 'TBD' and say Rabbit recommands you. ");
  }

  public void NelsonOption2()
  {
    ConfigOptionLine("Nelson: And you know I am a police. Get off my face before I arrest you for assaulting a police. ");
  }
}
