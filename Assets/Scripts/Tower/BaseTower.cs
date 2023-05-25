using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class BaseTower : MonoBehaviour
{

    public ushort objectID;

    public float timer;
    public float currentTimer;

    public float minDist = 0f;
    public float maxDist = 20f;

    public Transform slimeTarget;
    //public Transform tower;

    private void Start()
    {
        ShepProject.ShepGM.inst.EnemyManager.AddTowerToList(this);
    }

    // Update is called once per frame
    public virtual void Update()
    {


        if (slimeTarget != null)
        {
            if (!slimeTarget.gameObject.activeInHierarchy)
            {
                SearchForSlime();
            }
            else
            { 


                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                    Quaternion.LookRotation(slimeTarget.position - transform.position), Time.deltaTime * 180);

            }


        }
        else
        {
            SearchForSlime();
        }

        



        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        currentTimer = timer;

        if (slimeTarget != null)
        {

            ShootTurret();
        }
    }



    public void SearchForSlime()
    {
        //slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestVisibleObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
        slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
    }

    public virtual void ShootTurret()
    {


    }

}
