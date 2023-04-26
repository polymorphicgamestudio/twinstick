using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RobotAnimationHelper : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip wakeSound;
	[SerializeField] AudioClip shootSound;
	[SerializeField] ParticleSystem buildProjectorParticles;
	[SerializeField] DynamicBone dynamicDoor;

	void Awake(){
		audioSource = GetComponent<AudioSource>();
    }
	
	void EnterBuildMode() {
		buildProjectorParticles.Play();
	}
	void EnterAttackMode() {
		buildProjectorParticles.Stop();
	}
	void Shoot() {

	}
	
	
	
	
	
	public void PlayWakeSound() {
		audioSource.PlayOneShot(wakeSound);
	}
	public void PlayShootSound() {
		audioSource.PlayOneShot(shootSound);
	}
}