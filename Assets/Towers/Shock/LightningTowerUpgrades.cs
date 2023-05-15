using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : MonoBehaviour
{
    bool higherdamage;
    bool morechains;
    bool temp;
    bool longerchain;

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
            morechains = true;
        }

        if (funds >= 500 && upgradeselected && higherdamage)
        {
            temp = true;
        }

        if (funds >= 600 && upgradeselected && morechains)
        {
            longerchain = true;
        }

        if (funds >= 1000 && upgradeselected && temp)
        {
            // slimes stunned while electrocuted
        }

        if (funds >= 1200 && upgradeselected && longerchain)
        {
            // killed target has chance to explode
        }
    }
}
