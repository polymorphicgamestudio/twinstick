using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseTower : MonoBehaviour
{

    public ushort objectID;
    public static bool slimebool = false;

    float timer;
    float currentTimer;

    public static float minDist = 0f;
    public static float maxDist = 20f;

    public static Transform slimeTarget;
    public Transform tower;

    // Update is called once per frame
    void Update()
    {
        if (slimebool == false)
        {
            tower.eulerAngles = new Vector3(0, 0, 0);
            SearchForSlime();
            if (slimeTarget != null)
            {
                slimebool = true;
            }
        }

        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        currentTimer = timer;

        if (slimebool == true)
        {
            Vector3 newDirection = Vector3.RotateTowards(tower.forward, slimeTarget.position - this.transform.position, Time.deltaTime * 15, 0.0f);
            tower.rotation = Quaternion.LookRotation(newDirection);

            if (Vector3.Distance(this.transform.position, slimeTarget.position) > maxDist)
            {
                slimebool = false;
            }
        }
    }


    public void SearchForSlime()
    {
        //slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestVisibleObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
        slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
    }

}
