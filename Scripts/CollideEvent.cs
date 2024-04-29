using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideEvent : MonoBehaviour {
    [SerializeField] FMODUnity.EventReference collisionEvent;
    FMOD.Studio.EventInstance eventInstance;

    [SerializeField] float cooldownTimeSecs = 0.4f;
    float cooldownTimer = 0.0f;
    bool cooldown = true;

    // *** *** *** //

    void Start() {
        if (collisionEvent.IsNull) return;

        eventInstance = FMODUnity.RuntimeManager.CreateInstance(collisionEvent);
    }

    void Update() {
        if (cooldown) {
            cooldownTimer += Time.deltaTime;

            if (cooldownTimer >= cooldownTimeSecs) {
                cooldownTimer = 0.0f;
                cooldown = false;
            }
        }
    }

    void OnCollisionEnter(Collision col) {
        if (cooldown || collisionEvent.IsNull) return;
        // the impulse could be used for the intensity of the impact
        // float impulse = col.impulse.magnitude;

        // Required for spatialisation to work
        FMODUnity.RuntimeManager
            .AttachInstanceToGameObject(eventInstance, this.transform);
        eventInstance.start();

        cooldown = true;
    }
}
