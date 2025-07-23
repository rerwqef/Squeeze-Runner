using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class Ball : MonoBehaviour
{
    [Header("Resize Settings")]
    public Slider sizeSlider;
    public float minSize = 0.2f;
    public float maxSize = 3f;

    [Header("Rotation Settings")]
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // Y-axis rotation
    public float rotationSpeed = 90f; // Degrees per second

    private Rigidbody rb;
    private Quaternion currentRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
       // rb.isKinematic = true; // So physics doesn't interfere with rotation
        currentRotation = rb.rotation;

        if (sizeSlider != null)
        {
            sizeSlider.minValue = minSize;
            sizeSlider.maxValue = maxSize;
            sizeSlider.onValueChanged.AddListener(UpdateSize);
            UpdateSize(sizeSlider.value); // Initialize size
        }
        else
        {
            Debug.LogWarning("Size Slider not assigned in Inspector!");
        }
    }

    void FixedUpdate()
    {
        RotateBall();
    }

    void RotateBall()
    {
        Quaternion deltaRotation = Quaternion.Euler(rotationAxis * rotationSpeed * Time.fixedDeltaTime);
        currentRotation *= deltaRotation;
        rb.MoveRotation(currentRotation);
    }

    void UpdateSize(float newSize)
    {
        Vector3 scale = Vector3.one * Mathf.Clamp(newSize, minSize, maxSize);
        transform.localScale = scale;
    }
}
