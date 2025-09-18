using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;


public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue UI")]
    public GameObject dialogueBox;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Button nextButton;

    [Header("NPC Info")]
    public string npcName;
    [TextArea(1, 5)]
    public string[] dialogueLines;

    private int currentLine = 0;
    private bool isDialogueActive = false;

    void Start()
    {
        Debug.Log("Start.");
        Debug.Log("isDialogueActive: " + isDialogueActive);
        Debug.Log("dialogueLines.Length: " + dialogueLines.Length);
    }

    // void OnMouseOver()
    // {
    //     Debug.Log("Mouse over NPC");
    // }

    void OnMouseDown()
    {
        Debug.Log("Mouse Clicked on NPC");
        Debug.Log("isDialogueActive: " + isDialogueActive);
        Debug.Log("dialogueLines.Length: " + dialogueLines.Length);
    }

    void Update()
    {
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     Debug.Log("AAAAMouse Left Button was pressed this frame.");
        // }
  
    }


    void StartDialogue()
    {
        // dialogueBox.SetActive(true);
        // nameText.text = npcName;
        // currentLine = 0;
        // dialogueText.text = dialogueLines[currentLine];
        // isDialogueActive = true;

        // // 停止 NPC 移动
        // // if (npcMovement != null) npcMovement.StopMoving();

        // // UI 跟随 NPC 头顶
        // Vector3 npcScreenPos = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1.5f, 0));
        // dialogueBox.transform.position = npcScreenPos;
    }

    void NextDialogue()
    {
        // currentLine++;
        // if (currentLine < dialogueLines.Length)
        // {
        //     dialogueText.text = dialogueLines[currentLine];
        // }
        // else
        // {
        //     dialogueBox.SetActive(false);
        //     isDialogueActive = false;

        //     // 恢复移动
        //     // if (npcMovement != null) npcMovement.ResumeMoving();
        // }
    }
}
