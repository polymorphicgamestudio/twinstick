using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerArrays : MonoBehaviour
{
    public static GameObject[] Towers;
    public static GameObject[] Holograms;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameObject GetTower(int i)
    {
        return Towers[i];
    }

    public static GameObject GetHologram(int i)
    {
        return Holograms[i];
    }
}
