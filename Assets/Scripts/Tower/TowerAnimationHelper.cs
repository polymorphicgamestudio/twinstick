using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class TowerAnimationHelper : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip wakeSound;
	[SerializeField] AudioClip shootSound;
	[SerializeField] ParticleSystem shootParticles;

	void Awake(){
		audioSource = GetComponent<AudioSource>();
    }
	public void PlayWakeSound() {
		audioSource.PlayOneShot(wakeSound);
	}
	public void PlayShootSound() {
		audioSource.PlayOneShot(shootSound);
	}
	public void PlayShootParticles() {
		shootParticles.Play();
	}
}