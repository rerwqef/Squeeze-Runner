using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerLightController : MonoBehaviour
{
    [Header("Player Light Settings")]
    public Light pointLight;
    public Material emissionMaterial;

    [Header("Health Settings")]
    [Range(0f, 1f)] public float health = 1f;
    public Color emissionColor = Color.cyan;
    public float maxLightIntensity = 2f;
    public float minLightIntensity = 0.2f;
    public float maxLightRange = 8f;
    public float minLightRange = 2f;

    [Header("Health Decay")]
    public float decayRate = 0.05f; // units per second

    [Header("Post-Processing")]
    public Volume postVolume;
    private Vignette vignette;
    private Bloom bloom;

    [Header("Post Settings")]
    public float maxVignette = 0.6f;
    public float minVignette = 0.1f;
    public float maxBloom = 2.0f;
    public float minBloom = 0.1f;

    void Start()
    {
        // Get Post Processing overrides
        if (postVolume != null && postVolume.profile != null)
        {
            postVolume.profile.TryGet(out vignette);
            postVolume.profile.TryGet(out bloom);
        }
    }

    void Update()
    {
        // Automatically reduce health over time
        DrainHealth(decayRate);

        // Update lighting and effects based on current health
        UpdateLightAndEffects();
    }

    void UpdateLightAndEffects()
    {
        float t = Mathf.Clamp01(health); // Normalize health

        // Update Point Light
        if (pointLight)
        {
            pointLight.intensity = Mathf.Lerp(minLightIntensity, maxLightIntensity, t);
            pointLight.range = Mathf.Lerp(minLightRange, maxLightRange, t);
        }

        // Update Emission
        if (emissionMaterial)
        {
            float emissionPower = Mathf.Lerp(0.1f, 3f, t);
            Color finalColor = emissionColor * emissionPower;
            emissionMaterial.SetColor("_EmissionColor", finalColor);

            if (TryGetComponent(out Renderer rend))
                DynamicGI.SetEmissive(rend, finalColor);
        }

        // Update Vignette
        if (vignette != null)
            vignette.intensity.value = Mathf.Lerp(maxVignette, minVignette, t);

        // Update Bloom
        if (bloom != null)
            bloom.intensity.value = Mathf.Lerp(minBloom, maxBloom, t);
    }

    // Call this to simulate health loss
    public void DrainHealth(float rate)
    {
        health -= rate * Time.deltaTime;
        health = Mathf.Clamp01(health);
    }

    // Call this to recharge at energy station
    public void Recharge(float amount)
    {
        health += amount;
        health = Mathf.Clamp01(health);
    }
}
