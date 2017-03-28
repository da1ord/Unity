using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : MonoBehaviour 
{
	void Update () 
	{
        // Rotate object along Y-axis
		transform.Rotate( new Vector3( 0.0f, 80.0f, 0.0f ) * Time.deltaTime );
	}
}
