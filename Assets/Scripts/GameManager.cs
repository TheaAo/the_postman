using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager I { get; private set; }   // 单例实例
    public event Action<int> OnGoldChanged;             // 金币变化事件

    private int goldCount = 0;                          // 当前金币数

    // 在加载场景前自动创建 GameManager
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

    // 增减金币（默认+1）
    public void AddGold(int amount = 1)
    {
        goldCount += amount;
        if (goldCount < 0) goldCount = 0;
        OnGoldChanged?.Invoke(goldCount);

        // ✅ 调试输出
        Debug.Log($"当前金币数：{goldCount}");
    }

    // 获取当前金币数
    public int GetGold()
    {
        return goldCount;
    }

    // 设置金币数（如加载存档）
    public void SetGold(int newAmount)
    {
        goldCount = Mathf.Max(0, newAmount);
        OnGoldChanged?.Invoke(goldCount);
    }


}
