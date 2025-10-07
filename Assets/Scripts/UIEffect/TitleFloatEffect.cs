using UnityEngine;
using UnityEngine.UI;

public class TitleFloatEffect : MonoBehaviour
{
    public float floatSpeed = 2f;         // 上下浮动的速度
    public float floatAmplitude = 20f;    // 上下浮动的幅度
    public float alphaSpeed = 2f;         // Alpha 闪烁速度
    public float alphaMin = 0.6f;         // 最低透明度
    public float alphaMax = 1f;           // 最高透明度

    private RectTransform rectTransform;
    private Vector2 startPos;
    private Image image;
    private float time;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        startPos = rectTransform.anchoredPosition;

        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogWarning("No Image component found on Logo.");
        }
    }

    void Update()
    {
        time += Time.deltaTime;

        // 漂浮效果（上下浮动）
        float yOffset = Mathf.Sin(time * floatSpeed) * floatAmplitude;
        rectTransform.anchoredPosition = startPos + new Vector2(0, yOffset);

        // // Alpha 闪烁
        // if (image != null)
        // {
        //     float alpha = Mathf.Lerp(alphaMin, alphaMax, (Mathf.Sin(time * alphaSpeed) + 1f) / 2f);
        //     Color c = image.color;
        //     c.a = alpha;
        //     image.color = c;
        // }
    }
}
