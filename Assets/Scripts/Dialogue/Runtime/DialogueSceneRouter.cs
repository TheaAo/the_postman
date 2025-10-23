
using UnityEngine;
using Game.Dialogue.Runtime; // for DialogueEvents


/// <summary>
/// Listens for dialogue end, then routes to a target scene if a Route.* flag is present.
/// Keep routing out of the DialogueRunner to maintain separation of concerns.
/// </summary>
public sealed class DialogueSceneRouter : MonoBehaviour
{
    [SerializeField] private string ramenSceneName = "RamenShop_Scene";
    [SerializeField] private string guardSceneName = "BM_GuardGate_Scene";
    [SerializeField] private string policeSceneName = "GameScene2";
    [SerializeField] private string loadingMessage = "";

    public static DialogueSceneRouter Instance { get; private set; }

    private void Awake() {
        if (Instance != null && Instance != this) { Destroy(this.gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    private void OnEnable()
    {
        DialogueEvents.OnEnded += HandleDialogueEnded;
    }

    private void OnDisable()
    {
        DialogueEvents.OnEnded -= HandleDialogueEnded;
    }

    private void HandleDialogueEnded(string graphId)
    {
        if (GlobalFlagStore.I == null) return;
        if (SceneTransitionManager.Instance != null) {
            if (GlobalFlagStore.I.Consume("Route.GoRamen")) {
                SceneTransitionManager.Instance.LoadScene(ramenSceneName, loadingMessage);
                return;
            }
            if (GlobalFlagStore.I.Consume("Route.GoGuard")) {
                SceneTransitionManager.Instance.LoadScene(guardSceneName, loadingMessage);
                return;
            }
            if (GlobalFlagStore.I.Consume("Route.GoPolice")) {
                SceneTransitionManager.Instance.LoadScene(policeSceneName, loadingMessage);
                return;
            }
        }
        if (PuzzleManager.Instance != null) {
            if (GlobalFlagStore.I.Consume("StartPuzzle")) {
                PuzzleManager.Instance.StartPuzzle();
                Debug.Log("About to start the puzzle");
                return;
            }
        }
            
    }
}
