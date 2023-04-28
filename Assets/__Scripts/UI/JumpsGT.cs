using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent( typeof(Text) )]
public class JumpsGT : MonoBehaviour {
    Text    txt;

	// Use this for initialization
	void Awake () {
        txt = GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
         
        txt.text = (PlayerShip.JUMPS >= 0) ? PlayerShip.JUMPS + " Jumps" : "";
	}
}
