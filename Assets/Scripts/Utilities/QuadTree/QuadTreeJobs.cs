using Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

namespace ShepProject
{

    public struct SortIterationJob : IJobParallelFor
    {

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> objectIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> sortedObjectIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectPositions;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;

        //this is the list to process
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Quad> readFrom;

        //once done sorting, add the quad to the list
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Quad> writeTo;


        //public CommandBuilder builder;

        public bool zSort;
        public int bucketSize;

        public void Execute(int index)
        {

            //should only be at this point if the number of positions needed to be sorted
            //is greater than the maximum value allowed in a quad

            Quad readQuad = readFrom[index];

            Quad leftQuad = new Quad(-1, -1);
            Quad rightQuad = new Quad(-1, -1);

            short startIndex = readQuad.startIndex;
            short endIndex = readQuad.endIndex;

            if (startIndex < 0 || endIndex < 0)
            {

                startIndex = -1;
                endIndex = -1;

                if (!zSort)
                {

                    leftQuad.position = new float2((readQuad.position.x) - (readQuad.halfLength / 2f),
                        readQuad.position.y);
                    rightQuad.position = new float2((readQuad.position.x) + (readQuad.halfLength / 2f),
                        readQuad.position.y);

                    leftQuad.halfLength = readQuad.halfLength;
                    rightQuad.halfLength = readQuad.halfLength;

                    leftQuad.key = readQuad.key;
                    leftQuad.key.LeftBranch();

                    rightQuad.key = readQuad.key;
                    rightQuad.key.RightBranch();

                }

                if (zSort)
                {

                    leftQuad.position = new float2(readQuad.position.x,
                        (readQuad.position.y) - (readQuad.halfLength / 2f));
                    rightQuad.position = new float2(readQuad.position.x,
                        (readQuad.position.y) + (readQuad.halfLength / 2f));

                    leftQuad.halfLength = readQuad.halfLength / 2f;
                    rightQuad.halfLength = readQuad.halfLength / 2f;

                    leftQuad.key = readQuad.key;
                    leftQuad.key.LeftBranch();

                    rightQuad.key = readQuad.key;
                    rightQuad.key.RightBranch();

                    leftQuad.key.SetDivided(false);
                    rightQuad.key.SetDivided(false);


                    quads.Add(leftQuad.key, leftQuad);
                    quads.Add(rightQuad.key, rightQuad);


                    //Vector3 pos = new Vector3(0, 1, 0);
                    //pos.x = leftQuad.position.x;
                    //pos.z = leftQuad.position.y;

                    //builder.Label2D(pos, leftQuad.key.ToString(), Color.white);


                    //pos.x = rightQuad.position.x;
                    //pos.z = rightQuad.position.y;

                    //builder.Label2D(pos, rightQuad.key.ToString(), Color.white);

                }

                writeTo[index * 2] = leftQuad;
                writeTo[index * 2 + 1] = rightQuad;

                return;

            }


            while (startIndex < endIndex)
            {

                if (zSort)
                {

                    //since quad tree is 2d,
                    //using Y as the Z dimension to save memory

                    while (IsLessThanOrEqual(objectPositions[objectIDs[startIndex]].y, ref readQuad)
                        && startIndex < endIndex)
                    {

                        startIndex++;

                    }
                    while (IsGreaterThan(objectPositions[objectIDs[endIndex]].y, ref readQuad)
                        && endIndex > startIndex)
                    {
                        endIndex--;
                    }

                }
                else
                {

                    while (IsLessThanOrEqual(objectPositions[objectIDs[startIndex]].x, ref readQuad)
                        && startIndex < endIndex)
                    {

                        startIndex++;

                    }
                    while (IsGreaterThan(objectPositions[objectIDs[endIndex]].x, ref readQuad)
                        && endIndex > startIndex)
                    {
                        endIndex--;
                    }
                }


                if (startIndex < endIndex)
                {

                    ushort temp = objectIDs[startIndex];
                    objectIDs[startIndex] = objectIDs[endIndex];
                    objectIDs[endIndex] = temp;

                    startIndex++;
                    endIndex--;



                }

            }

            //the sorting has finished for this iteration,
            //add the quads to the list for further sorting if needed


            float value = 0;
            if (zSort)
            {
                value = objectPositions[objectIDs[startIndex]].y;
            }
            else
            {
                value = objectPositions[objectIDs[startIndex]].x;
            }

            if (IsLessThanOrEqual(value, ref readQuad))
            {
                //start index is lesser, so endIndex needs to be adjusted

                if (startIndex >= readQuad.endIndex)
                {
                    endIndex = -1;

                }
                else
                {
                    endIndex = (short)(startIndex + 1);
                }

            }
            else
            {
                //endIndex is greater

                if (endIndex <= readQuad.startIndex)
                {
                    startIndex = -1;

                }
                else
                {
                    startIndex = (short)(endIndex - 1);
                }

            }

            if (startIndex == -1)
            {

                leftQuad.startIndex = -1;
                leftQuad.endIndex = -1;
            }
            else
            {
                leftQuad.startIndex = readQuad.startIndex;
                leftQuad.endIndex = startIndex;
            }

            leftQuad.key = readQuad.key;
            leftQuad.key.LeftBranch();

            if (leftQuad.BucketSize > bucketSize)
            {
                leftQuad.key.SetDivided();
            }
            else
            {

                leftQuad.key.SetDivided(false);
                if (startIndex > -1)
                    SetSortedIndices(leftQuad);
            }


            if (endIndex == -1)
            {

                rightQuad.startIndex = -1;
                rightQuad.endIndex = -1;

            }
            else
            {
                rightQuad.startIndex = endIndex;
                rightQuad.endIndex = readQuad.endIndex;
            }

            rightQuad.key = readQuad.key;
            rightQuad.key.RightBranch();

            if (rightQuad.BucketSize > bucketSize)
            {
                rightQuad.key.SetDivided();
            }
            else
            {
                rightQuad.key.SetDivided(false);

                if (endIndex > -1)
                    SetSortedIndices(rightQuad);
            }


            if (zSort)
            {

                leftQuad.position = new float2(readQuad.position.x,
                    (readQuad.position.y) - (readQuad.halfLength / 2f));

                rightQuad.position = new float2(readQuad.position.x, 
                    (readQuad.position.y) + (readQuad.halfLength / 2f));

                leftQuad.halfLength = readQuad.halfLength / 2f;
                rightQuad.halfLength = readQuad.halfLength / 2f;


                quads.TryAdd(leftQuad.key, leftQuad);
                quads.TryAdd(rightQuad.key, rightQuad);

                //Vector3 pos = new Vector3(0, 1, 0);
                //pos.x = leftQuad.position.x;
                //pos.z = leftQuad.position.y;


                //if (!leftQuad.key.IsDivided)

                //    builder.Label2D(pos, leftQuad.key.ToString(), Color.blue);


                //pos.x = rightQuad.position.x;
                //pos.z = rightQuad.position.y;


                //if (!rightQuad.key.IsDivided)

                //    builder.Label2D(pos, rightQuad.key.ToString(), Color.blue);


            }

            else
            {
                leftQuad.position = new float2((readQuad.position.x) - (readQuad.halfLength / 2f),
                    readQuad.position.y);
                rightQuad.position = new float2((readQuad.position.x) + (readQuad.halfLength / 2f),
                    readQuad.position.y);

                leftQuad.halfLength = readQuad.halfLength;
                rightQuad.halfLength = readQuad.halfLength;
            }

            writeTo[index * 2] = leftQuad;
            writeTo[index * 2 + 1] = rightQuad;


        }

