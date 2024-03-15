using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    public Light pointLight;
    public float minIntensity = 0.5f;
    public float maxIntensity = 1.5f;
    public float flickerFrequency = 0.5f;

    private float nextFlickerTime;

    void Start()
    {
        pointLight = GetComponent<Light>();
        // Initialize the next flicker time
        nextFlickerTime = Time.time + Random.Range(0f, flickerFrequency);
    }

    void Update()
    {
        // Check if it's time to flicker
        if (Time.time >= nextFlickerTime)
        {
            // Randomly change the intensity of the light
            float randomIntensity = Random.Range(minIntensity, maxIntensity);
            pointLight.intensity = randomIntensity;

            // Set the next flicker time
            nextFlickerTime = Time.time + flickerFrequency;
        }
    }
}