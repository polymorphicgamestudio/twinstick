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
    protected float currentBeamActivationTime;


    protected override void Start()
    {
        base.Start();



    }


    public override void ManualUpdate()
    {
        base.ManualUpdate();

        if (beam.enabled)
        {
            currentBeamActivationTime -= Time.deltaTime;

            if (currentBeamActivationTime <= 0)
                beam.enabled = false;


        }


    }

    public override void EndOfWave()
    {
        base.EndOfWave();

        Invoke(nameof(TurnOffBeam), currentBeamActivationTime);

    }

    protected void TurnOffBeam()
    {
        currentBeamActivationTime = 0;
        beam.enabled = false;
        barrelParticles.Stop();
    }

    public override void ShootTurret()
    {
        base.ShootTurret();

        if (barrelParticles != null) 
            barrelParticles.Play();
        currentBeamActivationTime = beamActivationTime;

        beam.enabled = true;


    }


}
