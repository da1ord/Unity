using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetGame : MonoBehaviour
{
    // Resets the game
	public void ResetScene()
    {
        SceneManager.LoadScene( "Shooter" );
    }
}
