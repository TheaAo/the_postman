using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.InputSystem;


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
              if (Keyboard.current.eKey.isPressed)
              {
                  textComponent.text = fullText;
                  break; 
              }


              currentText += fullText[i];
              textComponent.text = currentText;
              
              currentIndex = i;
              
              yield return new WaitForSeconds(revealSpeed);
          }
        } else
        {
            textComponent.text = fullText;
            if (scrollRect != null)
            {
                yield return null; 

                float contentHeight = scrollRect.content.rect.height;
                float viewportHeight = scrollRect.viewport.rect.height;

                if (contentHeight > viewportHeight)
                {
                    yield return new WaitForSeconds(5f);
                    yield return StartCoroutine(SmoothScrollToBottom(30f));
                }
            }
        }
        
        Debug.Log("Done!");
        actionButton.SetActive(true);
    }

    IEnumerator SmoothScrollToBottom(float duration)
{
    float startValue = scrollRect.verticalNormalizedPosition;
    float elapsed = 0f;

    while (elapsed < duration)
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / duration);

        scrollRect.verticalNormalizedPosition = Mathf.Lerp(startValue, 0f, t);

        yield return null;
    }

    scrollRect.verticalNormalizedPosition = 0f;
}
}