using UnityEngine;

public class InfiniteBG2D : MonoBehaviour
{
    public SpriteRenderer[] tiles; // 三块背景
    public Camera cam;
    float width;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (tiles == null || tiles.Length < 3)
        {
            Debug.LogError("请拖入3个SpriteRenderer（左中右）");
            enabled = false;
            return;
        }

        // 自动计算每块宽度
        width = tiles[0].bounds.size.x;

        // 排序，确保从左到右
        System.Array.Sort(tiles, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // 摆好间距
        tiles[1].transform.position = new Vector3(cam.transform.position.x, tiles[1].transform.position.y, tiles[1].transform.position.z);
        tiles[0].transform.position = tiles[1].transform.position + Vector3.left * width;
        tiles[2].transform.position = tiles[1].transform.position + Vector3.right * width;
    }

    void LateUpdate()
    {
        float camX = cam.transform.position.x;

        foreach (var t in tiles)
        {
            // 相机往右走
            if (camX - t.transform.position.x > width * 1.5f)
                t.transform.position += Vector3.right * width * tiles.Length;

            // 相机往左走
            else if (t.transform.position.x - camX > width * 1.5f)
                t.transform.position -= Vector3.right * width * tiles.Length;
        }
    }
}
