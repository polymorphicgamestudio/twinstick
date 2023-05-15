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

    public float minDist;
    public float maxDist;

    public Transform slimeTarget;
    public Transform tower;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (slimebool == false)
        {
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
            ShootTurret();
        }
    }

    public void LateUpdate()
    {
        //shooting and attacking
    }


    public virtual void ShootTurret()
    {

    }

    private void SearchForSlime()
    {
        slimeTarget = ShepProject.ShepGM.inst.EnemyManager.QuadTree.GetClosestVisibleObject(objectID, ShepProject.ObjectType.Slime, minDist, maxDist);
    }

}
