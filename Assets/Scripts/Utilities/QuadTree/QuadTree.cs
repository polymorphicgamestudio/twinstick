using System;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
		public NativeArray<ushort> objectQuadIDs;
		public NativeArray<float2> positions;

		public EnemyManager enemyManager;
		private Transform[] transforms;
		public Transform[] Transforms => transforms;

		private TransformAccessArray transformAccess;
		public TransformAccessArray TransformAccess => transformAccess;


		public NativeParallelHashMap<QuadKey, Quad> quads;
		public NativeArray<Quad> quadsList;


        public int maxNeighborQuads;
        public NativeArray<byte> neighborCounts;
        public NativeArray<QuadKey> objectNeighbors;


        private NativeArray<Quad> XQuads;
		private NativeArray<Quad> ZQuads;

		private NativeArray<int> lengths;

		int xQuadsLength { get => lengths[0]; set => lengths[0] = value; }
		public int QuadsListLength => lengths[1];

		public ushort bucketSize;

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

			Quad quad = quadsList[objectQuadIDs[1]];
            pos.x = quad.position.x;
            pos.z = quad.position.y;
            //DrawQuad();
            Gizmos.DrawCube(pos, scale);


            for (int i = maxNeighborQuads; i < maxNeighborQuads + neighborCounts[1]; i++)
            {
                quad = quads[objectNeighbors[i]];

                pos.x = quad.position.x;
				pos.y = 2;
                pos.z = quad.position.y;
                //DrawQuad();
                Gizmos.DrawCube(pos, scale);

            }


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
        public QuadTree(int positionCount, ushort bucketSize) {

			objectIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
			sortedObjectIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
            objectQuadIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);

			for (ushort i = 0; i < positionCount; i++) {

				objectIDs[i] = i;

			}

			positions = new NativeArray<float2>(positionCount, Allocator.Persistent);
			transforms = new Transform[positionCount];
			transformAccess = new TransformAccessArray(positionCount);


			maxNeighborQuads = 10;
			neighborCounts = new NativeArray<byte>(positionCount, Allocator.Persistent);
			objectNeighbors = new NativeArray<QuadKey>(positionCount * maxNeighborQuads, Allocator.Persistent);


			quadsList = new NativeArray<Quad>(positionCount, Allocator.Persistent);
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

			Quad first = new Quad();
			first.startIndex = 0;
			first.endIndex = (short)(positionCount);
			first.position = new float2(origin.x, origin.z);
			first.halfLength = halfLength;

			XQuads[0] = first;
			xQuadsLength = 1;
			lengths[1] = 0;
			if (XQuads[0].BucketSize <= bucketSize) {
				quadsList[0] = XQuads[0];
				sorted[0] = true;
				lengths[1] = 1;

				for (int i = 0; i <= positionCount; i++) {

					objectQuadIDs[i] = 0; 

				}

			}
			else
			{
				first.key.SetDivided();
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
				fj.quadsList = quadsList;
				fj.objectIDs = objectIDs;
				fj.quadsID = objectQuadIDs;
				fj.isSorted = sorted;
				fj.lengths = lengths;
				fj.bucketSize = bucketSize;	
				fj.Schedule().Complete();




			}

			NeighborSearchJob nsj = new NeighborSearchJob();
			nsj.quadsList = quadsList;
			nsj.quads = quads;
			nsj.objectIDs = objectIDs;
			nsj.objectQuadIDs = objectQuadIDs;
			nsj.positions = positions;
			nsj.neighborCounts = neighborCounts;
			nsj.objectNeighbors = objectNeighbors;
			nsj.maxNeighborQuads = maxNeighborQuads;
			//nsj.Run(positionCount);
			JobHandle handle = nsj.Schedule(positionCount, SystemInfo.processorCount);





			Profiler.BeginSample("Neighbor Search Job");

			handle.Complete();

			Profiler.EndSample();

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

			for (int i = 0; i < quadsList.Length; i++) {

				quadsList[i] = new Quad();

			}

			for (int i = 0; i < objectQuadIDs.Length; i++) {

				objectQuadIDs[i] = ushort.MaxValue;

			}

			//update the transform's positions
			NullChecks();

		}

		public Transform GetClosestObject(int objectID, ObjectType objectType) {

			if (transforms[objectID] == null) {
				throw new ArgumentException("Object ID does not exist in the quad tree! :(");
			}

			Quad current = quadsList[objectID];
			int closestIndex = -1;
			float tempSqDist = 0;
			float sqDist = 1000000;
			float viewRange = 50;
			float2 local = new float2();

			for (int i = current.startIndex; i <= current.endIndex; i++) {

				if (enemyManager.Genes.GetObjectType(objectID) != objectType)
					continue;

				local = (positions[objectIDs[i]] - positions[objectIDs[objectID]]);
				tempSqDist = (local.x * local.x) + (local.y * local.y);
				if (tempSqDist > sqDist)
					continue;

				sqDist = tempSqDist;
				closestIndex = objectIDs[i];
				
			}

			if (closestIndex != -1)
				return transforms[closestIndex];


			//if not within the same quad, then check the surrounding quads

			//check which node is closest to the current quad
			//continue to do that until object is found



			return transforms[closestIndex];

		}

		private Quad FindNextClosestQuad(int objectID, float viewRange, ref Quad current) {


			//checks if is over, not the closest
			float4 cardinal = new float4(
				//left
				(positions[objectIDs[objectID]].x - viewRange) - (current.position.x - current.halfLength),
				//top
				(positions[objectIDs[objectID]].y + viewRange) - (current.position.y + current.halfLength),
				//right
				(positions[objectIDs[objectID]].x + viewRange) - (current.position.x + current.halfLength),
				//bottom
				(positions[objectIDs[objectID]].y - viewRange) - (current.position.y - current.halfLength)
				);

			float4 cornerDirections = new float4();

			
			
			
			//after finding closest quad, then call the correct function




			return new Quad();

		}

		private void OverLeftQuad() {

		}
		private void OverRightQuad() {

		}
		private void OverTopQuad() {

		}
		private void OverBottomQuad() {

		}

		public ushort AddTransform(Transform transform) {

			positionCount++;
			transforms[positionCount] = transform;
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

			ushort temp = objectIDs[sortedObjectIDs[objectID]];
			ushort overwrite = objectIDs[positionCount];

            objectIDs[sortedObjectIDs[objectID]] = overwrite;
			objectIDs[positionCount] = temp;


            //transforms[objectID] = transforms[objectIDs[positionCount]];
            //transforms[objectIDs[positionCount]] = null;

            //update these pieces of data
            //objectID
            //quadID



            //quadID is easy, just get object id
            //how to do objectID?
            //get the end objectID (possible for it to be the player ID) then assign that ID to the 
            //id that's being removed so that array doesn't need to be refreshed



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


		private void NullChecks() {

			//this function will remove the need to remove transforms from the quad tree
			//will check whether objects are either null if they've been destroyed
			//or set as inactive if they've been killed


			ushort temp = 0;
			for (int i = 0; i < positionCount; i++) {

				if (transforms[i] == null) {
					temp = objectIDs[i];
					objectIDs[i] = objectIDs[positionCount - 1];
					objectIDs[positionCount - 1] = temp;

					transforms[i] = transforms[positionCount- 1];
					transforms[positionCount - 1] = null;
					positionCount--;
					continue;

				}
					

			}



		}

		public void Dispose() {
			objectIDs.Dispose();
			sortedObjectIDs.Dispose();

			positions.Dispose();
			XQuads.Dispose();
			ZQuads.Dispose();
			sorted.Dispose();
			lengths.Dispose();
			quadsList.Dispose();
			transformAccess.Dispose();
			objectQuadIDs.Dispose();
			quads.Dispose();


			neighborCounts.Dispose();
			objectNeighbors.Dispose();



		}



	}


}