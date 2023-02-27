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

			if (startIndex < 0 || endIndex < 0) {

				startIndex = -1;
				endIndex = -1;
			}

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
						&& startIndex < endIndex) {

						startIndex++;

					}
					while (IsGreaterThan(positions[positionIndices[endIndex]].x, ref readQuad)
						&& endIndex > startIndex) {
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


			float value = 0;
			if (zSort) {
				value = positions[positionIndices[left]].y;
			}
			else {
				value = positions[positionIndices[left]].x;
			}


			if (left >= right) {
				//find out which was changed last
				//then update indices correctly and return true

				//no swap needed for this case

				if (IsLessThanOrEqual(value, ref q)) {
					//left should be at this index, so update right correctly

					if (left + 1 >= q.endIndex) {
						left = q.endIndex;
						right = -1;
					}
					else {
						left++;
						right = (short)(left + 1);
					}

				}
				else {
					//right should be at this index, so update left correctly
					if (right - 1 <= q.startIndex) {
						right = q.startIndex;
						left = -1;

					}
					else {
						right--;
						left = (short)(right - 1);
					}

				}

				return true;

			}

			/*if (left < right)*/
			else {

				//then just swap items
				//check if they're equal or greater, if so need to check which is greater
				//then adjust and return whichever value
				//then return true to say its finished sorting

				ushort temp = positionIndices[left];

				positionIndices[left] = positionIndices[right];
				positionIndices[right] = temp;

				if (left < (right - 1)) {
					left++;
					right--;


					//could be at the same index now
					//check if that's the case and update correctly if so

					if (left == right) {

						if (zSort) {
							value = positions[positionIndices[left]].y;
						}
						else {
							value = positions[positionIndices[left]].x;
						}

						if (IsLessThanOrEqual(value, ref q)) {
							//left should be at this index, so update right correctly

							if (left + 1 >= q.endIndex) {
								left = q.endIndex;
								right = -1;
							}
							else {
								left++;
								right = (short)(left + 1);
							}

						}
						else {
							//right should be at this index, so update left correctly
							if (right - 1 <= q.startIndex) {
								right = q.startIndex;
								left = -1;

							}
							else {
								right--;
								left = (short)(right - 1);
							}

						}



						return true;
					}

				}
				//otherwise left is one below right and everything is ok
				else if (left == (right - 1)) {
					return true;
				}

				return false;

			}

		}


	}


	public struct QuadFilteringJob : IJob
	{

		public NativeArray<Quad> readFrom;
		public NativeArray<Quad> quadsList;

		public NativeArray<ushort> quadsID;

		public NativeArray<int> lengths;

		public NativeArray<bool> isSorted;

		public ushort bucketSize;

		public void Execute() {

			int startIndex = 0;
			int endIndex = (lengths[0] * 4) - 1;

			while (startIndex < endIndex) {
				
				while (!(readFrom[startIndex].startIndex < 0) && !(readFrom[startIndex].endIndex < 0)
					&& readFrom[startIndex].BucketSize > bucketSize) {
					
					startIndex++;

					
				}

				if (!(readFrom[startIndex].startIndex < 0) && !(readFrom[startIndex].endIndex < 0)
					&& readFrom[startIndex].BucketSize <= bucketSize) {

					quadsList[lengths[1]] = readFrom[startIndex];
					lengths[1]++;

					for (int i = readFrom[startIndex].startIndex; i <= readFrom[startIndex].endIndex; i++) {

						quadsID[i] = (ushort)(lengths[1] - 1);

					}


				}


				while (((readFrom[endIndex].startIndex < 0) || (readFrom[endIndex].endIndex < 0)
					|| (readFrom[endIndex].BucketSize < bucketSize)) && (endIndex >= startIndex && endIndex > 0)) {

					if (!(readFrom[endIndex].startIndex < 0) && !(readFrom[endIndex].endIndex < 0)
						&& readFrom[endIndex].BucketSize < bucketSize) {
						quadsList[lengths[1]] = readFrom[endIndex];
						lengths[1]++;


						for (int i = readFrom[endIndex].startIndex; i <= readFrom[endIndex].endIndex; i++) {

							quadsID[i] = (ushort)(lengths[1] - 1);

						}

					}


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

			if (endIndex == 0 && readFrom[endIndex].BucketSize < bucketSize) {
				isSorted[0] = true;

			}
			else
				lengths[0] = (endIndex + 1);

		}
	}


	public struct ReadTransformsJob : IJobParallelForTransform {

		public NativeArray<float2> positions;
		public int maxIndex;

		public void Execute(int index, TransformAccess transform) {

			if (!transform.isValid || index > maxIndex)
				return;

			positions[index] = new float2(transform.position.x, transform.position.z);
			
		}


	}

	public struct WriteTransformsJob : IJobParallelForTransform {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float2> positions;
		public int maxIndex;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> rotation;

		public void Execute(int index, TransformAccess transform) {

			//transform.position = new Vector3(positions[index].x, .5f, positions[index].y);
			transform.position = new Vector3(transform.position.x, 0, transform.position.z);
			Quaternion q = transform.rotation;
			q.eulerAngles = new Vector3(0, math.degrees(rotation[index]), 0);
			transform.rotation = q;
			

		}



	}




}