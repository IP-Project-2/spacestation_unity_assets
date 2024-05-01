using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public CharacterController charC;

    [SerializeField] float speed = 5;
    [SerializeField] float creepSpeedScale = 0.5f;
    [SerializeField] float sprintSpeedScale = 2.0f;

    const float TERMINAL_VELOCITY = 50.0f;
    [SerializeField] float gravity = -9.8f;
    [SerializeField] float jumpHeight = 2f;

    public Vector3 velocity;

    [SerializeField] Transform groundCheck;
    [SerializeField] float groundDistance = 0.4f;
    [SerializeField] LayerMask groundMask;
    [SerializeField] bool isGrounded;
    const float GROUND_TOLERANCE = 0.1f;
    [SerializeField] bool withinGroundTolerance = false;

    [SerializeField] float distanceFromGround = 0.0f;
    [SerializeField] bool canLand = false;

    [SerializeField] FMODUnity.EventReference playerFootstepEvent;
    FMOD.Studio.EventInstance playerFootstep;
    [SerializeField] FMODUnity.EventReference playerLandEvent;
    FMOD.Studio.EventInstance playerLand;
    [SerializeField] FMOD.Studio.PARAMETER_ID movementStateParamID;
    float movementStateLast = 0.0f;
    const string FMOD_MOVEMENT_STATE = "MovementState";
    const string FMOD_GROUND_MATERIAL = "GroundMaterial";

    bool isSprintKeyDown = false;
    bool isCreepKeyDown = false;

    bool isFootstepStopped = false;

    [SerializeField] Camera cameraRef;
    [SerializeField] float interactDistance = 3.0f;

    public bool enabledGravity = true;

    void Start() {
        charC.detectCollisions = true;
        playerFootstep = FMODUnity.RuntimeManager.CreateInstance(playerFootstepEvent);
        playerLand = FMODUnity.RuntimeManager.CreateInstance(playerLandEvent);

        movementStateParamID = ParamFrom(playerFootstep, FMOD_MOVEMENT_STATE);
    }

    public static FMOD.Studio.PARAMETER_ID ParamFrom(
        FMOD.Studio.EventInstance eventInstance,
        string paramName
    ) {
        FMOD.Studio.EventDescription eventDesc;
        eventInstance.getDescription(out eventDesc);
        FMOD.Studio.PARAMETER_DESCRIPTION paramDesc;
        eventDesc.getParameterDescriptionByName(paramName, out paramDesc);
        return paramDesc.id;
    }

    void Update() {
        float xmove = Input.GetAxis("Horizontal");
        float ymove = Input.GetAxis("Vertical");
        Vector2 movement = new Vector2(xmove, ymove);

        bool isMoving = Mathf.Abs(movement.x) > 0.0f
            || Mathf.Abs(movement.y) > 0.0f;

        if (isMoving) {
            UpdateSpeedKeys();

            if (isFootstepStopped && withinGroundTolerance) {
                StartFootsteps();
                isFootstepStopped = false;
            }

            if (!withinGroundTolerance) {
                StopFootsteps();
                isFootstepStopped = true;
            }
        }
        else if (!isFootstepStopped) {
            StopFootsteps();
            isFootstepStopped = true;
        }

        Vector3 moveHor = transform.right * movement.x + transform.forward * movement.y;

        charC.Move(moveHor * GetSpeedScale() * Time.deltaTime);

        // Create downward force to simulate gravity becasue we are not using a rigidbody
        if (enabledGravity) {
            if (isGrounded) {
                if (Input.GetKey(KeyCode.Space))
                    velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                else
                    velocity.y = gravity;
            }
            else {
                velocity.y = Mathf.Max(velocity.y + gravity * Time.deltaTime * 2.0f, -TERMINAL_VELOCITY);
            }
            charC.Move(velocity * Time.deltaTime);
        }


        groundCheck.position = transform.position + new Vector3(0.0f, -0.6f, 0.0f);
        GroundRaycast();

        if (Input.GetKeyDown(KeyCode.E))
            InteractRaycast();
    }

    // This allows other rigidbodies to be forcefully pushed by the player.
    void OnControllerColliderHit(ControllerColliderHit hit) {
        var body = hit.collider.attachedRigidbody;
        if (body == null || body.isKinematic || hit.moveDirection.y < -0.3f) return;

        var pushForce = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);
        body.AddForce(pushForce, ForceMode.Impulse);
    }

    void InteractRaycast() {
        RaycastHit hit;

        if (Physics.Raycast(cameraRef.transform.position,
            cameraRef.transform.forward * interactDistance,
            out hit)
        ) {
            InteractRaycast interaction;
            if (hit.collider.TryGetComponent(out interaction))
                interaction.Interact();
        }
    }

    void GroundRaycast() {
        RaycastHit hit;
        if (Physics.Raycast(groundCheck.position, Vector3.down, out hit, 10.0f, groundMask)) {
            SetGroundMaterial(hit.collider.gameObject.tag);

            distanceFromGround = hit.distance - groundDistance * 0.5f;

            VerifyJumpDistance();

            isGrounded = distanceFromGround <= 0.05f;
            withinGroundTolerance = distanceFromGround <= GROUND_TOLERANCE;
        }
        else {
            distanceFromGround = float.PositiveInfinity;
            isGrounded = false;
            withinGroundTolerance = false;
            canLand = false;
        }
    }

    void SetGroundMaterial(string colliderTag) {
        string groundMaterial = colliderTag == "Stairs" ? "MetalLight" : "MetalHeavy";

        FMODUnity.RuntimeManager
            .StudioSystem
            .setParameterByNameWithLabel(FMOD_GROUND_MATERIAL, groundMaterial);
    }

    void VerifyJumpDistance() {
        if (distanceFromGround >= 0.3f)
            canLand = true;
        else if (canLand && withinGroundTolerance) {
            PlayerLand();
            canLand = false;
        }
    }

    void PlayerLand() {
        FMODUnity.RuntimeManager.AttachInstanceToGameObject(playerLand, this.transform);
        playerLand.start();
    }

    void UpdateSpeedKeys() {
        isSprintKeyDown = Input.GetKey(KeyCode.LeftShift);
        isCreepKeyDown = Input.GetKey(KeyCode.LeftControl)
            || Input.GetKey(KeyCode.LeftCommand);
    }

    void StartFootsteps() {
        FMODUnity.RuntimeManager
            .AttachInstanceToGameObject(playerFootstep, this.transform);
        playerFootstep.start();
    }

    void StopFootsteps() {
        FMODUnity.RuntimeManager
            .DetachInstanceFromGameObject(playerFootstep);
        playerFootstep.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    float GetSpeedScale() {
        float movementScale = 1.0f;
        float paramValue = 1.0f;

        if (isSprintKeyDown) {
            if (!isCreepKeyDown) {
                movementScale = sprintSpeedScale;
                paramValue = 2.0f;
            }
        }
        else if (isCreepKeyDown) {
            movementScale = creepSpeedScale;
            paramValue = 0.0f;
        }

        if (paramValue != movementStateLast)
            playerFootstep.setParameterByID(movementStateParamID, paramValue);

        // alternative:
        // playerFootstep.setParameterByNameWithLabel(...);

        movementStateLast = paramValue;

        return speed * movementScale;
    }
}
