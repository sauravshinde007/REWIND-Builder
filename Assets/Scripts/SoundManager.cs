using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Audio Clips")]
    public AudioClip background;
    public AudioClip p_run;
    public AudioClip p_jump;
    public AudioClip door;
    public AudioClip button;
    public AudioClip death;

    private void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(Instance); return;
    }

    private void Start()
    {
        musicSource.clip = background;
        musicSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }
}
