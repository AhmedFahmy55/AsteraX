using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent( typeof(ParticleSystem) )]
public class OnlyEmitParticlesInBounds : MonoBehaviour {
    
    private ParticleSystem.EmissionModule emitter;

	void Awake () 
    {
        
        emitter = GetComponent<ParticleSystem>().emission;
	}
	
	
	void LateUpdate () 
    {
        if (ScreenBounds.OOB( transform.position )) {
            emitter.enabled = false;
        } else {
            emitter.enabled = true;
        }
	}

}
