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
        volumeSlider_.onValueChanged.AddListener( delegate {
            volumeSliderChanged();
        } );

        mouseSensitivitySlider_.onValueChanged.AddListener( delegate {
            mouseSensitivitySliderChanged();
        } );
    }

    void volumeSliderChanged()
    {
        AudioListener.volume = volumeSlider_.value;
    }

    void mouseSensitivitySliderChanged()
    {
        PlayerControllerAnimated.mouseSensitivity_ = mouseSensitivitySlider_.value;
    }
}
