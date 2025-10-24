using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialogue.UI {

    public interface IDialogueView {
        void RenderLine(DialogueLineVM line, Action onCompleted);
        void ShowOptions(IReadOnlyList<string> options, Action<int> onChoose);
        void Clear();
    }

    // Lightweight VM (display only) 
    [Serializable]
    public class DialogueLineVM {
        public string speaker;
        public string text;

   
        public Sprite portrait;
    }

    // Option button script
    public class OptionButton : MonoBehaviour {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        private int _index;
        private Action<int> _onClick;

        public void Setup(int index, string text, Action<int> onClick) {
            _index = index;
            _onClick = onClick;
            if (label) label.text = text;

            // Prevent multiple overlapping listens
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClick?.Invoke(_index));
        }

        public void SetVisible(bool v) => gameObject.SetActive(v);
    }

    //View implement
    public class DialogueView : MonoBehaviour, IDialogueView {
        [Header("UI Refs")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image portrait;

        [Header("Options")]
        [SerializeField] private Transform optionsRoot;
        [SerializeField] private OptionButton optionPrefab;

        [Header("Typewriter (¿ÉÑ¡)")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField, Range(0.5f, 60f)] private float charsPerSecond = 30f;


        private readonly List<OptionButton> _pool = new();
        private Coroutine _typingCo;
        private bool _isTyping;
        private Action _lineCompletedCallback;

        // IDialogueView
        public void RenderLine(DialogueLineVM line, Action onCompleted) {
            StopTypingIfAny();
            HideAllOptions();

            _lineCompletedCallback = onCompleted;

            if (nameText) nameText.text = line?.speaker ?? string.Empty;
            if (portrait) portrait.sprite = line?.portrait;

            if (!enableTypewriter || string.IsNullOrEmpty(line?.text)) {
                if (bodyText) bodyText.text = line?.text ?? string.Empty;
                // completion
                _lineCompletedCallback?.Invoke();
                _lineCompletedCallback = null;
                return;
            }

            // typewriters
            _typingCo = StartCoroutine(TypeRoutine(line.text));
        }

        public void ShowOptions(IReadOnlyList<string> options, Action<int> onChoose) {
            EnsureCount(options.Count);
            for (int i = 0; i < _pool.Count; i++) {
                bool active = i < options.Count;
                _pool[i].SetVisible(active);
                if (active) {
                    _pool[i].Setup(i, options[i], idx => onChoose?.Invoke(idx));
                }
            }
        }

        public void Clear() {
            StopTypingIfAny();
            if (nameText) nameText.text = string.Empty;
            if (bodyText) bodyText.text = string.Empty;
            if (portrait) portrait.sprite = null;
            HideAllOptions();
        }

        // typewriters
        private IEnumerator TypeRoutine(string fullText) {
            _isTyping = true;
            bodyText.text = string.Empty;

            // Add characters per frame
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

            _isTyping = false;
            _typingCo = null;

            _lineCompletedCallback?.Invoke();
            _lineCompletedCallback = null;
        }

        private void StopTypingIfAny() {
            if (_typingCo != null) {
                StopCoroutine(_typingCo);
                _typingCo = null;
            }
            _isTyping = false;
        }

        // For external or input system calls: skips while typing and instantly displays in full
        public void SkipTypeIfTyping() {
            if (!_isTyping) return;
            StopTypingIfAny();
            // Supplementary text
            // Note: bodyText.text is already a partial of the previous sentence; for simplicity, trigger the completion callback directly.
            _lineCompletedCallback?.Invoke();
            _lineCompletedCallback = null;
        }

        // Option Button Pool
        private void EnsureCount(int n) {
            // Generate or recycle
            while (_pool.Count < n) {
                var btn = Instantiate(optionPrefab, optionsRoot);
                btn.SetVisible(false);
                _pool.Add(btn);
            }
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].SetVisible(i < n);
        }

        private void HideAllOptions() {
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].SetVisible(false);
        }
    }
}
