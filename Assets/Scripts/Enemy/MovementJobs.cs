using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using static System.Collections.Specialized.BitVector32;

namespace ShepProject {


	public struct ChooseTargetJob : IJobParallelFor {

		[NativeDisableContainerSafetyRestriction]
		public NativeList<ushort> choosableTargets;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> targetIDs;
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<float2> positions;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<ushort> objectIDs;


		public void Execute(int index) {


			//if there is a target, return
			if (targetIDs[objectIDs[index]] != ushort.MaxValue)
				return;

			//else if no target, check to see which is the closest and start attacking

			float minDistSq = 1000000;
			float current = 0;

			for (ushort i = 0; i < choosableTargets.Length; i++) {

				if (choosableTargets[i] == ushort.MaxValue)
					continue;

				current = math.distancesq(positions[objectIDs[index]], positions[choosableTargets[i]]);

				if (current < minDistSq) {

					minDistSq = current;
					targetIDs[objectIDs[index]] = choosableTargets[i];

				}


			}

			


		}
	}


    /*
	 * 
	 * want to have an optimal distance for slimes and towers
	 *	both will be different, will have them as genes
	 *	
	 * when less than an optimal distance, will move away
	 * 
	 * when greater than an optimal distance, will move towards
	 * 
	 * 
	 * 
	 */


    public struct GatherForcesWithinRangeJob : IJobParallelFor
    {


        /* 
		 * for each agent, search within its given range for a type of object
		 * 
		 */
        #region Variables

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> objectIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ushort> targetIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;

        [NativeDisableContainerSafetyRestriction]
        public GenesArray genes;

        [NativeDisableContainerSafetyRestriction]
        public NativeParallelHashMap<QuadKey, Quad> quads;
         
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> sheepDistancesToSlime;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectForces;

		public ObjectType targetType;

        #endregion

        public void Execute(int index)
        {

            int objectID = objectIDs[index];

			//checking for player ID, since player is always the first thing spawned
			if (objectID == 0)
                return;

			ObjectType objType = genes.GetObjectType(objectID);

            if (objType == ObjectType.Wall || objType == ObjectType.Tower)
                return;

            //now at this point, should only be sheep and slimes

            if (objType == ObjectType.Slime && targetType == ObjectType.Sheep)
            {

                objectForces[(objectID * (int)ObjectType.Count) + (int)targetType]
                    = math.normalize(positions[targetIDs[objectID]] - positions[objectID])
                    * genes.GetAttraction(objectID, ObjectType.Sheep);


            }
            else if (objType == ObjectType.Sheep && targetType == ObjectType.Slime)
            {
                if (targetType == ObjectType.Slime)
                {
                    sheepDistancesToSlime[(objectID)] =
                        SearchChildrenForClosestObjectDistance(quads[new QuadKey()].key, objectID, ObjectType.Slime, 8);
                }
            }

            //now search all the buckets including neighbors
            //will only be the bucket 

            SearchChildrenForForce(quads[new QuadKey()].key, objectID, objType);


        }

        private void SearchChildrenForForce(QuadKey parentKey, int objectID, ObjectType objType)
        {
            //this will search through all child quads for bottom level quads and then check if those quads 
            //have the required objects inside of them and a search of the bucket will be required

            if (!quads[parentKey].ContainsObjectType(targetType))
                return;

            QuadKey checkKey = parentKey;
            float maxDistance = genes.GetViewRange(objectID, ViewRange.Tower);

            //slime max distance
            if (8 > maxDistance) 
            {
                maxDistance = 8;
            }


            if (!quads[parentKey].key.IsDivided)
            {
                //check this quad for the required object

                //need to update return type from float2 to objectForces
                objectForces[(objectID * (int)ObjectType.Count) + (int)targetType] += 
                    //(objectForces[(objectID * (int)ObjectType.Count) + (int)targetType]
                    GatherBucketForces(objectID, objType, parentKey);

                return;

            }
            //else only check children

            //top left quad
			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(quads[checkKey].key, objectID, objType);
            }

			checkKey = parentKey;
			checkKey.LeftBranch();
            checkKey.LeftBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(quads[checkKey].key, objectID, objType);
			}

			checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.LeftBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(quads[checkKey].key, objectID, objType);
			}

			checkKey = parentKey;
            checkKey.RightBranch();
            checkKey.RightBranch();
            if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				SearchChildrenForForce(quads[checkKey].key, objectID, objType);
			}

		}

        private float SearchChildrenForClosestObjectDistance(QuadKey parentKey, int objectID, ObjectType objType, float maxDistance) 
        {

            //top down for searching for this object
            QuadKey checkKey = parentKey;
            float minDistance = 10000;
            float tempMin = 0;
			if (!quads[parentKey].key.IsDivided) {
				//check this quad for the required object

				return SearchBucketForClosestObject(parentKey, objectID, objType, maxDistance);

			}

			//top left quad
			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
                minDistance = SearchChildrenForClosestObjectDistance(quads[checkKey].key, objectID, objType, maxDistance);
			}

			checkKey = parentKey;
			checkKey.LeftBranch();
			checkKey.LeftBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
                tempMin = SearchChildrenForClosestObjectDistance(quads[checkKey].key, objectID, objType, maxDistance);

                if (tempMin < minDistance)
                    minDistance = tempMin;

			}

			checkKey = parentKey;
			checkKey.RightBranch();
			checkKey.LeftBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				tempMin = SearchChildrenForClosestObjectDistance(quads[checkKey].key, objectID, objType, maxDistance);

				if (tempMin < minDistance)
					minDistance = tempMin;
			}

			checkKey = parentKey;
			checkKey.RightBranch();
			checkKey.RightBranch();
			if (quads[checkKey].IsWithinDistance(positions[objectID], maxDistance)) 
            {
				tempMin = SearchChildrenForClosestObjectDistance(quads[checkKey].key, objectID, objType, maxDistance);

				if (tempMin < minDistance)
					minDistance = tempMin;
			}

            return minDistance;

		}

        private float SearchBucketForClosestObject(QuadKey key, int objectID, ObjectType objType, float maxDistance) 
        {


			float minDist = 10000;

			if (!quads[key].ContainsObjectType(targetType))
                return minDist;

            float tempMin = 0;
            for (int i = quads[key].startIndex; i <= quads[key].endIndex; i++) 
            {
                if (genes.GetObjectType(objectIDs[i]) != targetType) 
                    continue;
                tempMin = math.distancesq(positions[objectIDs[i]], positions[objectID]);

				if (tempMin < minDist)
                    minDist = tempMin;

            }


            return minDist;
        }


		private float2 GatherBucketForces(int objectID, ObjectType objType, QuadKey key)
        {

            float maxDist = genes.GetViewRange(objectID, (ViewRange)targetType);
            float maxDistSq = maxDist * maxDist;
            float2 localPosition = new float2();

            float2 moveTowards = new float2();

            Quad current = new Quad();
            current = quads[key];


            if (current.startIndex < 0)
                return moveTowards;

            for (int i = current.startIndex; i <= current.endIndex; i++)
            {

                //to ignore itself in all calculations
                if (objectIDs[i] == objectID)
                    continue;

                //ignore it if it is not the type we're looking for
                if (genes.GetObjectType(objectIDs[i]) != targetType)
                    continue;

                localPosition = (positions[objectIDs[i]] - positions[objectID]);


                ////for debugging rays only
                //Vector3 pos = new Vector3();
                //pos.x = positions[objectID].x;
                //pos.y = 1;
                //pos.z = positions[objectID].y;

                //Vector3 local = new Vector3();

                float sqDist = (math.pow(localPosition.x, 2) + math.pow(localPosition.y, 2));

                //if greater than this distance, ignore and continue on
                if (sqDist > maxDistSq)
                {

                    continue;
                }

                //that divided by maxDist to get the scaledVector
                float angle = math.atan2(localPosition.y, localPosition.x);

                float2 one = new float2(math.cos(angle), math.sin(angle));

                //float2 oneDegrees = new float2(math.degrees(one.x), math.degrees(one.y));

                //if (objType != ObjectType.Slime)
                //{
                    //eventually will be
                    //localPosition /= traitValue of the viewDistance for this specific object type
                    localPosition /= maxDist;
                    one -= localPosition;

                //}


                //now it is same type, so add to the force based on the attraction

                switch (targetType)
                {
                    case ObjectType.Slime:
                    {
                        if (objType == ObjectType.Slime)
                        {

                            //geneValue for optimalDistance will be from 0-1
                            //and then it will be ran through the sigmoid in order
                            //to get the actual value
                            //
                            //y = ((2 * (magnitude of localPosition)) / ViewDistance) + (traitValue - 1)
                            //

                            /*
                             * 
                             * gene value is ran through sigmoid between (-infinity, infinity)
                             * 
                             * then gives trait value between (minVal, maxVal) after being ran through the sigmoid
                             * 
                             * 
                             * 
                             */

                            float optimalDist = genes.GetOptimalDistance(objectID, OptimalDistance.Slime);
                            if (sqDist < (optimalDist * optimalDist))
                            {



                                /*
                                 *                                 
                                 * maxDist is optimalDist
                                 * then, if sufficiently large enough, need
                                 * to shift max values over
                                 * ex. if optimal distance is 6
                                 * want to shift max repulsion value to start at 3 or 4
                                 * 
                                 * 
                                 */

                                //was trying this for getting force going away from
                                //don't think it will end up using this
                                //localPosition /= optimalDist;
                                //one -= localPosition;


                                //slimes are too close, so get further away
                                moveTowards -= one * (genes.GetAttraction(objectID, ObjectType.Slime) * 2);

                            }
                            else
                            {


                                //slimes are too far, so get closer
                                moveTowards += one * (genes.GetAttraction(objectID, ObjectType.Slime));

                                //local.x = one.x;
                                //local.z = one.y;
                                //local.y = 0;

                                //Debug.DrawRay(pos, local, Color.green);
                            }


                        }
                        else
                        {
                            moveTowards -= one * genes.GetAttraction(objectID, ObjectType.Slime);


                            //local.x = one.x;
                            //local.z = one.y;
                            //local.y = 0;
                            //local *= -1;

                            //Debug.DrawRay(pos, local, Color.red);

                        }
                        break;

                    }
                    //case ObjectType.Sheep:
                    //{

                    //    break;
                    //}
                    case ObjectType.Wall:
                    {

                        //everything wants to avoid walls
                        moveTowards -= (one
                            * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));
                        break;
                    }
                    case ObjectType.Tower:
                    {

                        //slimes want to avoid towers
                        if (objType == ObjectType.Slime)
                        {
                            moveTowards -= (one
                                * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));

                        }
                        //sheep want to get closer to towers
                        else if (objType == ObjectType.Sheep)
                        {
                            moveTowards += (one
                                * genes.GetAttraction(objectID, (int)genes.GetObjectType(objectIDs[i])));
                        }

                        break;
                    }
                    //case ObjectType.Player:
                    //    break;
                    //case ObjectType.Count:
                    //    break;
                    default:
                        break;
                }


            }



            return moveTowards;
        }

    }

    public struct CalculateHeadingJob : IJobParallelFor
    {


        /*
		 * search for objects in each of the four quadrants
		 * 
		 * 
		 * 
		 */

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectForces;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> headings;
        public GenesArray genes;

        public float deltaTime;


        public void Execute(int index)
        {

            ObjectType type = genes.GetObjectType(index);

            if (type != ObjectType.Sheep && type != ObjectType.Slime)
                return;


            float2 moveTowards = new float2();

            for (int i = 0; i < (int)ObjectType.Count; i++)
            {
                //if ((ObjectType)i == ObjectType.Tower 
                //    && !objectForces[(index * (int)ObjectType.Count) + i].Equals(float2.zero))
                //{
                //    int test = 0;

                //    float2 mag = objectForces[(index * (int)ObjectType.Count) + i];


                //}

                moveTowards +=
                    MathUtil.ClampMagnitude(objectForces[(index * (int)ObjectType.Count) + i], 
                    genes.GetAttraction(index, (ObjectType)i));

                //moveTowards += objectForces[(index * (int)ObjectType.Count) + i];

            }
            


            float headingCalculation = math.atan2(moveTowards.x, moveTowards.y);


            if (headingCalculation < 0)
            {

                headingCalculation += 2 * math.PI;
            }

            float local = headingCalculation - headings[index];

            if (local < -math.PI)
            {

                //turn right
                headings[index] -= 2 * math.PI;
            }
            else if (local > math.PI)
            {
                //turn left
                headings[index] += 2 * math.PI;
            }

            headings[index]
                //= headingCalculation;
                = math.lerp(headings[index], headingCalculation, deltaTime * 2);




        }
    }

}



