using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public Animator animator;
    public TextMeshProUGUI loadingText;
    public float fadeTime = 1f;
    public float holdTime = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(string sceneName, string message = "Loading...")
    {
      Debug.Log($"SceneManager Loading scene: {sceneName} with message: {message}");
      StartCoroutine(DoTransition(sceneName, message));
    }

    private IEnumerator DoTransition(string sceneName, string message)
    {
        Debug.Log($"DoTransition: {sceneName} with message: {message}");
        loadingText.text = message;
        animator.SetTrigger("FadeOut");
        yield return new WaitForSeconds(fadeTime);

        SceneManager.LoadScene(sceneName);
        yield return null;
    }
}
