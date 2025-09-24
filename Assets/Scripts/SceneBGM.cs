using System.Collections;
using UnityEngine;

public class SceneBGM : MonoBehaviour
{
    [Header("BGM Scene setting")]
    public int bgmIndex = 0; //
    
    void Start()
    {
      
        StartCoroutine(PlaySceneBGM());
    }
    
    IEnumerator PlaySceneBGM()
    {
      
        while (AudioManager.Instance == null)
        {
            yield return null;
        }
        
        AudioManager.Instance.PlayBGM(bgmIndex);
    }
}