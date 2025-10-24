using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    // public TextMeshProUGUI storyText;      // refer to StoryText
    // [TextArea(3,10)]
    // public string fullStory;               // text of a play
    // public float typeSpeed = 0.05f;        // Display interval per letter
    public string nextScene = "GameScene"; // Name of the main scene of the game

    // private bool skip = false;

    // void Start()
    // {
    //     StartCoroutine(ShowText());
    // }

    // void Update()
    // {
    //     if (Input.anyKeyDown)
    //         skip = true;
    // }

    // IEnumerator ShowText()
    // {
    //     storyText.text = "";
    //     foreach (char c in fullStory)
    //     {
    //         if (skip) break;
    //         storyText.text += c;
    //         yield return new WaitForSeconds(typeSpeed);
    //     }
    //     storyText.text = fullStory;

    //     // Wait for the player to press any key
    //     while (!Input.anyKeyDown)
    //         yield return null;

    //     // SceneManager.LoadScene(nextScene);
    //     SkipIntro();
    // }

    public void SkipIntro()
    {
        Debug.Log("Next Button Clicked");
        SceneManager.LoadScene(nextScene);

    }
}
