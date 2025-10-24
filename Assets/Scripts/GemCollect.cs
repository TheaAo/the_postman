using UnityEngine;

public class GemCollect: MonoBehaviour
{
    public AudioClip pickupSound;   // Put the sound you want to play when you pick up in the Inspector.
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Debug and see if it triggers the
        Debug.Log("Hit: " + collision.name);
        // Confirm that you've met a player.
        if (collision.CompareTag("Gem"))
        {
            // Play sound effects
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            // Disappearance of objects
            Destroy(collision.gameObject);

            // Here you can also add pickup sound effects, add scores, backpack logic, etc.

        }
    }
}
