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
    public Vector3 rotationAxis = new Vector3(0, 1, 0); // Y-axis
    public float rotationSpeed = 90f; // Degrees per second

    private Rigidbody rb;
    private Quaternion currentRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentRotation = rb.rotation;

        if (sizeSlider != null)
        {
            sizeSlider.minValue = -1f;  // Center is 0
            sizeSlider.maxValue = 1f;
            sizeSlider.value = 0f;      // Start at center
            sizeSlider.onValueChanged.AddListener(UpdateSize);
            UpdateSize(sizeSlider.value); // Initialize
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

    void UpdateSize(float sliderValue)
    {
        float newSize;

        if (sliderValue >= 0)
            newSize = Mathf.Lerp(1f, maxSize, sliderValue); // Increase
        else
            newSize = Mathf.Lerp(1f, minSize, -sliderValue); // Decrease

        Vector3 scale = Vector3.one * newSize;
        transform.localScale = scale;
    }
}
