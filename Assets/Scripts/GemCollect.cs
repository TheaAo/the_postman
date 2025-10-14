using UnityEngine;

public class GemCollect: MonoBehaviour
{
    public AudioClip pickupSound;   // 在 Inspector 里放入pick up时要播放的音效
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 调试输出，看看有没有触发到
        Debug.Log("Hit: " + collision.name);
        // 确认碰到的是玩家
        if (collision.CompareTag("Gem"))
        {
            // 播放音效
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // 让物体消失
            Destroy(collision.gameObject);

            // 这里也可以加拾取音效、加分数、背包逻辑等
       
        }
    }
}
