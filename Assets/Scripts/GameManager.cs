using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }   
    public event Action<int> OnGoldChanged;             // Gold change events

    private int goldCount = 0;                          // Current number of coins

    // Automatically creates GameManager before loading the scene 
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void AutoCreate()
    {
        if (I == null)
        {
            GameObject go = new GameObject("GameManager");
            I = go.AddComponent<GameManager>();
            DontDestroyOnLoad(go);
        }
    }

    // Increase or decrease gold (default +1)
    public void AddGold(int amount = 1)
    {
        goldCount += amount;
        if (goldCount < 0) goldCount = 0;
        OnGoldChanged?.Invoke(goldCount);

        Debug.Log($"Current number of coins：{goldCount}");
    }

    // Get the current number of coins
    public int GetGold()
    {
        return goldCount;
    }

    // Setting the number of coins (e.g. loading an archive)
    public void SetGold(int newAmount)
    {
        goldCount = Mathf.Max(0, newAmount);
        OnGoldChanged?.Invoke(goldCount);
    }


}
