using Drawing;
using System;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace ShepProject
{




    public class QuadTree
    {

        public Vector3 origin;
        public float halfLength;
        public int positionCount;
        public NativeArray<ushort> objectIDs;
        public NativeArray<ushort> sortedObjectIDs;

        public NativeList<ushort> deletions;
        public NativeArray<float2> positions;

        public NativeHashMap<int, ushort> objectIDsFromInstanceID;

        public NPCManager npcManager;
        private Transform[] transforms;
        public Transform[] Transforms => transforms;

        private TransformAccessArray transformAccess;
        public TransformAccessArray TransformAccess => transformAccess;


        public NativeParallelHashMap<QuadKey, Quad> quads;
        public NativeArray<ObjectType> objTypes;

        private NativeArray<Quad> XQuads;
        private NativeArray<Quad> ZQuads;

        private NativeArray<int> lengths;

        int xQuadsLength { get => lengths[0]; set => lengths[0] = value; }

        public short bucketSize;

        private NativeArray<bool> sorted;



        #region Debugging


        public void DrawGizmos()
        {


            //draw quads for debugging
            //issue with unity's code getting quads over certain amount, starts at about 150-160
            //might be from using 2021 instead of a newer version of unity, not sure
            NativeArray<QuadKey> q = quads.GetKeyArray(Allocator.Temp);
            Vector3 pos = new Vector3(0, 2, 0);
            for (int i = 0; i < q.Length; i++)
            {

                DrawQuad(quads[q[i]], Color.white);
                pos.x = quads[q[i]].position.x;
                pos.z = quads[q[i]].position.y;

                if (!q[i].IsDivided)

                    Draw.Label2D(pos + new Vector3(0, UnityEngine.Random.value * .5f, 0),
                        quads[q[i]].key.ToString(), 10,// + " Bucket: " + quads[q[i]].BucketSize + " Pos: " + pos,
                        Color.cyan);


            }
            q.Dispose();



            Vector3 scale = new Vector3(.5f, .5f, .5f);


            //for debugging neighbors

            //Quad quad = quadsList[objectQuadIDs[1]];
            //         pos.x = quad.position.x;
            //         pos.z = quad.position.y;
            //         //DrawQuad();
            //         Gizmos.DrawCube(pos, scale);


            //         for (int i = maxNeighborQuads; i < maxNeighborQuads + neighborCounts[1]; i++)
            //         {
            //             quad = quads[objectNeighbors[i]];

            //             pos.x = quad.position.x;
            //	pos.y = 2;
            //             pos.z = quad.position.y;
            //             //DrawQuad();
            //             Gizmos.DrawCube(pos, scale);

            //         }


            //Vector3 position = new Vector3();
            //for (int i = 0; i <= positionCount; i++)
            //{

            //	position.x = positions[i].x;
            //	position.z = positions[i].y;

            //	Drawing.Draw.Label2D(position, i.ToString());

            //}





        }

        private void DrawQuad(Quad q, Color c)
        {
            float3 pos = new float3(q.position.x, 1, q.position.y);
            float3 half = new float3(q.halfLength, 0, q.halfLength);
            float hl = q.halfLength;

            //top left to top right
            Debug.DrawLine(pos + new float3(-hl, 0, hl), pos + half);

            //top right to bottom right
            Debug.DrawLine(pos + half, pos + new float3(hl, 0, -hl));

            half.z *= -1;

            //bottom right to bottom left
            Debug.DrawLine(pos + half, pos + new float3(-hl, 0, -hl));


            half.x *= -1;
            //bottom left to top left

            Debug.DrawLine(pos + half, pos + new float3(-hl, 0, hl));
        }

        #endregion

        /// <summary>
        /// Create this with maximum number of positions that will be sorted for better performance.
        /// </summary>
        /// <param name="positionCount"></param>
        public QuadTree(int positionCount, short bucketSize, Allocator type = Allocator.Persistent)
        {

            objectIDs = new NativeArray<ushort>(positionCount, type);
            sortedObjectIDs = new NativeArray<ushort>(positionCount, type);
            deletions = new NativeList<ushort>(100, type);
            objectIDsFromInstanceID = new NativeHashMap<int, ushort>(positionCount, type);

            for (ushort i = 0; i < positionCount; i++)
            {

                objectIDs[i] = i;

            }

            positions = new NativeArray<float2>(positionCount, type);
            transforms = new Transform[positionCount];
            transformAccess = new TransformAccessArray(positionCount);


            XQuads = new NativeArray<Quad>((positionCount / bucketSize), type);
            ZQuads = new NativeArray<Quad>(XQuads.Length, type);
            quads = new NativeParallelHashMap<QuadKey, Quad>((int)((float)positionCount / bucketSize) * 5, type);
            objTypes = new NativeArray<ObjectType>(positionCount, type);

            sorted = new NativeArray<bool>(1, type);

            lengths = new NativeArray<int>(2, type);

            halfLength = 90;

            //start position count at -1 so it takes first slots
            this.positionCount = -1;

            this.bucketSize = bucketSize;


        }

        /// <summary>
        /// call this each frame to sort it based on newest positions
        /// </summary>
        public void Update()
        {

            //update positions from transforms
            ReadTransformData();
            int assignTypesSize = 0;
            Quad first = new Quad();
            first.startIndex = 0;
            first.endIndex = (short)(positionCount);
            first.position = new float2(origin.x, origin.z);
            first.halfLength = halfLength;

            XQuads[0] = first;
            xQuadsLength = 1;
            lengths[1] = 0;
            if (XQuads[0].BucketSize <= bucketSize)
            {

                sorted[0] = true;
                lengths[1] = 1;

                for (ushort i = 0; i <= positionCount; i++)
                {

                    sortedObjectIDs[objectIDs[i]] = i;
                }
                assignTypesSize = 1;

            }
            else
            {
                first.key.SetDivided();
                assignTypesSize = 4;
            }

            quads.Add(first.key, first);

            while (!sorted[0])
            {

                SortIterationJob sij = new SortIterationJob();
                sij.objectPositions = positions;
                sij.objectIDs = objectIDs;
                sij.sortedObjectIDs = sortedObjectIDs;
                sij.readFrom = XQuads;
                sij.writeTo = ZQuads;
                sij.bucketSize = bucketSize;
                sij.zSort = false;
                sij.zSort = false;
                sij.quads = quads;

                //sij.builder = DrawingManager.GetBuilder();
                sij.Run(xQuadsLength);
                //sij.Schedule(xQuadsLength, SystemInfo.processorCount).Complete();

                //sij.builder.Dispose();

                sij = new SortIterationJob();
                sij.objectPositions = positions;
                sij.objectIDs = objectIDs;
                sij.sortedObjectIDs = sortedObjectIDs;
                sij.readFrom = ZQuads;
                sij.writeTo = XQuads;
                sij.bucketSize = bucketSize;
                sij.zSort = true;
                sij.quads = quads;

                //sij.builder = DrawingManager.GetBuilder();
                sij.Run(xQuadsLength * 2);
                //sij.Schedule(xQuadsLength * 2, SystemInfo.processorCount).Complete();


                //sij.builder.Dispose();


                QuadFilteringJob fj = new QuadFilteringJob();
                fj.readFrom = XQuads;
                fj.objectIDs = objectIDs;
                fj.isSorted = sorted;
                fj.lengths = lengths;
                fj.bucketSize = bucketSize;
                fj.Schedule().Complete();


            }


            if (assignTypesSize > 1)
            {

                NativeArray<AssignTypesJob> jobs
                    = new NativeArray<AssignTypesJob>(assignTypesSize, Allocator.TempJob);

                NativeArray<JobHandle> handles
                    = new NativeArray<JobHandle>(assignTypesSize, Allocator.TempJob);

                for (byte i = 0; i < assignTypesSize; i++)
                {
                    AssignTypesJob asj = new AssignTypesJob();
                    asj.objectIDs = objectIDs.Slice(0, positionCount + 1);
                    asj.objectTypes = objTypes.AsReadOnly();
                    asj.quads = quads;
                    asj.positionIndex = i;
                    asj.searchers = new NativeList<QuadKey>(200, Allocator.TempJob);
                    //asj.builder = DrawingManager.GetBuilder();

                    jobs[i] = asj;
                    //asj.Run();
                    handles[i] = asj.Schedule();
                    //asj.Schedule(assignTypesSize, 1).Complete();
                }

                for (int i = 0; i < handles.Length; i++)
                {

                    handles[i].Complete();
                    jobs[i].searchers.Dispose();
                    //jobs[i].builder.Dispose();
                }


                jobs.Dispose();
                handles.Dispose();


                Quad top = quads[new QuadKey()];
                QuadKey key = new QuadKey();
                key.LeftBranch();
                key.RightBranch();

                top.ContainsTypes = quads[key].ContainsTypes;

                key = new QuadKey();
                key.LeftBranch();
                key.LeftBranch();

                top.ContainsTypes |= quads[key].ContainsTypes;

                key = new QuadKey();
                key.RightBranch();
                key.RightBranch();

                top.ContainsTypes |= quads[key].ContainsTypes;

                key = new QuadKey();
                key.RightBranch();
                key.LeftBranch();

                top.ContainsTypes |= quads[key].ContainsTypes;

                quads[new QuadKey()] = top;





            }
            else
            {
                Quad quad = quads[new QuadKey()];

                for (int i = quads[quad.key].startIndex; i <= quads[quad.key].endIndex; i++)
                {
                    quad.containsTypes[objTypes[objectIDs[i]]] = true;

                }

                quads[quad.key] = quad;

            }

            //DrawGizmos();

        }

        public void NewFrame()
        {
            lengths[1] = 0;
            sorted[0] = false;
            quads.Clear();


            for (int i = 0; i < XQuads.Length; i++)
            {

                XQuads[i] = new Quad(-1, -1);
                ZQuads[i] = new Quad(-1, -1);
            }

        }


        #region Object Searching

        private int SearchQuadForObjectType(int objectID, Quad current, ObjectType objectType, float minSquareDist = 0, float maxSquareDist = 10)
        {

            if (current.Empty)
                return -1;

            int closestIndex = -1;
            float tempSqDist = 0;
            float sqDist = 1000000;
            float2 local = new float2();

            for (int i = current.startIndex; i <= current.endIndex; i++)
            {

                if (objTypes[objectIDs[i]] != objectType)
                    continue;

                local = (positions[objectIDs[i]] - positions[objectID]);
                tempSqDist = (local.x * local.x) + (local.y * local.y);

                if (tempSqDist < minSquareDist || tempSqDist > maxSquareDist)
                    continue;

                if (tempSqDist > sqDist)
                    continue;


                sqDist = tempSqDist;

                closestIndex = objectIDs[i];
            }


            //doesn't contain this type of item
            return closestIndex;

        }

        private int SearchQuadForVisibleObjectType(int objectID, Quad current, ObjectType objectType, float minDist = 0, float maxDist = 10)
        {

            if (current.Empty)
                return -1;

            int closestIndex = -1;
            float tempSqDist = 0;
            float minSquareDist = minDist * minDist;
            float maxSquareDist = maxDist * maxDist;
            float sqDist = 1000000;
            float2 local = new float2();

            for (int i = current.startIndex; i <= current.endIndex; i++)
            {

                if (objTypes[objectIDs[i]] != objectType)
                    continue;

                local = (positions[objectIDs[i]] - positions[objectID]);
                tempSqDist = (local.x * local.x) + (local.y * local.y);

                if (tempSqDist < minSquareDist || tempSqDist > maxSquareDist)
                    continue;

                if (!Physics.Raycast(new Ray(transforms[objectID].position + Vector3.up * .25f, new float3(local.x, 0, local.y)),
                    out RaycastHit info, maxDist, LayerMask.GetMask(objectType.ToString())))
                {
                    continue;
                }

                if (tempSqDist > sqDist)
                    continue;

                sqDist = tempSqDist;

                closestIndex = objectIDs[i];
            }


            //doesn't contain this type of item
            return closestIndex;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectID">ID of the object that is calling this function.</param>
        /// <param name="objectType">Type of the object that needs to be found.</param>
        /// <param name="minDist">minimum distance away the object can be.</param>
        /// <param name="maxDist">maximum distance away the object can be.</param>
        /// <returns>Transform of object if one is found within range, or null if no object is found within range.</returns>
        public Transform GetClosestObject(int objectID, ObjectType objectType, float minDist = 0, float maxDist = 10)
        {

            if (transforms[objectID] == null)
            {
                Debug.LogError("Quad tree doesn't contain the object you are trying to search with!");
                return null;
                //throw new ArgumentException("Object ID does not exist in the quad tree! :(");
            }

            QuadKey topLevelKey = new QuadKey();
            if (positionCount > bucketSize)
                topLevelKey.SetDivided(true);

            Quad topLevelQuad = quads[topLevelKey];

            int closestObjectID = CheckChildQuadsInRange(topLevelKey, objectID, objectType, minDist, maxDist);

            if (closestObjectID == -1)
                return null;

            return transforms[closestObjectID];

        }

        public Transform GetClosestVisibleObject(int objectID, ObjectType objectType, float minDist = 0, float maxDist = 10)
        {
            QuadKey topLevelKey = new QuadKey();
            if (positionCount > bucketSize)
                topLevelKey.SetDivided(true);

            Quad topLevelQuad = quads[topLevelKey];

            int closestObjectID = CheckChildQuadsInRange(topLevelKey, objectID, objectType, minDist, maxDist, true);

            if (closestObjectID == -1)
                return null;


            return null;
        }

        public Transform GetClosestObjectByPathing(int objectID, ObjectType objectType, float minDist = 0, float maxDist = 10)
        {
            //need reference to pathfinding manager and read cached path costs for that object.

            return null;
        }

        private int CheckWhichObjectIsCloser(int objectID, int one, int two)
        {

            if (two == -1)
                return one;

            if (one == -1)
                return two;

            if (math.distancesq(positions[one], positions[objectID]) < math.distancesq(positions[two], positions[objectID]))
                return one;
            else
                return two;


        }

        private int CheckChildQuadsInRange(QuadKey key, int objectID, ObjectType objectType, float minDist, float maxDist, bool searchVisible = false)
        {

            int closestID = -1;
            int temp = -1;
            QuadKey checkKey;

            if (!quads[key].key.IsDivided)
            {

                if (searchVisible)
                    return SearchQuadForVisibleObjectType(objectID, quads[key], objectType, minDist, maxDist);
                //search this quad for the required object
                return SearchQuadForObjectType(objectID, quads[key], objectType, minDist * minDist, maxDist * maxDist);

            }


            void QuadContains()
            {
                if (quads[checkKey].IsWithinDistance(positions[objectID], maxDist))
                {
                    temp = CheckChildQuadsInRange(quads[checkKey].key, objectID, objectType, minDist, maxDist);

                    closestID = CheckWhichObjectIsCloser(objectID, closestID, temp);

                }
            }

            checkKey = key;
            checkKey.LeftBranch();
            checkKey.RightBranch();

            QuadContains();

            checkKey = key;
            checkKey.LeftBranch();
            checkKey.LeftBranch();

            QuadContains();

            checkKey = key;
            checkKey.RightBranch();
            checkKey.LeftBranch();

            QuadContains();

            checkKey = key;
            checkKey.RightBranch();
            checkKey.RightBranch();

            QuadContains();


            return closestID;

        }

        #endregion

        #region Add/Remove Related

        public void QueueDeletion(ushort objectID)
        {
            deletions.Add(objectID);

        }

        public void ProcessDeletions()
        {
            deletions.Sort();

            for (int i = deletions.Length - 1; i >= 0; i--)
            {

                RemoveTransform(deletions[i]).gameObject.SetActive(false);

            }

            deletions.Clear();

        }

        public ushort AddTransform(Transform transform, ObjectType objType)
        {

            positionCount++;
            transforms[objectIDs[positionCount]] = transform;
            objTypes[objectIDs[positionCount]] = objType;
            objectIDsFromInstanceID.Add(transform.gameObject.GetInstanceID(), objectIDs[positionCount]);

            return objectIDs[positionCount];

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectID"></param>
        /// <param name="replacementOldID"></param>
        /// <returns></returns>
        public Transform RemoveTransform(ushort objectID)
        {


            Transform inactive = transforms[objectID];
            objectIDsFromInstanceID.Remove(inactive.gameObject.GetInstanceID());

            //inactive.gameObject.SetActive(false);
            transforms[objectID] = null;



            ushort sorted = sortedObjectIDs[objectID];
            ushort overwrite = objectIDs[positionCount];
            //ushort overwriteSorted = sortedObjectIDs[objectIDs[positionCount]];

            sortedObjectIDs[objectIDs[positionCount]] = sortedObjectIDs[objectID];
            sortedObjectIDs[objectID] = (ushort)positionCount;

            objectIDs[sorted] = overwrite;
            objectIDs[positionCount] = objectID;
            //objTypes[objectIDs[sorted]] = ObjectType.None;
            //need to update the sortedIDs as well to make sure there are no errors



            positionCount--;
            return inactive;

        }

        #endregion

        private void ReadTransformData()
        {


            transformAccess.SetTransforms(transforms);
            ReadTransformsJob job = new ReadTransformsJob();
            job.positions = positions;
            job.maxIndex = positionCount;
            job.Schedule(transformAccess).Complete();


        }

        public void Dispose()
        {
            objectIDs.Dispose();
            sortedObjectIDs.Dispose();

            positions.Dispose();

            XQuads.Dispose();
            ZQuads.Dispose();

            sorted.Dispose();
            lengths.Dispose();
            transformAccess.Dispose();
            quads.Dispose();
            deletions.Dispose();
            objTypes.Dispose();
            objectIDsFromInstanceID.Dispose();


        }



    }


}