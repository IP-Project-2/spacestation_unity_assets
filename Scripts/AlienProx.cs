using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienProx : MonoBehaviour
{
    public GameObject alien;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    
    
    var dist = Vector3.Distance(transform.position,alien.transform.position);
    var alienDistance = Mathf.Clamp(20.0f - dist, 0.0f, 20.0f);

    FMODUnity.RuntimeManager.StudioSystem.setParameterByName("Enemy Proximety", alienDistance);
    Debug.Log(alienDistance);

    //FMODUnity.RuntimeManager.StudioSystem.setParameterByNameWithLabel("NAME OF PARAMETER", "NAME OF PARAM STATE");

    }
}
