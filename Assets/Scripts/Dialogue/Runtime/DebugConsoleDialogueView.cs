using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem; // 新输入系统

namespace Game.Dialogue.Runtime {
    /// <summary>
    /// 仅使用新输入系统的 Console 调试视图：
    /// Space 确认；数字键 1..9（含小键盘）选择；可自动确认/自动选第一项。
    /// </summary>
    public class DebugConsoleDialogueView : MonoBehaviour, IRuntimeDialogueView {
        [Header("行为设置")]
        [Tooltip("是否自动确认下一句（无需按 Space）")]
        public bool autoConfirm = true;

        [Tooltip("是否自动选择第一个选项（无需按数字键）")]
        public bool autoChooseFirstOption = true;

        [Tooltip("自动确认/自动选择的延迟秒数")]
        [Range(0f, 2f)] public float autoDelay = 0.05f;

        private Action<int> _onChosen;
        private int _optionCount;

        public void ShowLine(string speaker, string text) {
            if (!string.IsNullOrEmpty(speaker))
                Debug.Log($"[{speaker}] {text}");
            else
                Debug.Log(text);
        }

        public IEnumerator WaitForConfirm() {
            if (autoConfirm) {
                if (autoDelay > 0f) yield return new WaitForSeconds(autoDelay);
                yield break;
            }

            while (true) {
                if (Keyboard.current != null &&
                    Keyboard.current.spaceKey.wasPressedThisFrame)
                    yield break;

                yield return null;
            }
        }

        public void ShowOptions(IReadOnlyList<string> options, Action<int> onChosen) {
            _onChosen = onChosen;
            _optionCount = options?.Count ?? 0;

            if (_optionCount == 0) {
                Debug.Log("<options: none>");
                _onChosen?.Invoke(0);
                return;
            }

            for (int i = 0; i < options.Count; i++)
                Debug.Log($"  [{i + 1}] {options[i]}");

            if (autoChooseFirstOption)
                StartCoroutine(AutoPickFirst());
            else
                StartCoroutine(WaitNumberKey());
        }

        IEnumerator AutoPickFirst() {
            if (autoDelay > 0f) yield return new WaitForSeconds(autoDelay);
            _onChosen?.Invoke(0);
        }

        IEnumerator WaitNumberKey() {
            while (true) {
                if (Keyboard.current != null && TryGetNumberPressed(Keyboard.current, out var idx)) {
                    _onChosen?.Invoke(idx);
                    yield break;
                }
                yield return null;
            }
        }

        public void ClearOptions() {
            _onChosen = null;
            _optionCount = 0;
        }

        // -------- 输入工具（新输入系统） --------
        bool TryGetNumberPressed(Keyboard kb, out int index) {
            index = -1;
            // 顶部数字 1..9
            if (kb.digit1Key.wasPressedThisFrame) index = 0;
            else if (kb.digit2Key.wasPressedThisFrame) index = 1;
            else if (kb.digit3Key.wasPressedThisFrame) index = 2;
            else if (kb.digit4Key.wasPressedThisFrame) index = 3;
            else if (kb.digit5Key.wasPressedThisFrame) index = 4;
            else if (kb.digit6Key.wasPressedThisFrame) index = 5;
            else if (kb.digit7Key.wasPressedThisFrame) index = 6;
            else if (kb.digit8Key.wasPressedThisFrame) index = 7;
            else if (kb.digit9Key.wasPressedThisFrame) index = 8;

            // 小键盘 1..9（如果上面没命中）
            if (index < 0) {
                if (kb.numpad1Key.wasPressedThisFrame) index = 0;
                else if (kb.numpad2Key.wasPressedThisFrame) index = 1;
                else if (kb.numpad3Key.wasPressedThisFrame) index = 2;
                else if (kb.numpad4Key.wasPressedThisFrame) index = 3;
                else if (kb.numpad5Key.wasPressedThisFrame) index = 4;
                else if (kb.numpad6Key.wasPressedThisFrame) index = 5;
                else if (kb.numpad7Key.wasPressedThisFrame) index = 6;
                else if (kb.numpad8Key.wasPressedThisFrame) index = 7;
                else if (kb.numpad9Key.wasPressedThisFrame) index = 8;
            }

            if (index >= 0 && index < _optionCount) return true;
            index = -1;
            return false;
        }
    }
}
