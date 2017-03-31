using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EthanInverseBending : MonoBehaviour 
{
    // Player controller script
	PlayerControllerAnimated playerController;

    // Init function
    void Start()
	{
        // Get player controller script component
		playerController = GetComponentInParent<PlayerControllerAnimated>();
	}

    // Late update function
	void LateUpdate() 
	{
		// Compensate the head rotation
		transform.localEulerAngles = new Vector3( 0.0f, 0.0f, playerController.xRot_/2.0f );
		transform.localPosition = new Vector3( -0.05f, -playerController.xRot_ / 2000.0f, 0.0f );
	}
}
