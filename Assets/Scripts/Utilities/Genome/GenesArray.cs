using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace ShepProject
{


    public enum GeneGroups
    {

        Type,
        StatStartIndex = 1 // object type
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

    public enum ObjectType
    {

        Slime,
        Tower,
        Player,
        Wall,
        Sheep,
        Count

    }

    #endregion

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

    public struct GenesArray
    {

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> genes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ObjectType> objectTypes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ushort> geneIDs;

        private ushort objectCount;

        public ushort ObjectCount => objectCount;

        public GenesArray(int maxObjects, int genesPerObject, Allocator type)
        {

            genes = new NativeArray<float>(maxObjects * genesPerObject, type);
            objectTypes = new NativeArray<ObjectType>(maxObjects, type);
            geneIDs = new NativeArray<ushort>(maxObjects, type);

            for (int i = 0; i < geneIDs.Length; i++)
            {
                geneIDs[i] = ushort.MaxValue;

            }

            objectCount = 0;
        }



        #region Get Specific Gene Methods

        public void AddGenesToObject(ushort objectID)
        {
            geneIDs[objectID] = objectCount;
            objectCount++;
        }

        private int IDTypeIndex(int id)
        {

            if (geneIDs[id] == ushort.MaxValue)
            {
                Debug.LogError("Object hasn't been assigned.");
                return ushort.MaxValue;
            }

            return geneIDs[id] * (int)GeneGroups.TotalGeneCount;
        }

        public ObjectType GetObjectType(int id)
        {
            return objectTypes[id];
        }

        public void SetObjectType(int id, ObjectType type)
        {
            objectTypes[id] = type;
        }

        public float GetAttraction(int id, int attraction)
        {
            return genes[IDTypeIndex(id) + 1 + attraction];
        }

        public float GetAttraction(int id, ObjectType attraction)
        {

            return GetAttraction(id, (int)attraction);
        }
        public void SetAttraction(int id, ObjectType attraction, float value)
        {

            genes[IDTypeIndex(id) + 1 + (int)attraction] = value;

        }

        public float GetViewRange(int id, ViewRange range)
        {

            return genes[IDTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range];
        }

        public void SetViewRange(int id, ViewRange range, float value)
        {

            genes[IDTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range] = value;
        }

        public float GetResistance(int id, DamageType damageType)
        {

            return genes[IDTypeIndex(id) + (int)damageType];
        }

        public void SetResistance(int id, DamageType damageType, float value)
        {
            genes[IDTypeIndex(id) + (int)damageType] = value;
        }

        public float GetOptimalDistance(int id, OptimalDistance optimalDistance)
        {
            return genes[IDTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance];
        }

        public void SetOptimalDistance(int id, OptimalDistance optimalDistance, float value)
        {
            genes[IDTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance] = value;
        }

        public float GetSpeed(int id)
        {

            return genes[IDTypeIndex(id) + (int)GeneGroups.Speed];
        }

        public void SetSpeed(int id, float speed)
        {

            genes[IDTypeIndex(id) + (int)GeneGroups.Speed] = speed;
        }

        public float GetTurnRate(int id)
        {

            return genes[IDTypeIndex(id) + (int)GeneGroups.TurnRate];
        }

        public void SetTurnRate(int id, float value)
        {

            genes[IDTypeIndex(id) + (int)GeneGroups.TurnRate] = value;

        }

        public float GetHealth(int id)
        {
            int index = IDTypeIndex(id) + (int)GeneGroups.Health;

            return genes[index];
        }

        public void SetHealth(int id, float value)
        {
            int index = IDTypeIndex(id) + (int)GeneGroups.Health;
            genes[index] = value;

        }


        #endregion

        public void ResetIDGenes(int id)
        {

            int startIndex = id * (int)GeneGroups.TotalGeneCount;


            for (int i = startIndex; i < startIndex + (int)GeneGroups.TotalGeneCount; i++)
            {
                genes[i] = -1;

            }


        }

        /// <summary>
        /// When an object's ID is updated, need to call this method to update the information
        /// to still be correct in the array.
        /// </summary>
        /// <param name="readFrom"></param>
        /// <param name="writeTo"></param>
        public void TransferGenes(int readFrom, int writeTo)
        {

            int readStartIndex = readFrom * (int)GeneGroups.TotalGeneCount;
            int writeStartIndex = writeTo * (int)GeneGroups.TotalGeneCount;


            for (int i = 0; i < (int)GeneGroups.TotalGeneCount; i++)
            {
                genes[writeStartIndex + i] = genes[readStartIndex + i];
                genes[readStartIndex + i] = -1;

            }



        }




        public void Dispose()
        {
            genes.Dispose();
            geneIDs.Dispose();
            objectTypes.Dispose();
        
        }



    }

}