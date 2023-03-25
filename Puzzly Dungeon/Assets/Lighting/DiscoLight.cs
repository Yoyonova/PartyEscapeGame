using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DiscoLight : MonoBehaviour
{
    [SerializeField] private float maxRotation, duration, durationVariance, maxIntensity, intensityVariance, startingIntensityFactor;
    [SerializeField] private Light2D spotLight;
    private float currentDuration = 0f;

    void Start()
    {
        maxIntensity *= 1f - intensityVariance + (2 * intensityVariance * Random.value);
        duration *= 1f - durationVariance + (2 * durationVariance * Random.value);
        transform.eulerAngles = new Vector3(0f, 0f, 180f - maxRotation + (2 * maxRotation * Random.value));

        spotLight.color = Random.ColorHSV(0f, 1f, 0.8f, 0.8f, 0.8f, 0.8f);

        float currentIntensityFactor = Mathf.Sin(currentDuration / duration * Mathf.PI) * (1 - startingIntensityFactor) + startingIntensityFactor;

        spotLight.intensity = currentIntensityFactor * maxIntensity;
    }

    void Update()
    {
        currentDuration += Time.deltaTime;
        if (currentDuration >= duration) Destroy(this.gameObject);

        float currentIntensityFactor = Mathf.Sin(currentDuration / duration * Mathf.PI) * (1 - startingIntensityFactor) + startingIntensityFactor;

        spotLight.intensity = currentIntensityFactor * maxIntensity;
    }
}
