using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialogue.UI {
    // =============== 接口（照你的图） ===============
    public interface IDialogueView {
        void RenderLine(DialogueLineVM line, Action onCompleted);
        void ShowOptions(IReadOnlyList<string> options, Action<int> onChoose);
        void Clear();
    }

    // =============== 轻量 VM（只做显示） ===============
    [Serializable]
    public class DialogueLineVM {
        public string speaker;
        public string text;

        // 可空；如果你只有 portraitKey，在外层把 key -> Sprite 转好再塞进来即可
        public Sprite portrait;
    }

    // =============== 选项按钮脚本（挂到按钮预制体上） ===============
    public class OptionButton : MonoBehaviour {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        private int _index;
        private Action<int> _onClick;

        public void Setup(int index, string text, Action<int> onClick) {
            _index = index;
            _onClick = onClick;
            if (label) label.text = text;

            // 防止多次叠加监听
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => _onClick?.Invoke(_index));
        }

        public void SetVisible(bool v) => gameObject.SetActive(v);
    }

    // =============== View 实现（MonoBehaviour） ===============
    public class DialogueView : MonoBehaviour, IDialogueView {
        [Header("UI Refs")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image portrait;

        [Header("Options")]
        [SerializeField] private Transform optionsRoot;
        [SerializeField] private OptionButton optionPrefab;

        [Header("Typewriter (可选)")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField, Range(0.5f, 60f)] private float charsPerSecond = 30f;

        // —— 内部态 —— //
        private readonly List<OptionButton> _pool = new();
        private Coroutine _typingCo;
        private bool _isTyping;
        private Action _lineCompletedCallback;

        // ========== IDialogueView ==========
        public void RenderLine(DialogueLineVM line, Action onCompleted) {
            StopTypingIfAny();
            HideAllOptions();

            _lineCompletedCallback = onCompleted;

            if (nameText) nameText.text = line?.speaker ?? string.Empty;
            if (portrait) portrait.sprite = line?.portrait;

            if (!enableTypewriter || string.IsNullOrEmpty(line?.text)) {
                if (bodyText) bodyText.text = line?.text ?? string.Empty;
                // 直接完成
                _lineCompletedCallback?.Invoke();
                _lineCompletedCallback = null;
                return;
            }

            // 打字机
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

        // ========== 打字机（可选） ==========
        private IEnumerator TypeRoutine(string fullText) {
            _isTyping = true;
            bodyText.text = string.Empty;

            // 每帧增加字符
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

        // 供外部或输入系统调用：正在打字时跳过并瞬间显示完整
        public void SkipTypeIfTyping() {
            if (!_isTyping) return;
            StopTypingIfAny();
            // 补全文本
            // 注意：bodyText.text 此时已经是上一句的局部；简单起见直接触发完成回调
            _lineCompletedCallback?.Invoke();
            _lineCompletedCallback = null;
        }

        // ========== 选项按钮池 ==========
        private void EnsureCount(int n) {
            // 生成或回收
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
