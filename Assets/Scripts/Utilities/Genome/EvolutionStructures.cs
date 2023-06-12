using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;


namespace ShepProject
{

    public enum GeneGroups
    {

        MainType,
        SecondaryType,
        StatStartIndex = 2 // Main Type and Secondary Type
            + (int)ObjectType.Count //for all the possible attractions an object can have
            + (int)ViewRange.Count
            + (int)DamageType.Count
            + (int)OptimalDistance.Count,
        Speed,
        TurnRate,
        Health,
        TotalGeneCount

    }

    #region Same ordering needed for these enums

    public enum ViewRange
    {
        Slime,
        Tower,
        Player,
        Wall,
        Count
    }

    #endregion

    public enum SlimeType
    {
        Blaster,
        Fire,
        Acid,
        Lightning,
        Ice,
        Laser,
        Count


    }

    public enum OptimalDistance
    {
        Slime,
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
    public struct Sigmoid
    {

        [SerializeField]
        private float magnitude;
        [SerializeField]
        private float slope;
        [SerializeField]
        private float verticalOffset;
        [SerializeField]
        private float horizontalOffset;

        public Sigmoid(float magnitude, float verticalOffset, float horizontalOffset, float slope)
        {

            this.magnitude = magnitude;
            this.slope = slope;
            this.horizontalOffset = horizontalOffset;
            this.verticalOffset = verticalOffset;

        }

        public float GetTraitValue(float geneValue)
        {

            return (magnitude / (1 + math.pow(math.E, (slope * geneValue) + horizontalOffset))) + verticalOffset;

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