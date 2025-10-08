using UnityEngine;

public interface IDialogueUI {
    void ShowLine(string speaker, string text);
    void ShowOptions(System.Collections.Generic.IReadOnlyList<string> options);
    void HideOptions();
    System.Action<int> OnOptionSelected { get; set; }
}

