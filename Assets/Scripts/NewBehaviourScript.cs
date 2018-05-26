using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		Debug.Log ("created");
	}
	
	// Update is called once per frame
	void OnDestroy () {
		Debug.Log ("destroyed");
	}
}
