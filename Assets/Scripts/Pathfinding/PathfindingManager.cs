using Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace ShepProject
{

    public class PathfindingManager : SystemBase
    {

        /*
         * will be square grid
         * 
         * need to set up grid with positions in awake
         *      then set up vector field for all nodes
         *      
         *      
         * need a way to query for the shortest path from 
         *      starting node to destination node using A* algorithm
         *      - one way is using vector field for nodes that already know the shortest path
         *      
         *      - other way will just calculate path from nodes
         *          just in case we need it somewhere
         *      
         * need to queue jobs up in update and then by late update the jobs
         *      will be finished so that they cause any halting
         * 
         * 
         * 
         * setting up vector field
         *      needs to all be within the playable area, otherwise there will be no path
         *      
         *      start with node[0] and then solve all of its paths
         *      then after that is done 
         *      
         *      
         */


        #region Variables

        //public Transform gridOrigin;
        //public Transform testPosition;

        public Collider playableArea;

        [Space(20)]
        public Transform StartPosition;
        public Transform EndPosition;

        public NativeArray<OverlapBoxCommand> overlapCommands;
        public NativeArray<ColliderHit> overlapResults;
        public NativeArray<SquareNode> nodes;
        private NativeArray<float2> directions;

        public NativeParallelMultiHashMap<int, PathNode> openNodeDifficulties;
        public NativeParallelHashMap<int, PathNode> closedNodes;
        public NativeParallelHashMap<int, int> openNodeKeys;
        public NativeList<int> fCostKeys;
        public NativeArray<byte> vectorField;
        public NativeArray<bool> vectorPathsFilled;

        public NativeArray<PathNode> finalPathIndices;

        [Space(20)]
        public SquareGridSetupData setupData;

        [Space(20)]
        [SerializeField]
        private bool vectorFieldNeedsUpdate;
        private int currentRows;
        private int currentColumns;
        private float currentNodeLength;

        public bool drawIndexLabels;
        public bool jobPath;
        public bool drawNodeInfo;

        #endregion

        #region Gizmos

        private void OnDrawGizmos()
        {

            /*
             * draw the grid
             * nodes when game isn't playing will need to be stored inside a C# array, not a native array
             *      if colors are wanted
             * 
             */

            if (setupData.rows < 1 || setupData.columns < 1)
                return;
            if (!Application.isPlaying)
            {


                setupData.origin = transform.position;
                DrawGridJobNotPlaying drawGrid = new DrawGridJobNotPlaying();
                drawGrid.setupData = setupData;
                drawGrid.builder = DrawingManager.GetBuilder();
                drawGrid.drawLabels = drawIndexLabels;
                drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


                drawGrid.builder.Dispose();

            }
            else
            {

            }




        }

        #endregion

        private void Awake()
        {
            setupData.origin = transform.position;
            setupData.originfloat2 = new float2(transform.position.x, transform.position.z);

            openNodeDifficulties = new NativeParallelMultiHashMap<int, PathNode>(10000, Allocator.Persistent);
            closedNodes = new NativeParallelHashMap<int, PathNode>(10000, Allocator.Persistent);
            openNodeKeys = new NativeParallelHashMap<int, int>(10000, Allocator.Persistent);
            fCostKeys = new NativeList<int>(10000, Allocator.Persistent);
            finalPathIndices = new NativeArray<PathNode>(1000, Allocator.Persistent);
            //CreateNodes();

            directions = new NativeArray<float2>(8, Allocator.Persistent);
            directions[0] = new float2(-1, 0);        // math.PI; 
            directions[1] = new float2(-1f, 1f);    //math.radians(135);
            directions[2] = new float2(0, 1);    //math.PI / 2f;
            directions[3] = new float2(1, 1);    //math.radians(45);
            directions[4] = new float2(1, 0);    //0;
            directions[5] = new float2(1, -1);    //math.radians(315);
            directions[6] = new float2(0, -1);    //3 * math.PI / 2f;
            directions[7] = new float2(-1, -1);    //math.radians(225);




        }

        // Update is called once per frame
        void Update()
        {

            setupData.origin = transform.position;
            if (currentRows != setupData.rows || currentColumns != setupData.columns)
            {
                CreateNodes();
                vectorFieldNeedsUpdate = true;
            }

            else if (currentNodeLength != setupData.nodeLength)
            {

                UpdateNodeInfo();
                vectorFieldNeedsUpdate = true;
            }


            //GetNodeIndexFromPosition(testPosition.position);

            if (vectorFieldNeedsUpdate)
            {
                vectorFieldNeedsUpdate = false;

                //whenever something is placed on grid or removed, need to update the walkable nodes
                UpdateWalkableNodes();

                Profiler.BeginSample("Setup Vector Field");

                SetupVectorField();

                Profiler.EndSample();


            }

            //int toNode = GetNodeIndexFromPosition(StartPosition.position);
            //float3 dir = new float3();
            //float3 pos = new float3();

            //for (int i = 0; i < nodes.Length; i++)
            //{
            //    if (toNode == i)
            //        continue;

            //    pos.x = nodes[i].position.x;
            //    pos.z = nodes[i].position.y;

            //    int vectorFieldIndex = i * (currentColumns * currentRows) + toNode;
            //    int direction = vectorField[vectorFieldIndex];

            //    if (direction >= 8)
            //    {
            //        int test = 0;
            //        continue;
            //    }

            //    dir.x = directions[direction].x;
            //    dir.z = directions[direction].y;

            //    Draw.Arrow(pos, pos + dir);
                

            //}


            
            //draw grid every function if gizmos are enabled
            DrawGridJobPlaying drawGrid = new DrawGridJobPlaying();
            drawGrid.setupData = setupData;
            drawGrid.builder = DrawingManager.GetBuilder();
            drawGrid.nodes = nodes;
            drawGrid.drawLabels = drawIndexLabels;
            drawGrid.Schedule(setupData.rows * setupData.columns, 1).Complete();
            //drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


            drawGrid.builder.Dispose();

        }

        private void OnDisable()
        {
            if (nodes.IsCreated)
                nodes.Dispose();

            directions.Dispose();

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();

            }

            openNodeDifficulties.Dispose();
            closedNodes.Dispose();
            openNodeKeys.Dispose();
            fCostKeys.Dispose();
            finalPathIndices.Dispose();
            vectorField.Dispose();
        }


        public void QueueVectorFieldUpdate()
        {

            vectorFieldNeedsUpdate = true;

        }

        #region Setting Up Grid

        private void CreateNodes()
        {

            if (nodes.IsCreated)
            {
                nodes.Dispose();
                vectorField.Dispose();
                vectorPathsFilled.Dispose();
            }

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();
                overlapResults.Dispose();
            }

            if (setupData.rows < 1 || setupData.columns < 1)
                return;


            currentRows = setupData.rows;
            currentColumns = setupData.columns;
            currentNodeLength = setupData.nodeLength;

            nodes = new NativeArray<SquareNode>(setupData.rows * setupData.columns, Allocator.Persistent);
            vectorField = new NativeArray<byte>((int)(math.pow((currentRows * currentColumns), 2)), Allocator.Persistent);
            vectorPathsFilled = new NativeArray<bool>(currentColumns * currentRows, Allocator.Persistent);

            overlapCommands = new NativeArray<OverlapBoxCommand>(setupData.rows * setupData.columns, Allocator.Persistent);
            //overlapResults = new NativeArray<ColliderHit>(setupData.rows * setupData.columns * 3, Allocator.Persistent);



            CreateNodesJob cnj = new CreateNodesJob();
            cnj.nodes = nodes;
            cnj.nodeLength = setupData.nodeLength;
            cnj.columns = setupData.columns;
            cnj.rows = setupData.rows;
            cnj.origin = new float2(transform.position.x, transform.position.z);

            cnj.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


        }

        private void UpdateNodeInfo()
        {


            currentNodeLength = setupData.nodeLength;

            UpdateNodeInfoJob uni = new UpdateNodeInfoJob();
            uni.nodes = nodes;
            uni.nodeLength = setupData.nodeLength;
            uni.columns = setupData.columns;
            uni.rows = setupData.rows;
            uni.origin = new float2(transform.position.x, transform.position.z);
            uni.Schedule(setupData.rows, SystemInfo.processorCount).Complete();



        }

        private void UpdateWalkableNodes()
        {

            CreateOverlapCommandsJob coc = new CreateOverlapCommandsJob();
            coc.mask = LayerMask.GetMask("Wall");// | LayerMask.GetMask("Tower");
            coc.commands = overlapCommands;
            coc.nodes = nodes;
            coc.rows = currentRows;
            coc.columns = currentColumns;
            coc.origin = setupData.origin;
            coc.halfNodeLength = setupData.nodeLength / 2f;
            coc.Schedule(currentRows * currentColumns, SystemInfo.processorCount).Complete();

            overlapResults = new NativeArray<ColliderHit>(currentRows * currentColumns * 3, Allocator.TempJob);

            OverlapBoxCommand.ScheduleBatch
                (overlapCommands, overlapResults, SystemInfo.processorCount, 3).Complete();



            #region Update Node Connections

            Vector3 vect = new Vector3();
            Vector3 local = new Vector3();

            for (int i = 0; i < overlapCommands.Length; i++)
            {
                SquareNode node = nodes[i];
                node.ResetObstructions();

                int index = i * 3;
                local.x = nodes[i].position.x;
                local.y = 0;
                local.z = nodes[i].position.y;

                vect.x = nodes[i].position.x;
                vect.y = 0;
                vect.z = nodes[i].position.y;

                float x = 0;
                float z = 0;

                for (int j = 0; j < 3; j++)
                {


                    if (overlapResults[index].instanceID != 0)
                    {

                        local = overlapResults[index].collider.ClosestPointOnBounds(vect);
                        local -= vect;

                        x = math.abs(local.x);
                        z = math.abs(local.z);
                        
                        Draw.Label2D(new Vector3(nodes[i].position.x, 0, nodes[i].position.y), local.ToString());

                        if (x > 0)
                        {

                            //is directly to one of the sides

                            if (local.x < 0)
                            {
                                //no connection to the diagonal top and bottom left as well as orthaginal left

                                //directions.w = true;


                                if (z < 0.00001f)
                                {
                                    node.TopLeftObstructed = true;
                                    node.BottomLeftObstructed = true;
                                }

                                else if (local.z > 0)
                                {
                                    node.TopLeftObstructed = true;
                                }
                                else
                                {
                                    node.BottomLeftObstructed = true;
                                }


                                if (z < 0.00001f)
                                    node.LeftObstructed = true;


                            }
                            else
                            {
                                //no connection to the diagonal top and bottom left as well as orthaginal left
                                //directions.x = true;

                                if (z < 0.00001f)
                                {
                                    node.TopRightObstructed = true;
                                    node.BottomRightObstructed = true;
                                }

                                else if (local.z > 0)
                                {
                                    node.TopRightObstructed = true;
                                }
                                else
                                {
                                    node.BottomRightObstructed = true;
                                }


                                if (z < 0.00001f)
                                    node.RightObstructed = true;


                            }

                        }

                        if (z > 0)
                        {

                            //is directly to one of the sides

                            if (local.z < 0)
                            {
                                //no connection to the diagonal left and right below as well as orthaginal below
                                //directions.y = true;

                                if (x < 0.00001f)
                                {
                                    node.BottomLeftObstructed = true;
                                    node.BottomRightObstructed = true;
                                }

                                else if (local.x < 0)
                                {
                                    node.BottomLeftObstructed = true;
                                }
                                else
                                {
                                    node.BottomRightObstructed = true;
                                }

                                if (x < 0.00001f)
                                    node.BottomObstructed = true;


                            }
                            else
                            {
                                //above 
                                //directions.z = true;

                                if (x < 0.00001f)
                                {
                                    node.TopLeftObstructed = true;
                                    node.TopRightObstructed = true;
                                }
                                else if (local.x < 0)
                                {
                                    node.TopLeftObstructed = true; 
                                }
                                else
                                {
                                    node.TopRightObstructed = true;
                                }



                                if (x < 0.00001f)
                                    node.TopObstructed = true;


                            }

                        }


                        //node.walkable = false;
                    }


                    index++;

                }

                node.walkable = true;
                nodes[i] = node;


            }




            overlapResults.Dispose();

            #endregion


        }

        #endregion

        #region Path Related


        private void SetupVectorField()
        {

            Profiler.BeginSample("Clear Variables");

            ResetNativeArrayJob<byte> resetJob = new ResetNativeArrayJob<byte>();
            resetJob.array = vectorField;
            JobHandle handle = resetJob.Schedule(vectorField.Length, SystemInfo.processorCount);

            ResetNativeArrayJob<bool> resetFields = new ResetNativeArrayJob<bool>();
            resetFields.array = vectorPathsFilled;
            JobHandle fieldBools = resetFields.Schedule(vectorPathsFilled.Length, SystemInfo.processorCount);

            openNodeDifficulties.Clear();
            closedNodes.Clear();
            openNodeKeys.Clear();
            fCostKeys.Clear();



            Profiler.EndSample();

            GenerateVectorFieldJob job = new GenerateVectorFieldJob();
            job.nodes = nodes;
            job.openNodeDifficulties = openNodeDifficulties;
            job.closedNodes = closedNodes;
            job.openNodeKeys = openNodeKeys;
            job.vectorField = vectorField;
            job.vectorPathsFilled = vectorPathsFilled;

            job.fCostKeys = fCostKeys;
            job.finalPathIndices = finalPathIndices;
            job.builder = DrawingManager.GetBuilder();
            job.origin = setupData.origin;
            job.columns = currentColumns;
            job.rows = currentRows;
            job.nodeLength = currentNodeLength;
            job.scalar = 100;
            job.drawNodeInfo = drawNodeInfo;
            job.startNodeIndex = GetNodeIndexFromPosition(StartPosition.position);
            job.endNodeIndex = GetNodeIndexFromPosition(EndPosition.position);

            //complete the reset job
            handle.Complete();
            fieldBools.Complete();


            //do the vector field setup now
            job.Schedule().Complete();

            job.builder.Dispose();

        }

        private bool ContainsInGrid (float3 position)
        {
            if (position.x > (setupData.origin.x + (setupData.nodeLength * setupData.columns)) ||
                position.x < setupData.origin.x)
            {
                return false;

            }

            if (position.z > (setupData.origin.z + ((setupData.nodeLength) * setupData.rows)) ||
                position.z < setupData.origin.z)
            {
                return false;

            }


            return true;


        }

        private int GetNodeIndexFromPosition(float3 position)
        {

            if (!ContainsInGrid(position))
            {

                Debug.LogError("Position not contained within grid.");

                return -1;

            }

            float3 localPosition = (float3)(position - setupData.origin);

            //int rowStart = (int)(localPosition.x / setupData.nodeLength);
            //int column = (int)(localPosition.z / setupData.nodeLength) * currentColumns;

            //int index = rowStart + column;

            //debugging purposes only
            //float3 pos = setupData.origin + new float3(nodes[index].position.x, 0, nodes[index].position.y);
            //Drawing.Draw.Label2D(position, "I: " + index, Color.black);
            ////"L: " + localPosition);
            //Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(setupData.nodeLength), Color.blue);

            return (int)(localPosition.x / setupData.nodeLength)
                + (int)(localPosition.z / setupData.nodeLength) * currentColumns;

        }


        #endregion

        public PathQueryStructure GetQueryingStructure()
        {

            return new PathQueryStructure(vectorField, directions, setupData);

        }

    }


}