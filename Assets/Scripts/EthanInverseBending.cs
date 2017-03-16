using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EthanInverseBending : MonoBehaviour 
{
	PlayerControllerAnimated playerController;

	void Start()
	{
		playerController = GetComponentInParent<PlayerControllerAnimated>();
	}

	void LateUpdate() 
	{
		// Minimize the head rotation - compensate the neck rotation
		transform.localEulerAngles = new Vector3( 0.0f, 0.0f, playerController.xRot_/2.0f );
		transform.localPosition = new Vector3( -0.05f, -playerController.xRot_ / 2000.0f, 0.0f );
	}
}
