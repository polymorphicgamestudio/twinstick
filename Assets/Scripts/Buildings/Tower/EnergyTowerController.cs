using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BeamTowerController : BaseTower
{
    public ParticleSystem barrelParticles;

    public LineRenderer beam;
    protected Vector3 direction;

    public float beamActivationTime;
    private float currentBeamActivationTime;

    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (beam.enabled)
        {
            currentBeamActivationTime -= Time.deltaTime;

            if (currentBeamActivationTime < 0)
                beam.enabled = false;


        }


    }

    public override void ShootTurret()
    {
        if (barrelParticles != null) 
            barrelParticles.Play();
        currentBeamActivationTime = beamActivationTime;

        beam.enabled = true;
        //beam.SetPosition(0, barrel.position);
        //beam.SetPosition(1, hit.point);


    }


}
