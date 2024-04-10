using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractRaycast : MonoBehaviour {
    // Data related to one callback/FMOD event.
    [System.Serializable]
    public class CallbackData {
        // Any targeted GameObject. If null, the callback will target this GameObject.
        public GameObject targetObject = null;
        // The name of any function to call. If empty, no message is sent.
        public string functionName = "";
        // A reference to an FMOD event. If null, no event is called.
        public FMODUnity.EventReference fmodEvent;
        // Whether the FMOD event should be attached to a GameObject in 3D space or not.
        [SerializeField] bool spatialEvent = true;
        FMOD.Studio.EventInstance eventIns;

        public CallbackData() {
            if (!fmodEvent.IsNull)
                eventIns = FMODUnity.RuntimeManager.CreateInstance(fmodEvent);
        }

        public void Call(GameObject self) {
            var obj = targetObject == null ? self : targetObject;

            if (obj == null) {
                Debug.LogError("Tried to interact with a null object.");
                return;
            }

            if (functionName.Length > 0)
                obj.SendMessage(functionName);

            if (!fmodEvent.IsNull) {
                if (spatialEvent)
                    FMODUnity.RuntimeManager.AttachInstanceToGameObject(eventIns, obj.transform);

                eventIns.start();
            }
        }
    }

    [SerializeField]
    List<CallbackData> callbacks;

    // *** *** *** //

    void Start() { }

    public void Interact() {
        foreach (var cb in callbacks) {
            cb.Call(this.gameObject);
        }
    }
}
