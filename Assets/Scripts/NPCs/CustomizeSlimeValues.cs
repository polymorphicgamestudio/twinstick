using ShepProject;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InitialSlimeValues
{

    //[Range(0, 5000)]
    public int slimeCount;

    //[Range(0, 1)]
    public float sheepAttraction;
    //[Range(-1, 1)]
    public float towerAttraction;
    //[Range(-1, 1)]
    public float slimeAttraction;
    //[Range(-1, 1)]
    public float wallAttraction;

    //[Range(0, 50)]
    public float towerViewRange;
    //[Range(0, 50)]
    public float slimeViewRange;
    //[Range(0, 50)]
    public float playerViewRange;
    //[Range(0, 50)]
    public float wallViewRange;

    //[Range(-1, 1)]
    public float slimeOptimalDistance;

    //[Range(.1f, 100)]
    public float slimeSpeed;
    //[Range(0, 1)]
    public float slimeTurnRate;
    //[Range(1, 1000)]
    public float slimeHealth;

    public float standardDeviation;

    public float mutationChance;

    public float typeMutationChance;


    public override bool Equals(object obj)
    {
        return obj is InitialSlimeValues values &&
               slimeCount == values.slimeCount &&
               sheepAttraction == values.sheepAttraction &&
               towerAttraction == values.towerAttraction &&
               slimeAttraction == values.slimeAttraction &&
               wallAttraction == values.wallAttraction &&
               towerViewRange == values.towerViewRange &&
               slimeViewRange == values.slimeViewRange &&
               playerViewRange == values.playerViewRange &&
               wallViewRange == values.wallViewRange &&
               slimeOptimalDistance == values.slimeOptimalDistance &&
               slimeSpeed == values.slimeSpeed &&
               slimeTurnRate == values.slimeTurnRate &&
               slimeHealth == values.slimeHealth;
    }

    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(slimeCount);
        hash.Add(sheepAttraction);
        hash.Add(towerAttraction);
        hash.Add(slimeAttraction);
        hash.Add(wallAttraction);
        hash.Add(towerViewRange);
        hash.Add(slimeViewRange);
        hash.Add(playerViewRange);
        hash.Add(wallViewRange);
        hash.Add(slimeOptimalDistance);
        hash.Add(slimeSpeed);
        hash.Add(slimeTurnRate);
        hash.Add(slimeHealth);
        hash.Add(standardDeviation);
        hash.Add(mutationChance);
        hash.Add(typeMutationChance);
        return hash.ToHashCode();
    }
}



public class CustomizeSlimeValues : MonoBehaviour
{
    public NPCManager manager;

    public Toggle toggleShow;
    [Space(15)]
    public GameObject sliderBackground;
    public GameObject descriptionBackground;

    [Space(15)]
    public Slider sheepAttraction;
    public Slider towerAttraction;
    public Slider slimeAttraction;
    public Slider wallAttraction;

    public Slider towerViewRange;
    public Slider slimeViewRange;
    public Slider wallViewRange;

    public Slider slimeOptimalDistance;

    public Slider slimeSpeed;
    public Slider slimeTurnRate;
    public Slider slimeHealth;

    public Slider slimeCount;

    public Slider standardDeviation;
    public Slider mutationChance;
    public Slider typeMutationChance;

    public InitialSlimeValues values;

    private void Awake()
    {

        values = manager.slimeValues;

        sheepAttraction.value = values.sheepAttraction;
        towerAttraction.value = values.towerAttraction;
        slimeAttraction.value = values.slimeAttraction;
        wallAttraction.value = values.wallAttraction;

        towerViewRange.value = values.towerViewRange;
        slimeViewRange.value = values.slimeViewRange;
        wallViewRange.value = values.wallViewRange;

        slimeOptimalDistance.value = values.slimeOptimalDistance;

        slimeSpeed.value = values.slimeSpeed;
        slimeTurnRate.value = values.slimeTurnRate;
        slimeHealth.value = values.slimeHealth;
        slimeCount.value = values.slimeCount;

        standardDeviation.value = values.standardDeviation;
        mutationChance.value = values.mutationChance;
        typeMutationChance.value = values.typeMutationChance;


        //values = new InitialSlimeValues();


    }


    public void Update()
    {
        if (!toggleShow.isOn)
        {
            if (sliderBackground.activeInHierarchy)
            {
                sliderBackground.SetActive(false);
                descriptionBackground.SetActive(false);
            }

            return;
        }

        if (!sliderBackground.activeInHierarchy)
        {
            sliderBackground.SetActive(true);
            descriptionBackground.SetActive(true);
        }


        values.slimeCount = (int)slimeCount.value;
        values.sheepAttraction = sheepAttraction.value;
        values.towerAttraction = towerAttraction.value;
        values.slimeAttraction = slimeAttraction.value;
        values.wallAttraction = wallAttraction.value;

        values.towerViewRange = towerViewRange.value;
        values.slimeViewRange = slimeViewRange.value;
        values.wallViewRange = wallViewRange.value;

        values.slimeOptimalDistance = slimeOptimalDistance.value;

        values.slimeSpeed = slimeSpeed.value;
        values.slimeTurnRate = slimeTurnRate.value;
        values.slimeHealth = slimeHealth.value;


        if (values.GetHashCode() != manager.slimeValues.GetHashCode())
            manager.UpdateSlimeValues(values);

        //manager.slimeValues = values;


    }


}
