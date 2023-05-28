using System;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Profiling;

namespace ShepProject {

	public class QuadTree {

		public Vector3 origin;
		public float halfLength;
		public int positionCount;
		public NativeArray<ushort> objectIDs;
		public NativeArray<ushort> sortedObjectIDs;
		//public NativeArray<ushort> objectQuadIDs;
		public NativeList<ushort> deletions;
		public NativeArray<float2> positions;

		public AIManager enemyManager;
		private Transform[] transforms;
		public Transform[] Transforms => transforms;

		private TransformAccessArray transformAccess;
		public TransformAccessArray TransformAccess => transformAccess;


		public NativeParallelHashMap<QuadKey, Quad> quads;
		//public NativeArray<Quad> quadsList;


        public int maxNeighborQuads;
        public NativeArray<byte> neighborCounts;
        public NativeArray<QuadKey> objectNeighbors;


        private NativeArray<Quad> XQuads;
		private NativeArray<Quad> ZQuads;

		private NativeArray<int> lengths;

        int xQuadsLength { get => lengths[0]; set => lengths[0] = value; }
		//public int QuadsListLength => lengths[1];

		public short bucketSize;

		private NativeArray<bool> sorted;



        #region Debugging

        public void OnDrawGizmos()
		{


            //draw quads for debugging
            //issue with unity's code getting quads over certain amount, starts at about 150-160
            //might be from using 2021 instead of a newer version of unity, not sure
            NativeArray<Quad> q = quads.GetValueArray(Allocator.Temp);
            for (int i = 0; i < q.Length; i++)
            {

                DrawQuad(q[i], Color.white);



                //Handles.Label(pos, q[i].key.GetHeirarchyBinaryString());


            }
            q.Dispose();


            Vector3 pos = new Vector3(0, 1, 0);
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
			//for (int i = 0; i <= positionCount; i++) {

			//	position.x = positions[i].x;
			//	position.z = positions[i].y;

			//	Handles.Label(position, i.ToString());

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
        public QuadTree(int positionCount, short bucketSize) {

			objectIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
			sortedObjectIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
            //objectQuadIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
			deletions = new NativeList<ushort>(100, Allocator.Persistent);

			for (ushort i = 0; i < positionCount; i++) {

				objectIDs[i] = i;

			}

			positions = new NativeArray<float2>(positionCount, Allocator.Persistent);
			transforms = new Transform[positionCount];
			transformAccess = new TransformAccessArray(positionCount);


			maxNeighborQuads = 10;
			neighborCounts = new NativeArray<byte>(positionCount, Allocator.Persistent);
			objectNeighbors = new NativeArray<QuadKey>(positionCount * maxNeighborQuads, Allocator.Persistent);


			//quadsList = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			XQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			ZQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			quads = new NativeParallelHashMap<QuadKey, Quad>((int)((float)positionCount / bucketSize) * 4, Allocator.Persistent);
			
			sorted = new NativeArray<bool>(1, Allocator.Persistent);

			lengths = new NativeArray<int>(2, Allocator.Persistent);

			halfLength = 100;
			
			//start position count at -1 so it takes first slots
			this.positionCount = -1;

			this.bucketSize = bucketSize;


		}

		/// <summary>
		/// call this each frame to sort it based on newest positions
		/// </summary>
		public void Update() {



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
			if (XQuads[0].BucketSize <= bucketSize) {
				//quadsList[0] = XQuads[0];
				sorted[0] = true;
				lengths[1] = 1;

				for (ushort i = 0; i <= positionCount; i++) {

					//objectQuadIDs[objectIDs[i]] = 0;
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

            while (!sorted[0]) {

				SortIterationJob sij = new SortIterationJob();
				sij.objectPositions = positions;
				sij.objectIDs = objectIDs;
				sij.sortedObjectIDs = sortedObjectIDs;
				sij.readFrom = XQuads;
				sij.writeTo = ZQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = false;
				sij.quads = quads;
				//sij.Run(xQuadsLength);
				sij.Schedule(xQuadsLength, SystemInfo.processorCount).Complete();

				sij = new SortIterationJob();
				sij.objectPositions = positions;
				sij.objectIDs = objectIDs;
                sij.sortedObjectIDs = sortedObjectIDs;
                sij.readFrom = ZQuads;
				sij.writeTo = XQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = true;
				sij.quads = quads;
				//sij.Run(xQuadsLength * 2);
				sij.Schedule(xQuadsLength * 2, SystemInfo.processorCount).Complete();


				QuadFilteringJob fj = new QuadFilteringJob();
				fj.readFrom = XQuads;
				//fj.quadsList = quadsList;
				fj.objectIDs = objectIDs;
				//fj.quadsID = objectQuadIDs;
				fj.isSorted = sorted;
				fj.lengths = lengths;
				fj.bucketSize = bucketSize;	
				fj.Schedule().Complete();


			}

            AssignTypesJob asj = new AssignTypesJob();
            asj.objectIDs = objectIDs;
            asj.genes = enemyManager.Genes;
            asj.quads = quads;
			asj.size = assignTypesSize;
            asj.Schedule(assignTypesSize, 1).Complete();

			if (assignTypesSize > 1)
			{

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


        }

		public void NewFrame() {
			lengths[1] = 0;
			sorted[0] = false;
			quads.Clear();

			for (int i = 0; i < neighborCounts.Length; i++)
			{
				neighborCounts[i] = 0;

			}


			for (int i = 0; i < XQuads.Length; i++) {

				XQuads[i] = new Quad(-1, -1);
				ZQuads[i] = new Quad(-1, -1);
			}

			//for (int i = 0; i < quadsList.Length; i++) {

			//	quadsList[i] = new Quad();

			//}

			//for (int i = 0; i < objectQuadIDs.Length; i++) {

			//	objectQuadIDs[i] = ushort.MaxValue;

			//}

			//update the transform's positions
			//NullChecks();

		}


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

                if (enemyManager.Genes.GetObjectType(objectIDs[i]) != objectType)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objectID">ID of the object that is calling this function.</param>
        /// <param name="objectType">Type of the object that needs to be found.</param>
        /// <param name="minDist">minimum distance away the object can be.</param>
        /// <param name="maxDist">maximum distance away the object can be.</param>
        /// <returns>Transform of object if one is found within range, or null if no object is found within range.</returns>
        public Transform GetClosestObject(int objectID, ObjectType objectType, float minDist = 0, float maxDist = 10) {

			if (transforms[objectID] == null) {
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

        private int CheckChildQuadsInRange(QuadKey key, int objectID, ObjectType objectType, float minDist, float maxDist)
        {

            int closestID = -1;
            int temp = -1;
            QuadKey checkKey;

            if (!quads[key].key.IsDivided)
            {
                //search this quad for the required object
                return SearchQuadForObjectType(objectID, quads[key], objectType, minDist, maxDist * maxDist);

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

		public void QueueDeletion(ushort objectID)
		{
			deletions.Add(objectID);

		}

        public void ProcessDeletions()
		{
			deletions.Sort();

			for (int i = deletions.Length - 1; i >= 0 ; i--)
			{

				RemoveTransform(deletions[i]);

			}

			deletions.Clear();

		}

        public ushort AddTransform(Transform transform) {

			positionCount++;
			transforms[objectIDs[positionCount]] = transform;
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
			inactive.gameObject.SetActive(false);
			transforms[objectID] = null;



            ushort sorted = sortedObjectIDs[objectID];
			ushort overwrite = objectIDs[positionCount];
			//ushort overwriteSorted = sortedObjectIDs[objectIDs[positionCount]];



			objectIDs[sorted] = overwrite;
            objectIDs[positionCount] = objectID;

			//need to update the sortedIDs as well to make sure there are no errors

			sortedObjectIDs[objectIDs[positionCount]] = sortedObjectIDs[objectID];
			sortedObjectIDs[objectID] = (ushort)positionCount;


            positionCount--;
			return inactive;

		}

		private void ReadTransformData() {


			transformAccess.SetTransforms(transforms);
			ReadTransformsJob job = new ReadTransformsJob();
			job.positions = positions;
			job.maxIndex = positionCount;
			job.Schedule(transformAccess).Complete();


		}

		public void Dispose() {
			objectIDs.Dispose();
			sortedObjectIDs.Dispose();

			positions.Dispose();
			XQuads.Dispose();
			ZQuads.Dispose();
			sorted.Dispose();
			lengths.Dispose();
			//quadsList.Dispose();
			transformAccess.Dispose();
			//objectQuadIDs.Dispose();
			quads.Dispose();
			deletions.Dispose();


            neighborCounts.Dispose();
			objectNeighbors.Dispose();



		}



	}


}