using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

namespace ShepProject {

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



                }



                writeTo[index * 2] = leftQuad;
                writeTo[index * 2 + 1] = rightQuad;



                return;

			}


			while (startIndex < endIndex) {

				if (zSort) {

					//since quad tree is 2d,
					//using Y as the Z dimension to save memory

					while (IsLessThanOrEqual(objectPositions[objectIDs[startIndex]].y, ref readQuad)
						&& startIndex < endIndex) {

						startIndex++;

					}
					while (IsGreaterThan(objectPositions[objectIDs[endIndex]].y, ref readQuad)
						&& endIndex > startIndex) {
						endIndex--;
					}

				}
				else {

					while (IsLessThanOrEqual(objectPositions[objectIDs[startIndex]].x, ref readQuad)
						&& startIndex < endIndex) {

						startIndex++;

					}
					while (IsGreaterThan(objectPositions[objectIDs[endIndex]].x, ref readQuad)
						&& endIndex > startIndex) {
						endIndex--;
					}
				}


				if (startIndex < endIndex) {

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
			if (zSort) {
				value = objectPositions[objectIDs[startIndex]].y;
			}
			else {
				value = objectPositions[objectIDs[startIndex]].x;
			}

			if (IsLessThanOrEqual(value, ref readQuad)) {
				//start index is lesser, so endIndex needs to be adjusted

				if (startIndex >= readQuad.endIndex) {
					endIndex = -1;

				}
				else {
					endIndex = (short)(startIndex + 1);
				}

			}
			else {
				//endIndex is greater

				if (endIndex <= readQuad.startIndex) {
					startIndex = -1;

				}
				else {
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

			if (leftQuad.BucketSize > bucketSize) {
				leftQuad.key.SetDivided();
			}
			else {

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

			if (rightQuad.BucketSize > bucketSize) {
				rightQuad.key.SetDivided();
			}
			else {
				rightQuad.key.SetDivided(false);

                if (endIndex > -1)
                    SetSortedIndices(rightQuad);
			}


			if (zSort) {

				leftQuad.position = new float2(readQuad.position.x, (readQuad.position.y) - (readQuad.halfLength / 2f));
				rightQuad.position = new float2(readQuad.position.x, (readQuad.position.y) + (readQuad.halfLength / 2f));

				leftQuad.halfLength = readQuad.halfLength / 2f;
				rightQuad.halfLength = readQuad.halfLength / 2f;


				quads.Add(leftQuad.key, leftQuad);
				quads.Add(rightQuad.key, rightQuad);
			}

			else {
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

		private void SetSortedIndices(Quad quad) {

			for (short i = quad.startIndex; i <= quad.endIndex; i++) {

				sortedObjectIDs[objectIDs[i]] = (ushort)i;

			}


		}


	}


	public struct QuadFilteringJob : IJob {

		public NativeArray<Quad> readFrom;
		//public NativeArray<Quad> quadsList;

		public NativeArray<ushort> objectIDs;
		//public NativeArray<ushort> quadsID;

		public NativeArray<int> lengths;

		public NativeArray<bool> isSorted;

		public short bucketSize;

		public void Execute() {

			int startIndex = 0;
			int endIndex = (lengths[0] * 4) - 1;

			while (startIndex < endIndex) {

				while (!(readFrom[startIndex].startIndex < 0) && !(readFrom[startIndex].endIndex < 0)
					&& readFrom[startIndex].BucketSize > bucketSize) {

					startIndex++;


				}


				while (((readFrom[endIndex].startIndex < 0) || (readFrom[endIndex].endIndex < 0)
					|| (readFrom[endIndex].BucketSize <= bucketSize)) && (endIndex >= startIndex && endIndex > 0)) {


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

			if (endIndex == 0 && readFrom[endIndex].BucketSize <= bucketSize) {
				isSorted[0] = true;

			}
			else
				lengths[0] = (endIndex + 1);

		}
	}

	public struct ReadTransformsJob : IJobParallelForTransform {

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;
		public int maxIndex;

		public void Execute(int index, TransformAccess transform) {

			if (!transform.isValid)
				return;

			positions[index] = new float2(transform.position.x, transform.position.z);

		}


	}

	public struct WriteTransformsJob : IJobParallelForTransform {

		public GenesArray genes;

        [NativeDisableContainerSafetyRestriction]
		public NativeArray<float2> positions;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> rotation;

		public void Execute(int index, TransformAccess transform) {

			if (genes.GetObjectType(index) == ObjectType.Tower)
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

	public struct AssignTypesJob : IJobParallelFor
	{
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> objectIDs;

		[NativeDisableContainerSafetyRestriction]
		public GenesArray genes;

        [NativeDisableContainerSafetyRestriction]	
        public NativeParallelHashMap<QuadKey, Quad> quads;
		public int size;


        public void Execute(int index)
        {

			QuadKey current = new QuadKey();
			if (size > 1)
				current.SetNextLevelPosition(index);

			TraverseDownTree(quads[current].key);


        }

		private ContainsTypes TraverseDownTree(QuadKey parentKey)
		{

			if (!quads[parentKey].key.IsDivided)
			{
				if (quads[parentKey].startIndex < 0)
					return new ContainsTypes();
				
				return SearchQuadForTypes(parentKey);
            }

            ContainsTypes contains = new ContainsTypes();
            QuadKey checkKey = parentKey;

            //top left quad
            checkKey = parentKey;
            checkKey.LeftBranch();
            checkKey.RightBranch();
			contains |= TraverseDownTree(quads[checkKey].key);

            checkKey = parentKey;
            checkKey.LeftBranch();
            checkKey.LeftBranch();
            contains |= TraverseDownTree(quads[checkKey].key);

            checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.LeftBranch();
            contains |= TraverseDownTree(quads[checkKey].key);

            checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.RightBranch();
            contains |= TraverseDownTree(quads[checkKey].key);

            //then from all those, assign the values here, then return another bool5 or w/e
            Quad current = quads[parentKey];
            current.ContainsTypes = contains;
            quads[parentKey] = current;

            return contains;
        }


		private ContainsTypes SearchQuadForTypes(QuadKey key)
		{
			Quad current = quads[key];
			ContainsTypes contains = new ContainsTypes();

            for (int i = quads[key].startIndex; i <= quads[key].endIndex; i++)
			{
				contains[genes.GetObjectType(objectIDs[i])] = true;

			}

			current.ContainsTypes = contains;
			quads[key] = current;

            return contains;

		}


    }


}