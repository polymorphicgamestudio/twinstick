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
using UnityEngine.Jobs;

namespace ShepProject {

	public struct SortIterationJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float2> objectPositions;
        //public NativeArray<bool> isSorted;
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


				if (startIndex < endIndex)
				{

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
            if (zSort)
            {
                value = objectPositions[objectIDs[startIndex]].y;
            }
            else
            {
                value = objectPositions[objectIDs[startIndex]].x;
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
			leftQuad.key = readQuad.key;
			leftQuad.key.LeftBranch();

			if (leftQuad.BucketSize > bucketSize)
			{
				leftQuad.key.SetDivided();
			}
			else
			{
				leftQuad.key.SetDivided(false);
			}


            rightQuad.startIndex = endIndex;
            rightQuad.endIndex = readQuad.endIndex;
			rightQuad.key = readQuad.key;
			rightQuad.key.RightBranch();

			if (rightQuad.BucketSize > bucketSize)
			{
				rightQuad.key.SetDivided();
			}
			else
			{
				rightQuad.key.SetDivided(false);
            }


            if (zSort)
            {

                leftQuad.position = new float2(readQuad.position.x, (readQuad.position.y) - (readQuad.halfLength / 2f));
                rightQuad.position = new float2(readQuad.position.x, (readQuad.position.y) + (readQuad.halfLength / 2f));

                leftQuad.halfLength = readQuad.halfLength / 2f;
                rightQuad.halfLength = readQuad.halfLength / 2f;


                quads.Add(leftQuad.key, leftQuad);
                quads.Add(rightQuad.key, rightQuad);
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

		public NativeArray<ushort> objectIDs;
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

						quadsID[objectIDs[i]] = (ushort)(lengths[1] - 1);

					}


				}


				while (((readFrom[endIndex].startIndex < 0) || (readFrom[endIndex].endIndex < 0)
					|| (readFrom[endIndex].BucketSize <= bucketSize)) && (endIndex >= startIndex && endIndex > 0)) {

					if (!(readFrom[endIndex].startIndex < 0) && !(readFrom[endIndex].endIndex < 0)
						&& readFrom[endIndex].BucketSize <= bucketSize) {
						quadsList[lengths[1]] = readFrom[endIndex];
						lengths[1]++;


						for (int i = readFrom[endIndex].startIndex; i <= readFrom[endIndex].endIndex; i++) {

							quadsID[objectIDs[i]] = (ushort)(lengths[1] - 1);

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

			if (endIndex == 0 && readFrom[endIndex].BucketSize <= bucketSize) {
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


    public struct NeighborSearchJob : IJobParallelFor
    {

		public NativeArray<float2> positions;
        public NativeArray<ushort> objectQuadIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Quad> quadsList;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<byte> neighborCounts;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<QuadKey> objectNeighbors;
		public int maxNeighborQuads;

        public void Execute(int index)
        {

			//probably need to add the genesArray in here to check whether
			//the object type is a wall, or is the players to make less work for the CPU


			/*
			 * 
			 * for each position, check if its within a certain distance of each wall
			 *		most likely whichever view distance they have it the highest
			 *		or just pick a somewhat large distance like 10 squared
			 * 
			 * then continue in that direction
			 * 
			 * 
			 * 
			 */

			int currentLevel = 0;
			int viewRange = 5;
			Quad current = quadsList[objectQuadIDs[index]];

            //left, top, right, bottom
            bool4 neighborDirections = new bool4();

            //if their view is over the left side of the quad
            if ((positions[index].x - viewRange) - (current.position.x - current.halfLength) < 0)
            {

                //check if this position is in right quad, if so, then just get the left quad through the parent

                //recursively go through each parent
                //until it finds a parent that contains the quad to the left of the one that contains
                //the child quad
                //then after switching the topmost bit, flip other bits to get its mirror location,
                //until at the lowest level

                //check for the first level where its the right quad
                currentLevel = (int)current.key.GetCount();
                QuadKey parent = current.key;

                while (!parent.GetHeirarchyBit((int)parent.GetCount() - 2))
                {
                    //while its still on the left side
                    parent = parent.GetParentKey();
                }


                //this quad is now at the same level as the quad we want to get
                parent = parent.GetParentKey();

                //now it's the parent of the quad we want to get, which means that we can now go down to the level

                Quad leftQuad = new Quad();
                QuadKey leftKey = parent;
                leftKey.LeftBranch();

                CopyBit(current.key, ref leftKey);

                while (quads.TryGetValue(leftKey, out leftQuad) && leftKey.GetCount() < current.key.GetCount())
                {

                    if (!leftQuad.key.IsDivided)
                        break;

                    leftKey.RightBranch();

                    CopyBit(current.key, ref leftKey);

                    //this only works up until the same level, if the other quad is divided more
                    //then we need to check all the children on the same border

                }

                //now check if there are any other children
                if (leftQuad.key.IsDivided)
                {
                    //there are other children, so check the top and bottom children on the right side
                    CheckOverLeftTopAndBottomChildren(index, leftQuad.key, viewRange);

                    //also might possibly need to check if the quads past these should be included
                    //most likely not though since they'll probably be too far to care about
                }
                else
                {
                    //now should have the mirrored version of the key and quad :)
                    //just need to add it to this object's list of neighbors
                    AddNeighborKey(index, leftQuad.key);
                }

                neighborDirections[0] = true;


            }


            #region Right Side


            //if over the right side
            if ((positions[index].x + viewRange) - (current.position.x + current.halfLength) > 0)
            {


                //need to search for the first quad that is to the left of this quad then

                //recursively go through each parent
                //until it finds a parent that contains the quad to the left of the one that contains
                //the child quad
                //then after switching the topmost bit, flip other bits to get its mirror location,
                //until at the lowest level

                currentLevel = (int)current.key.GetCount();
                QuadKey parent = current.key;

                while (parent.GetHeirarchyBit((int)parent.GetCount() - 2))
                {
                    //while its still on the right side
                    parent = parent.GetParentKey();
                }


                //this quad is now at the same level as the quad we want to get
                parent = parent.GetParentKey();

                //now it's the parent of the quad we want to get, which means that we can now go down to the level

                Quad rightQuad = new Quad();
                QuadKey rightKey = parent;
                rightKey.RightBranch();

                CopyBit(current.key, ref rightKey);

                while (quads.TryGetValue(rightKey, out rightQuad) && rightKey.GetCount() < current.key.GetCount())
                {

                    if (!rightQuad.key.IsDivided)
                        break;

                    rightKey.LeftBranch();

                    CopyBit(current.key, ref rightKey);

                    //this only works up until the same level, if the other quad is divided more
                    //then we need to check all the children on the same border

                }




                //now check if there are any other children
                if (rightQuad.key.IsDivided)
                {
                    //there are other children, so check the top and bottom children on the right side
                    CheckOverRightTopAndBottomChildren(index, rightQuad.key, viewRange);

                    //also might possibly need to check if the quads past these should be included
                    //most likely not though since they'll probably be too far to care about
                }
                else
                {
                    //now should have the mirrored version of the key and quad :)
                    //just need to add it to this object's list of neighbors
                    AddNeighborKey(index, rightQuad.key);
                }


                neighborDirections[0] = true;

            }

            #endregion

            #region Bottom Side

            //if over the bottom side
            if ((positions[index].y - viewRange) - (current.position.y - current.halfLength) < 0)
            {

                currentLevel = (int)current.key.GetCount();
                QuadKey parent = current.key;

                while (!parent.GetHeirarchyBit((int)parent.GetCount() - 1))
                {
                    //while its still on the right side
                    parent = parent.GetParentKey();
                }

                //this quad is now at the same level as the quad we want to get
                parent = parent.GetParentKey();

                //now it's the parent of the quad we want to get, which means that we can now go down to the level

                Quad bottomQuad = new Quad();
                QuadKey bottomKey = parent;

                CopyBit(current.key, ref bottomKey);
                bottomKey.LeftBranch();

                //now at the bottom portion of the key, where the other one is at the top

                while (quads.TryGetValue(bottomKey, out bottomQuad) && bottomKey.GetCount() < current.key.GetCount())
                {

                    if (!bottomQuad.key.IsDivided)
                        break;

                    CopyBit(current.key, ref bottomKey);

                    bottomKey.RightBranch();
                    //this only works up until the same level, if the other quad is divided more
                    //then we need to check all the children on the same border

                }

                //now check if there are any other children
                if (bottomQuad.key.IsDivided)
                {
                    //there are other children, so check the top and bottom children on the right side
                    CheckOverBottomLeftAndRightChildren(index, bottomQuad.key, viewRange);

                    //also might possibly need to check if the quads past these should be included
                    //most likely not though since they'll probably be too far to care about
                }
                else
                {
                    //now should have the mirrored version of the key and quad :)
                    //just need to add it to this object's list of neighbors
                    AddNeighborKey(index, bottomQuad.key);
                }

                
                neighborDirections[0] = true;
            }



            #endregion

            #region Top Side

            //if over the top side
            if ((positions[index].y + viewRange) - (current.position.y + current.halfLength) > 0)
            {

                currentLevel = (int)current.key.GetCount();
                QuadKey parent = current.key;

                while (parent.GetHeirarchyBit((int)parent.GetCount() - 1))
                {
                    //while its still on the right side
                    parent = parent.GetParentKey();
                }

                //this quad is now at the same level as the quad we want to get
                parent = parent.GetParentKey();

                //now it's the parent of the quad we want to get, which means that we can now go down to the level

                Quad topQuad = new Quad();
                QuadKey topKey = parent;

                CopyBit(current.key, ref topKey);
                topKey.RightBranch();

                //now at the bottom portion of the key, where the other one is at the top

                while (quads.TryGetValue(topKey, out topQuad) && topKey.GetCount() < current.key.GetCount())
                {

                    if (!topQuad.key.IsDivided)
                        break;

                    CopyBit(current.key, ref topKey);

                    topKey.LeftBranch();
                    //this only works up until the same level, if the other quad is divided more
                    //then we need to check all the children on the same border

                }

                //now check if there are any other children
                if (topQuad.key.IsDivided)
                {
                    //there are other children, so check the top and bottom children on the right side
                    CheckOverTopLeftAndRightChildren(index, topQuad.key, viewRange);

                    //also might possibly need to check if the quads past these should be included
                    //most likely not though since they'll probably be too far to care about
                }
                else
                {
                    //now should have the mirrored version of the key and quad :)
                    //just need to add it to this object's list of neighbors
                    AddNeighborKey(index, topQuad.key);
                }


                neighborDirections[0] = true;


            }



            #endregion


            #region Corners

            //need to figure out how to get corner keys

            ////if over top and left
            //if (neighborDirections[1] && neighborDirections[0])
            //{

            //}


            ////if over top and right
            //if (neighborDirections[1] && neighborDirections[2])
            //{

            //}


            ////if over bottom and left
            //if (neighborDirections[3] && neighborDirections[0])
            //{

            //}


            ////if over bottom and right
            //if (neighborDirections[3] && neighborDirections[2])
            //{

            //}




            #endregion




        }


        public void CheckOverLeftTopAndBottomChildren(int index, QuadKey parent, float viewRange)
		{
			//will need to check each of these keys to see if they're divided as well as 
			//whether they're close enough to object

            parent.RightBranch();

            QuadKey topKey = parent;
            topKey.RightBranch();

            Quad topQuad = new Quad();
           
            if (quads.TryGetValue(topKey, out topQuad))
            {
                if (topQuad.key.IsDivided)
                    CheckOverLeftTopAndBottomChildren(index, topQuad.key, viewRange);

                //check if object's range is within this quad
                else if ((positions[index].x - viewRange) < (topQuad.position.x + topQuad.halfLength))
                {

                    //within range of this quad
                    AddNeighborKey(index, topQuad.key);
                    
                }
            }

            
            QuadKey bottomKey = parent;
            bottomKey.LeftBranch();

            Quad bottomQuad = new Quad();


            if (quads.TryGetValue(bottomKey, out bottomQuad))
            {

                if (bottomQuad.key.IsDivided)
                {
                    CheckOverLeftTopAndBottomChildren(index, bottomQuad.key, viewRange);

                }
                else if((positions[index].x - viewRange) < (bottomQuad.position.x + bottomQuad.halfLength))
                {
                    //check if object's range is within this quad
                    AddNeighborKey(index, bottomQuad.key);

                    


                }

            }

        }


        public void CheckOverRightTopAndBottomChildren(int index, QuadKey parent, float viewRange)
        {
            //will need to check each of these keys to see if they're divided as well as 
            //whether they're close enough to object

            parent.LeftBranch();


            Quad topQuad = new Quad();
            QuadKey topKey = parent;
            topKey.RightBranch();

            if (quads.TryGetValue(topKey, out topQuad))
            {

                if (topQuad.key.IsDivided)
                {
                    CheckOverRightTopAndBottomChildren(index, topQuad.key, viewRange);
                }
                //else if within range of this quad
                else if ((positions[index].x + viewRange) > (topQuad.position.x - topQuad.halfLength))
                {
                    
                    AddNeighborKey(index, topQuad.key);
                    

                }
            }


            Quad bottomQuad = new Quad();
            QuadKey bottomKey = parent;
            bottomKey.LeftBranch();

            if (quads.TryGetValue(bottomKey, out bottomQuad))
            {

                if (bottomQuad.key.IsDivided)
                {
                    CheckOverRightTopAndBottomChildren(index, bottomQuad.key, viewRange);

                }
                else if ((positions[index].x + viewRange) > (bottomQuad.position.x - bottomQuad.halfLength))
                {
                    

                    AddNeighborKey(index, bottomQuad.key);

                }
            }


        }


        public void CheckOverBottomLeftAndRightChildren(int index, QuadKey parent, float viewRange)
        {
            //will need to check each of these keys to see if they're divided as well as 
            //whether they're close enough to object

            Quad topLeft = new Quad();
            QuadKey leftKey = parent;
            leftKey.LeftBranch();
            leftKey.RightBranch();

            if (quads.TryGetValue(leftKey, out topLeft))
            {

                if (topLeft.key.IsDivided)
                {
                    CheckOverBottomLeftAndRightChildren(index, topLeft.key, viewRange);
                }
                else if ((positions[index].y - viewRange) < (topLeft.position.y + topLeft.halfLength))
                {

                    AddNeighborKey(index, topLeft.key);

                }

            }



			Quad topRight = new Quad();
			QuadKey rightKey = parent;
            rightKey.RightBranch();
            rightKey.RightBranch();

            if (quads.TryGetValue(rightKey, out topRight))
            {



                if (topRight.key.IsDivided)
                {
                    CheckOverBottomLeftAndRightChildren(index, topRight.key, viewRange);

                }
                else if ((positions[index].y - viewRange) < (topRight.position.y + topRight.halfLength))
                {
                    //check if object's range is within this quad

                    AddNeighborKey(index, topRight.key);

                    

                    //otherwise not in range so no need to add it to list

                }

            }


		}


        public void CheckOverTopLeftAndRightChildren(int index, QuadKey parent, float viewRange)
        {
            //will need to check each of these keys to see if they're divided as well as 
            //whether they're close enough to object

            Quad bottomLeft = new Quad();
            QuadKey leftKey = parent;
            leftKey.LeftBranch();
            leftKey.LeftBranch();

            if (quads.TryGetValue(leftKey, out bottomLeft))
            {

                if (bottomLeft.key.IsDivided)
                {
                    CheckOverTopLeftAndRightChildren(index, bottomLeft.key, viewRange);
                }
                else if ((positions[index].y + viewRange) > (bottomLeft.position.y - bottomLeft.halfLength))
                {
                    
                    AddNeighborKey(index, bottomLeft.key);

                }
            }

            Quad topRight = new Quad();
            QuadKey rightKey = parent;
            rightKey.RightBranch();
            rightKey.LeftBranch();

            if (quads.TryGetValue(rightKey, out topRight))
            {

                if (topRight.key.IsDivided)
                {
                    CheckOverTopLeftAndRightChildren(index, topRight.key, viewRange);

                }
                else if ((positions[index].y + viewRange) > (topRight.position.y - topRight.halfLength))
                {
                   
                    AddNeighborKey(index, topRight.key);

                }

            }


        }





        public void AddNeighborKey(int index, QuadKey key) {

			objectNeighbors[(index * maxNeighborQuads) + neighborCounts[index]] = key;
			neighborCounts[index]++;

		}


		private void CopyBit(QuadKey reader, ref QuadKey key)
		{

            if (reader.GetHeirarchyBit((int)key.GetCount()))
            {

                key.RightBranch();

            }
            else
            {
                key.LeftBranch();

            }

        }

    }




}