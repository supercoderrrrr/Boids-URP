using UnityEngine;

namespace BoidsUnderwaterScene
{
    public sealed class UnderwaterCameraController : MonoBehaviour
    {
        [SerializeField] private float speed = 6f;
        private float yaw, pitch;
        private Vector3 velocity;
        private void OnEnable()
        {
            yaw = transform.eulerAngles.y;
            pitch = Mathf.DeltaAngle(0f, transform.eulerAngles.x);
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) Release();
            if (Input.GetMouseButtonDown(0)) { Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false; }
            if (Cursor.lockState != CursorLockMode.Locked) { velocity = Vector3.zero; return; }
            yaw += Input.GetAxisRaw("Mouse X") * 1.6f;
            pitch = Mathf.Clamp(pitch - Input.GetAxisRaw("Mouse Y") * 1.6f, -85f, 85f);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), (Input.GetKey(KeyCode.E) ? 1 : 0) - (Input.GetKey(KeyCode.Q) ? 1 : 0), Input.GetAxisRaw("Vertical"));
            Vector3 desired = (transform.right * input.x + Vector3.up * input.y + transform.forward * input.z).normalized;
            velocity = Vector3.Lerp(velocity, desired * speed * (Input.GetKey(KeyCode.LeftShift) ? 2.5f : 1f), 1f - Mathf.Exp(-8f * Time.deltaTime));
            Vector3 next = transform.position + velocity * Time.deltaTime;
            Vector3 motion = next - transform.position;
            if (motion.sqrMagnitude > .000001f && Physics.SphereCast(transform.position,.3f,motion.normalized,out RaycastHit hit,motion.magnitude+.1f,1<<6,QueryTriggerInteraction.Ignore))
            {
                next = transform.position + motion.normalized * Mathf.Max(0,hit.distance-.1f);
                velocity = Vector3.ProjectOnPlane(velocity,hit.normal);
            }
            next.y = Mathf.Clamp(next.y, 1.1f, 25.5f);
            transform.position = next;
        }
        private void OnDisable() => Release();
        private void OnApplicationFocus(bool focused) { if (!focused) Release(); }
        private static void Release() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}
