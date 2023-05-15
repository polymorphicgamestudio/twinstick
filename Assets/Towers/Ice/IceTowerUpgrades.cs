using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IceTowerUpgrades : MonoBehaviour
{

    bool higherdamage;
    bool temp;
    bool longerduration;
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
            temp = true;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            longerduration = true;
            IceTowerController.iceDuration = 3f;
            
        }

        if (funds >= 600 && upgradeselected && temp)
        {
            widercone = true;
        }

        if (funds >= 1000 && upgradeselected && longerduration)
        {
            IceTowerController.timeBetweenShots = 4.0f;
        }

        if (funds >= 1200 && upgradeselected && widercone)
        {
            //slower slowing
        }
    }
}
