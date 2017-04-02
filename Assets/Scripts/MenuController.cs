using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController :MonoBehaviour
{
    // Master volume slider
    Slider volumeSlider_;
    // Mouse sensitivity slider
    Slider mouseSensitivitySlider_;

    // Main menu
    GameObject mainMenu_;
    // Settings menu
    GameObject settingsMenu_;

    // HUD canvas
    Canvas hudCanvas_;
    // Map canvas
    Canvas mapCanvas_;

    // Settings back button
    Button newGameButton_;

    bool gameStarted_ = false;

    // Init function
    void Awake()
    {
        // Show mouse cursor
        Cursor.visible = true;
        // Unlock mouse cursor
        Cursor.lockState = CursorLockMode.None;

        // Get main menu canvas component
        mainMenu_ = GameObject.Find( "MainMenu" );
        // Get settings menu canvas component
        settingsMenu_ = GameObject.Find( "SettingsMenu" );
        
        // Get HUD canvas component
        hudCanvas_ = GameObject.Find( "HUD" ).GetComponent<Canvas>();
        // Get map canvas component
        mapCanvas_ = GameObject.Find( "Map" ).GetComponent<Canvas>();

        // Get New game button component
        newGameButton_ = GameObject.Find( "NewGameButton" ).GetComponent<Button>();

        // Get volume slider component
        volumeSlider_ = GameObject.Find( "VolumeSlider" ).GetComponent<Slider>();
        // Get mouse sensitivity slider component
        mouseSensitivitySlider_ = GameObject.Find( "MouseSensitivitySlider" ).GetComponent<Slider>();

        // Game is paused
        Time.timeScale = 0;
    }

    // Post init function
    void Start()
    {
        // Hide settings menu
        settingsMenu_.SetActive( false );
        // Hide HUD
        hudCanvas_.enabled = false;
    }

    // Starts/restarts the game
    public void StartGame()
    {
        // If game already started, use this to reset the game
        if( gameStarted_ )
        {
            SceneManager.LoadScene( "Shooter" );
        }

        ExitMenu();

        // Set game started flag
        gameStarted_ = true;
    }
    
    // Shows the main menu
    public void ShowMainMenu()
    {
        // If game already started, change the 'New game' button text
        if( gameStarted_ )
        {
            newGameButton_.GetComponentInChildren<Text>().text = "Reset game state";
        }

        // Pause the game
        Time.timeScale = 0;

        // Hide settings menu
        settingsMenu_.SetActive( false );
        // Show main menu
        mainMenu_.SetActive( true );
        // Hide HUD
        hudCanvas_.enabled = false;

        // Show mouse cursor
        Cursor.visible = true;
        // Unlock mouse cursor
        Cursor.lockState = CursorLockMode.None;
    }

    // Hides the settings menu
    public void ExitMenu()
    {
        // Unpause the game
        Time.timeScale = 1;

        // Hide main menu
        mainMenu_.SetActive( false );
        // Hide settings menu
        settingsMenu_.SetActive( false );
        // Show HUD
        hudCanvas_.enabled = true;

        // Hide mouse cursor
        Cursor.visible = false;
        // Lock mouse cursor to screen center
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Shows the settings menu
    public void ShowSettings()
    {
        // Hide main menu
        mainMenu_.SetActive( false );
        // Show settings menu
        settingsMenu_.SetActive( true );
    }
    
    // Hides the settings menu
    public void HideSettings()
    {
        // Show main menu
        mainMenu_.SetActive( true );
        // Hide settings menu
        settingsMenu_.SetActive( false );
    }

    // Quits the game
    public void Quit()
    {
        Application.Quit();
    }
    
    // Resets the game
    public void ResetScene()
    {
        SceneManager.LoadScene( "Shooter" );
    }

    // Shows the map
    public void ShowMap()
    {
        mapCanvas_.enabled = true;
    }
    // Hides the map
    public void HideMap()
    {
        mapCanvas_.enabled = false;
    }

    // Return game state
    public bool HasGameStarted()
    {
        return gameStarted_;
    }

    // Change master volume to the volume slider value
    public void onVolumeSliderChanged()
    {
        AudioListener.volume = volumeSlider_.value;
    }

    // Change mouse sensitivity to the mouse sensitivity slider value
    public void onMouseSensitivitySliderChanged()
    {
        PlayerControllerAnimated.mouseSensitivity_ = mouseSensitivitySlider_.value;
    }
}
