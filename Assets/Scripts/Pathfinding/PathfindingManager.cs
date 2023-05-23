using Drawing;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Profiling;
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

        public Transform gridOrigin;
        public Transform testPosition;

        public Collider playableArea;

        [Space(20)]
        public Transform StartPosition;
        public Transform EndPosition;

        public NativeArray<OverlapBoxCommand> overlapCommands;
        public NativeArray<ColliderHit> overlapResults;
        public NativeArray<SquareNode> nodes;

        [Space(20)]
        public SquareGridSetupData setupData;


        private int currentRows;
        private int currentColumns;
        private float currentNodeLength;

        public bool drawLabels;


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


                setupData.origin = gridOrigin.position;
                DrawGridJobNotPlaying drawGrid = new DrawGridJobNotPlaying();
                drawGrid.setupData = setupData;
                drawGrid.builder = DrawingManager.GetBuilder();
                drawGrid.drawLabels = drawLabels;
                drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


                drawGrid.builder.Dispose();

            }
            else
            {

            }




        }

        private void Awake()
        {
            setupData.origin = gridOrigin.position;
            CreateNodes();


        }

        // Start is called before the first frame update
        void Start()
        {

            

        }

        // Update is called once per frame
        void Update()
        {

            setupData.origin = gridOrigin.position;
            if (currentRows != setupData.rows || currentColumns != setupData.columns)
            {
                CreateNodes();

            }

            if (currentNodeLength != setupData.nodeLength)
            {

                UpdateNodeInfo();

            }


            //GetNodeIndexFromPosition(testPosition.position);

            //whenever something is placed on grid or removed, need to update the walkable nodes
            UpdateWalkableNodes();

            Profiler.BeginSample("Find Path");

            FindPath(StartPosition.position, EndPosition.position);

            Profiler.EndSample();

            //draw grid every function if gizmos are enabled
            DrawGridJobPlaying drawGrid = new DrawGridJobPlaying();
            drawGrid.setupData = setupData;
            drawGrid.builder = DrawingManager.GetBuilder();
            drawGrid.nodes = nodes;
            drawGrid.drawLabels = drawLabels;
            drawGrid.Schedule(setupData.rows * setupData.columns, 1).Complete();
            //drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


            drawGrid.builder.Dispose();

        }

        private void OnDisable()
        {
            if (nodes.IsCreated)
                nodes.Dispose();

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();
                overlapResults.Dispose();
            }
        }

        private void CreateNodes()
        {

            if (nodes.IsCreated)
                nodes.Dispose();

            if (overlapCommands.IsCreated)
            {
                overlapCommands.Dispose();
                overlapResults.Dispose();
            }

            if (setupData.rows < 1 || setupData.columns < 1)
                return;


            nodes = new NativeArray<SquareNode>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapCommands = new NativeArray<OverlapBoxCommand>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapResults = new NativeArray<ColliderHit>(setupData.rows * setupData.columns, Allocator.Persistent);
            currentRows = setupData.rows;
            currentColumns = setupData.columns;
            currentNodeLength = setupData.nodeLength;

            CreateNodesJob cnj = new CreateNodesJob();
            cnj.nodes = nodes;
            cnj.nodeLength = setupData.nodeLength;
            cnj.columns = setupData.columns;
            cnj.rows = setupData.rows;
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
            uni.Schedule(setupData.rows, SystemInfo.processorCount).Complete();



        }

        private void UpdateWalkableNodes()
        {

            CreateOverlapCommandsJob coc = new CreateOverlapCommandsJob();
            coc.mask = LayerMask.GetMask("Wall") | LayerMask.GetMask("Tower");
            coc.commands = overlapCommands;
            coc.nodes = nodes;
            coc.rows = currentRows;
            coc.columns = currentColumns;
            coc.origin = setupData.origin;
            coc.halfNodeLength = setupData.nodeLength / 2f;
            coc.Schedule(currentRows * currentColumns, SystemInfo.processorCount).Complete();

            OverlapBoxCommand.ScheduleBatch
                (overlapCommands, overlapResults, SystemInfo.processorCount, 1).Complete();


            UpdateWalkableNodesJob uwn = new UpdateWalkableNodesJob();
            uwn.nodes = nodes;
            uwn.overlapResults = overlapResults;
            uwn.Schedule(setupData.rows * setupData.columns, SystemInfo.processorCount).Complete();




        }


        private void QueuePath(Vector3 start, Vector3 end)
        {


        }

        private void FindPath(Vector3 start, Vector3 end)
        {


            /*
             * start by converting the starting position into a nodeIndex
             * same with the end position
             * 
             * 
             * 
             * 
             */



            /*
             * from starting node, check all adjacent nodes next closest
             * 
             * g = distance from starting node
             * h = distance from end node
             * f = cost of g + h
             * 
             * 
             * storing open and closed nodes
             * 
             * only way would be to store indices of nodes in a list/chunked array
             * nice to save some memory and only use ushorts for each open/closed index
             * 
             * 
             */



            Profiler.BeginSample("Get Node Index From Position");

            int startNodeIndex = GetNodeIndexFromPosition(start);
            int endNodeIndex = GetNodeIndexFromPosition(end);

            Profiler.EndSample();

            PathNode currentNode = new PathNode();
            currentNode.index = startNodeIndex;
            currentNode.hCost = math.distance(nodes[startNodeIndex].position,
                nodes[endNodeIndex].position);


            /*
             * 
             * Performance Increases
             * 
             * NativeParallelMultiHashMap for open nodes
             *      - key will be fCost, values will be PathNodes
             *      - get the lowest f cost node, faster than searching array
             *      - will have to store each different fcost in a list to use as keys
             *      
             *      1. Open Nodes, if fcost is key, how to easily get index to see if it's contained?
             *          - have another hashmap with indices as keys
             *              - not a big fan of this, seems quite expensive
             *          - 
             *      
             *      
             *      
             *  NativeParallelHashMap for closed nodes
             *      - to check if it contains any neighbor nodes that can be skipped
             *      - to also help trace the path back quicker to start
             *      use the node indices as the key
             *       
             * 
             * 
             * Use less memory inside of GetNodeIndexFromPosition
             * 
             * discuss whether we can switch from ints to ushorts for indices to conserver
             *      more memory and have higher amounts of cache hits
             * 
             * 
             */


            #region A* Version 2

            Profiler.BeginSample("Allocation");

            NativeParallelMultiHashMap<int, PathNode> openNodeDifficulties = new NativeParallelMultiHashMap<int, PathNode>(500, Allocator.Temp);
            NativeParallelMultiHashMapIterator<int> it;
            NativeParallelHashMap<int, int> openNodeKeys = new NativeParallelHashMap<int, int>(500, Allocator.Temp);

            NativeList<int> fCostKeys = new NativeList<int>(500, Allocator.Temp);

            NativeParallelHashMap<int, PathNode> closedNodes = new NativeParallelHashMap<int, PathNode>(500, Allocator.Temp);

            Profiler.EndSample();

            openNodeDifficulties.Add((int)(currentNode.FCost * 100), currentNode);
            openNodeKeys.Add(currentNode.index, 0);

            fCostKeys.Add((int)(currentNode.FCost * 100));

            int outDifficultyKey;
            PathNode checkNode = new PathNode();
            int minKeyCost = int.MaxValue;
            int minKeyCostIndex = 0;

            Profiler.BeginSample("Checking Nodes");

            while (currentNode.index != endNodeIndex)
            {

                minKeyCostIndex = 0;
                minKeyCost = int.MaxValue;


                Profiler.BeginSample("Check FCosts");

                for (int i = 0; i < fCostKeys.Length; i++)
                {
                    if (fCostKeys[i] < minKeyCost)
                    {
                        minKeyCost = fCostKeys[i];
                        minKeyCostIndex = i;
                    }

                }

                Profiler.EndSample();

                if (!openNodeDifficulties.TryGetFirstValue(minKeyCost, out currentNode, out it))
                {
                    try
                    {

                        fCostKeys.RemoveAt(minKeyCostIndex);
                    }
                    catch (Exception e)
                    {

                        int test = 0;
                    }

                    continue;
                }


                //has a new currentNode, remove it from open nodes, add it to closed nodes, then check neighbors

                closedNodes.Add(currentNode.index, currentNode);
                openNodeKeys.Remove(currentNode.index);
                openNodeDifficulties.Remove((int)(currentNode.FCost * 100), currentNode);

                Profiler.BeginSample("Check Neighbors");

                if (currentNode.index % currentColumns > 0)
                    //check left node
                    CheckNeighborNode(currentNode.index - 1);

                if (currentNode.index % (currentColumns) != currentColumns - 1 && currentNode.index != endNodeIndex)
                    //check right node
                    CheckNeighborNode(currentNode.index + 1);

                if ((currentNode.index + currentColumns) < (currentRows * (currentColumns - 1)) && currentNode.index != endNodeIndex)
                {
                    //check node above
                    CheckNeighborNode(currentNode.index + currentColumns);
                }

                if ((currentNode.index - currentColumns) > 0 && currentNode.index != endNodeIndex)
                {
                    //check node below
                    CheckNeighborNode(currentNode.index - currentColumns);
                }


                Profiler.EndSample();



                void CheckNeighborNode(int index)
                {

                    if (index < 0 || index >= nodes.Length)
                    {
                        Debug.LogError("Index out of range");
                        return;
                    }

                    if (closedNodes.ContainsKey(index))
                    {
                        return;
                    }

                    if (!nodes[index].walkable)
                    {

                        closedNodes.Add(index, new PathNode() { index = index });
                        //closedNodes.Add(new PathNode() { index = index });
                        return;
                    }


                    Profiler.BeginSample("Set up neighbor node");

                    PathNode neighborNode = new PathNode();
                    neighborNode.parentIndex = currentNode.index;
                    neighborNode.index = index;
                    neighborNode.gCost = math.distance(nodes[index].position, nodes[startNodeIndex].position);
                    neighborNode.hCost = math.distance(nodes[index].position, nodes[endNodeIndex].position);

                    Profiler.EndSample();

                    if (neighborNode.index == endNodeIndex)
                    {
                        currentNode = neighborNode;
                    }

                    if (openNodeKeys.TryGetValue(index, out outDifficultyKey))
                    {
                        if (openNodeDifficulties.TryGetFirstValue(outDifficultyKey, out checkNode, out it))
                        {
                            while (!checkNode.Equals(neighborNode))
                            {
                                openNodeDifficulties.TryGetNextValue(out checkNode, ref it);

                            }

                            if (checkNode.hCost < neighborNode.hCost)
                            {

                                openNodeDifficulties.SetValue(checkNode, it);
                                openNodeKeys[checkNode.index] = (int)(checkNode.FCost * 100);
                            }

                        }

                    }
                    else
                    {

                        openNodeDifficulties.Add((int)(neighborNode.FCost * 100), neighborNode);
                        openNodeKeys.Add(neighborNode.index, (int)(neighborNode.FCost * 100));
                        if (!fCostKeys.Contains((int)(neighborNode.FCost * 100)))
                        {
                            fCostKeys.Add((int)(neighborNode.FCost * 100));
                        }

                    }



                }


            }

            Profiler.EndSample();


            Profiler.BeginSample("Trace Path");

            NativeArray<int> indices = new NativeArray<int>(200, Allocator.Temp);

            int j = 0;
            while (currentNode.index != startNodeIndex)
            {
                indices[j] = currentNode.index;

                closedNodes.TryGetValue(currentNode.parentIndex, out currentNode);
                j++;

            }
            indices[j] = currentNode.index;
            j++;

            Profiler.EndSample();

            //draw final path

            #region Draw Final Path

            //Profiler.BeginSample("Draw Final Path");

            //float3 pos = new float3();
            //for (int k = 0; k < j; k++)
            //{
            //    pos.x = nodes[indices[k]].position.x;
            //    pos.z = nodes[indices[k]].position.y;
            //    pos += setupData.origin;

            //    Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(currentNodeLength), Color.cyan);

            //}

            //Profiler.EndSample();

            #endregion



            indices.Dispose();







            openNodeDifficulties.Dispose();
            openNodeKeys.Dispose();
            fCostKeys.Dispose();

            closedNodes.Dispose();


            #endregion

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

            int rowStart = (int)(localPosition.x / setupData.nodeLength);
            int column = (int)(localPosition.z / setupData.nodeLength) * currentColumns;

            int index = rowStart + column;

            //debugging purposes only
            //float3 pos = setupData.origin + new float3(nodes[index].position.x, 0, nodes[index].position.y);
            //Drawing.Draw.Label2D(position, "I: " + index, Color.black);
            ////"L: " + localPosition);
            //Drawing.Draw.SolidPlane(pos, new float3(0, 1, 0), new float2(setupData.nodeLength), Color.blue);

            return index;

            //return 0;
        
        }


        private void SetupVectorField()
        {


        }



    }


}