using UnityEngine;

public class ParallaxFollow2D : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [Range(0f, 1f)] public float parallax = 0.4f; // 0=不动, 1=跟相机一起动
    private Vector3 startPos;
    private Vector3 camStart;

    void Start()
    {
        if (!cam) cam = Camera.main;
        startPos = transform.position;
        camStart = cam.transform.position;
    }

    void LateUpdate()
    {
        var delta = cam.transform.position - camStart;
        transform.position = new Vector3(
            startPos.x + delta.x * parallax,
            startPos.y + delta.y * parallax,
            startPos.z
        );
    }
}

