using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.Dialogue.Runtime {

    // Implement IRuntimeDialogueView based on UGUI's dialogue view (docked to DialogueRunner).
    // - ShowLine: Brush a line of text to the UI (optional typewriter);
    // - WaitForConfirm: wait for "continue" confirmation (button or clickable mask), or autoconfirm;
    // - ShowOptions: generate/reuse pool of option buttons and callback the selection to the outer layer;
    // - ClearOptions: hide all option buttons.

    public class UnityUIDialogueView : MonoBehaviour, IRuntimeDialogueView {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image portraitImage;
        [SerializeField] private GameObject panelRoot;  // the root of the entire Dialogue Panel

        [Header("Comfirmed (continued)")]
        [SerializeField] private Button continueButton;     // Tapping on it equals "Confirm."
        [SerializeField] private Button clickAnywhereMask;  // Can be empty; if supplied, clicking on the whole area also equals "confirm"

        [Header("Options")]
        [SerializeField] private Transform optionsRoot;     // Options Button Container
        [SerializeField] private OptionButton optionPrefab; // Option button prefabricated body (see class below)

        [Header("typewriter")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField, Range(1, 120)] private float charsPerSecond = 45f;

        [Header("Automation (for testing)")]
        public bool autoConfirm = false;                    // WaitForConfirm automatically passes if true.
        public bool autoChooseFirstOption = false;          // The first option is automatically selected when true
        [Range(0f, 2f)] public float autoDelay = 0.05f;

        // internal states 
        private readonly List<OptionButton> _pool = new();
        private Action<int> _onChoose;
        private bool _waitingConfirm;
        private Coroutine _typingCo;
        private string _currentFullText;
        private GameObject Root => panelRoot ? panelRoot : gameObject;

        // IRuntimeDialogueView 
        private void SetPanelVisible(bool v) {
            if (Root) Root.SetActive(v);
        }

        void OnEnable() {
            SetPanelVisible(false); // Initial concealment
            Game.Dialogue.Runtime.DialogueEvents.OnStarted += HandleStarted;
            Game.Dialogue.Runtime.DialogueEvents.OnEnded += HandleEnded;
        }
        void OnDisable() {
            Game.Dialogue.Runtime.DialogueEvents.OnStarted -= HandleStarted;
            Game.Dialogue.Runtime.DialogueEvents.OnEnded -= HandleEnded;
        }
        private void HandleStarted(string graphId) => SetPanelVisible(true);
        private void HandleEnded(string _) {
            StopTypingIfAny();
            SetContinueInteractable(false);
            ClearOptions();
            if (bodyText) bodyText.text = "";
            if (speakerText) speakerText.text = "";
            SetPanelVisible(false);                  
        }

        public void ShowLine(string speaker, string text) {
            SetPanelVisible(true);
            // One line at a time: clear options, stop typewriter, update avatar & name
            ClearOptions();
            StopTypingIfAny();

            if (speakerText) speakerText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;

            _currentFullText = text ?? "";                    // Cache whole sentences

            if (bodyText) bodyText.text = enableTypewriter ? "" : (text ?? "");
            // The sprite of portraitImage is up to the outer layer to decide whether to set it or not, so I won't change it here.

            if (enableTypewriter && !string.IsNullOrEmpty(_currentFullText))
                _typingCo = StartCoroutine(TypeRoutine(_currentFullText));    // Starting with cache

            // Show "Continue" interaction
            SetContinueInteractable(true);
        }

        public IEnumerator WaitForConfirm() {
            if (autoConfirm) {
                if (autoDelay > 0f) yield return new WaitForSeconds(autoDelay);
                yield break;
            }

            _waitingConfirm = true;
            bool confirmed = false;

            // Bind button
            Action bind = () => {
                if (continueButton) {
                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(() =>
                    {
                        // If still typing: the first click skips full; the second actually confirms it
                        if (_typingCo != null) { StopTypingIfAny(); return; }
                        confirmed = true;
                    });
                }
                if (clickAnywhereMask) {
                    clickAnywhereMask.onClick.RemoveAllListeners();
                    clickAnywhereMask.onClick.AddListener(() =>
                    {
                        // If still typing: the first click skips full; the second actually confirms it
                        if (_typingCo != null) { StopTypingIfAny(); return; }
                        confirmed = true;
                    });
                }
            };
            bind();

            // IMPORTANT: Somehow this doesn't work.......
            // Listening to the spacebar at the same time（Input System
            while (!confirmed) {
                // The keyboard may be null (e.g. controller-only), so judge the null
                var kb = Keyboard.current;
                if (kb != null && kb.eKey.wasPressedThisFrame) {
                    if (_typingCo != null) { StopTypingIfAny(); }
                    else { confirmed = true; }
                }
                yield return null;
            }


            _waitingConfirm = false;
            SetContinueInteractable(false);
        }

        public void ShowOptions(IReadOnlyList<string> options, Action<int> onChosen) {
            SetPanelVisible(true);

            _onChoose = onChosen;
            EnsureCount(options?.Count ?? 0);

            for (int i = 0; i < _pool.Count; i++) {
                bool active = options != null && i < options.Count;
                _pool[i].gameObject.SetActive(active);
                if (active)
                    _pool[i].Setup(i, options[i], HandleChoose);
            }

            // AutoSelect (for quick debugging, consistent with DebugConsole behaviour)
            if ((_onChoose != null) && autoChooseFirstOption && _pool.Count > 0)
                StartCoroutine(AutoPickFirst());
        }

        public void ClearOptions() {
            _onChoose = null;
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(false);
        }

        // Internal: Pushbutton Pool / Typewriter 

        private void HandleChoose(int index) {
            // When you tap the option, the index is immediately called back to the Runner
            _onChoose?.Invoke(index);
        }

        private void EnsureCount(int n) {
            // generating
            while (_pool.Count < n) {
                var btn = Instantiate(optionPrefab, optionsRoot);
                btn.gameObject.SetActive(false);
                _pool.Add(btn);
            }
            // Hide the excess.
            for (int i = n; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(false);
        }

        private IEnumerator TypeRoutine(string fullText) {
            bodyText.text = "";
            float t = 0f;
            int shown = 0;

            while (shown < fullText.Length) {
                t += Time.unscaledDeltaTime * charsPerSecond;
                int target = Mathf.Clamp(Mathf.FloorToInt(t), 0, fullText.Length);
                if (target != shown) {
                    shown = target;
                    bodyText.text = fullText.Substring(0, shown);
                }
                yield return null;
            }
            _typingCo = null;
        }

        private void StopTypingIfAny() {
            if (_typingCo != null) {
                StopCoroutine(_typingCo);
                _typingCo = null;
                if (bodyText) bodyText.text = _currentFullText ?? "";  // Immediate replenishment
            }
        }

        private void SetContinueInteractable(bool v) {
            if (continueButton) continueButton.gameObject.SetActive(v);
            if (clickAnywhereMask) clickAnywhereMask.gameObject.SetActive(v);
        }

        private IEnumerator AutoPickFirst() {
            if (autoDelay > 0f) yield return new WaitForSeconds(autoDelay);
            _onChoose?.Invoke(0);
        }
    }
}
