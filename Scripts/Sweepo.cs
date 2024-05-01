using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sweepo : MonoBehaviour {
    [SerializeField] FMODUnity.EventReference sweepoGeneral;

    [SerializeField] float sweepoGeneralTimeSecs = 20.0f;
    float sweepoGeneralTimer = 0.0f;

    [SerializeField] FMODUnity.EventReference sweepoFall;
    [SerializeField] float fallVelocity = 5.0f;
    bool isFalling = false;
    Rigidbody rb;

    // *** *** *** //

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        var verticalVel = rb.velocity.y;

        if (verticalVel < -fallVelocity) {
            if (!isFalling) {
                var ins = FMODUnity.RuntimeManager.CreateInstance(sweepoFall);
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(ins, this.transform);
                ins.start();
            }

            isFalling = true;
        }
        else {
            ProgressGeneralTimer();
            isFalling = false;
        }
    }

    void ProgressGeneralTimer() {
        sweepoGeneralTimer += Time.deltaTime + Random.Range(-0.5f, 0.5f);

        if (sweepoGeneralTimer >= sweepoGeneralTimeSecs) {
            var ins = FMODUnity.RuntimeManager.CreateInstance(sweepoGeneral);
            FMODUnity.RuntimeManager.AttachInstanceToGameObject(ins, this.transform);
            ins.start();

            sweepoGeneralTimer -= sweepoGeneralTimeSecs;
        }
    }
}
