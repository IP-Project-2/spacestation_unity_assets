using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interact : MonoBehaviour {
    public AudioClip audioClip; // Assign your audio clip in the Unity Editor
    private AudioSource audioSource;

    // Start is called before the first frame update  
    void Start() {
        // Get the AudioSource component attached to the same GameObject
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other) {
        // Check if the colliding object is the player
        if (other.CompareTag("Player")) {
            Debug.Log("Player entered trigger area.");
        }
    }

    void OnTriggerStay(Collider other) {
        // Check if the colliding object is the player
        if (other.CompareTag("Player")) {
            // Check if the player presses the E key
            if (Input.GetKeyDown(KeyCode.E)) {
                // Play the audio clip
                if (audioClip != null && audioSource != null) {
                    audioSource.PlayOneShot(audioClip);
                    Debug.Log("Audio played.");
                }
            }
        }
    }
}
