using UnityEngine;

public class IceTowerController : BaseTower
{
    public ParticleSystem iceParticles;
    public Collider particlesCollider;

    public float particleActivationTime;
    public float currentParticleActivationTime;


    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (iceParticles.isPlaying)
        {
            currentParticleActivationTime -= Time.deltaTime;

            if (currentParticleActivationTime < 0)
                particlesCollider.enabled = false;

        }
        else
        {
            particlesCollider.enabled = false;
        }


    }



    public override void ShootTurret()
    {
        base.ShootTurret();

        iceParticles.Play();
        particlesCollider.enabled = true;

        currentParticleActivationTime = particleActivationTime;

        //play the particles for a set amount of time
        //then they will auto turn off

    }
}
