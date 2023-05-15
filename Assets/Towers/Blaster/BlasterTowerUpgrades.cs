using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlasterTowerUpgrades : MonoBehaviour
{

    bool higherdamage;
    bool fastershooting;
    bool longsightrange;
    bool widercone;

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
            fastershooting = true;
            BlasterTowerController.timeBetweenShots = 1.0f;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            longsightrange = true;
            BaseTower.maxDist = 25f;
        }

        if (funds >= 600 && upgradeselected && fastershooting)
        {
            widercone = true;
        }

        if (funds >= 1000 && upgradeselected && longsightrange)
        {
            //longer shot range
        }

        if (funds >= 1200 && upgradeselected && widercone)
        {
            //bleeding chance
        }
    }
}
