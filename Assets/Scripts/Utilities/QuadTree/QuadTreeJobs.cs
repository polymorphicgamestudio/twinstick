using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Jobs;

namespace ShepProject {

	public struct SortIterationJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> positionIndices;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float2> positions;
		//public NativeArray<bool> isSorted;

		//public NativeHashMap<int, Quad> quads;

		//this is the list to process
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Quad> readFrom;

		//once done sorting, add the quad to the list
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Quad> writeTo;

		public bool zSort;
		public int bucketSize;

		public void Execute(int index) {

			//should only be at this point if the number of positions needed to be sorted
			//is greater than the maximum value allowed in a quad

			Quad readQuad = readFrom[index];

			short startIndex = readQuad.startIndex;
			short endIndex = readQuad.endIndex;

			Quad leftQuad = new Quad(-1,-1);
			Quad rightQuad = new Quad(-1,-1);

			while (startIndex < endIndex) {

				if (zSort) {

					//since quad tree is 2d,
					//using Y as the Z dimension to save memory

					while (IsLessThanOrEqual(positions[positionIndices[startIndex]].y, ref readQuad) 
						&& startIndex < endIndex) {

						startIndex++;

					}
					while (IsGreaterThan(positions[positionIndices[endIndex]].y, ref readQuad)
						&& endIndex > startIndex) {
						endIndex--;
					}

					if (SwapItems(ref startIndex, ref endIndex, ref readQuad)) {
						//finished this iteration of sorting so exit
						//add the two new quad to the hashMap now
						leftQuad.startIndex = readQuad.startIndex;
						leftQuad.endIndex = startIndex;

						rightQuad.startIndex = endIndex;
						rightQuad.endIndex = readQuad.endIndex;

						leftQuad.position = new float2(readQuad.position.x, (readQuad.position.y) - (readQuad.halfLength / 2f));
						rightQuad.position = new float2(readQuad.position.x, (readQuad.position.y) + (readQuad.halfLength / 2f));

						leftQuad.halfLength = readQuad.halfLength / 2f;
						rightQuad.halfLength = readQuad.halfLength / 2f;


						break;
					}



				}
				else {

					while (IsLessThanOrEqual(positions[positionIndices[startIndex]].x, ref readQuad)
						&& startIndex <= readQuad.endIndex) {

						startIndex++;

					}
					while (IsGreaterThan(positions[positionIndices[endIndex]].x, ref readQuad)
						&& endIndex > readQuad.startIndex) {
						endIndex--;
					}

					if (SwapItems(ref startIndex, ref endIndex, ref readQuad)) {

						leftQuad.startIndex = readQuad.startIndex;
						leftQuad.endIndex = startIndex;

						rightQuad.startIndex = endIndex;
						rightQuad.endIndex = readQuad.endIndex;

						leftQuad.position =  new float2((readQuad.position.x) - (readQuad.halfLength / 2f), readQuad.position.y);
						rightQuad.position = new float2((readQuad.position.x) + (readQuad.halfLength / 2f), readQuad.position.y);

						leftQuad.halfLength = readQuad.halfLength;
						rightQuad.halfLength = readQuad.halfLength;

						break;

					}



				}

			}

			//the sorting has finished for this iteration,
			//add the quads to the list for further sorting if needed

			writeTo[index * 2] = leftQuad;
			writeTo[index * 2 + 1] = rightQuad;


		}

		private bool IsLessThanOrEqual(float value, ref Quad q) {

			if (value <= q.Middle(zSort)) {
				return true;
			}

			return false;

		}

		private bool IsGreaterThan(float value, ref Quad q) {
			if (value > q.Middle(zSort)) {
				return true;
			}
			return false;
		}

		/// <summary>
		/// Swaps items if needed, and returns whether or not that should be the last sort for this iteration
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		private bool SwapItems(ref short left, ref short right, ref Quad q) {
			//both items are now on an object that needs to be swapped
			//check that they're not on the same object and if not, swap

			if (left == right) {


				if (IsLessThanOrEqual(positions[positionIndices[left]].y, ref q)) {
					if (left == q.endIndex) {
						right = -1;
					}
					else
						left++;
				}
				else {
					if (right == q.startIndex) {
						left = -1;
					}
					else
					right--;
				}

				return true;


			}
			else {

				
				
				ushort temp = positionIndices[left];

				positionIndices[left] = positionIndices[right];
				positionIndices[right] = temp;
				left++;
				right--;

				if (left > right) {
					short tempIndex = left;
					left = right;
					right = tempIndex;


					return true;

				}
				else if (left == right) {


					if (IsLessThanOrEqual(positions[left].y, ref q)) {
						if (left == q.endIndex) {
							right = -1;
						}
						else
							left++;
					}
					else {
						if (right == q.startIndex) {
							left = -1;
						}
						else
							right--;
					}

				}

				return false;

			}



		}


	}


	public struct QuadFilteringJob : IJob
	{

		public NativeArray<Quad> readFrom;
		//public NativeArray<Quad> writeTo;
		public NativeArray<int> lengths;

		public NativeArray<bool> isSorted;

		public ushort bucketSize;

		public void Execute() {

			int startIndex = 0;
			int endIndex = (lengths[0] * 4) - 1;

			while (startIndex < endIndex) {
				
				while (!(readFrom[startIndex].startIndex < 0) && !(readFrom[startIndex].endIndex < 0)
					&& (readFrom[startIndex].BucketSize > bucketSize)) {
					startIndex++;
				}
				while (((readFrom[endIndex].startIndex < 0) || (readFrom[endIndex].endIndex < 0)
					|| (readFrom[endIndex].BucketSize < bucketSize)) && (endIndex >= startIndex && endIndex > 0)) {
					endIndex--;
				}

				//swap start and endindex

				if (startIndex >= endIndex) {
					break;
				}
				else {
					//since readfrom[StartIndex] is invalid
					readFrom[startIndex] = readFrom[endIndex];
					readFrom[endIndex] = new Quad(-1, -1);
					startIndex++;
					endIndex--;
				}


			}

			if (endIndex == 0) {
				isSorted[0] = true;

			}
			else
				lengths[0] = endIndex;

		}
	}



}