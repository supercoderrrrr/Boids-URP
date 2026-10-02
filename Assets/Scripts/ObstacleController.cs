using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private BoxCollider movementBounds;
    [SerializeField] private float boundsPadding = 0.5f;

    private void Update()
    {
        Vector3 input = new Vector3(
            Input.GetAxisRaw("Horizontal"),
            0f,
            Input.GetAxisRaw("Vertical")
        );

        if (Input.GetKey(KeyCode.E))
        {
            input.y += 1f;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            input.y -= 1f;
        }

        if (input.sqrMagnitude > 1f)
        {
            input.Normalize();
        }

        transform.position += input * moveSpeed * Time.deltaTime;

        KeepInsideBounds();
    }

    private void KeepInsideBounds()
    {
        if (movementBounds == null)
        {
            return;
        }

        Vector3 localPosition = movementBounds.transform.InverseTransformPoint(transform.position);
        localPosition -= movementBounds.center;

        Vector3 halfSize = movementBounds.size * 0.5f;
        halfSize -= Vector3.one * boundsPadding;

        localPosition.x = Mathf.Clamp(localPosition.x, -halfSize.x, halfSize.x);
        localPosition.y = Mathf.Clamp(localPosition.y, -halfSize.y, halfSize.y);
        localPosition.z = Mathf.Clamp(localPosition.z, -halfSize.z, halfSize.z);

        transform.position = movementBounds.transform.TransformPoint(localPosition + movementBounds.center);
    }
}