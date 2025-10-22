
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Cross-scene flag store. One source of truth for narrative flags.
/// </summary>
public sealed class GlobalFlagStore : MonoBehaviour
{
    public static GlobalFlagStore I { get; private set; }

    private readonly HashSet<string> _flags = new HashSet<string>(System.StringComparer.Ordinal);

    private void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool Has(string key) => !string.IsNullOrEmpty(key) && _flags.Contains(key);

    public void Set(string key)
    {
        if (string.IsNullOrEmpty(key)) return;
        _flags.Add(key);
    }

    /// <summary>Check and remove the key in one step. Returns true if removed.</summary>
    public bool Consume(string key)
    {
        if (string.IsNullOrEmpty(key)) return false;
        return _flags.Remove(key);
    }

    public void ClearAll() => _flags.Clear();
}
