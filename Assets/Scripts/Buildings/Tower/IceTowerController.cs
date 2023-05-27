using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.UI.Image;

public class IceTowerController : BaseTower
{
    public ParticleSystem iceParticles;



    public override void ShootTurret()
    {

        iceParticles.Play();

        //play the particles for a set amount of time
        //then they will auto turn off

    }
}
