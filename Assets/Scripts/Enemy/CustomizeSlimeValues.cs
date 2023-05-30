using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class InitialSlimeValues
{

    [Range(0, 1)]
    public float sheepAttraction;
    [Range(-1, 1)]
    public float towerAttraction;
    [Range(0, 1)]
    public float slimeAttraction;
    [Range(0, 1)]
    public float wallAttraction;

    [Range(0, 50)]
    public float towerViewRange;
    [Range(0, 50)]
    public float slimeViewRange;
    [Range(0, 50)]
    public float playerViewRange;
    [Range(0, 50)]
    public float wallViewRange;

    [Range(-1, 1)]
    public float slimeOptimalDistance;

    [Range(.1f, 100)]
    public float slimeSpeed;
    [Range(0, 1)]
    public float slimeTurnRate;
    [Range(1, 1000)]
    public float slimeHealth;



}



public class CustomizeSlimeValues : MonoBehaviour
{
    ////[Range(0, 1)] 
    //public float sheepAttraction;
    ////[Range(-1, 1)] 
    //public float towerAttraction;
    ////[Range(0, 1)] 
    //public float slimeAttraction;
    ////[Range(0, 1)] 
    //public float wallAttraction;

    //public float towerViewRange;
    //public float slimeViewRange;
    //public float playerViewRange;
    //public float wallViewRange;

    ////[Range(-1, 1)] 
    //public float slimeOptimalDistance;

    //public float slimeSpeed;
    ////[Range(0, 1)] 
    //public float slimeTurnRate;
    //public float slimeHealth;

    public AIManager manager;

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

        manager.slimeValues = values;


    }


}