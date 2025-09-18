using UnityEngine;
using UnityEngine.SceneManagement;

public class StartGame : MonoBehaviour
{
    public void StartTheGame()
    {
        Debug.Log("Start Button Clicked");
        SceneManager.LoadScene("IntroScene");

    }
}
