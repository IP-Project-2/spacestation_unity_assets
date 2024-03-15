using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public float openHeight = 2f; // Height the door should open to
    public float animationDuration = 1f; // Duration of the opening/closing animation
    public float autoCloseDelay = 3f; // Delay before the door automatically closes
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1); // Animation curve for smooth movement

    private Vector3 initialPosition;
    private bool isOpening = false;
    private float animationStartTime;
    private float triggerTime;

    private void Start()
    {
        // Store the initial position of the door
        initialPosition = transform.position;
    }

    private void Update()
    {
        // Check if the door is in the process of opening
        if (isOpening)
        {
            // Calculate the time elapsed since the animation started
            float timeElapsed = Time.time - animationStartTime;

            // Calculate the completion percentage of the animation based on time
            float t = Mathf.Clamp01(timeElapsed / animationDuration);

            // Use the animation curve to smooth the movement
            float curveValue = animationCurve.Evaluate(t);

            // Calculate the new position of the door based on the open height
            Vector3 newPosition = initialPosition + Vector3.up * openHeight * curveValue;

            // Move the door to the new position
            transform.position = newPosition;

            // If the animation is complete, stop opening the door
            if (t >= 1f)
            {
                isOpening = false;
                triggerTime = Time.time;
            }
        }
        else
        {
            // Check if the door should start closing automatically
            if (Time.time - triggerTime >= autoCloseDelay)
            {
                // Start the closing animation
                StartClosingAnimation();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the door is not already opening and the trigger is activated
        if (!isOpening && other.CompareTag("Player"))
        {
            // Start the opening animation
            StartOpeningAnimation();
        }
    }

    private void StartOpeningAnimation()
    {
        // Set the flag to indicate that the door is opening
        isOpening = true;

        // Store the time when the animation started
        animationStartTime = Time.time;
    }

    private void StartClosingAnimation()
    {
        // Calculate the completion percentage of the closing animation based on time
        float t = Mathf.Clamp01((Time.time - triggerTime - autoCloseDelay) / animationDuration);

        // Use the animation curve to smooth the movement
        float curveValue = animationCurve.Evaluate(t);

        // Calculate the new position of the door based on the open height
        Vector3 newPosition = initialPosition + Vector3.up * openHeight * (1 - curveValue);

        // Move the door to the new position
        transform.position = newPosition;

        // If the animation is complete, reset the trigger time
        if (t >= 1f)
        {
            ResetDoor();
        }
    }

    // Reset the door to its initial position
    private void ResetDoor()
    {
        transform.position = initialPosition;
        triggerTime = 0f;
    }
}
