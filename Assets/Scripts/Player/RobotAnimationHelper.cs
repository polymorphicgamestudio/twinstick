using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RobotAnimationHelper : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip shootSound;
	[SerializeField] ParticleSystem shootParticles;
	[SerializeField] AudioClip enterBuildModeSound;
	[SerializeField] AudioClip enterAttackModeSound;
	[SerializeField] ParticleSystem buildProjectorParticles;

	void Awake(){
		audioSource = GetComponent<AudioSource>();
    }
	
	void EnterBuildMode() {
		buildProjectorParticles.Play();
		audioSource.PlayOneShot(enterBuildModeSound);
	}
	void EnterAttackMode() {
		buildProjectorParticles.Stop();
		audioSource.PlayOneShot(enterAttackModeSound);
	}
	void Shoot() {
		shootParticles.Play();
		audioSource.PlayOneShot(shootSound);
	}
}