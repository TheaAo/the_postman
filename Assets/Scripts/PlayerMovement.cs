using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class BasicMovement : MonoBehaviour
{
    public float speed = 3f;
    private Animator animator;
   
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    // 2D Movement, move left and right in a 2D side-scroller

    void Update()
    {
        float horizontal = GetHorizontalAxis();
        if (animator != null)
        {
            animator.SetFloat("Horizontal", horizontal);
        }
        Vector3 movementInput = GetMovementInput();
        transform.position += movementInput * speed * Time.deltaTime;
    }

    private float GetHorizontalAxis()
    {
		float axis = 0f;

		// Keyboard (A/D and Left/Right arrows)
		if (Keyboard.current != null)
		{
			if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
			{
				axis -= 1f;
			}
			if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
			{
				axis += 1f;
			}
		}

		return axis;
    }

    private Vector3 GetMovementInput()
    {
		float axis = GetHorizontalAxis();
		return new Vector3(axis, 0f, 0f);
    }
}
