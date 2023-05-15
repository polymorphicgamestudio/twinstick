using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTowerUpgrades : MonoBehaviour
{

    bool higherdamage;
    bool largerprojectile;
    bool fasterprojectile;
    bool fasterfirerate;

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
            largerprojectile = true;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            fasterprojectile = true;
            FireTowerController.bombSpeed = 40f;
        }

        if (funds >= 600 && upgradeselected && largerprojectile)
        {
            fasterfirerate = true;
            FireTowerController.timeBetweenShots = 1.0f;
        }

        if (funds >= 1000 && upgradeselected && fasterprojectile)
        {
            //burning chance
        }

        if (funds >= 1200 && upgradeselected && fasterfirerate)
        {
            //bigger AOE
        }
    }
}
