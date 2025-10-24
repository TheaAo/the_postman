using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PuzzleTile : MonoBehaviour
{
    public int originalIndex;    // Index of this block at the initial correct position (0..gridSize*gridSize-2)
    [HideInInspector] public int currentIndex; // Current position index in the board
    private PuzzleManager manager;
    private Image image;
    private Button button;

    // initialisation：manager、Raw Index、sprite
    public void Init(PuzzleManager mgr, int origIndex, Sprite sprite)
    {
        manager = mgr;
        originalIndex = origIndex;
        currentIndex = origIndex;
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        image.sprite = sprite;
        image.preserveAspect = true;

        // Bind on click (dynamic binding avoids manual binding in Inspector)
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        // Request manager to move this piece
        manager.TryMoveTile(this);
    }

    // Determining whether to return to the original position (victory decision)
    public bool IsAtOrigin()
    {
        return currentIndex == originalIndex;
    }
}
