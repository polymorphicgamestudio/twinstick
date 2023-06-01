using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTower : BaseTower
{

    public AIManager manager;

    public override void ShootTurret()
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator WaitToAdd()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        manager.AddTowerToList(this);


    }

    // Start is called before the first frame update
    void Start()
    {

        StartCoroutine(WaitToAdd());


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
