using UnityEngine;
using TMPro;

public class UICashDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text cashText;
    [SerializeField] private string prefix = "Cash: ";

    void Awake()
    {
        if (cashText == null) cashText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (GameManager.I != null)
        {
            GameManager.I.OnGoldChanged += UpdateCashText;
            UpdateCashText(GameManager.I.GetGold()); // 进场同步
        }
    }

    void OnDisable()
    {
        if (GameManager.I != null)
            GameManager.I.OnGoldChanged -= UpdateCashText;
    }

    void UpdateCashText(int cash)
    {
        cashText.text = $"{prefix}{cash}";
    }
}
