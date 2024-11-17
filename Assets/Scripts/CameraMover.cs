using UnityEngine;

public class CameraMover : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of left-right movement

    void Update()
    {
        HandleHorizontalMovement();
    }

    private void HandleHorizontalMovement()
    {
        // Move the camera left or right using A/D or Left/Right Arrow
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow
        transform.position += new Vector3(horizontal, 0, 0) * moveSpeed * Time.deltaTime;
    }
}