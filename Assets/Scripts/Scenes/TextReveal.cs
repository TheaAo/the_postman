using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TextReveal : MonoBehaviour
{
    [Header("Components")]
    public ScrollRect scrollRect;
    public TextMeshProUGUI textComponent; 

    public GameObject actionButton;
    
    [Header("Settings")]
    public string fullText;
    public float revealSpeed = 0.05f;
    public bool autoScrollToTop = true;
    public bool revealByCharacter = true;
    
    private string currentText = "";
    private int currentIndex = 0;
    
    void Start()
    {
        if (autoScrollToTop)
        {
            scrollRect.verticalNormalizedPosition = 1f; 
        }
        
        if (string.IsNullOrEmpty(fullText))
        {
            fullText = textComponent.text; 
        }
        
        textComponent.text = "";
        actionButton.SetActive(false);
        StartCoroutine(RevealText());
    }
    
    IEnumerator RevealText()
    {
        currentText = "";
        
        if (revealByCharacter)
        {
          for (int i = 0; i < fullText.Length; i++)
          {
              currentText += fullText[i];
              textComponent.text = currentText;
              
              currentIndex = i;
              
              yield return new WaitForSeconds(revealSpeed);
          }
        } else
        {
            textComponent.text = fullText;
        }
        
        Debug.Log("Done!");
        actionButton.SetActive(true);
    }
}