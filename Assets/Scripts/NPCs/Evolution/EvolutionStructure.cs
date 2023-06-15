using System.IO;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace ShepProject
{


    public struct EvolutionStructure
    {

        #region Variables

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

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> fitnessRanges;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ChromosoneParents> chromosoneParents;

        #endregion

        public EvolutionStructure(int maxGeneticObjects, int maxObjects,
             int genesPerObject, SigmoidInfo[] sigmoids, Allocator type = Allocator.Persistent)
        {

            traits = new NativeArray<float>(maxObjects * genesPerObject, type);
            genes = new NativeArray<float>(maxObjects * genesPerObject, type);

            ids = new NativeArray<ushort>(maxGeneticObjects, type);
            availables = new NativeList<ushort>(maxGeneticObjects, type);

            previousGenes = new NativeArray<float>(maxObjects * genesPerObject, type);
            slimeFitnesses = new NativeArray<int>(maxObjects, type);
            fitnessRanges = new NativeArray<int>(maxObjects, type);

            chromosoneParents = new NativeArray<ChromosoneParents>(maxObjects, type);

            //need a way to set these 
            this.sigmoids = new NativeArray<Sigmoid>(sigmoids.Length, type);

            for (int i = 0; i < sigmoids.Length; i++)
            {

                this.sigmoids[i] = sigmoids[i].sigmoid;

            }


            for (int i = 0; i < ids.Length; i++)
            {
                ids[i] = ushort.MaxValue;

            }
            for (int i = 0; i < availables.Capacity; i++)
            {
                availables.AddNoResize((ushort)((availables.Capacity) - i));

            }


            SetupInitialSlimeValues();

            WriteValuesToFile(sigmoids);

        }


        public void Dispose()
        {
            traits.Dispose();
            genes.Dispose();
            ids.Dispose();
            availables.Dispose();

            previousGenes.Dispose();
            sigmoids.Dispose();


            slimeFitnesses.Dispose();
            fitnessRanges.Dispose();


        }




        public void SetupInitialSlimeValues(float mutationSize = 0)
        {

            int objectTypeIndex = 0;
            int sigmoidIndex = 0;
            Random r = Random.CreateFromIndex((uint)(Time.time * Time.realtimeSinceStartup * 902385));

            for (int i = 0; i < ids.Length; i++)
            {

                //main slime type
                genes[objectTypeIndex] = r.NextInt(0, (int)SlimeType.Count);
                traits[objectTypeIndex] = genes[objectTypeIndex];

                //secondary slime type
                objectTypeIndex++;
                genes[objectTypeIndex] = r.NextInt(0, (int)SlimeType.Count);
                traits[objectTypeIndex] = genes[objectTypeIndex];

                sigmoidIndex = 0;
                for (int j = (int)Genes.MainResistance; j < (int)Genes.Health; j++)
                {

                    objectTypeIndex++;
                    //this will need to be able to have a range for a mutation
                    genes[objectTypeIndex] = 0;
                    traits[objectTypeIndex] = sigmoids[sigmoidIndex].GetTraitValue(genes[objectTypeIndex]);

                    sigmoidIndex++;

                }

                objectTypeIndex++;
                genes[objectTypeIndex] = 100;
                traits[objectTypeIndex] = 100;

                objectTypeIndex++;

            }




        }

        private void GenerateSlimesForNextWave(bool writeToFile, int waveNumber, SigmoidInfo[] info)
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
                writeToFileThread = new Thread(new ParameterizedThreadStart(WriteValuesToFile));
                writeToFileThread.Start(info);



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

        public void WriteValuesToFile(object infoObject)
        {


            SigmoidInfo[] info = (SigmoidInfo[])infoObject;

            //need to talk about where we want to put this file

            string filePath = Application.dataPath + "/geneWriteFile.csv";

            FileStream geneFile = File.Open(filePath, FileMode.OpenOrCreate);
            StreamWriter writer = new StreamWriter(geneFile);

            string output = "Player Distance Fitness, Main Type, Secondary Type,";

            for (int i = 0; i < info.Length; i++)
            {
                output += info[i].name +" Gene, " + info[i].name + " Trait, ";

            }

            output += " Health,";
            writer.WriteLine(output);



            int objectTypeIndex = 0;
            for (int h = 0; h < ids.Length; h++)
            {

                //player distance fitness
                writer.Write(slimeFitnesses[h] + ", ");

                writer.Write(traits[objectTypeIndex] + ", ");

                objectTypeIndex++;
                writer.Write(traits[objectTypeIndex] + ", ");

                //sigmoidIndex = 0;
                //otherwise just write the wave number and then the gene values underneath that
                for (int i = (int)Genes.MainResistance; i < (int)Genes.Health; i++)
                {
                    objectTypeIndex++;

                    writer.Write(genes[objectTypeIndex] + ", " + traits[objectTypeIndex] + ", ");


                }

                objectTypeIndex++;
                writer.Write(traits[objectTypeIndex] + ", ");


                //write fitness



                objectTypeIndex++;
                writer.WriteLine();

            }

            //writer.WriteLine();
            writer.Close();
            geneFile.Close();

            //then for each slime, write all of its gene values then do a new line
            //and continue writing them in the csv file



        }

        private int ObjectTypeIndex(int id)
        {

            if (ids[id] == ushort.MaxValue)
            {
                Debug.LogError("Object " + id + " hasn't been assigned.");
                return ids[id];
            }

            return ids[id] * (int)Genes.TotalGeneCount;
        }


        #region Get Specific Gene Methods

        public float GetAttraction(int id, int attraction)
        {
            return traits[ObjectTypeIndex(id) + 1 + attraction];
        }

        public float GetAttraction(int id, ObjectType attraction)
        {

            return GetAttraction(id, (int)attraction);
        }
        public void SetAttraction(int id, ObjectType attraction, float value)
        {

            traits[ObjectTypeIndex(id) + 1 + (int)attraction] = value;

        }

        public float GetViewRange(int id, ViewRange range)
        {

            return traits[ObjectTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range];
        }

        public void SetViewRange(int id, ViewRange range, float value)
        {

            traits[ObjectTypeIndex(id) + 1 + (int)ObjectType.Count + (int)range] = value;
        }

        //public float GetMainResistance(int id)
        //{
        //    return traits[ObjectTypeIndex(id)];
        //}

        //public void SetMainResistance(int id, float value)
        //{
        //    traits[ObjectTypeIndex(id)] = value;
        //}

        //public float GetSecondaryResistance(int id)
        //{
        //    return traits[ObjectTypeIndex(id)];
        //}

        //public void SetSecondaryResistance(int id, float value)
        //{
        //    traits[ObjectTypeIndex(id)] = value;
        //}



        public float GetOptimalDistance(int id, OptimalDistance optimalDistance)
        {
            return traits[ObjectTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance];
        }

        public void SetOptimalDistance(int id, OptimalDistance optimalDistance, float value)
        {
            traits[ObjectTypeIndex(id)
                + (int)ObjectType.Count + (int)ViewRange.Count
                + (int)DamageType.Count + (int)optimalDistance] = value;
        }

        public float GetSpeed(int id)
        {

            return traits[ObjectTypeIndex(id) + (int)GeneGroups.Speed];
        }

        public void SetSpeed(int id, float speed)
        {

            traits[ObjectTypeIndex(id) + (int)GeneGroups.Speed] = speed;
        }

        public float GetTurnRate(int id)
        {

            return traits[ObjectTypeIndex(id) + (int)GeneGroups.TurnRate];
        }

        public void SetTurnRate(int id, float value)
        {

            traits[ObjectTypeIndex(id) + (int)GeneGroups.TurnRate] = value;

        }

        public float GetHealth(int id)
        {

            return traits[ObjectTypeIndex(id) + (int)GeneGroups.Health];
        }

        public void SetHealth(int id, float value)
        {

            traits[ObjectTypeIndex(id) + (int)GeneGroups.Health] = value;

        }


        #endregion






        public float GetGene(int id, Genes gene)
        {

            return genes[ObjectTypeIndex(id) + (int)gene];
         
        }

        public float GetTrait(int id, Genes gene)
        {

            return traits[ObjectTypeIndex(id) + (int)gene];
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

    }






}
