using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PuzzleManager : MonoBehaviour
{
    public static PuzzleManager Instance { get; private set; }

    [Header("Setup")]
    public GameObject puzzlePanel;   // refer to PuzzleBoard (including GridLayoutGroup)
    public GameObject tilePrefab;      // refer to tile prefab (UI Button + Image + PuzzleTile)
    public Sprite sourceImage;         // Picture of a square to be cut (Sprite)
    public int gridSize = 3;           // 3 -> 3x3
    public int shuffleMoves = 100;     // Randomly disrupted steps (using legal moves to ensure solvability)

    [Header("Layout")]
    public GridLayoutGroup gridLayout; // Points to the GridLayoutGroup on the PuzzleBoard.

    // Operational data
    private List<PuzzleTile> tiles = new List<PuzzleTile>(); // 8 个 tile
    private int emptyIndex; // Index of spaces（0..gridSize*gridSize-1）
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
        // Empty existing child objects (if run repeatedly)
        tiles.Clear();
        for (int i = gridLayout.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(gridLayout.transform.GetChild(i).gameObject);
        }
    }

    // Create a puzzle (in sibling order of rows top to bottom, left to right)
    void CreatePuzzle()
    {
        ClearPuzzle();

        // The texture of the source texture (note the y-direction).
        Texture2D tex = sourceImage.texture;
        // If the sourceImage is a subrect of a Sprite (e.g. atlas), use sourceImage.rect
        int fullWidth = Mathf.RoundToInt(sourceImage.rect.width);
        int fullHeight = Mathf.RoundToInt(sourceImage.rect.height);
        float pixelsPerUnit = sourceImage.pixelsPerUnit;

        int pieceW = fullWidth / gridSize;
        int pieceH = fullHeight / gridSize;

        int total = gridSize * gridSize;
        emptyIndex = total - 1; // The last position is empty (bottom right)

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

    // Check for proximity to spaces (top, bottom, left, right)
    bool IsAdjacent(int a, int b)
    {
        int ax = a % gridSize, ay = a / gridSize;
        int bx = b % gridSize, by = b / gridSize;
        return (Mathf.Abs(ax - bx) + Mathf.Abs(ay - by)) == 1;
    }

    // Try to move the tile (called when the tile is clicked)
    public bool TryMoveTile(PuzzleTile tile)
    {
        if (!IsAdjacent(tile.currentIndex, emptyIndex))
            return false;

        Transform parent = gridLayout.transform;
        Transform emptySlot = parent.GetChild(emptyIndex); // Find the space object

        int tileOldIndex = tile.currentIndex;

        // Swap hierarchical order (move tile to space position, space to old tile position)
        tile.transform.SetSiblingIndex(emptyIndex);
        emptySlot.SetSiblingIndex(tileOldIndex);

        // Update Logical Index
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
