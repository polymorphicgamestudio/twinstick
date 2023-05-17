using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidTowerUpgrades : MonoBehaviour
{

    bool higherdamage;
    bool longersightrange;
    bool fasterprojectile;
    bool temp;

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
            longersightrange = true;
            BaseTower.maxDist = 40f;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            fasterprojectile = true;
            AcidTowerController.bombSpeed = 40f;
        }

        if (funds >= 600 && upgradeselected && longersightrange)
        {
            temp = true;
        }

        if (funds >= 1000 && upgradeselected && fasterprojectile)
        {
            //toxic cloud or puddle
        }

        if (funds >= 1200 && upgradeselected && temp)
        {
            //bigger AOE
        }
    }
}
