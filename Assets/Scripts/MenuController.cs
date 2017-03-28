using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Slider volumeSlider_;
    public Slider mouseSensitivitySlider_;

    void Start()
    {
        // Add volume slider onValueChanged listener
        volumeSlider_.onValueChanged.AddListener( delegate {
            volumeSliderChanged();
        } );

        // Add mouse sensitivity slider onValueChanged listener
        mouseSensitivitySlider_.onValueChanged.AddListener( delegate {
            mouseSensitivitySliderChanged();
        } );
    }

    // Change master volume to the volume slider value
    void volumeSliderChanged()
    {
        AudioListener.volume = volumeSlider_.value;
    }

    // Change mouse sensitivity to the mouse sensitivity slider value
    void mouseSensitivitySliderChanged()
    {
        PlayerControllerAnimated.mouseSensitivity_ = mouseSensitivitySlider_.value;
    }
}
