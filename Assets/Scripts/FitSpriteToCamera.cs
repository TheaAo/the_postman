using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FitSpriteToCamera : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField, Range(1f, 2f)] private float fillFactor = 1.15f; // A little bigger than the screen
    [SerializeField] private bool keepAspect = true;

    void Awake()
    {
        if (!cam) cam = Camera.main;

        var sr = GetComponent<SpriteRenderer>();
        if (!sr || !sr.sprite) return;

        // Current world size (with scale)
        var size = sr.bounds.size;

        float worldH = cam.orthographicSize * 2f;
        float worldW = worldH * cam.aspect;

        float sx = worldW / size.x;
        float sy = worldH / size.y;

        if (keepAspect)
        {
            float s = Mathf.Max(sx, sy) * fillFactor; // Coverage and slightly larger
            transform.localScale *= s;
        }
        else
        {
            var ls = transform.localScale;
            transform.localScale = new Vector3(ls.x * sx * fillFactor, ls.y * sy * fillFactor, ls.z);
        }

        // Background centre aligned with camera centre (initial)
        transform.position = new Vector3(cam.transform.position.x, cam.transform.position.y, transform.position.z);
    }
}

