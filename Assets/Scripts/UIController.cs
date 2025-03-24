using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Slider _musicSlider, _effectsSlider;

    public void ToggleMusic()
    {
        AudioManager.Instance.ToggleMusic();
    }

    public void ToggleEffects()
    {
        AudioManager.Instance.ToggleEffects();
    }

    public void MusicVolume()
    {
        AudioManager.Instance.MusicVolume(_musicSlider.value);
    }

    public void EffectsVolume()
    {
        AudioManager.Instance.EffectsVolume(_effectsSlider.value);
    }
}
