using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    public enum CoinType { Small, Medium, Large }

    [SerializeField] private CoinType coinType;       // 金币类型
    [SerializeField] private int value = 1;           // 金币价值
    [SerializeField] private string playerTag = "Player";

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;  // 自动设为触发器
    }

    void Start()
    {
        // 根据类型设置金币价值（也可在 Inspector 手动改）
        switch (coinType)
        {
            case CoinType.Small:
                value = 50;
                break;
            case CoinType.Medium:
                value = 100;
                break;
            case CoinType.Large:
                value = 300;
                break;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        GameManager.I?.AddGold(value);    // 增加金币
        Destroy(gameObject);              // 销毁金币
    }
}