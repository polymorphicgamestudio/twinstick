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
		public ushort positionCount;
		public NativeArray<ushort> indices;
		public NativeArray<float2> items;
		//private TransformAccessArray jobTransforms;
		public List<Transform> transforms;

		public NativeArray<Quad> XQuads;
		private NativeArray<int> lengths;

		int xQuadsLength { get => lengths[0]; set => lengths[0] = value; }

		public NativeArray<Quad> ZQuads;
		int zQuadsLength { get => lengths[1]; set => lengths[1] = value; }


		public ushort bucketSize;

		private NativeArray<bool> sorted;


		/// <summary>
		/// Create this with maximum number of positions that will be sorted for better performance.
		/// </summary>
		/// <param name="positionCount"></param>
		public QuadTree(int positionCount, ushort bucketSize) {

			indices = new NativeArray<ushort>(positionCount, Allocator.Persistent);
			items = new NativeArray<float2>(positionCount, Allocator.Persistent);
			transforms = new List<Transform>(positionCount);

			XQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			ZQuads = new NativeArray<Quad>(positionCount, Allocator.Persistent);
			sorted = new NativeArray<bool>(1, Allocator.Persistent);

			lengths = new NativeArray<int>(2, Allocator.Persistent);

			halfLength = 30;


			this.bucketSize = bucketSize;


		}

		/// <summary>
		/// call this each frame to sort it based on newest positions
		/// </summary>
		public void Update() {



			Quad first = new Quad();
			first.startIndex = 0;
			first.endIndex = (short)(positionCount - 1);
			first.position = new float2(origin.x, origin.z);
			first.halfLength = halfLength;

			XQuads[0] = first;
			xQuadsLength = 1;
			while (!sorted[0]) {

				SortIterationJob sij = new SortIterationJob();
				sij.positions = items;
				sij.positionIndices = indices;
				//sij.quads = 
				sij.readFrom = XQuads;
				sij.writeTo = ZQuads;
				sij.zSort = false;
				sij.Schedule(xQuadsLength, SystemInfo.processorCount - 1).Complete();

				sij = new SortIterationJob();
				sij.positions = items;
				sij.positionIndices = indices;
				sij.readFrom = ZQuads;
				sij.writeTo = XQuads;
				sij.zSort = true;
				sij.Schedule(xQuadsLength * 2, SystemInfo.processorCount - 1).Complete();


				QuadFilteringJob fj = new QuadFilteringJob();
				fj.readFrom = XQuads;
				fj.isSorted = sorted;
				fj.lengths = lengths;
				fj.bucketSize = bucketSize;	
				fj.Schedule().Complete();

				//sorted[0] = true;




			}


		}

		public void NewFrame() {
			positionCount = 0;
			sorted[0] = false;

			//update the transform's positions
			NullChecks();

		}


		public void AddTransform(Transform transform) {

			transforms.Add(transform);
			

		}

		public void AddPosition(Vector3 position) {

			indices[positionCount] = positionCount;
			items[positionCount] = new float2(position.x, position.z);
			positionCount++;

		}

		//public void RemoveTransform(int id) {

		//	//will only be called when an enemy dies or a tower/wall is removed.
		//	//will swap the unwanted ID for the object at end of the array
		//	//all objects will keep their same id
		//	//and any new objects will be assigned the ID at the end of the ID array



		//}


		private void NullChecks() {

			//this function will remove the need to remove transforms from the quad tree
			//will check whether objects are either null if they've been destroyed
			//or set as inactive if they've been killed



			for (int i = 0; i < transforms.Count;) {

				if (transforms[i] == null) {
					transforms.RemoveAt(i);
					continue;
				}

				i++;
			}

		}

		public void Dispose() {
			indices.Dispose();
			items.Dispose();
			XQuads.Dispose();
			ZQuads.Dispose();
			sorted.Dispose();
			lengths.Dispose();

		}



	}


}