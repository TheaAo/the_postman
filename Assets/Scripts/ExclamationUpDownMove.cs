using UnityEngine;

public class ExclamationUpDownMove : MonoBehaviour
{
     [Header("浮动设置")]
    public float floatSpeed = 2f;      // 浮动速度
    public float floatHeight = 0.3f;   // 浮动高度
    
    private Vector3 startPosition;
    
    void Start()
    {
        // 记住初始位置
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        // 使用Sin函数实现上下浮动
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
