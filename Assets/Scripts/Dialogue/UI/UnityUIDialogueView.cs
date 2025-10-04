using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialogue.Runtime {
    /// <summary>
    /// 基于 UGUI 的对话视图，实现 IRuntimeDialogueView（对接 DialogueRunner）。
    /// - ShowLine：把一行文字刷到 UI（可选打字机）；
    /// - WaitForConfirm：等待“继续”确认（按钮或点击遮罩），也可自动确认；
    /// - ShowOptions：生成/复用选项按钮池，并把选择回调给外层；
    /// - ClearOptions：隐藏所有选项按钮。
    /// </summary>
    public class UnityUIDialogueView : MonoBehaviour, IRuntimeDialogueView {
        [Header("UI 引用")]
        [SerializeField] private TextMeshProUGUI speakerText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private Image portraitImage;

        [Header("确认（继续）")]
        [SerializeField] private Button continueButton;     // 点它等于“确认”
        [SerializeField] private Button clickAnywhereMask;  // 可空；若提供，则点击整块区域也等于“确认”

        [Header("选项")]
        [SerializeField] private Transform optionsRoot;     // 选项按钮容器
        [SerializeField] private OptionButton optionPrefab; // 选项按钮预制体（见下方类）

        [Header("打字机（可选）")]
        [SerializeField] private bool enableTypewriter = true;
        [SerializeField, Range(1, 120)] private float charsPerSecond = 45f;

        [Header("自动化（与 DebugConsole 行为保持一致）")]
        public bool autoConfirm = false;                    // true 时 WaitForConfirm 自动通过
        public bool autoChooseFirstOption = false;          // true 时自动选择第一个选项
        [Range(0f, 2f)] public float autoDelay = 0.05f;

        // --- 内部状态 ---
        private readonly List<OptionButton> _pool = new();
        private Action<int> _onChoose;
        private bool _waitingConfirm;
        private Coroutine _typingCo;

        // ================= IRuntimeDialogueView =================

        public void ShowLine(string speaker, string text) {
            // 每次显示一行：清掉选项、停止打字机、更新头像&名字
            ClearOptions();
            StopTypingIfAny();

            if (speakerText) speakerText.text = string.IsNullOrEmpty(speaker) ? "" : speaker;
            if (bodyText) bodyText.text = enableTypewriter ? "" : (text ?? "");
            // portraitImage 的 sprite 由外层自己决定是否设置，这里不改

            if (enableTypewriter && !string.IsNullOrEmpty(text))
                _typingCo = StartCoroutine(TypeRoutine(text));

            // 显示“继续”交互
            SetContinueInteractable(true);
        }

        public IEnumerator WaitForConfirm() {
            if (autoConfirm) {
                if (autoDelay > 0f) yield return new WaitForSeconds(autoDelay);
                yield break;
            }

            _waitingConfirm = true;
            bool confirmed = false;

            // 绑定按钮
            Action bind = () => {
                if (continueButton) {
                    continueButton.onClick.RemoveAllListeners();
                    continueButton.onClick.AddListener(() => confirmed = true);
                }
                if (clickAnywhereMask) {
                    clickAnywhereMask.onClick.RemoveAllListeners();
                    clickAnywhereMask.onClick.AddListener(() => confirmed = true);
                }
            };
            bind();

            // 等待
            while (!confirmed) yield return null;

            _waitingConfirm = false;
            SetContinueInteractable(false);
        }

        public void ShowOptions(IReadOnlyList<string> options, Action<int> onChosen) {
            _onChoose = onChosen;
            EnsureCount(options?.Count ?? 0);

            for (int i = 0; i < _pool.Count; i++) {
                bool active = options != null && i < options.Count;
                _pool[i].gameObject.SetActive(active);
                if (active)
                    _pool[i].Setup(i, options[i], HandleChoose);
            }

            // 自动选择（用于快速联调，和 DebugConsole 行为一致）
            if ((_onChoose != null) && autoChooseFirstOption && _pool.Count > 0)
                StartCoroutine(AutoPickFirst());
        }

        public void ClearOptions() {
            _onChoose = null;
            for (int i = 0; i < _pool.Count; i++)
                _pool[i].gameObject.SetActive(false);
        }

        // ================= 内部：按钮池 / 打字机 =================

        private void HandleChoose(int index) {
            // 点选项时，立即把索引回调给 Runner
            _onChoose?.Invoke(index);
        }

        private void EnsureCount(int n) {
            // 生成
            while (_pool.Count < n) {
                var btn = Instantiate(optionPrefab, optionsRoot);
                btn.gameObject.SetActive(false);
                _pool.Add(btn);
            }
            // 隐掉多余的
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
