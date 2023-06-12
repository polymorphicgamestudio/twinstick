using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;


namespace ShepProject
{


    public struct TraitStructure
    {

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> traits;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> genes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ushort> ids;

        [NativeDisableContainerSafetyRestriction]
        private NativeList<ushort> availables;

        private NativeArray<float> previousGenes;

        private NativeArray<Sigmoid> sigmoids;


        public TraitStructure(int maxObjects, int genesPerObject, Sigmoid[] sigmoids, Allocator type = Allocator.Persistent)
        {

            traits = new NativeArray<float>(maxObjects * genesPerObject, type);
            genes = new NativeArray<float>(maxObjects * genesPerObject, type);

            ids = new NativeArray<ushort>(maxObjects, type);
            availables = new NativeList<ushort>(maxObjects, type);

            previousGenes = new NativeArray<float>(maxObjects * genesPerObject, type);


            //need a way to set these 
            this.sigmoids = new NativeArray<Sigmoid>(sigmoids, type);


            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = ushort.MaxValue;

            }
            for (int i = 0; i < availables.Capacity; i++)
            {
                availables.AddNoResize((ushort)((availables.Capacity) - i));

            }




        }


        private int ObjectTypeIndex(int id)
        {

            if (ids[id] == ushort.MaxValue)
            {
                Debug.LogError("Object " + id + " hasn't been assigned.");
                return ids[id];
            }

            return ids[id] * (int)GeneGroups.TotalGeneCount;
        }

        public void AddGenesToObject(ushort objectID)
        {


            ids[objectID] = availables[availables.Length - 1];
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

            availables.AddNoResize(ids[id]);


            //need to implement this again

            //objectTypes[id] = ObjectType.None;
            ids[id] = ushort.MaxValue;
        }

        public void Dispose()
        {
            traits.Dispose();
            genes.Dispose();
            ids.Dispose();
            availables.Dispose();

            previousGenes.Dispose();
            sigmoids.Dispose();

        }








        #region Specific Trait Methods








        #endregion

    }






}
