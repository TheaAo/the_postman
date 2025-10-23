using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Setup")]
    public GameObject puzzlePanel;   // 指向 PuzzleBoard (含 GridLayoutGroup)
    public GameObject tilePrefab;      // 指向 tile prefab (UI Button + Image + PuzzleTile)
    public Sprite sourceImage;         // 要切分的正方形图片 (Sprite)
    public int gridSize = 3;           // 3 -> 3x3
    public int shuffleMoves = 100;     // 随机打乱的步数（使用合法移动保证可解）

    [Header("Layout")]
    public GridLayoutGroup gridLayout; // 指向 PuzzleBoard 上的 GridLayoutGroup

    // 运行数据
    private List<PuzzleTile> tiles = new List<PuzzleTile>(); // 8 个 tile
    private int emptyIndex; // 空格的索引（0..gridSize*gridSize-1）
    private bool isShuffling = false;

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
    }

    void Start()
  {
    if (gridLayout == null) Debug.LogError("GridLayout not assigned!");
    puzzlePanel.SetActive(false);
  }
    
    void ClearPuzzle()
    {
        // 清空已有子对象（如果重复运行）
        tiles.Clear();
        for (int i = gridLayout.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridLayout.transform.GetChild(i).gameObject);
        }
    }

    // 创建拼图（按行上到下、左到右的 sibling 顺序）
    void CreatePuzzle()
    {
        ClearPuzzle();

        // 源贴图的 texture（注意 y 方向）
        Texture2D tex = sourceImage.texture;
        // 如果 sourceImage 是一个 Sprite 的 subrect（例如 atlas），则需用 sourceImage.rect
        int fullWidth = Mathf.RoundToInt(sourceImage.rect.width);
        int fullHeight = Mathf.RoundToInt(sourceImage.rect.height);
        float pixelsPerUnit = sourceImage.pixelsPerUnit;

        int pieceW = fullWidth / gridSize;
        int pieceH = fullHeight / gridSize;

        int total = gridSize * gridSize;
        emptyIndex = total - 1; // 最后一个位置为空（右下角）

        // IMPORTANT: GridLayoutGroup Start Corner = Upper Left, but Texture y=0 是底部
        // We'll create sprites row by row top->bottom so compute y offset accordingly.
        for (int idx = 0; idx < total; idx++)
        {
            int x = idx % gridSize;
            int yRow = idx / gridSize; // 0..gridSize-1, top row = 0
            if (idx == emptyIndex)
            {
                // create an empty placeholder (optional), but we simply skip creating a tile
                // To keep sibling indices consistent, we will add an empty GameObject
                GameObject emptyGO = new GameObject("EmptySlot", typeof(RectTransform));
                emptyGO.transform.SetParent(gridLayout.transform, false);
                // Optionally add LayoutElement to occupy space (GridLayout will size)
                continue;
            }

            // compute rect in texture space: for top row yRow=0 => y = fullHeight - pieceH
            int yTex = fullHeight - (yRow + 1) * pieceH;
            int xTex = x * pieceW;

            Rect rect = new Rect(sourceImage.rect.x + xTex, sourceImage.rect.y + yTex, pieceW, pieceH);
            // create sprite
            Sprite piece = Sprite.Create(tex, rect, new Vector2(0.5f, 0.5f), pixelsPerUnit);

            // instantiate tile prefab as child of gridLayout
            GameObject go = Instantiate(tilePrefab, gridLayout.transform, false);
            go.name = "Tile_" + idx;

            PuzzleTile tile = go.GetComponent<PuzzleTile>();
            tile.Init(this, idx, piece);
            tile.currentIndex = idx;
            tiles.Add(tile);
        }
    }

    // 检查是否与空格相邻（上下左右）
    bool IsAdjacent(int a, int b)
    {
        int ax = a % gridSize, ay = a / gridSize;
        int bx = b % gridSize, by = b / gridSize;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by)) == 1;
    }

    // 尝试移动 tile（当点击 tile 时调用）
    public bool TryMoveTile(PuzzleTile tile)
    {
        if (!IsAdjacent(tile.currentIndex, emptyIndex))
            return false;

        Transform parent = gridLayout.transform;
        Transform emptySlot = parent.GetChild(emptyIndex); // 找到空格对象

        int tileOldIndex = tile.currentIndex;

        // 交换层级顺序（移动 tile 到空格位置，空格到 tile 旧位置）
        tile.transform.SetSiblingIndex(emptyIndex);
        emptySlot.SetSiblingIndex(tileOldIndex);

        // 更新逻辑索引
        tile.currentIndex = emptyIndex;
        emptyIndex = tileOldIndex;

        if(!isShuffling) CheckWin();
        return true;
    }


    // Shuffle by doing legal random moves from solved state
    void ShufflePuzzle()
  {
        System.Random rng = new System.Random();
        isShuffling = true;
        for (int i = 0; i < shuffleMoves; i++)
        {
            // gather movable tiles
            List<PuzzleTile> movables = tiles.FindAll(t => IsAdjacent(t.currentIndex, emptyIndex));
            if (movables.Count == 0) continue;
            PuzzleTile sel = movables[rng.Next(movables.Count)];
            // move chosen tile; this will update emptyIndex
            TryMoveTile(sel);
        }
        isShuffling = false;
    }

  // Check win: all tiles at their original indices
  void CheckWin()
  {
    foreach (var t in tiles)
    {
      if (!t.IsAtOrigin()) return;
    }
    Debug.Log("You win!");
    if (SceneTransitionManager.Instance != null) SceneTransitionManager.Instance.LoadScene("LetterScene", "");
  }

    public void StartPuzzle()
    {
        Debug.Log("Start Puzzle pressed");
        CreatePuzzle();
        ShufflePuzzle();
        puzzlePanel.SetActive(true);
    }
    public void ExitPuzzle()
    {
        Debug.Log("Exit pressed");
        ClearPuzzle();
        puzzlePanel.SetActive(false);
    }
}
