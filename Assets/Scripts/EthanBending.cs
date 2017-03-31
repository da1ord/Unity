using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EthanBending : MonoBehaviour
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
        // Compensate the torso rotation
        transform.localEulerAngles = new Vector3( 0.0f, 0.0f, -playerController.xRot_ );
	}
}
