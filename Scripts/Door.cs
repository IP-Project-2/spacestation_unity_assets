using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour {
    public float openHeight = 2f; // Height the door should open to
    public float animationDuration = 1f; // Duration of the opening/closing animation
    public float autoCloseDelay = 3f; // Delay before the door automatically closes
    [SerializeField] bool forceOpen = false;
    // If there are any gameobjects attached to the door, then adding them to
    // this list will ensure that they move with it.
    [SerializeField] List<GameObject> attachedObjects = new List<GameObject>();
    List<Vector3> attachedObjectsInitPos = new List<Vector3>();
    public AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, 1, 1); // Animation curve for smooth movement

    [SerializeField] FMODUnity.EventReference doorOpenEvent;
    FMOD.Studio.EventInstance doorOpenIns;
    [SerializeField] FMODUnity.EventReference doorCloseEvent;
    FMOD.Studio.EventInstance doorCloseIns;

    // TODO: these may be better as separate scripts
    enum DoorType {
        VerticalSlide,
        Hinged,
        HorizontalDouble,
        Hangar,
    }

    [SerializeField] DoorType doorType = DoorType.VerticalSlide;

    private Vector3 initialPosition;
    private bool isOpening = false;
    private float animationStartTime;
    private float triggerTime;

    private bool hasInteracted = false;

    bool isOpen = false;
    bool hasPlayedCloseSound = false;

    private void Start() {
        // Store the initial position of the door
        initialPosition = transform.position;

        for (int i = 0; i < attachedObjects.Count; i++)
            attachedObjectsInitPos.Add(attachedObjects[i].transform.position);

        if (forceOpen) {
            transform.Translate(new Vector3(0.0f, openHeight, 0.0f));
            SetAttachedObjectsPositions();
        }

        if (!doorOpenEvent.IsNull)
            doorOpenIns = FMODUnity.RuntimeManager.CreateInstance(doorOpenEvent);
        if (!doorCloseEvent.IsNull)
            doorCloseIns = FMODUnity.RuntimeManager.CreateInstance(doorCloseEvent);
    }

    private void Update() {
        // Avoid doing any animation if the door is set to be forced open
        if (forceOpen) return;

        // Check if the door is in the process of opening
        if (isOpening) {
            hasPlayedCloseSound = false;

            // Calculate the time elapsed since the animation started
            float timeElapsed = Time.time - animationStartTime;

            // Calculate the completion percentage of the animation based on time
            float t = Mathf.Clamp01(timeElapsed / animationDuration);

            // Use the animation curve to smooth the movement
            float curveValue = animationCurve.Evaluate(t);

            // Calculate the new position of the door based on the open height
            Vector3 newPosition =
                initialPosition + Vector3.up * openHeight * curveValue;

            // Move the door to the new position
            transform.position = newPosition;
            SetAttachedObjectsPositions();

            // If the animation is complete, stop opening the door
            if (t >= 1f) {
                isOpening = false;
                isOpen = true;
                triggerTime = Time.time;
            }
            else
                isOpen = false;
        }
        else if (hasInteracted) {
            // Check if the door should start closing automatically
            if (Time.time - triggerTime >= autoCloseDelay) {
                if (!hasPlayedCloseSound) {
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(doorCloseIns, this.transform);
                    doorCloseIns.start();
                    hasPlayedCloseSound = true;
                }
                // Start the closing animation
                StartClosingAnimation();
            }
        }
    }

    public void ToggleDoor() {
        if (isOpen) {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(doorCloseIns, this.transform);
            doorCloseIns.start();
            StartClosingAnimation();
        }
        else {
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(doorOpenIns, this.transform);
            doorOpenIns.start();
            StartOpeningAnimation();
        }
    }

    // private void OnCollisionEnter(Collision other) {
    //     // Check if the door is not already opening and the trigger is activated
    //     if (!isOpening && other.gameObject.CompareTag("Player")) {
    //         // Start the opening animation
    //         StartOpeningAnimation();
    //     }
    // }

    private void StartOpeningAnimation() {
        // Avoid doing any animation if the door is set to be forced open
        if (forceOpen) return;

        hasInteracted = true;

        // Set the flag to indicate that the door is opening
        isOpening = true;

        // Store the time when the animation started
        animationStartTime = Time.time;
    }

    private void StartClosingAnimation() {
        // Avoid doing any animation if the door is set to be forced open
        if (forceOpen) return;

        hasInteracted = true;
        isOpen = false;

        // Calculate the completion percentage of the closing animation based on time
        float t = Mathf.Clamp01((Time.time - triggerTime - autoCloseDelay) / animationDuration);

        // Use the animation curve to smooth the movement
        float curveValue = animationCurve.Evaluate(t);

        // Calculate the new position of the door based on the open height
        Vector3 newPosition = initialPosition + Vector3.up * openHeight * (1 - curveValue);

        // Move the door to the new position
        transform.position = newPosition;
        SetAttachedObjectsPositions();

        // If the animation is complete, reset the trigger time
        if (t >= 1f) {
            ResetDoor();
        }
    }

    // Reset the door to its initial position
    public void ResetDoor() {
        transform.position = initialPosition;
        triggerTime = 0f;
    }

    void SetAttachedObjectsPositions() {
        var deltaY = transform.position.y - initialPosition.y;

        for (int i = 0; i < attachedObjects.Count; i++) {
            var newPos = attachedObjectsInitPos[i];
            newPos.y += deltaY;
            attachedObjects[i].transform.position = newPos;
        }
    }
}
