using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace ShepProject
{

    public class EvolutionDataFileInfo
    {
        public SigmoidInfo[] info;
        public int waveNumber;

    }

    public enum ObjectType
    {
        None = -1,
        Slime,
        Tower,
        Player,
        Wall,
        Sheep,
        Count

    }

    public enum Genes
    {
        MainType,
        SecondaryType,
        MainResistance,
        SecondaryResistance,
        SlimeViewRange,
        TowerViewRange,
        PlayerViewRange,
        WallViewRange,
        SheepViewRange,
        SlimeAttraction,
        TowerAttraction,
        PlayerAttraction,
        WallAttraction,
        SheepAttraction,
        SlimeOptimalDistance,
        Speed,
        TurnRate,
        SprintDuration,
        SprintCooldown,
        Health,
        TotalGeneCount

    }

    public enum SlimeType
    {
        Player,
        Blaster,
        Fire,
        Acid,
        Lightning,
        Ice,
        Laser,
        Balanced,
        Count


    }

    public enum DamageType
    {

        Player,
        Blaster,
        Fire,
        Acid,
        Lightning,
        Ice,
        Laser,
        Count

    }


    [System.Serializable]
    public struct ChromosoneParents
    {

        public ushort parentOne;
        public ushort parentTwo;


    }


    [System.Serializable]
    public class SigmoidInfo
    {

        public string name;
        public Sigmoid sigmoid;


    }

    [System.Serializable]
    public struct Sigmoid
    {

        [SerializeField]
        public float magnitude;
        [SerializeField]
        public float slope;
        [SerializeField]
        public float verticalOffset;
        [SerializeField]
        public float horizontalOffset;

        public Sigmoid(float magnitude, float verticalOffset, float horizontalOffset, float slope)
        {

            this.magnitude = magnitude;
            this.slope = slope;
            this.horizontalOffset = horizontalOffset;
            this.verticalOffset = verticalOffset;

        }

        public float GetTraitValue(float geneValue)
        {

            return (magnitude / (1 + math.pow(math.E, -1 * ((slope * geneValue) + horizontalOffset)))) + verticalOffset;

        }


        //public override bool Equals(object obj)
        //{
        //    return obj is Sigmoid sigmoid &&
        //           magnitude == sigmoid.magnitude &&
        //           slope == sigmoid.slope &&
        //           verticalOffset == sigmoid.verticalOffset &&
        //           horizontalOffset == sigmoid.horizontalOffset;
        //}

        public override int GetHashCode()
        {
            return HashCode.Combine(magnitude, slope, verticalOffset, horizontalOffset);
        }





    }















}