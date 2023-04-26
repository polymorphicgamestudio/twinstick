using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RobotAnimationHelper : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip shootSound;
	[SerializeField] ParticleSystem shootParticles;
	[SerializeField] ParticleSystem buildProjectorParticles;

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
		shootParticles.Play();
		audioSource.PlayOneShot(shootSound);
	}
	
}