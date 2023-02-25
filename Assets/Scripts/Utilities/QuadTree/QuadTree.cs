using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Jobs;


namespace ShepProject {

	public class QuadTree {

		public Vector3 origin;
		public float halfLength;
		public short positionCount;
		public NativeArray<ushort> objectIDs;
		public NativeArray<ushort> objectQuadIDs;
		public NativeArray<float2> positions;

		private Transform[] transforms;
		private TransformAccessArray transformAccess;
		public TransformAccessArray TransformAccess => transformAccess;

		public NativeArray<Quad> quadsList;

		public NativeArray<Quad> XQuads;
		public NativeArray<Quad> ZQuads;

		private NativeArray<int> lengths;

		int xQuadsLength { get => lengths[0]; set => lengths[0] = value; }
		public int QuadsListLength => lengths[1];

		public ushort bucketSize;

		private NativeArray<bool> sorted;


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

			quadsList = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			XQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			ZQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			sorted = new NativeArray<bool>(1, Allocator.Persistent);

			lengths = new NativeArray<int>(2, Allocator.Persistent);

			halfLength = 60;
			
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
			first.endIndex = (short)(positionCount - 1);
			first.position = new float2(origin.x, origin.z);
			first.halfLength = halfLength;

			XQuads[0] = first;
			xQuadsLength = 1;
			lengths[1] = 0;
			if (XQuads[0].BucketSize <= bucketSize) {
				quadsList[0] = XQuads[0];
				sorted[0] = true;
				lengths[1] = 1;
			}
			while (!sorted[0]) {

				SortIterationJob sij = new SortIterationJob();
				sij.positions = positions;
				sij.positionIndices = objectIDs;
				sij.readFrom = XQuads;
				sij.writeTo = ZQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = false;
				sij.Schedule(xQuadsLength, SystemInfo.processorCount).Complete();

				sij = new SortIterationJob();
				sij.positions = positions;
				sij.positionIndices = objectIDs;
				sij.readFrom = ZQuads;
				sij.writeTo = XQuads;
				sij.bucketSize = bucketSize;
				sij.zSort = true;
				sij.Schedule(xQuadsLength * 2, SystemInfo.processorCount).Complete();


				QuadFilteringJob fj = new QuadFilteringJob();
				fj.readFrom = XQuads;
				fj.quadsList = quadsList;
				fj.quadsID = objectQuadIDs;
				fj.isSorted = sorted;
				fj.lengths = lengths;
				fj.bucketSize = bucketSize;	
				fj.Schedule().Complete();




			}


		}

		public void NewFrame() {
			lengths[1] = 0;
			sorted[0] = false;

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

		}



	}


}