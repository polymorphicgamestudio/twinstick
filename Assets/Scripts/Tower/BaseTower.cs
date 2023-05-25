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
    void Update()
    {


        if (slimeTarget != null)
        {
            if (!slimeTarget.gameObject.activeInHierarchy)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                SearchForSlime();
            }


        }
        else
        {
            transform.eulerAngles = new Vector3(0, 0, 0);
            SearchForSlime();
        }



        currentTimer -= Time.deltaTime;

        if (currentTimer > 0)
            return;

        currentTimer = timer;

        if (slimeTarget != null)
        {
            Vector3 newDirection = Vector3.RotateTowards(transform.forward, slimeTarget.position - this.transform.position, Time.deltaTime * 15, 0.0f);
            transform.rotation = Quaternion.LookRotation(newDirection);

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
