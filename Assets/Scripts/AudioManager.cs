using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("BGM files")]
    public AudioClip[] bgmClips; // store all the bgms
    
    private AudioSource bgmSource;
    
    [Header("volume setting")]
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    
    void Awake()
    {
       
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
            bgmSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true; 
        }
    }
    
   private int currentBGMIndex = -1; 

public void PlayBGM(int clipIndex)
{
    if (bgmSource != null && clipIndex >= 0 && clipIndex < bgmClips.Length && bgmClips[clipIndex] != null)
    {
        
        if (currentBGMIndex == clipIndex && bgmSource.isPlaying)
        {
            return; 
        }
        
        
        currentBGMIndex = clipIndex;
        bgmSource.clip = bgmClips[clipIndex];
        bgmSource.loop = true; 
        bgmSource.Play();
    }
}
   
    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
}