using UnityEngine;

public class DragObject : MonoBehaviour
{
    private Rigidbody _draggedObject;
    private Vector3 _offset;
    private float _mouseZPos;
    private bool _isDragging;

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Нажатие ЛКМ
        {
            StartDrag();
        }
        else if (Input.GetMouseButtonUp(0) && _isDragging) // Отпускание ЛКМ
        {
            StopDrag();
        }

        if (_isDragging) // Удержание ЛКМ
        {
            Drag();
        }
    }

    void StartDrag()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.attachedRigidbody != null)
            {
                _draggedObject = hit.collider.attachedRigidbody;
                _mouseZPos = Camera.main.WorldToScreenPoint(_draggedObject.position).z;
                _offset = _draggedObject.position - GetMouseWorldPos();
                _isDragging = true;

                // Отключаем гравитацию для плавного перемещения
                _draggedObject.useGravity = false;
            }
        }
    }

    void Drag()
{
    _draggedObject.linearVelocity = Vector3.zero;
    _draggedObject.angularVelocity = Vector3.zero;

    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
    RaycastHit hit;

    if (Physics.Raycast(ray, out hit, 100f, LayerMask.GetMask("Ground"))) // "Ground" — имя слоя для поверхности
    {
        Vector3 targetPos = hit.point + _offset;
        targetPos.y = _draggedObject.position.y; // Сохраняем текущую высоту
        _draggedObject.MovePosition(targetPos);
    }
}


    void StopDrag()
    {
        if (_draggedObject != null)
        {
            _draggedObject.useGravity = true; // Включаем гравитацию обратно
            _draggedObject = null;
            _isDragging = false;
        }
    }

    Vector3 GetMouseWorldPos()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = _mouseZPos;
        return Camera.main.ScreenToWorldPoint(mousePos);
    }
}