        private bool IsLessThanOrEqual(float value, ref Quad q)
        {

            if (value <= q.Middle(zSort))
            {
                return true;
            }

            return false;

        }

        private bool IsGreaterThan(float value, ref Quad q)
        {
            if (value > q.Middle(zSort))
            {
                return true;
            }
            return false;
        }

        private void SetSortedIndices(Quad quad)
        {

            for (short i = quad.startIndex; i <= quad.endIndex; i++)
            {

                sortedObjectIDs[objectIDs[i]] = (ushort)i;

            }


        }


    }

    public struct QuadFilteringJob : IJob
    {

        public NativeArray<Quad> readFrom;
        //public NativeArray<Quad> quadsList;

        public NativeArray<ushort> objectIDs;
        //public NativeArray<ushort> quadsID;

        public NativeArray<int> lengths;

        public NativeArray<bool> isSorted;

        public short bucketSize;

        public void Execute()
        {

            int startIndex = 0;
            int endIndex = (lengths[0] * 4) - 1;

            while (startIndex < endIndex)
            {

                while (!(readFrom[startIndex].startIndex < 0) && !(readFrom[startIndex].endIndex < 0)
                    && readFrom[startIndex].BucketSize > bucketSize)
                {

                    startIndex++;


                }


                while (((readFrom[endIndex].startIndex < 0) || (readFrom[endIndex].endIndex < 0)
                    || (readFrom[endIndex].BucketSize <= bucketSize)) && (endIndex >= startIndex && endIndex > 0))
                {


                    endIndex--;

                }


                //swap start and endindex

                if (startIndex >= endIndex)
                {
                    break;
                }
                else
                {
                    //since readfrom[StartIndex] is invalid
                    readFrom[startIndex] = readFrom[endIndex];
                    readFrom[endIndex] = new Quad(-1, -1);
                    startIndex++;
                    endIndex--;

                }


            }

            if (endIndex == 0 && readFrom[endIndex].BucketSize <= bucketSize)
            {
                isSorted[0] = true;

            }
            else
                lengths[0] = (endIndex + 1);

        }
    }

