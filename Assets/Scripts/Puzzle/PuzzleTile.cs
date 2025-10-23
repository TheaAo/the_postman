using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PuzzleTile : MonoBehaviour
{
    public int originalIndex;    // 这块在初始正确位置的索引（0..gridSize*gridSize-2）
    [HideInInspector] public int currentIndex; // 当前在 board 中的位置索引
    private PuzzleManager manager;
    private Image image;
    private Button button;

    // 初始化：manager、原始索引、sprite
    public void Init(PuzzleManager mgr, int origIndex, Sprite sprite)
    {
        manager = mgr;
        originalIndex = origIndex;
        currentIndex = origIndex;
        image = GetComponent<Image>();
        button = GetComponent<Button>();

        image.sprite = sprite;
        image.preserveAspect = true;

        // 绑定点击（动态绑定可避免在 Inspector 手动绑定）
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);
    }

    private void OnClicked()
    {
        // 请求 manager 移动这块
        manager.TryMoveTile(this);
    }

    // 判断是否回到原始位置（胜利判定）
    public bool IsAtOrigin()
    {
        return currentIndex == originalIndex;
    }
}
