using UnityEngine;

public class ExclamationUpDownMove : MonoBehaviour
{
     [Header("浮动设置")]
    public float floatSpeed = 2f;      
    public float floatHeight = 0.3f;   
    
    private Vector3 startPosition;
    
    void Start()
    {
        // the initial position
        startPosition = transform.localPosition;
    }
    
    void Update()
    {
        // Using the Sin function to float up and down
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = new Vector3(startPosition.x, newY, startPosition.z);
    }
}
