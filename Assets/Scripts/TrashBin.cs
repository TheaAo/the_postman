using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; 
#endif

public class TrashBin : MonoBehaviour
{
     public GameObject exclamationMark;

    [Header("Three coin prefab to be generated")]
    public GameObject silverPrefab;     // Silver coins
    public GameObject goldPrefab;       // Gold coins
    public GameObject banknotePrefab;   // cash

    public enum SpawnMode { Fixed, Random, Sequential } // Fixed/random/sequential generation
    [Header("Generation rules")]
    public SpawnMode mode = SpawnMode.Random;
    public enum MoneyType { Silver, Gold, Banknote }
    public MoneyType fixedType = MoneyType.Gold; // Used when the mode is Fixed
    public int spawnCount = 1;                   // Quantity generated at one time

    [Header("Interactive settings")]
#if ENABLE_INPUT_SYSTEM
    public Key interactKey = Key.F;          
#else
    public KeyCode interactKey = KeyCode.F;  
#endif
    public bool triggerOnEnter = false;      // if it is automatically triggered by the player's proximity
    public float reuseCooldown = 1.0f;       // Cooldown time 

    [Header("Generate location and parameters")]
    public Transform spawnPoint;             // Generation location (can be empty)
    public float upOffset = 0.4f;            // Generate point uplift height
    public float popForce = 3.5f;            // upward initial velocity
    public float scatter = 1.2f;             // Horizontal Random Offset

    [Header("Appearance Switching")]
    public GameObject closedVisual;          // Trashcan Closure Appearance
    public GameObject openedVisual;          // Trashcan Open appearance
    public bool openOnlyOnce = true;         // Whether to open only once

    bool playerIn;       // Whether the player is in the trigger zone
    bool onCd;           // Is it on cooldown
    bool hasOpened;      // Is it open
    int seqIdx;          // Counting in Sequential Mode
    public GameObject Prompt_trashboxNotice;
    void Awake()
    {
        // Automatically find sub-objects (if no manual drag references)
        if (!closedVisual) closedVisual = transform.Find("ClosedVisual")?.gameObject;
        if (!openedVisual) openedVisual = transform.Find("OpenedVisual")?.gameObject;

        // Initial display: closed status
        if (closedVisual) closedVisual.SetActive(true);
        if (openedVisual) openedVisual.SetActive(false);

        if (Prompt_trashboxNotice != null) Prompt_trashboxNotice.SetActive(false);
    }

    // When player enters the trigger zone
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = true;
        if (triggerOnEnter) TrySpawn();

    }

    // When player leaves the trigger zone
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIn = false;
    }

    void Update()
    {
        if (Prompt_trashboxNotice != null) Prompt_trashboxNotice.SetActive(playerIn);
        // Triggered by pressing a key when the player is in the trigger zone and the cooldown is over
        if (triggerOnEnter || onCd || !playerIn) return;
        if (InteractPressed()) 
        {
            Destroy(Prompt_trashboxNotice);
            Prompt_trashboxNotice = null;
            if (exclamationMark != null)
                {
                    exclamationMark.SetActive(false);
                }
            TrySpawn();
        }
        }

    bool InteractPressed()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb == null) return false;
        var keyCtrl = kb[interactKey];
        return keyCtrl != null && keyCtrl.wasPressedThisFrame;
#else
        return Input.GetKeyDown(interactKey);
#endif
    }

    // Generate Coins + Switch Appearance
    void TrySpawn()
    {
        if (onCd) return;

        for (int i = 0; i < spawnCount; i++)
        {
            var prefab = PickPrefab(ChooseType());
            if (!prefab) continue;

            Vector3 pos = spawnPoint ? spawnPoint.position : GetTopPos();
            var go = Instantiate(prefab, pos, Quaternion.identity);
            go.transform.rotation = Quaternion.identity;

            // Set initial velocity 
            var rb2d = go.GetComponent<Rigidbody2D>();
            if (rb2d)
            {
                float hx = Random.Range(-1f, 1f) * scatter;
#if UNITY_2023_3_OR_NEWER
                rb2d.linearVelocity = new Vector2(hx, popForce);
#else
                rb2d.velocity = new Vector2(hx, popForce);
#endif
            }
        }

        OpenVisual();

        if (reuseCooldown < 0f || openOnlyOnce) { enabled = false; }
        else StartCoroutine(Cooldown());
    }

    // Switch Appearance
    void OpenVisual()
    {
        if (hasOpened && openOnlyOnce) return;
        hasOpened = true;

        if (closedVisual) closedVisual.SetActive(false);
        if (openedVisual) openedVisual.SetActive(true);
    }

    // Cooldown timer
    IEnumerator Cooldown()
    {
        onCd = true;
        yield return new WaitForSeconds(reuseCooldown);
        onCd = false;
    }

    // Decide which coins to generate
    MoneyType ChooseType()
    {
        switch (mode)
        {
            case SpawnMode.Fixed:
                return fixedType;
            case SpawnMode.Sequential:
                MoneyType[] order = { MoneyType.Silver, MoneyType.Gold, MoneyType.Banknote };
                var t = order[seqIdx % order.Length];
                seqIdx++;
                return t;
            default:
                return (MoneyType)Random.Range(0, 3);
        }
    }

    // Returns prefab bodies by type
    GameObject PickPrefab(MoneyType t)
    {
        switch (t)
        {
            case MoneyType.Silver: return silverPrefab;
            case MoneyType.Gold: return goldPrefab;
            case MoneyType.Banknote: return banknotePrefab;
            default: return null;
        }
    }

    // Automatically calculates the position above the dustbin
    Vector3 GetTopPos()
    {
        var col = GetComponent<Collider2D>();
        if (col)
        {
            var b = col.bounds;
            return new Vector3(b.center.x, b.max.y + upOffset, transform.position.z);
        }
        return transform.position + Vector3.up * upOffset;
    }
}
