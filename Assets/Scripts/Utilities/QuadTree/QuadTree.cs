using System.Collections;
using System.Collections.Generic;
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
		public short positionCount;
		public NativeArray<ushort> objectIDs;
		public NativeArray<ushort> objectQuadIDs;
		public NativeArray<float2> positions;

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


		public void OnGUI()
		{

			//draw quads for debugging
			//issue with unity's code getting quads over certain amount, starts at about 150-160
			//might be from using 2021 instead of a newer version of unity, not sure
			NativeArray<Quad> q = quads.GetValueArray(Allocator.Temp);
			for (int i = 0; i < q.Length; i++)
			{

				float3 pos = new float3(q[i].position.x, 1, q[i].position.y);
				float3 half = new float3(q[i].halfLength, 0, q[i].halfLength);
				float hl = q[i].halfLength;

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


				//Handles.Label(pos, q[i].key.GetHeirarchyBinaryString());


			}

			q.Dispose();



		}

        /// <summary>
        /// Create this with maximum number of positions that will be sorted for better performance.
        /// </summary>
        /// <param name="positionCount"></param>
        public QuadTree(int positionCount, ushort bucketSize) {

			objectIDs = new NativeArray<ushort>(positionCount, Allocator.Persistent);
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
				sij.readFrom = XQuads;
				sij.writeTo = ZQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = false;
				sij.quads = quads;
				sij.Run(xQuadsLength);
				//sij.Schedule(xQuadsLength, SystemInfo.processorCount).Complete();

				sij = new SortIterationJob();
				sij.objectPositions = positions;
				sij.objectIDs = objectIDs;
				sij.readFrom = ZQuads;
				sij.writeTo = XQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = true;
				sij.quads = quads;
				sij.Run(xQuadsLength * 2);
				//sij.Schedule(xQuadsLength * 2, SystemInfo.processorCount).Complete();


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
			nsj.objectQuadIDs = objectQuadIDs;
			nsj.positions = positions;
			nsj.neighborCounts = neighborCounts;
			nsj.objectNeighbors = objectNeighbors;
			nsj.maxNeighborQuads = maxNeighborQuads;
			nsj.Run(positionCount);
			//JobHandle handle = nsj.Schedule(positionCount, SystemInfo.processorCount);





			Profiler.BeginSample("Neighbor Search Job");

			//handle.Complete();

			Profiler.EndSample();

		}

		public void NewFrame() {
			lengths[1] = 0;
			sorted[0] = false;
			quads.Clear();

			//for (int i = 0; i < neighborCounts.Length; i++)
			//{
			//	for (int j = 0; j < neighborCounts[i]; j++)
			//	{
			//		objectNeighbors[j * maxNeighborQuads] = new QuadKey();

			//	}


			//}

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


		public ushort AddTransform(Transform transform) {

			positionCount++;
			transforms[positionCount] = transform;
			return objectIDs[positionCount];

		}

		//public void AddPosition(Vector3 position) {

		//	objectIDs[positionCount] = positionCount;
		//	positions[positionCount] = new float2(position.x, position.z);
		//	positionCount++;

		//}

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