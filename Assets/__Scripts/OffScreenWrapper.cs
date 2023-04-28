﻿//#define DEBUG_AnnounceOnTriggerExit

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OffScreenWrapper : MonoBehaviour {

	Bullet bulletScript;
	
    
	void Start(){
		// Get the Bullet component of this GameObject if it exists.
		// If there is no Bullet (Script) component, bulletScript will be null
		bulletScript = gameObject.GetComponent<Bullet>();
    }
    
    
    private void OnTriggerExit(Collider other)
    {
        // NOTE: OnTriggerExit is still called when this.enabled==false
        if (!enabled)
        {
            return;
        }

        // Ensure that the other is OnScreenBounds
        ScreenBounds bounds = other.GetComponent<ScreenBounds>();
        if (bounds == null) {
            // Check for runaway GameObjects using ExtraBounds child of ScreenBounds
            bounds = other.GetComponentInParent<ScreenBounds>();
            if (bounds == null) { // If bounds is still null, give up and return
            return;
            } else {
                // Move this GameObject closer to ScreenBounds edges, making use
                //  of the ComponentDivision extension method in Vector3Extensions
                Vector3 pos = transform.position.ComponentDivide(other.transform.localScale);
                pos.z = 0; // Make sure it's in the z=0 plane.
                transform.position = pos;
                Debug.LogWarning("OffScreenWrapper:OnTriggerExit() - Runaway object caught by ExtraBounds: "+gameObject.name);
            }
        }

        ScreenWrap(bounds);


#if DEBUG_AnnounceOnTriggerExit
        // GetComponent is pretty slow, but because this is in a debug test case and 
        //  only happens once every few seconds, it's okay here.
		if (GetComponent<Asteroid>() != null) {
    		Debug.LogWarning(gameObject.name+" OnTriggerExit "+Time.time);
		}
#endif
    }


    
    private void ScreenWrap(ScreenBounds bounds) {

        // Wrap whichever direction is necessary
        Vector3 relativeLoc = bounds.transform.InverseTransformPoint(transform.position);
        // Because this is now a child of OnScreenBounds, 0.5f is the edge of the screen.
        if (Mathf.Abs(relativeLoc.x) > 0.5f)
        {
            relativeLoc.x *= -1;
        }
        if (Mathf.Abs(relativeLoc.y) > 0.5f)
        {
            relativeLoc.y *= -1;
        }
        transform.position = bounds.transform.TransformPoint(relativeLoc);
        
        // If the GameObject that just wrapped has a Bullet script (i.e., is a Bullet,
        //  then set bDidWrap on that Bullet to true. This will be used for Lucky Shots
        if (bulletScript != null)
        {
            bulletScript.bDidWrap = true;
        }

    }

}