using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace ShepProject
{


    public struct UpdatePlayerDistanceFitnessJob : IJobParallelFor
    {


        public NativeArray<ushort> ids;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float2> positions;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> slimeFitnesses;

        public void Execute(int index)
        {

            if (ids[index] == ushort.MaxValue)
                return;

            float fitness = 10000f / math.distancesq(positions[index], positions[0]);
            if (slimeFitnesses[ids[index]] < fitness)
                slimeFitnesses[ids[index]] = fitness;

            
        }

    }


    public struct ChooseParentSlimes : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<float> fitnessRanges;

        [ReadOnly]
        public NativeArray<float> slimeFitnesses;


        public NativeArray<ChromosoneParents> parents;

        public float elapsedTime;

        public void Execute(int index)
        {

            Random rand = Random.CreateFromIndex((uint)(index * elapsedTime * 14121));

            ChromosoneParents currentParents = new ChromosoneParents();

            float first = rand.NextFloat(0, fitnessRanges[fitnessRanges.Length - 1] + 1);
            float second = rand.NextFloat(0, fitnessRanges[fitnessRanges.Length - 1] + 1);

            currentParents.parentOne = GetSlimeParent(first);

            do
            {
                currentParents.parentTwo = GetSlimeParent(rand.NextFloat(0, fitnessRanges[fitnessRanges.Length - 1] + 1));

            } while (currentParents.parentOne == currentParents.parentTwo);

            parents[index] = currentParents;

        }


        private ushort GetSlimeParent(float randValue)
        {

            int startIndex = 0;
            int endIndex = fitnessRanges.Length - 1;

            int fraction = 2;

            while ((endIndex - startIndex) > 8)
            {

                if (randValue < fitnessRanges[endIndex - (fitnessRanges.Length / fraction)])
                {

                    endIndex = endIndex - (fitnessRanges.Length / fraction);

                }
                else
                {

                    startIndex += (fitnessRanges.Length / fraction);

                }

                fraction *= 2;

            }


            for (ushort i = (ushort)startIndex; i <= endIndex; i++)
            {

                if (randValue < fitnessRanges[i])
                {
                    i--;
                    return i;

                }


            }


            return ushort.MaxValue;


        }

    }

    public struct CreateNextGeneration : IJobParallelFor
    {


        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<ChromosoneParents> parents;
        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> parentGenes;

        //[ReadOnly]
        //[NativeDisableContainerSafetyRestriction]
        //public NativeArray<Sigmoid> sigmoids;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> childGenes;


        public float mutationMean;
        public float mutationStandardDeviation;

        public float elapsedTime;

        public void Execute(int index)
        {

            /*
             * 
             * how to create a genome
             * 
             * 
             *      for each gene that isn't one of the two types,
             *          choose randomly between parentOne and ParentTwo
             *          
             *      from there, randomly choose then if it will mutate 
             *      
             *      then assign that gene to the child
             *      
             *      
             *      
             *      if it's a type then randomly choose each one from the two of each parent
             *          then randomly choose if it will mutate from one of those types
             *          
             *          
             *      Question about resistances, 
             *          if the type mutates will it stay at the same strength
             *              or will the resistance reset?
             *          
             *      
             *      
             *      
             *      
             */


            Random rand = Random.CreateFromIndex((uint)(index * 213984 * elapsedTime));
            //int parentIndex = 0;
            float value = 0;


            //need to do main type and secondary type


            for (int i = (int)Genes.MainResistance; i < (int)Genes.Health; i++)
            {

                value = 0;
                if (rand.NextInt(0, 1001) <= 500)
                {
                    //parent one
                    value = parentGenes[(parents[index].parentOne * (int)Genes.TotalGeneCount) + i];

                }
                else
                {
                    //parent two
                    value = parentGenes[(parents[index].parentTwo * (int)Genes.TotalGeneCount) + i];

                }


                if (rand.NextInt(0, 1001) > 300)
                {

                    //if tests need to be done to check if evolution is working, just do + or - 1
                    //tested by checking values and seemed to be working

                    if (rand.NextInt(0, 1001) <= 500)
                    {

                        //will mutate
                        value -= MathUtil.RandomGaussianJobThread(mutationStandardDeviation, mutationMean, ref rand);

                    }
                    else
                    {
                        value += MathUtil.RandomGaussianJobThread(mutationStandardDeviation, mutationMean, ref rand);

                    }
                    
                }

                //start index of child genes
                childGenes[(index * (int)Genes.TotalGeneCount) + i] = value;



            }


        }


    }

    public struct UpdateTraitValues : IJobParallelFor
    {
        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> genes;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<float> traits;


        [ReadOnly]
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<Sigmoid> sigmoids;


        public void Execute(int index)
        {

            int sigmoidIndex = 0;
            for (int i = (int)Genes.MainResistance; i < (int)Genes.Health; i++)
            {

                traits[(index * (int)Genes.TotalGeneCount) + i] 
                    = sigmoids[sigmoidIndex].GetTraitValue(genes[(index * (int)Genes.TotalGeneCount) + i]);

                sigmoidIndex++;

            }
            


        }



    }






}


