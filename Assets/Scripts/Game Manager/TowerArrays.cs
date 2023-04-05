using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerArrays : MonoBehaviour
{
    public Transform[] Towers;
    public Transform[] Holograms;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Transform GetTower(int i)
    {
        return Towers[i];
    }

    public Transform GetHologram(int i)
    {
        return Holograms[i];
    }
}
