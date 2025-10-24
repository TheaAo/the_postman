using UnityEngine;

public class InfiniteBG2D : MonoBehaviour
{
    public SpriteRenderer[] tiles; // Three backgrounds
    public Camera cam;
    float width;

    void Start()
    {
        if (cam == null) cam = Camera.main;
        if (tiles == null || tiles.Length < 3)
        {
            Debug.LogError("Please drag in 3 SpriteRenderers (centre left and right)");
            enabled = false;
            return;
        }

        // Automatic calculation of the width of each block
        width = tiles[0].bounds.size.x;

        // Sort, make sure it's left to right
        System.Array.Sort(tiles, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

        // Spacing.
        tiles[1].transform.position = new Vector3(cam.transform.position.x, tiles[1].transform.position.y, tiles[1].transform.position.z);
        tiles[0].transform.position = tiles[1].transform.position + Vector3.left * width;
        tiles[2].transform.position = tiles[1].transform.position + Vector3.right * width;
    }

    void LateUpdate()
    {
        float camX = cam.transform.position.x;

        foreach (var t in tiles)
        {
            // Camera to the right.
            if (camX - t.transform.position.x > width * 1.5f)
                t.transform.position += Vector3.right * width * tiles.Length;

            // Camera to the left.
            else if (t.transform.position.x - camX > width * 1.5f)
                t.transform.position -= Vector3.right * width * tiles.Length;
        }
    }
}
