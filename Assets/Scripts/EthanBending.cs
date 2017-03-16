using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EthanBending : MonoBehaviour 
{
	PlayerControllerAnimated playerController;

	void Start()
	{
		playerController = GetComponentInParent<PlayerControllerAnimated>();
	}

	void LateUpdate() 
	{
//		transform.localEulerAngles = new Vector3( -20.0f, 0.0f, -playerController.xRot_ );
		transform.localEulerAngles = new Vector3( 0.0f, 0.0f, -playerController.xRot_ );
	}
}
