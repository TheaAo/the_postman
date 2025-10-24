// TrashBin.cs —— 2D版 + 新旧输入系统兼容 + 开盖生成钱币
using UnityEngine;
using System.Collections;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 新输入系统
#endif

public class TrashBin : MonoBehaviour
{
     public GameObject exclamationMark;

    [Header("要生成的三个钱币预制体")]
    public GameObject silverPrefab;     // 银币
    public GameObject goldPrefab;       // 金币
    public GameObject banknotePrefab;   // 纸币

    public enum SpawnMode { Fixed, Random, Sequential } // 固定/随机/顺序生成
    [Header("生成规则")]
    public SpawnMode mode = SpawnMode.Random;
    public enum MoneyType { Silver, Gold, Banknote }
    public MoneyType fixedType = MoneyType.Gold; // 当模式为Fixed时使用
    public int spawnCount = 1;                   // 一次生成数量

    [Header("交互设置")]
#if ENABLE_INPUT_SYSTEM
    public Key interactKey = Key.F;          // 新输入系统按键
#else
    public KeyCode interactKey = KeyCode.F;  // 旧输入系统按键
#endif
    public bool triggerOnEnter = false;      // 玩家靠近是否自动触发
    public float reuseCooldown = 1.0f;       // 冷却时间（<0为一次性）

    [Header("生成位置和弹出参数")]
    public Transform spawnPoint;             // 生成位置（可为空）
    public float upOffset = 0.4f;            // 生成点上移高度
    public float popForce = 3.5f;            // 向上初速度
    public float scatter = 1.2f;             // 水平随机偏移

    [Header("外观切换（方案A：两个子物体）")]
    public GameObject closedVisual;          // 垃圾桶关闭外观
    public GameObject openedVisual;          // 垃圾桶打开外观
    public bool openOnlyOnce = true;         // 是否只开一次

    bool playerIn;       // 玩家是否在触发区内
    bool onCd;           // 是否在冷却中
    bool hasOpened;      // 是否已经打开
    int seqIdx;          // 顺序模式下的计数
    public GameObject Prompt_trashboxNotice;
    void Awake()
    {
        // 自动查找子物体（如果没有手动拖引用）
        if (!closedVisual) closedVisual = transform.Find("ClosedVisual")?.gameObject;
        if (!openedVisual) openedVisual = transform.Find("OpenedVisual")?.gameObject;

        // 初始显示：关闭状态
        if (closedVisual) closedVisual.SetActive(true);
        if (openedVisual) openedVisual.SetActive(false);

        //if (Prompt_trashboxNotice != null && Prompt_trashboxNotice.activeSelf) 
        Prompt_trashboxNotice.SetActive(false);
    }

    // 当玩家进入触发区
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerIn = true;
        if (triggerOnEnter) TrySpawn();

    }

    // 玩家离开触发区
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerIn = false;
    }

    void Update()
    {
        Prompt_trashboxNotice.SetActive(playerIn);
        // 玩家在触发区且冷却结束时按键触发
        if (triggerOnEnter || onCd || !playerIn) return;
        if (InteractPressed()) 
        {
            Destroy(Prompt_trashboxNotice);
            if (exclamationMark != null)
                {
                    exclamationMark.SetActive(false);
                }
            TrySpawn();
        }
        }

    // 兼容新旧输入系统的按键检测
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

    // 生成钱币 + 切换外观
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

            // 给初速度（Rigidbody2D）
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

        // 打开垃圾桶外观
        OpenVisual();

        if (reuseCooldown < 0f || openOnlyOnce) { enabled = false; }
        else StartCoroutine(Cooldown());
    }

    // 切换外观
    void OpenVisual()
    {
        if (hasOpened && openOnlyOnce) return;
        hasOpened = true;

        if (closedVisual) closedVisual.SetActive(false);
        if (openedVisual) openedVisual.SetActive(true);
    }

    // 冷却计时
    IEnumerator Cooldown()
    {
        onCd = true;
        yield return new WaitForSeconds(reuseCooldown);
        onCd = false;
    }

    // 决定生成哪种钱币
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

    // 根据类型返回预制体
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

    // 自动计算垃圾桶上方位置
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
