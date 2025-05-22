using UnityEngine;

public class PlayButtonSound : MonoBehaviour
{
    public AudioClip soundClip;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = soundClip;
    }

    public void PlaySound()
    {
        audioSource.Play();
    }
}
