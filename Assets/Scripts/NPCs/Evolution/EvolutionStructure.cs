using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;


namespace ShepProject
{


    public struct EvolutionStructure
    {

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> traits;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> genes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ushort> ids;

        [NativeDisableContainerSafetyRestriction]
        private NativeList<ushort> availables;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> previousGenes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Sigmoid> sigmoids;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> slimeFitnesses;
        private NativeArray<int> fitnessRanges;

        private NativeArray<ChromosoneParents> chromosoneParents;

        public EvolutionStructure(int maxObjects, int genesPerObject, Sigmoid[] sigmoids, Allocator type = Allocator.Persistent)
        {

            traits = new NativeArray<float>(maxObjects * genesPerObject, type);
            genes = new NativeArray<float>(maxObjects * genesPerObject, type);

            ids = new NativeArray<ushort>(maxObjects, type);
            availables = new NativeList<ushort>(maxObjects, type);

            previousGenes = new NativeArray<float>(maxObjects * genesPerObject, type);
            slimeFitnesses = new NativeArray<int>(maxObjects, type);
            fitnessRanges = new NativeArray<int>(maxObjects, type);

            chromosoneParents = new NativeArray<ChromosoneParents>(maxObjects, type);

            //need a way to set these 
            this.sigmoids = new NativeArray<Sigmoid>(sigmoids, type);


            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = ushort.MaxValue;

            }
            for (int i = 0; i < availables.Capacity; i++)
            {
                availables.AddNoResize((ushort)((availables.Capacity) - i));

            }




        }


        public void WriteGenesToFile()
        {


            //need to talk about where we want to put this file

            string filePath = Application.dataPath + "\\geneWriteFile.csv";
            bool exists = false;

            if (File.Exists(filePath))
                exists = true;

            FileStream geneFile = File.Open(filePath, FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(geneFile);
            
            if (!exists)
            {
                //write the column heads

            }
            else
            {
                //otherwise just write the wave number and then the gene values underneath that


            }
            
            //writer.WriteLine();


            //then for each slime, write all of its gene values then do a new line
            //and continue writing them in the csv file



        }




        private void GenerateSlimesForNextWave(bool writeToFile)
        {

            


            /*
             * 
             * need to move the slimes genes over to previousGenes
             * 
             * need a job to decide which slimes should reproduce
             * 
             * 
             * then from those genes, start a job to create the new chromosones 
             * 
             * and if writing needs to happen, start a job to write to a file while job for chromosones are creating
             *      this will need to use a C# thread
             * 
             * 
             * 
             */

            CopyNativeArrayJob<float> copyGenes = new CopyNativeArrayJob<float>();
            copyGenes.copyFrom = genes;
            copyGenes.copyTo = previousGenes;
            JobHandle handle = copyGenes.Schedule(genes.Length, SystemInfo.processorCount);

            fitnessRanges[0] = 0;
            fitnessRanges[1] = slimeFitnesses[0];

            for (int i = 2; i < slimeFitnesses.Length; i++)
            {
                fitnessRanges[i] = fitnessRanges[i - 1] + slimeFitnesses[i - 1];

            }

            ChooseParentSlimes chooseSlimes = new ChooseParentSlimes();
            chooseSlimes.fitnessRanges = fitnessRanges;
            chooseSlimes.slimeFitnesses = slimeFitnesses;

            chooseSlimes.parents = chromosoneParents;
            JobHandle evolutionHandle = chooseSlimes.Schedule(ids.Length, SystemInfo.processorCount);

            handle.Complete();

            Thread writeToFileThread = null;

            if (writeToFile)
            {
                writeToFileThread = new Thread(new ThreadStart(WriteGenesToFile));
                writeToFileThread.Start();
                


            }


            evolutionHandle.Complete();


            CreateNextGeneration createChromosones = new CreateNextGeneration();
            createChromosones.parentGenes = previousGenes;
            createChromosones.childGenes = genes;
            createChromosones.parents = chromosoneParents;

            handle = createChromosones.Schedule(ids.Length, SystemInfo.processorCount);


            handle.Complete();

            if (writeToFile)
            {

                writeToFileThread.Join();

            }



        }

        private int ObjectTypeIndex(int id)
        {

            if (ids[id] == ushort.MaxValue)
            {
                Debug.LogError("Object " + id + " hasn't been assigned.");
                return ids[id];
            }

            return ids[id] * (int)GeneGroups.TotalGeneCount;
        }

        public void AddGenesToObject(ushort objectID)
        {


            ids[objectID] = availables[availables.Length - 1];
            availables.RemoveAt(availables.Length - 1);

        }

        public void ResetIDGenes(int id)
        {

            for (int i = ObjectTypeIndex(id);
                i < ObjectTypeIndex(id) + (int)GeneGroups.TotalGeneCount; i++)
            {
                genes[i] = -1;

            }

            //need to add the returned ID to the end
            //and then the id at the end position to where this one was

            availables.AddNoResize(ids[id]);


            //need to implement this again

            //objectTypes[id] = ObjectType.None;
            ids[id] = ushort.MaxValue;
        }

        public void Dispose()
        {
            traits.Dispose();
            genes.Dispose();
            ids.Dispose();
            availables.Dispose();

            previousGenes.Dispose();
            sigmoids.Dispose();

        }








        #region Specific Trait Methods








        #endregion

    }






}
