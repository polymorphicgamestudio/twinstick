using System.IO;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;
using System.Collections.Generic;

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

        //[NativeDisableContainerSafetyRestriction]
        //private NativeList<ushort> availables;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> previousGenes;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<float> previousTraits;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<Sigmoid> sigmoids;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> slimeFitnesses;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<int> fitnessRanges;

        [NativeDisableContainerSafetyRestriction]
        private NativeArray<ChromosoneParents> parents;

        //[NativeDisableContainerSafetyRestriction]
        //private NativeArray<ChromosoneParents> previousParents;


        private ushort idIndex;

        private static string outputFilePath => Application.dataPath + "/geneWriteFile.csv";

        #endregion

        public EvolutionStructure(int maxGeneticObjects, int maxObjects,
             int genesPerObject, SigmoidInfo[] sigmoids, Allocator type = Allocator.Persistent)
        {

            traits = new NativeArray<float>(maxGeneticObjects * genesPerObject, type);
            genes = new NativeArray<float>(maxGeneticObjects * genesPerObject, type);

            ids = new NativeArray<ushort>(maxObjects, type);
            //availables = new NativeList<ushort>(maxGeneticObjects, type);

            previousGenes = new NativeArray<float>(maxGeneticObjects * genesPerObject, type);
            previousTraits = new NativeArray<float>(maxGeneticObjects * genesPerObject, type);
            slimeFitnesses = new NativeArray<int>(maxGeneticObjects, type);
            fitnessRanges = new NativeArray<int>(maxGeneticObjects, type);

            parents = new NativeArray<ChromosoneParents>(maxGeneticObjects, type);
            //previousParents = new NativeArray<ChromosoneParents>(maxGeneticObjects, type);

            idIndex = 0;
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

            SetupInitialSlimeValues();
            File.Delete(outputFilePath);


            

            /*
             * 
             * these two jobs are for writing initial values to file
             * 
             */
            CopyNativeArrayJob<float> copyGenes = new CopyNativeArrayJob<float>();
            copyGenes.copyFrom = genes;
            copyGenes.copyTo = previousGenes;
            JobHandle genesHandle = copyGenes.Schedule(genes.Length, SystemInfo.processorCount);

            CopyNativeArrayJob<float> copyTraits = new CopyNativeArrayJob<float>();
            copyTraits.copyFrom = traits;
            copyTraits.copyTo = previousTraits;
            JobHandle traitsHandle = copyTraits.Schedule(traits.Length, SystemInfo.processorCount);


            Random r = Random.CreateFromIndex((uint)(Time.realtimeSinceStartup * Time.deltaTime * 1290847));
            for (int i = 0; i < slimeFitnesses.Length; i++)
            {
                slimeFitnesses[i] = r.NextInt(10, 500);

            }


            genesHandle.Complete();
            traitsHandle.Complete();

            for (int i = 0; i < 5; i++)
            {
                GenerateSlimesForNextWave(true, new EvolutionDataFileInfo()
                {
                    info = sigmoids,
                    waveNumber = i
                });

            }


        }


        public void Dispose()
        {
            

            traits.Dispose();
            previousTraits.Dispose();

            genes.Dispose();
            ids.Dispose();

            previousGenes.Dispose();
            sigmoids.Dispose();


            slimeFitnesses.Dispose();
            fitnessRanges.Dispose();

            parents.Dispose();
            //previousParents.Dispose();

        }

        public void SetupInitialSlimeValues(float mutationSize = 0)
        {

            int objectTypeIndex = 0;
            int sigmoidIndex = 0;
            Random r = Random.CreateFromIndex((uint)(Time.time * Time.realtimeSinceStartup * 902385));

            for (int i = 0; i < slimeFitnesses.Length; i++)
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

        public void GenerateSlimesForNextWave(bool writeToFile, EvolutionDataFileInfo fileInfo)
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

            idIndex = 0;
            CopyNativeArrayJob<float> copyGenes = new CopyNativeArrayJob<float>();
            copyGenes.copyFrom = genes;
            copyGenes.copyTo = previousGenes;
            JobHandle genesHandle = copyGenes.Schedule(genes.Length, SystemInfo.processorCount);

            CopyNativeArrayJob<float> copyTraits = new CopyNativeArrayJob<float>();
            copyTraits.copyFrom = traits;
            copyTraits.copyTo = previousTraits;
            JobHandle traitsHandle = copyTraits.Schedule(traits.Length, SystemInfo.processorCount);


            fitnessRanges[0] = 0;
            fitnessRanges[1] = slimeFitnesses[0];

            for (int i = 2; i < slimeFitnesses.Length; i++)
            {
                fitnessRanges[i] = fitnessRanges[i - 1] + slimeFitnesses[i - 1];

            }


            ChooseParentSlimes chooseSlimes = new ChooseParentSlimes();
            chooseSlimes.fitnessRanges = fitnessRanges;
            chooseSlimes.slimeFitnesses = slimeFitnesses;
            chooseSlimes.elapsedTime = Time.realtimeSinceStartup;

            chooseSlimes.parents = parents;
            JobHandle evolutionHandle = chooseSlimes.Schedule(slimeFitnesses.Length, SystemInfo.processorCount);

            evolutionHandle.Complete();

            genesHandle.Complete();
            traitsHandle.Complete();

            Thread writeToFileThread = null;

            if (writeToFile)
            {
                writeToFileThread = new Thread(new ParameterizedThreadStart(WriteValuesToFile));
                writeToFileThread.Start(fileInfo);

            }

            CreateNextGeneration createChromosones = new CreateNextGeneration();
            createChromosones.parentGenes = previousGenes;
            createChromosones.childGenes = genes;
            createChromosones.parents = parents;
            createChromosones.elapsedTime = Time.realtimeSinceStartup;
            createChromosones.Schedule(slimeFitnesses.Length, SystemInfo.processorCount).Complete();

            UpdateTraitValues utv = new UpdateTraitValues();
            utv.genes = genes;
            utv.sigmoids = sigmoids;
            utv.traits = traits;
            utv.Schedule(slimeFitnesses.Length, SystemInfo.processorCount).Complete();

            if (writeToFile)
            {

                writeToFileThread.Join();

            }



        }

        public void WriteValuesToFile(object infoObject)
        {

            EvolutionDataFileInfo writeInfo = (EvolutionDataFileInfo)infoObject;
            SigmoidInfo[] info = writeInfo.info;

            //need to talk about where we want to put this file

            //need to also have two columns for parent's IDs of current slime

            if (!File.Exists(outputFilePath))
                File.Create(outputFilePath).Close();



            List<string> newLines = new List<string>(slimeFitnesses.Length + 10);

            if (writeInfo.waveNumber == 0)
            {
                newLines.Add("Initial Values");
            }
            else
            {
                newLines.Add("Wave " + writeInfo.waveNumber);
            }
                
            string output = "Player Distance Fitness, Parent One, Parent Two, Main Type, Secondary Type,";

            for (int i = 0; i < info.Length; i++)
            {
                output += info[i].name +" Gene, " + info[i].name + " Trait, ";

            }

            output += " Health,";
            newLines.Add(output);


            //int objectTypeIndex = 0;
            for (int h = 0; h < slimeFitnesses.Length; h++)
            {

                //player distance fitness
                output = slimeFitnesses[h] + ", ";

                output += parents[h].parentOne + ", " + parents[h].parentTwo + ", ";

                //main type
                output += traits[(h * (int)Genes.TotalGeneCount)] + ", ";

                //objectTypeIndex++;
                output += traits[(h * (int)Genes.TotalGeneCount) + 1] + ", ";

                //sigmoidIndex = 0;
                //otherwise just write the wave number and then the gene values underneath that
                for (int i = (int)Genes.MainResistance; i < (int)Genes.Health; i++)
                {
                    //objectTypeIndex++;

                    output += previousGenes[(h * (int)Genes.TotalGeneCount) + i] + ", "
                            + previousTraits[(h * (int)Genes.TotalGeneCount) + i] + ", ";


                }

                //objectTypeIndex++;
                output += previousTraits[(h * (int)Genes.TotalGeneCount) + (int)Genes.Health] + ", ";


                //write fitness



                //objectTypeIndex++;
                newLines.Add(output);

            }

            //writer.WriteLine();
            //writer.Close();
            //geneFile.Close();

            File.AppendAllLines(outputFilePath, newLines);

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

        public void AddGenesToObject(ushort objectID)
        {

            ids[objectID] = idIndex;
            idIndex++;
            // availables[availables.Length - 1];
            //availables.RemoveAt(availables.Length - 1);

        }

        //public void ResetIDGenes(int id)
        //{

        //    for (int i = ObjectTypeIndex(id);
        //        i < ObjectTypeIndex(id) + (int)GeneGroups.TotalGeneCount; i++)
        //    {
        //        genes[i] = -1;

        //    }

        //    //need to add the returned ID to the end
        //    //and then the id at the end position to where this one was

        //    //availables.AddNoResize(ids[id]);


        //    //need to implement this again

        //    //objectTypes[id] = ObjectType.None;
        //    ids[id] = ushort.MaxValue;
        //}

    }






}
