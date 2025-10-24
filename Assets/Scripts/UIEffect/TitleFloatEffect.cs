using UnityEngine;
using UnityEngine.UI;

public class TitleFloatEffect : MonoBehaviour
{
    public float floatSpeed = 2f;         // Speed of float up and down
    public float floatAmplitude = 20f;    // Up and down range
    public float alphaSpeed = 2f;         // Alpha flash rate
    public float alphaMin = 0.6f;         // Minimum transparency
    public float alphaMax = 1f;           // Maximum transparency

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

        // Floating effect (up and down)
        float yOffset = Mathf.Sin(time * floatSpeed) * floatAmplitude;
        rectTransform.anchoredPosition = startPos + new Vector2(0, yOffset);

        // // Alpha blinks
        // if (image != null)
        // {
        //     float alpha = Mathf.Lerp(alphaMin, alphaMax, (Mathf.Sin(time * alphaSpeed) + 1f) / 2f);
        //     Color c = image.color;
        //     c.a = alpha;
        //     image.color = c;
        // }
    }
}
