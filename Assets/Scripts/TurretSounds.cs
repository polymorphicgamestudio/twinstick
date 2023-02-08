using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TurretSounds : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip wakeSound;
	[SerializeField] AudioClip shootSound;

	void Awake(){
		audioSource = GetComponent<AudioSource>();
    }
	public void PlayWakeSound() {
		audioSource.PlayOneShot(wakeSound);
	}
	public void PlayShootSound() {
		audioSource.PlayOneShot(shootSound);
	}
}