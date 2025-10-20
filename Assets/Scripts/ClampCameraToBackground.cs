using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ClampCameraToBackground : MonoBehaviour
{
    [SerializeField] private SpriteRenderer background; // 拖你的背景 SpriteRenderer
    private Camera cam;

    void Awake() => cam = GetComponent<Camera>();

    void LateUpdate()
    {
        if (!background) return;

        var bounds = background.bounds;

        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;

        // 计算可移动范围（背景边界扣掉半个视口）
        float minX = bounds.min.x + halfW;
        float maxX = bounds.max.x - halfW;
        float minY = bounds.min.y + halfH;
        float maxY = bounds.max.y - halfH;

        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;
    }
}
