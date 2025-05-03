using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 3f, -4f); // Увеличиваем Y и Z для высокого персонажа
    public float rotationSpeed = 5f;
    public float distance = 12f; // Увеличиваем расстояние, чтобы видеть персонажа полностью
    private float currentYaw = 0f;
    private float currentPitch = 10f; // Уменьшаем начальный наклон
    public float heightOffset = 6f; // Фокус на уровне груди (рост 11.55 / 2 ≈ 5.775)

    void LateUpdate()
    {
        if (target == null) return;

        if (Input.GetMouseButton(1))
        {
            currentYaw += Input.GetAxis("Mouse X") * rotationSpeed;
            currentPitch -= Input.GetAxis("Mouse Y") * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, -10f, 45f); // Ограничиваем наклон
        }

        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0);
        Vector3 rotatedOffset = rotation * offset.normalized * distance;
        transform.position = target.position + rotatedOffset;

        // Смотрим на середину персонажа
        Vector3 lookAtPosition = target.position + new Vector3(0, heightOffset, 0);
        transform.LookAt(lookAtPosition);
    }
}