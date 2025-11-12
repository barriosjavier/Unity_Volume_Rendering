using UnityEngine;

public class ObjectRotator : MonoBehaviour
{
    public float rotationSpeed = 5f; // sensibilidad del ratón
    private bool isDragging = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            isDragging = true;

        if (Input.GetMouseButtonUp(0))
            isDragging = false;

        if (isDragging)
        {
            float rotX = Input.GetAxis("Mouse X") * rotationSpeed;
            float rotY = Input.GetAxis("Mouse Y") * rotationSpeed;

            // Rota el objeto en sus ejes locales
            transform.Rotate(Vector3.up, -rotX, Space.World);
            transform.Rotate(Vector3.right, rotY, Space.World);
        }
    }
}