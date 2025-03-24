using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{


    public static AudioManager Instance;

    public Sound[] musicSounds, effectsSounds;
    public AudioSource musicSource, effectsSource;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(gameObject);
            //un comment the above line if you want the audio manager to persist through scenes
        }
        else 
        {
            Destroy(gameObject);
        }
    }

   
    public void PlayAudio(AudioClip audioClip)
    {
        effectsSource.PlayOneShot(audioClip);
    }

    public void PlayMusic(AudioClip audioClip)
    {
        musicSource.clip = audioClip;
        musicSource.Play();
    }

    public bool ToggleEffects()
    {
        effectsSource.mute = !effectsSource.mute;
        return effectsSource.mute;
    }

    public bool ToggleMusic()
    {
        musicSource.mute = !musicSource.mute;
        return musicSource.mute;
    }

    public void MusicVolume(float volume)
    {
        musicSource.volume = volume; 
    }

    public void EffectsVolume(float volume)
    {
        effectsSource.volume = volume;
    }
}