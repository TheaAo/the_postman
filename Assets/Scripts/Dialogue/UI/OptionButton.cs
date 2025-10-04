using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialogue.Runtime {
    /// <summary>
    /// 选项按钮脚本：挂在你的选项预制体根物体上
    /// 预制体内需要 Button + TextMeshProUGUI
    /// </summary>
    public class OptionButton : MonoBehaviour {
        [SerializeField] private Button button;
        [SerializeField] private TextMeshProUGUI label;

        private int _index;
        private Action<int> _onClick;

        public void Setup(int index, string text, Action<int> onClick) {
            _index = index;
            _onClick = onClick;

            if (label) label.text = text ?? "";
            if (button) {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => _onClick?.Invoke(_index));
            }
        }
    }
}