    public struct ReadTransformsJob : IJobParallelForTransform
    {

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;
        public int maxIndex;

        public void Execute(int index, TransformAccess transform)
        {

            if (!transform.isValid)
                return;

            positions[index] = new float2(transform.position.x, transform.position.z);

        }


    }

    public struct WriteTransformsJob : IJobParallelForTransform
    {

        public EvolutionStructure evolutionStructure;

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly]
        public NativeArray<ObjectType> objTypes;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> rotation;

        public void Execute(int index, TransformAccess transform)
        {

            if (objTypes[index] != ObjectType.Slime
                && objTypes[index] != ObjectType.Sheep)
            {
                return;
            }

            if (!transform.isValid)
                return;

            //sometimes gets NaN as a value for the rotation

            transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            Quaternion q = transform.rotation;

            q.eulerAngles = new Vector3(0, math.degrees(rotation[index]), 0);
            transform.rotation = q;


        }



    }

    public struct AssignTypesJob : IJob
    {


        /*
         * ways to cache quad info and whether they contain certain info
         *      top down and then assign as it goes back up
         * 
         * 
         * 
         * 
         * 
         * 
         */


        [ReadOnly]
        public NativeSlice<ushort> objectIDs;

        [ReadOnly]
        public NativeArray<ObjectType>.ReadOnly objectTypes;

        public NativeList<QuadKey> searchers;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;
        public byte positionIndex;

        //public CommandBuilder builder;

        public void Execute()
        {

            QuadKey current = new QuadKey();
            current.SetNextLevelPosition(positionIndex);

            searchers.AddNoResize(current);

            TraverseDownTree(quads[current].key);


        }

        //private void TraverseDownTreeNew()
        //{


        //    /*
        //     * 
        //     * traverse to bottom
        //     * search that quad
        //     * then search each of the quads in that level
        //     * if a quad is divided, add it to a list to search
        //     *      then skip searching it for now
        //     * 
        //     * then do entire level and then set higher level type contains
        //     * 
        //     * 
        //     */

        //    Quad quad = new Quad();
        //    ContainsTypes types = new ContainsTypes();
        //    //bool2 position = new bool2(false, false);
        //    byte positionIndex = 0;

        //    while (searchers.Length > 0)
        //    {

        //        positionIndex = 0;
        //        quad = quads[searchers[0]];
        //        searchers.RemoveAt(0);

        //        //traversing to bottom of tree
        //        while (quad.key.IsDivided)
        //        {
        //            //go all the way down to the bottom left corner check that one

        //            quad.key.SetNextLevelPosition(positionIndex);
        //            quad = quads[quad.key];


        //        }

        //        //searching the quad to check which types it contains
        //        //quad.containsTypes = new ContainsTypes();

        //        if (quad.startIndex >= 0)
        //        {

        //            for (int i = quad.startIndex; i <= quad.endIndex; i++)
        //            {
        //                quad.containsTypes[objectTypes[objectIDs[i]]] = true;

        //            }

        //            //float3 pos = new float3();
        //            //pos.x = quad.position.x;
        //            //pos.z = quad.position.y;
        //            //for (int i = 0; i < 5; i++)
        //            //{

        //            //    pos.z = quad.position.y - i;
        //            //    builder.Label2D(pos,
        //            //        (ObjectType)i + " " + quad.containsTypes[(ObjectType)i].ToString());

        //            //}

        //            //quad.position = quads[quad.key].position;
        //            quads[quad.key] = quad;
        //            types |= quad.containsTypes;

        //        }


        //        if (quad.key.GetCount() <= 2)
        //            continue;


        //        while (positionIndex < 4)
        //        {

        //            positionIndex++;

        //            quad.key.SetCurrentLevel(positionIndex);
        //            quad = quads[quad.key];

        //            if (quad.startIndex < 0)
        //                continue;

        //            if (quad.key.IsDivided)
        //            {
        //                searchers.AddNoResize(quad.key);

        //            }
        //            else
        //            {

        //                //searching the quad to check which types it contains
        //                //quad.containsTypes = new ContainsTypes();



        //                for (int i = quad.startIndex; i <= quad.endIndex; i++)
        //                {
        //                    quad.containsTypes[objectTypes[objectIDs[i]]] = true;

        //                }


        //                //float3 pos = new float3();
        //                //pos.x = quad.position.x;
        //                //pos.z = quad.position.y;

        //                //for (int i = 0; i < 5; i++)
        //                //{

        //                //    pos.z = quad.position.y - (i / 2f);
        //                //    builder.Label2D(pos,
        //                //        (ObjectType)i + " " + quad.containsTypes[(ObjectType)i].ToString());

        //                //}


        //                //quad.position = quads[quad.key].position;
        //                quads[quad.key] = quad;
        //                types |= quad.containsTypes;

        //            }

        //        }

        //        //now traverse up to the top of the tree
        //        //and assign all the types the quads contain
        //        //TraverseToTop(quad.key, types);

        //        while (quad.key.GetCount() > 2)
        //        {
        //            quad = quads[quad.key.GetParentKey()];
        //            quad.containsTypes |= types;

        //            //quad.position = quads[quad.key].position;
        //            quads[quad.key] = quad;


        //        }


        //    }



        //}

        //private void TraverseToTop(QuadKey childKey, ContainsTypes contains)
        //{

        //    Quad current = quads[childKey];




        //}


        private ContainsTypes TraverseDownTree(QuadKey parentKey)
        {

            if (!quads[parentKey].key.IsDivided)
            {
                if (quads[parentKey].startIndex < 0)
                    return new ContainsTypes();

                return SearchQuadForTypes(parentKey);
            }

            Quad current = quads[parentKey];
            current.ContainsTypes = new ContainsTypes();

            //top left quad
            current.key = parentKey;
            current.key.LeftBranch();
            current.key.RightBranch();
            current.ContainsTypes |= TraverseDownTree(quads[current.key].key);

            current.key = parentKey;
            current.key.LeftBranch();
            current.key.LeftBranch();
            current.ContainsTypes |= TraverseDownTree(quads[current.key].key);

            current.key = parentKey;
            current.key.RightBranch();
            current.key.LeftBranch();
            current.ContainsTypes |= TraverseDownTree(quads[current.key].key);

            current.key = parentKey;
            current.key.RightBranch();
            current.key.RightBranch();
            current.ContainsTypes |= TraverseDownTree(quads[current.key].key);

            //then from all those, assign the values here, then return another bool5 or w/e
            current.key = parentKey;

            quads[parentKey] = current;

            return current.ContainsTypes;
        }


        private ContainsTypes SearchQuadForTypes(QuadKey key)
        {
            Quad current = quads[key];
            current.containsTypes = new ContainsTypes();

            for (int i = quads[key].startIndex; i <= quads[key].endIndex; i++)
            {
                current.containsTypes[objectTypes[objectIDs[i]]] = true;

            }

            quads[key] = current;

            return current.containsTypes;

        }


    }


}