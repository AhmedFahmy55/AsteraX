using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour {
    public float degPerSec = 90;
	
	
	void Update () {
        
        transform.rotation = Quaternion.Euler(0, 0, degPerSec * Time.realtimeSinceStartup);
	}
}
