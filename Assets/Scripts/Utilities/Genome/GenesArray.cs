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
        None = -1,
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

        public NativeArray<ObjectType>.ReadOnly ObjectTypes => objectTypes.AsReadOnly();

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ushort> geneIDs;

        [NativeDisableContainerSafetyRestriction]
        private NativeList<ushort> availables;



        public GenesArray(int maxObjects, int genesPerObject, Allocator type)
        {

            genes = new NativeArray<float>(maxObjects * genesPerObject, type);
            objectTypes = new NativeArray<ObjectType>(maxObjects, type);
            geneIDs = new NativeArray<ushort>(maxObjects, type);
            availables = new NativeList<ushort>(maxObjects, type);

            for (int i = 0; i < geneIDs.Length; i++)
            {
                geneIDs[i] = ushort.MaxValue;

            }
            for (int i = 0; i < availables.Capacity; i++)
            {
                availables.AddNoResize((ushort)((availables.Capacity) - i));

            }

        }



        #region Get Specific Gene Methods

        private int ObjectTypeIndex(int id)
        {

            if (geneIDs[id] == ushort.MaxValue)
            {
                Debug.LogError("Object " + id + " hasn't been assigned.");
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
            return genes[ObjectTypeIndex(id) + 1 + attraction];
        }

        public float GetAttraction(int id, ObjectType attraction)
        {

            return GetAttraction(id, (int)attraction);
        }
        public void SetAttraction(int id, ObjectType attraction, float value)
        {

            genes[ObjectTypeIndex(id) + 1 + (int)attraction] = value;

        }

        public float GetViewRange(int id, ViewRange range)
        {

            return genes[ObjectTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range];
        }

        public void SetViewRange(int id, ViewRange range, float value)
        {

            genes[ObjectTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range] = value;
        }

        public float GetResistance(int id, DamageType damageType)
        {

            return genes[ObjectTypeIndex(id) + (int)damageType];
        }

        public void SetResistance(int id, DamageType damageType, float value)
        {
            genes[ObjectTypeIndex(id) + (int)damageType] = value;
        }

        public float GetOptimalDistance(int id, OptimalDistance optimalDistance)
        {
            return genes[ObjectTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance];
        }

        public void SetOptimalDistance(int id, OptimalDistance optimalDistance, float value)
        {
            genes[ObjectTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance] = value;
        }

        public float GetSpeed(int id)
        {

            return genes[ObjectTypeIndex(id) + (int)GeneGroups.Speed];
        }

        public void SetSpeed(int id, float speed)
        {

            genes[ObjectTypeIndex(id) + (int)GeneGroups.Speed] = speed;
        }

        public float GetTurnRate(int id)
        {

            return genes[ObjectTypeIndex(id) + (int)GeneGroups.TurnRate];
        }

        public void SetTurnRate(int id, float value)
        {

            genes[ObjectTypeIndex(id) + (int)GeneGroups.TurnRate] = value;

        }

        public float GetHealth(int id)
        {

            return genes[ObjectTypeIndex(id) + (int)GeneGroups.Health];
        }

        public void SetHealth(int id, float value)
        {

            genes[ObjectTypeIndex(id) + (int)GeneGroups.Health] = value;

        }


        #endregion

        public void AddGenesToObject(ushort objectID)
        {
            //availableIDsEndIndex++;
            //geneIDs[objectID] = availables[availableIDsEndIndex];

            geneIDs[objectID] = availables[availables.Length - 1];
            availables.RemoveAt(availables.Length - 1);

        }


        public void ResetIDGenes(int id)
        {

            for (int i = ObjectTypeIndex(id);
                i < ObjectTypeIndex(id) + (int)GeneGroups.TotalGeneCount; i++)
            {
                genes[i] = -1;

            }

            //need to add the returned ID to the end
            //and then the id at the end position to where this one was

            availables.AddNoResize(geneIDs[id]);


            objectTypes[id] = ObjectType.None;
            geneIDs[id] = ushort.MaxValue;
        }

        public void Dispose()
        {
            genes.Dispose();
            geneIDs.Dispose();
            objectTypes.Dispose();
            availables.Dispose();
        }



    }

}