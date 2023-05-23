using Drawing;
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

        [ReadOnly]
        public NativeArray<int> idsToCheck;


        public CommandBuilder builder;

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

                float3 pos = new float3();
                pos.x = positions[objectID].x;
                pos.y = 0;
                pos.z = positions[objectID].y;

                float3 dir = new float3();
                dir.x = objectForces[(objectID * (int)ObjectType.Count) + (int)targetType].x;
                dir.z = objectForces[(objectID * (int)ObjectType.Count) + (int)targetType].y;
                dir.y = 0;
                builder.Ray(pos,dir, Color.green);

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


            if (idsToCheck.Contains(objectID) && targetType == ObjectType.Slime)
            {
                int test = 0;
            }

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


                //for debugging rays only
                Vector3 pos = new Vector3();
                pos.x = positions[objectID].x;
                pos.y = 1;
                pos.z = positions[objectID].y;

                Vector3 local = new Vector3();

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

                if (objType != ObjectType.Slime)
                {
                    //eventually will be
                    //localPosition /= traitValue of the viewDistance for this specific object type
                    localPosition /= maxDist;
                    one -= localPosition;

                }
                //else if (targetType != ObjectType.Slime)
                //{
                //    localPosition /= (maxDist / );
                //    one -= localPosition;

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

                            if (idsToCheck.Contains(objectID))
                            {
                                int test = 0;
                            }

                            float magnitude = MathUtil.Magnitude(localPosition);
                            float2 normalized = localPosition / magnitude;

                            float slimeForce = ((2 * magnitude)
                                / genes.GetViewRange(objectID, ViewRange.Slime));

                            slimeForce += (genes.GetOptimalDistance(objectID, OptimalDistance.Slime) - 1);
                            slimeForce *= genes.GetAttraction(objectID, ObjectType.Slime);

                            slimeForce = math.clamp(slimeForce, -1, 1);

                            moveTowards += (normalized * slimeForce);

                            local.x = (normalized * slimeForce).x;
                            local.z = (normalized * slimeForce).y;
                            local.y = 0;
                            //local *= -1;
                            
                            if (slimeForce < 0)
                                builder.Ray(pos, local, Color.red);
                            else
                            {
                                builder.Ray(pos, local, Color.green);
                            }

                        }
                        else
                        {
                            moveTowards -= one * genes.GetAttraction(objectID, ObjectType.Slime);


                            local.x = one.x;
                            local.z = one.y;
                            local.y = 0;
                            local *= -1;

                            Debug.DrawRay(pos, local, Color.red);

                        }
                        break;

                    }
                    //case ObjectType.Sheep:
                    //{

                    //    break;
                    //}
                    case ObjectType.Wall:
                    {


                        moveTowards += (one *

                            //distance falloff which makes scales force between 
                            //slimes not caring at all and caring at max value they can
                            //viewDistance / 2 is where it start to 
                            math.clamp(((-2 * MathUtil.Magnitude(localPosition))
                            / genes.GetViewRange(objectID, ViewRange.Wall)) + 2, 0, 1)

                            * genes.GetAttraction(objectID, targetType));

                        break;
                    }
                    case ObjectType.Tower:
                    {

                        //slimes want to avoid towers
                        if (objType == ObjectType.Slime)
                        {


                            moveTowards += (one *

                                //distance falloff which makes scales force between 
                                //slimes not caring at all and caring at max value they can
                                //viewDistance / 2 is where it start to 
                                math.clamp(((-2 * MathUtil.Magnitude(localPosition))
                                / genes.GetViewRange(objectID, ViewRange.Tower)) + 2, 0, 1)

                                * genes.GetAttraction(objectID, targetType));



                        }
                        ////sheep will go close to towers since 
                        //else if (objType == ObjectType.Sheep)
                        //{
                        //    moveTowards += (one
                        //        * genes.GetAttraction(objectID, genes.GetObjectType(objectIDs[i])));
                        //}

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
        //[NativeDisableContainerSafetyRestriction]
        //public NativeArray<ushort> objectIDs;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> objectForces;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> headings;
        public GenesArray genes;

        public NativeArray<int> idsToCheck;

        public CommandBuilder builder;

        public float deltaTime;


        public void Execute(int index)
        {

            ObjectType type = genes.GetObjectType(index);

            if (type != ObjectType.Sheep && type != ObjectType.Slime)
                return;


            float2 moveTowards = new float2();

            if (idsToCheck.Contains(index))
            {
                int test = 0;
            }


            float3 pos = new float3();
            pos.x = positions[index].x;
            pos.y = .7f;
            pos.z = positions[index].y;

            float3 dir = new float3();

            float maxMagnitude = 0;
            float tempMagnitude = 0;
            for (int i = 0; i < (int)ObjectType.Sheep; i++)
            {

                tempMagnitude = MathUtil.SqrMagnitude(objectForces[(index * (int)ObjectType.Count) + i]);

                if (tempMagnitude == 0)
                    continue;

                tempMagnitude = math.sqrt(tempMagnitude);

                if (tempMagnitude > maxMagnitude)
                    maxMagnitude = tempMagnitude;


                objectForces[(index * (int)ObjectType.Count) + i]
                    = objectForces[(index * (int)ObjectType.Count) + i];// / tempMagnitude;

                if (tempMagnitude > 1)
                {
                    objectForces[(index * (int)ObjectType.Count) + i] /= tempMagnitude;
                }


                moveTowards += objectForces[(index * (int)ObjectType.Count) + i];


                dir.x = objectForces[(index * (int)ObjectType.Count) + i].x;
                dir.y = 0;
                dir.z = objectForces[(index * (int)ObjectType.Count) + i].y;


                builder.Label2D(pos + (math.normalize(dir) / 2), ((ObjectType)i).ToString(), 12);

                builder.Ray(pos, dir, Color.cyan);



            }

            if (maxMagnitude > 1)
            {
                maxMagnitude = 1;
            }
            else if (maxMagnitude == 0)
            {

                int test = 0;
                maxMagnitude = genes.GetAttraction(index, ObjectType.Sheep);
            }
            else
            {
                int test = 0;
            }

            moveTowards += MathUtil.ClampMagnitude(objectForces[(index * (int)ObjectType.Count) + (int)(ObjectType.Sheep)], maxMagnitude);
            pos.y = .5f;



            dir.x = objectForces[(index * (int)ObjectType.Count) + (int)ObjectType.Sheep].x;
            dir.y = 0;
            dir.z = objectForces[(index * (int)ObjectType.Count) + (int)ObjectType.Sheep].y;

            builder.Label2D(pos + (math.normalize(dir) / 2), ObjectType.Sheep.ToString(), 12);

            builder.Ray(pos, dir, Color.cyan);


            dir.x = moveTowards.x;
            dir.y = 0;
            dir.z = moveTowards.y;

            builder.Ray(pos, dir, Color.yellow);

            float headingCalculation = math.atan2(moveTowards.x, moveTowards.y);
            pos.y = .25f;

            builder.Ray(pos, math.forward(quaternion.Euler(new float3(0, headingCalculation, 0))), Color.magenta);

            if (headingCalculation < 0)
            {
                headingCalculation += 2 * math.PI;
            }

            float local = headingCalculation - headings[index];

            if (local == 0)
                return;

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
                = math.lerp(headings[index], headingCalculation,
                math.clamp((math.PI * 3 * genes.GetTurnRate(index)) * deltaTime
                / math.abs(local), 0, 1));

            if (headings[index] == float.NaN)
            {
                int test = 0;
            }



        }
    }

}



