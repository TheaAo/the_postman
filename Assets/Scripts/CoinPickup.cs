using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    public enum CoinType { Small, Medium, Large }

    [SerializeField] private CoinType coinType;       
    [SerializeField] private int value = 1;           
    [SerializeField] private string playerTag = "Player";

    void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;  // Automatically set isTrigger to be true
    }

    void Start()
    {
        // Set gold value according to type (can also be changed manually in Inspector)
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

        GameManager.I?.AddGold(value);    // Increase Gold
        Destroy(gameObject);              // Destroy Coin object
    }
}