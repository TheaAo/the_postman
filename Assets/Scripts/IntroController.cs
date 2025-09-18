using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class IntroController : MonoBehaviour
{
    // public TextMeshProUGUI storyText;      // 指向 StoryText
    // [TextArea(3,10)]
    // public string fullStory;               // 剧情文字
    // public float typeSpeed = 0.05f;        // 每个字母显示间隔
    public string nextScene = "GameScene"; // 游戏主场景名字

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

    //     // 等待玩家按任意键
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
