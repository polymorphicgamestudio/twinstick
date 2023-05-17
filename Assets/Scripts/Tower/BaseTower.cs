using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseTower : MonoBehaviour
{

    public ushort objectID;
    bool slimebool = false;

    float timer;
    float currentTimer;

    public static float minDist;
    public static float maxDist;

    public static Transform slimeTarget;
    public Transform tower;

    public static float timeBetweenShots;

    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ShootTurret", timeBetweenShots, timeBetweenShots);
    }

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
        //if there's not a target search for a target

        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        currentTimer = timer;

        if (slimebool == true)
        {
            Vector3 newDirection = Vector3.RotateTowards(tower.forward, slimeTarget.position - this.transform.position, Time.deltaTime * 15, 0.0f);
            tower.rotation = Quaternion.LookRotation(newDirection);
        }
    }

    public void LateUpdate()
    {
        if (slimebool == true) {
            ShootTurret();
        }
    }


    public virtual void ShootTurret()
    {

    }

    private void SearchForSlime()
    {
        //slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestVisibleObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
        slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
    }

}
