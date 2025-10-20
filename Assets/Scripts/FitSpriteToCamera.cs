using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField, Range(1f, 2f)] private float fillFactor = 1.15f; // 比屏幕大一些
    [SerializeField] private bool keepAspect = true;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        var sr = GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite) return;

        // 当前世界尺寸（含scale）
        var size = sr.bounds.size;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        float sx = worldW / size.x;
        float sy = worldH / size.y;

        if (keepAspect)
        {
            float s = Mathf.Max(sx, sy) * fillFactor; // 覆盖并稍大
            transform.localScale *= s;
        }
        else
        {
            var ls = transform.localScale;
            transform.localScale = new Vector3(ls.x * sx * fillFactor, ls.y * sy * fillFactor, ls.z);
        }

        // 背景中心跟相机中心对齐（初始）
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }
}

