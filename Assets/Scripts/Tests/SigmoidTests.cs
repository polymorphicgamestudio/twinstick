using Drawing;
using ShepProject;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class SigmoidTests : MonoBehaviour
{
	
    public Sigmoid sigmoid;

    public SigmoidInfo sigmoidInfo;

    [Range(.01f, 2f)]
    public float pointSpace;

    public int2 range;
    public float geneValue;
    public float traitValue;


    private void OnValidate()
    {

        traitValue = sigmoid.GetTraitValue(geneValue);

    }

    // Update is called once per frame
    void Update()
    {


        float min = range.x;
        float max = range.y;

        Draw.Line(new float3(range.x, 0, 0), new float3(range.y, 0, 0), Color.cyan);

        traitValue = sigmoid.GetTraitValue(geneValue);

        while (min < max)
        {
            Drawing.Draw.SolidCircle(new float3(min, sigmoid.GetTraitValue(min), 0), new float3(0,0,1), pointSpace / 2f);
            min += pointSpace;

        }

    }
}
