using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float sprintMultiplier = 2.5f;

    [Header("Mouse Look")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorOnStart = true;

    private float yaw;
    private float pitch;

    private void Start()
    {
        Vector3 eulerAngles = transform.eulerAngles;
        yaw = eulerAngles.y;
        pitch = Mathf.DeltaAngle(0f, eulerAngles.x);

        if (lockCursorOnStart)
        {
            LockCursor();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            UnlockCursor();
        }

        if (Input.GetMouseButtonDown(0))
        {
            LockCursor();
        }

        if (Cursor.lockState == CursorLockMode.Locked)
        {
            UpdateMouseLook();
        }

        UpdateMovement();
    }

    private void UpdateMouseLook()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    private void UpdateMovement()
    {
        Vector3 input = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            input += transform.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            input -= transform.forward;
        }

        if (Input.GetKey(KeyCode.D))
        {
            input += transform.right;
        }

        if (Input.GetKey(KeyCode.A))
        {
            input -= transform.right;
        }

        if (Input.GetKey(KeyCode.E))
        {
            input += Vector3.up;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            input -= Vector3.up;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentSpeed *= sprintMultiplier;
        }

        transform.position += input * currentSpeed * Time.deltaTime;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        UnlockCursor();
    }
}
