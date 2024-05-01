using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// stage 0-1: ext door closes
// stage 1-2: int door closes
// stage 2-3: lift "moves"
// stage 3-4: int door opens
// stage 4-5: ext door opens
// stage 5..: lift resets in "opposite" position

// this is pretty horrible code but it works :)
public class LiftBehavior : MonoBehaviour {
    [SerializeField] GameObject player;

    [SerializeField] GameObject liftCarriage;
    Vector3 carriageInitPos;

    [SerializeField] GameObject[] carriageDoorLeft = new GameObject[2];
    Vector3[] carriageDoorLeftInitPos = new Vector3[2];
    [SerializeField] GameObject[] carriageDoorRight = new GameObject[2];
    Vector3[] carriageDoorRightInitPos = new Vector3[2];

    [SerializeField] GameObject[] externalDoorsTop = new GameObject[2];
    Vector3[] extDoorsTopInitPos = new Vector3[2];
    [SerializeField] GameObject[] externalDoorsBottom = new GameObject[2];
    Vector3[] extDoorsBottomInitPos = new Vector3[2];

    [SerializeField]
    AnimationCurve intDoorAnimationCurve
        = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
    [SerializeField]
    AnimationCurve extDoorAnimationCurve
        = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);

    const float CARRIAGE_TOP_HEIGHT_OFFSET = 4.7f;
    const float EXTERNAL_DOOR_OFFSET = 0.7f;
    const float INTERNAL_DOOR_OFFSET = 0.72f;

    [SerializeField] float internalDoorTime;
    [SerializeField] float externalDoorTime;
    [SerializeField] float liftWaitTime;

    [SerializeField] GameObject upperDoorAudioEmitter;
    [SerializeField] GameObject lowerDoorAudioEmitter;
    [SerializeField] GameObject upperCarriageMusicEmitter;
    [SerializeField] GameObject lowerCarriageMusicEmitter;

    [SerializeField] FMODUnity.EventReference doorOpenEvent;
    FMOD.Studio.EventInstance doorOpenIns;
    [SerializeField] FMODUnity.EventReference doorCloseEvent;
    FMOD.Studio.EventInstance doorCloseIns;
    [SerializeField] FMODUnity.EventReference doorsClosingAnnounceEvent;
    FMOD.Studio.EventInstance doorsClosingAnnounce;

    bool hasPlayedClosingAnnounce = false;

    [SerializeField] FMODUnity.EventReference liftEvent;
    FMOD.Studio.EventInstance liftIns;
    [SerializeField] FMODUnity.EventReference liftArriveEvent;
    FMOD.Studio.EventInstance liftArriveIns;

    [SerializeField] FMODUnity.EventReference liftMusicEvent;
    FMOD.Studio.EventInstance liftMusicIns;
    [SerializeField] FMODUnity.ParamRef liftMusicDuckingParam;

    float animStartTime = 0.0f;

    bool isIdle = true;
    bool onBottomFloor = true;

    uint stage = 0;
    float[] stageTimes;

    bool playerHasTeleported = false;
    bool playerInLift = false;

    CapsuleCollider liftDetectZone;

    // *** *** *** //

    void Start() {
        SetFMODEvents();
        SetStageTimes();

        liftDetectZone = GetComponent<CapsuleCollider>();
        liftDetectZone.isTrigger = true;

        carriageInitPos = liftCarriage.transform.position;

        for (int i = 0; i < 2; i++) {
            carriageDoorLeftInitPos[i] = carriageDoorLeft[i].transform.position;
            carriageDoorRightInitPos[i] = carriageDoorRight[i].transform.position;
            extDoorsTopInitPos[i] = externalDoorsTop[i].transform.position;
            extDoorsBottomInitPos[i] = externalDoorsBottom[i].transform.position;
        }

        externalDoorsTop[0].transform.position +=
            new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
        externalDoorsTop[1].transform.position -=
            new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
    }

    void Update() {
        if (!isIdle) Animate();
    }

    void OnTriggerEnter(Collider collider) {
        if (!collider.CompareTag("Player") || playerInLift) return;
        ToggleLift();
        playerInLift = true;
    }

    void OnTriggerExit(Collider collider) {
        playerInLift = false;
    }

    void SummonToBottom() {
        if (onBottomFloor || !isIdle) return;
        Summon();
    }

    void SummonToTop() {
        if (!onBottomFloor || !isIdle) return;
        // in order to speed up the lift, we can bypass stages here.
        Summon();
    }

    void Summon() {
        stage = 4;
        isIdle = false;
        animStartTime = Time.time - stageTimes[stage];

        var doors =
            onBottomFloor ? externalDoorsBottom : externalDoorsTop;
        var doorsInit =
            onBottomFloor ? extDoorsBottomInitPos : extDoorsTopInitPos;

        if (onBottomFloor) {
            externalDoorsBottom[0].transform.position +=
                new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
            externalDoorsBottom[1].transform.position -=
                new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
        }
        else {
            externalDoorsTop[0].transform.position +=
                new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
            externalDoorsTop[1].transform.position -=
                new Vector3(EXTERNAL_DOOR_OFFSET, 0.0f, 0.0f);
        }

        CallEventForStage();
    }

    void ToggleLift() {
        if (!isIdle) return;

        isIdle = false;
        playerHasTeleported = false;
        animStartTime = Time.time;

        CallEventForStage();
    }

    void Animate() {
        float elapsed = Time.time - animStartTime;
        float t = InterpForStage(
            elapsed / TotalAnimTime(),
            stage
        );

        if (t >= 1.0f) {
            t -= 1.0f;

            if (++stage >= 5) {
                stage = 0;
                isIdle = true;
                onBottomFloor = !onBottomFloor;

                hasPlayedClosingAnnounce = false;

                t = 0.0f;
            }
            else {
                CallEventForStage();
            }
        }

        float intCurve = intDoorAnimationCurve.Evaluate(t);
        float extCurve = extDoorAnimationCurve.Evaluate(t);

        // this lambda is used to close a slight gap in the external doors. 
        // bit awkward but it does the job
        Func<float, int> stage1Action = c => {
            var doors =
                onBottomFloor ? externalDoorsBottom : externalDoorsTop;
            var doorsInit =
                onBottomFloor ? extDoorsBottomInitPos : extDoorsTopInitPos;

            var newL = doorsInit[0];
            var newR = doorsInit[1];
            newL.x += EXTERNAL_DOOR_OFFSET * c;
            newR.x -= EXTERNAL_DOOR_OFFSET * c;

            doors[0].transform.position = newL;
            doors[1].transform.position = newR;
            return 0;
        };

        switch (stage) {
            // ext
            case 0:
                stage1Action(intCurve);
                break;
            // int
            case 1: {
                    stage1Action(1.0f);

                    for (int i = 0; i < 2; i++) {
                        var currPosL = carriageDoorLeft[i].transform.position;
                        var currPosR = carriageDoorRight[i].transform.position;
                        var newXLeft = carriageDoorLeftInitPos[i].x;
                        var newXRight = carriageDoorRightInitPos[i].x;

                        newXLeft += INTERNAL_DOOR_OFFSET * intCurve;
                        newXRight -= INTERNAL_DOOR_OFFSET * intCurve;
                        carriageDoorLeft[i].transform.position =
                            new Vector3(newXLeft, currPosL.y, currPosL.z);
                        carriageDoorRight[i].transform.position =
                            new Vector3(newXRight, currPosR.y, currPosR.z);
                    }
                }
                break;
            // wait
            case 2:
                if (!playerHasTeleported) {
                    Teleport();
                }
                break;
            // int
            case 3: {
                    player.GetComponent<PlayerController>().enabledGravity = true;
                    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("LiftMusicDucking", 0.0f);
                    liftMusicIns.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                    liftIns.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                    for (int i = 0; i < 2; i++) {
                        var currPosL = carriageDoorLeft[i].transform.position;
                        var currPosR = carriageDoorRight[i].transform.position;
                        var newXLeft = carriageDoorLeftInitPos[i].x;
                        var newXRight = carriageDoorRightInitPos[i].x;

                        newXLeft += INTERNAL_DOOR_OFFSET * (1.0f - intCurve);
                        newXRight -= INTERNAL_DOOR_OFFSET * (1.0f - intCurve);
                        carriageDoorLeft[i].transform.position =
                            new Vector3(newXLeft, currPosL.y, currPosL.z);
                        carriageDoorRight[i].transform.position =
                            new Vector3(newXRight, currPosR.y, currPosR.z);
                    }
                }
                break;
            // ext
            case 4: {
                    var doors =
                        !onBottomFloor ? externalDoorsBottom : externalDoorsTop;
                    var doorsInit =
                        !onBottomFloor ? extDoorsBottomInitPos : extDoorsTopInitPos;

                    var newL = doorsInit[0];
                    var newR = doorsInit[1];
                    newL.x += EXTERNAL_DOOR_OFFSET * (1.0f - intCurve);
                    newR.x -= EXTERNAL_DOOR_OFFSET * (1.0f - intCurve);

                    doors[0].transform.position = newL;
                    doors[1].transform.position = newR;
                }
                break;
        }
    }

    void Teleport() {
        // it's really important here to disable the character controller and any 
        // other vertical motion for the player to keep the teleport smooth
        var pc = player.GetComponent<PlayerController>();
        pc.enabledGravity = false;
        pc.velocity = Vector3.zero;
        var cc = pc.GetComponent<CharacterController>();
        cc.enabled = false;

        var offset = onBottomFloor ? 4.7f : -5.1f;

        if (CanTeleportPlayer())
            player.transform.position += new Vector3(0.0f, offset, 0.0f);

        cc.enabled = true;
        playerHasTeleported = true;
    }

    float TotalAnimTime() {
        return internalDoorTime * 2.0f
            + externalDoorTime * 2.0f
            + liftWaitTime;
    }

    float InterpForStage(float currentInterp, uint stage) {
        if (currentInterp > 1.0f) return 1.0f;
        else if (currentInterp < 0.0f) return 0.0f;

        currentInterp *= TotalAnimTime();

        switch (stage) {
            case 0:
                return (currentInterp - stageTimes[0]) / externalDoorTime;
            case 1:
                return (currentInterp - stageTimes[1]) / internalDoorTime;
            case 2:
                return (currentInterp - stageTimes[2]) / liftWaitTime;
            case 3:
                return (currentInterp - stageTimes[3]) / internalDoorTime;
            case 4:
                return (currentInterp - stageTimes[4]) / externalDoorTime;

            default:
                return 0.0f;
        }
    }

    bool CanTeleportPlayer() {
        return liftDetectZone
            .bounds
            .Contains(player.transform.position);
    }

    void CallEventForStage() {
        var st01Emitter = onBottomFloor ? lowerDoorAudioEmitter : upperDoorAudioEmitter;
        var st02Emitter = !onBottomFloor ? lowerDoorAudioEmitter : upperDoorAudioEmitter;

        switch (stage) {
            case 0:
            case 1:
                FMODUnity.RuntimeManager.AttachInstanceToGameObject(
                    doorCloseIns, st01Emitter.transform
                );
                doorCloseIns.start();

                if (!hasPlayedClosingAnnounce) {
                    var closingEmitter = onBottomFloor
                        ? lowerCarriageMusicEmitter : upperCarriageMusicEmitter;

                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(
                        doorsClosingAnnounce, closingEmitter.transform
                    );
                    doorsClosingAnnounce.start();

                    hasPlayedClosingAnnounce = true;
                }

                break;

            case 2:
                if (!CanTeleportPlayer()) break;

                FMODUnity.RuntimeManager.StudioSystem.setParameterByName("LiftMusicDucking", 1.0f);

                liftIns.setParameterByID(liftMusicDuckingParam.ID, 1.0f);

                var emitter = !onBottomFloor ? lowerCarriageMusicEmitter : upperCarriageMusicEmitter;

                FMODUnity.RuntimeManager.AttachInstanceToGameObject(liftMusicIns, emitter.transform);
                liftMusicIns.start();

                FMODUnity.RuntimeManager.AttachInstanceToGameObject(liftIns, player.transform);
                liftIns.start();
                break;

            // this is when the "lift arrive" beep plays
            case 3:
                liftIns.setParameterByID(liftMusicDuckingParam.ID, 0.0f);

                var arriveEmitter = !onBottomFloor
                    ? lowerCarriageMusicEmitter : upperCarriageMusicEmitter;

                FMODUnity.RuntimeManager.AttachInstanceToGameObject(
                    liftArriveIns, arriveEmitter.transform
                );
                liftArriveIns.start();

                FMODUnity.RuntimeManager.AttachInstanceToGameObject(
                    doorCloseIns, st02Emitter.transform
                );
                doorCloseIns.start();

                break;

            case 4:
                if (!doorCloseEvent.IsNull) {
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(
                        doorCloseIns, st02Emitter.transform
                    );
                    doorCloseIns.start();
                }
                break;

            default:
                break;
        }
    }

    void SetStageTimes() {
        stageTimes = new float[] {
            0.0f,
            externalDoorTime,
            externalDoorTime + internalDoorTime,
            externalDoorTime + internalDoorTime + liftWaitTime,
            externalDoorTime + internalDoorTime + liftWaitTime
                + internalDoorTime,
            TotalAnimTime()
        };
    }

    void SetFMODEvents() {
        if (!doorOpenEvent.IsNull)
            doorOpenIns = FMODUnity.RuntimeManager.CreateInstance(doorOpenEvent);
        if (!doorCloseEvent.IsNull)
            doorCloseIns = FMODUnity.RuntimeManager.CreateInstance(doorCloseEvent);
        if (!doorsClosingAnnounceEvent.IsNull)
            doorsClosingAnnounce = FMODUnity.RuntimeManager.CreateInstance(doorsClosingAnnounceEvent);

        if (!liftEvent.IsNull)
            liftIns = FMODUnity.RuntimeManager.CreateInstance(liftEvent);
        if (!liftArriveEvent.IsNull)
            liftArriveIns = FMODUnity.RuntimeManager.CreateInstance(liftArriveEvent);
        if (!liftMusicEvent.IsNull)
            liftMusicIns = FMODUnity.RuntimeManager.CreateInstance(liftMusicEvent);
    }
}
