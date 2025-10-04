using UnityEngine;
using UnityEngine.EventSystems;

public class DialogueBoxOnClick : MonoBehaviour, IPointerClickHandler
{
  // Start is called once before the first execution of Update after the MonoBehaviour is created
  void Start()
  {
      Debug.Log("DialogueBoxOnClick started.");
  }

  public void OnPointerClick(PointerEventData eventData)
  {
    Debug.Log($"{gameObject.name} 被点击了！");
    // Find the DialogueManager in the scene
    DialogueManager manager = FindFirstObjectByType<DialogueManager>();

    if (manager != null && manager.dialogueBox.activeSelf)
    {
      manager.NextLine();
    }
    else
    {
      Debug.LogWarning("DialogueManager not found in scene!");
    }
  }
}
