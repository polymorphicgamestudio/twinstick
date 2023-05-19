using Drawing;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;



namespace ShepProject
{

    public class PathfindingManager : MonoBehaviour
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
         * 
         */

        public Transform gridOrigin;

        public NativeArray<OverlapBoxCommand> overlapCommands;
        public NativeArray<ColliderHit> overlapResults;
        public NativeArray<SquareNode> nodes;
        public SquareGridSetupData setupData;

        private int currentRows;
        private int currentColumns;

        public bool drawLabels;


        private void OnDrawGizmos()
        {

            /*
             * draw the grid
             * nodes when game isn't playing will need to be stored inside a C# array, not a native array
             *      if colors are wanted
             * 
             */

            if (setupData.rows == 0 || setupData.columns == 0)
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

            UpdateWalkableNodes();

            DrawGridJobPlaying drawGrid = new DrawGridJobPlaying();
            drawGrid.setupData = setupData;
            drawGrid.builder = DrawingManager.GetBuilder();
            drawGrid.nodes = nodes;
            drawGrid.drawLabels = drawLabels;
            drawGrid.Schedule(setupData.rows * setupData.columns, 1).Complete();
            //drawGrid.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


            drawGrid.builder.Dispose();

        }

        public void SetupNodes()
        {


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

            nodes = new NativeArray<SquareNode>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapCommands = new NativeArray<OverlapBoxCommand>(setupData.rows * setupData.columns, Allocator.Persistent);
            overlapResults = new NativeArray<ColliderHit>(setupData.rows * setupData.columns, Allocator.Persistent);
            currentRows = setupData.rows;
            currentColumns = setupData.columns;

            CreateNodesJob cnj = new CreateNodesJob();
            cnj.nodes = nodes;
            cnj.columns = setupData.columns;
            cnj.rows = setupData.rows;
            cnj.Schedule(setupData.rows, SystemInfo.processorCount).Complete();


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


    }


}