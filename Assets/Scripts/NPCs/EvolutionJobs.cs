using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace ShepProject
{



    public struct ChooseParentSlimes : IJobParallelFor
    {

        [ReadOnly]
        public NativeArray<int> fitnessRanges;

        [ReadOnly]
        public NativeArray<int> slimeFitnesses;


        public NativeArray<ChromosoneParents> parents;

        public float elapsedTime;

        public void Execute(int index)
        {

            Random rand = Random.CreateFromIndex((uint)(index * elapsedTime * 14121));

            ChromosoneParents currentParents = new ChromosoneParents();

            int first = rand.NextInt(0, fitnessRanges[fitnessRanges.Length - 1] + 1);
            int second = rand.NextInt(0, fitnessRanges[fitnessRanges.Length - 1] + 1);

            currentParents.parentOne = GetSlimeParent(first);

            do
            {
                currentParents.parentTwo = GetSlimeParent(second);

            } while (currentParents.parentOne == currentParents.parentTwo);

            //if (currentParents.parentOne == ushort.MaxValue || currentParents.parentTwo == ushort.MaxValue)
            //{
            //    int test = 0;

            //    GetSlimeParent(first);
            //    GetSlimeParent(second);
            //}

            parents[index] = currentParents;

        }


        private ushort GetSlimeParent(int randValue)
        {

            int startIndex = 0;
            int endIndex = 1000000;

            int fraction = 2;

            if (randValue < fitnessRanges[1])
            {

                return 0;

            }
            else if (randValue > fitnessRanges[fitnessRanges.Length - 2])
            {

                return (ushort)(fitnessRanges.Length - 1);
            }

            while ((endIndex - startIndex) > 8)
            {

                if (randValue < fitnessRanges[startIndex + (fitnessRanges.Length / fraction)])
                {

                    endIndex = startIndex + (fitnessRanges.Length / fraction);

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

            for (int i = 0; i < (int)Genes.Health; i++)
            {

                if (rand.NextInt(0, 1001) < 500)
                {
                    //parent one
                    //parentIndex = parents[index].parentOne;
                    value = parentGenes[(parents[index].parentOne * (int)Genes.TotalGeneCount) + i];

                }
                else
                {
                    //parent two
                    //parentIndex = parents[index].parentTwo;
                    value = parentGenes[(parents[index].parentTwo * (int)Genes.TotalGeneCount) + i];

                }


                if (rand.NextInt(0, 1001) > 300)
                {

                    if (rand.NextInt(0, 1001) < 500)
                    {
                        //will mutate
                        value -= 1;

                    }
                    else
                    {
                        value += 1;

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


