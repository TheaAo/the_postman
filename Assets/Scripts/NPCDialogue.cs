using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class NPCDialogue : MonoBehaviour, IClickable
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
        Debug.Log("NPC Dialog Starts.");
    }


    public void OnClick()
    {
        Debug.Log("NPC Clicked. isDialogueActive: " + isDialogueActive);
        if (!isDialogueActive)
        {
            StartDialogue();
        }
        else
        {
            NextDialogue();
        }
    }


    void StartDialogue()
    {
        Debug.Log("Starting dialogue with " + npcName);
        dialogueBox.SetActive(true);
        nameText.text = npcName;
        currentLine = 0;
        dialogueText.text = dialogueLines[currentLine];
        isDialogueActive = true;
    }

    void NextDialogue()
    {
        Debug.Log("Next dialogue.");
        currentLine++;
        if (currentLine < dialogueLines.Length)
        {
            dialogueText.text = dialogueLines[currentLine];
        }
        else
        {
            dialogueBox.SetActive(false);
            isDialogueActive = false;

        }
    }
}
