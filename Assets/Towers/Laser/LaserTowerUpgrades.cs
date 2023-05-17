using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTowerUpgrades : MonoBehaviour
{

    bool higherdamage;
    bool temp;
    bool longersightrange;
    bool widerbeam;

    bool upgradeselected;
    int funds;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (funds >= 200 && upgradeselected)
        {
            higherdamage = true;
        }

        if (funds >= 300 && upgradeselected)
        {
            temp = true;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            longersightrange = true;
            BaseTower.maxDist = 20f;
        }

        if (funds >= 600 && upgradeselected)
        {
            widerbeam = true;
        }

        if (funds >= 1000 && upgradeselected)
        {
            //faster cooldown
            LaserTowerController.timeBetweenShots = 1.5f;
        }

        if (funds >= 1200 && upgradeselected)
        {
            //longer beam duration
            LaserTowerController.beamDuration = 1f;
        }
    }
}
