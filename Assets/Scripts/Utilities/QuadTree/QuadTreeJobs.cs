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

            Quad leftQuad = new Quad(-1, -1);
            Quad rightQuad = new Quad(-1, -1);

            short startIndex = readQuad.startIndex;
			short endIndex = readQuad.endIndex;

			if (startIndex < 0 || endIndex < 0) {

				startIndex = -1;
				endIndex = -1;


                writeTo[index * 2] = leftQuad;
                writeTo[index * 2 + 1] = rightQuad;

				return;

            }


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
				}


				if (startIndex < endIndex)
				{

					ushort temp = positionIndices[startIndex];
					positionIndices[startIndex] = positionIndices[endIndex];
					positionIndices[endIndex] = temp;

					startIndex++;
					endIndex--;



				}

			}

            //the sorting has finished for this iteration,
            //add the quads to the list for further sorting if needed


            float value = 0;
            if (zSort)
            {
                value = positions[positionIndices[startIndex]].y;
            }
            else
            {
                value = positions[positionIndices[startIndex]].x;
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


            leftQuad.startIndex = readQuad.startIndex;
            leftQuad.endIndex = startIndex;

            rightQuad.startIndex = endIndex;
            rightQuad.endIndex = readQuad.endIndex;

            if (zSort)
            {

                leftQuad.position = new float2(readQuad.position.x, (readQuad.position.y) - (readQuad.halfLength / 2f));
                rightQuad.position = new float2(readQuad.position.x, (readQuad.position.y) + (readQuad.halfLength / 2f));

                leftQuad.halfLength = readQuad.halfLength / 2f;
                rightQuad.halfLength = readQuad.halfLength / 2f;


            }

            else
            {
                leftQuad.position = new float2((readQuad.position.x) - (readQuad.halfLength / 2f), readQuad.position.y);
                rightQuad.position = new float2((readQuad.position.x) + (readQuad.halfLength / 2f), readQuad.position.y);

                leftQuad.halfLength = readQuad.halfLength;
                rightQuad.halfLength = readQuad.halfLength;
            }




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