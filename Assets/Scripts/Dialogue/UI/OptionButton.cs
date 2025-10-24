using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Dialogue.Runtime {
    // Option Button Script: hooked to the root object of your option prefab.
    // Button + TextMeshProUGUI are needed inside the prefab.
    // 
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
