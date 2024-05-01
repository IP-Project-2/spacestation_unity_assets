using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioVolume : MonoBehaviour {
    // The music volume parameter.
    [Range(0.0f, 1.0f)] public float musicVolume = 1.0f;
    // The music volume parameter name in FMOD.
    const string FMOD_MUSIC_VOLUME = "MusicVolume";

    // The ambience volume parameter.
    [Range(0.0f, 1.0f)] public float ambienceVolume = 1.0f;
    // The ambience volume parameter name in FMOD.
    const string FMOD_AMBIENCE_VOLUME = "AmbienceVolume";

    // The SFX volume parameter.
    [Range(0.0f, 1.0f)] public float fxVolume = 1.0f;
    // The SFX volume parameter names in FMOD.
    readonly string[] FMOD_FX_VOLUMES = {
        "Jamie_FXVolume",
        "",
    };

    // *** *** *** //

    void Update() {
        // update music volume
        FMODUnity.RuntimeManager
            .StudioSystem
            .setParameterByName(FMOD_MUSIC_VOLUME, musicVolume);

        // update ambience volume
        FMODUnity.RuntimeManager
            .StudioSystem
            .setParameterByName(FMOD_AMBIENCE_VOLUME, ambienceVolume);

        // update SFX volumes
        foreach (var param in FMOD_FX_VOLUMES) {
            FMODUnity.RuntimeManager
            .StudioSystem
            .setParameterByName(param, fxVolume);
        }
    }
}
