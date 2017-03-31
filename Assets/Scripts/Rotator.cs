using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour
{
    // Update function
    void Update () 
	{
        // Rotate the object along Y-axis
		transform.Rotate( new Vector3( 0.0f, 90.0f, 0.0f ) * Time.deltaTime );
	}
}
