using UnityEngine;

public class GemCollect: MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 调试输出，看看有没有触发到
        Debug.Log("Hit: " + collision.name);
        // 确认碰到的是玩家
        if (collision.CompareTag("Gem"))
        {
            // 让物体消失
            Destroy(collision.gameObject);

            // 这里也可以加拾取音效、加分数、背包逻辑等

        }
    }
}
