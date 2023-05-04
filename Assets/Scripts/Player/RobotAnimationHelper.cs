using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RobotAnimationHelper : MonoBehaviour {

	AudioSource audioSource;
	[SerializeField] AudioClip shootSound;
	[SerializeField] ParticleSystem shootParticles;
	[SerializeField] AudioClip enterBuildModeSound;
	[SerializeField] AudioClip enterAttackModeSound;
	//[SerializeField] ParticleSystem buildProjectorParticles // handled in tower placement script

	void Awake(){
		audioSource = GetComponent<AudioSource>();
    }
	
	void EnterBuildMode() {
		audioSource.PlayOneShot(enterBuildModeSound);
	}
	void EnterAttackMode() {
		audioSource.PlayOneShot(enterAttackModeSound);
	}
	void Shoot() {
		shootParticles.Play();
		audioSource.PlayOneShot(shootSound);
	}
}