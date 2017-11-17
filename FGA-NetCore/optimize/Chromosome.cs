using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FGA_NetCore.optimize
{
    public class Chromosome
    {
        private static Random rand = new Random();
        public double[,] ChromosomeGenes;
        public int ChromosomeLength;
        public double ChromosomeFitness;
        public static double ChromosomeMutationRate;
        public static double ChromosomeCrossoverRate;
        public int TN_value_best;
        public int FN_value_best;
        public int TP_value_best;
        public int FP_value_best;
        public double Sensitivity;
        public double Specificity;
        public double Accuracy;
        public int A;
        public int B;

        //สร้าง chromosome 
        public Chromosome(int length, bool createGenes)
        {
            ChromosomeLength = length;
            ChromosomeGenes = new double[length, 4];
            //double temp;
            if (createGenes)
            {
                for (int i = 0; i < ChromosomeLength; i++)
                {

                    IOrderedEnumerable<int> subChromosomeSorted = Enumerable.Range(0, 4).Select(a => rand.Next(0, 8)).OrderBy(num => num);
                    int idx = 0;
                    foreach (double _rand in subChromosomeSorted)
                    {
                        ChromosomeGenes[i, idx] = _rand;
                        idx++;
                    }
                }

            }
        }


        public void Crossover(ref Chromosome Chromosome2, out Chromosome child1, out Chromosome child2)
        {
            int position_of_attribute1;
            int position_of_attribute2;
            int position_of_attribute3;
            int position_of_attribute4;
            int position;
            child1 = new Chromosome(ChromosomeLength, false);
            child2 = new Chromosome(ChromosomeLength, false);
            int count_fail_child1 = 0;
            int count_fail_child2 = 0;
            int[] problem_child1 = new int[4]; // the problem_child = 0 is mean correct form otherwise mean incorrect
            int[] problem_child2 = new int[4];
            bool status_child1 = true;
            bool status_child2 = true;

            while (true)
            {
                position_of_attribute1 = rand.Next(0, 8);

                if (rand.NextDouble() < ChromosomeCrossoverRate)
                    for (int i = 0; i < ChromosomeLength; i++)
                    {
                        if (i == position_of_attribute1)
                        {
                            position = rand.Next(0, 3);
                            for (int x = 0; x < 4; x++)
                            {
                                if (x < position)
                                {
                                    child1.ChromosomeGenes[i, x] = ChromosomeGenes[i, x];
                                    child2.ChromosomeGenes[i, x] = Chromosome2.ChromosomeGenes[i, x];
                                }
                                else
                                {
                                    child1.ChromosomeGenes[i, x] = Chromosome2.ChromosomeGenes[i, x];
                                    child2.ChromosomeGenes[i, x] = ChromosomeGenes[i, x];
                                }
                            }
                        }
                        else
                        {
                            for (int x = 0; x < 4; x++)
                            {
                                child1.ChromosomeGenes[i, x] = ChromosomeGenes[i, x];
                                child2.ChromosomeGenes[i, x] = Chromosome2.ChromosomeGenes[i, x];
                            }
                        }
                    }
                else
                {
                    for (int i = 0; i < ChromosomeLength; i++)
                    {
                        for (int x = 0; x < 4; x++)
                        {
                            child1.ChromosomeGenes[i, x] = ChromosomeGenes[i, x];
                            child2.ChromosomeGenes[i, x] = Chromosome2.ChromosomeGenes[i, x];
                        }
                    }
                    break;
                }

                //checking wheter gene is in correct form a<=b<=c<=d
                //checking child1
                if (child1.ChromosomeGenes[position_of_attribute1, 0] <= child1.ChromosomeGenes[position_of_attribute1, 1])
                { problem_child1[0] = 0; }
                else
                { problem_child1[0] = 1; count_fail_child1++; }
                if ((child1.ChromosomeGenes[position_of_attribute1, 1] >= child1.ChromosomeGenes[position_of_attribute1, 0]) && (child1.ChromosomeGenes[position_of_attribute1, 1] <= child1.ChromosomeGenes[position_of_attribute1, 2]))
                { problem_child1[1] = 0; }
                else
                { problem_child1[1] = 1; count_fail_child1++; }
                if ((child1.ChromosomeGenes[position_of_attribute1, 2] >= child1.ChromosomeGenes[position_of_attribute1, 1]) && (child1.ChromosomeGenes[position_of_attribute1, 2] <= child1.ChromosomeGenes[position_of_attribute1, 3]))
                { problem_child1[2] = 0; }
                else
                { problem_child1[2] = 1; count_fail_child1++; }
                if (child1.ChromosomeGenes[position_of_attribute1, 3] >= child1.ChromosomeGenes[position_of_attribute1, 2])
                { problem_child1[3] = 0; }
                else
                { problem_child1[3] = 1; count_fail_child1++; }

                //checking child2
                if (child2.ChromosomeGenes[position_of_attribute1, 0] <= child2.ChromosomeGenes[position_of_attribute1, 1])
                { problem_child2[0] = 0; }
                else
                { problem_child2[0] = 1; count_fail_child2++; }
                if ((child2.ChromosomeGenes[position_of_attribute1, 1] >= child2.ChromosomeGenes[position_of_attribute1, 0]) && (child2.ChromosomeGenes[position_of_attribute1, 1] <= child2.ChromosomeGenes[position_of_attribute1, 2]))
                { problem_child2[1] = 0; }
                else
                { problem_child2[1] = 1; count_fail_child2++; }
                if ((child2.ChromosomeGenes[position_of_attribute1, 2] >= child2.ChromosomeGenes[position_of_attribute1, 1]) && (child2.ChromosomeGenes[position_of_attribute1, 2] <= child2.ChromosomeGenes[position_of_attribute1, 3]))
                { problem_child2[2] = 0; }
                else
                { problem_child2[2] = 1; count_fail_child2++; }
                if (child2.ChromosomeGenes[position_of_attribute1, 3] >= child2.ChromosomeGenes[position_of_attribute1, 2])
                { problem_child2[3] = 0; }
                else
                { problem_child2[3] = 1; count_fail_child2++; }

                //fix condition child1
                if (count_fail_child1 == 1)
                    for (int l = 0; l < 4; l++)
                    {
                        if (problem_child1[l] == 1)
                        {
                            if (l == 0)
                                child1.ChromosomeGenes[position_of_attribute1, l] = rand.Next(0, (int)child1.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else if (l == 1)
                                child1.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child1.ChromosomeGenes[position_of_attribute1, l - 1], (int)child1.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else if (l == 2)
                                child1.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child1.ChromosomeGenes[position_of_attribute1, l - 1], (int)child1.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else
                                child1.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child1.ChromosomeGenes[position_of_attribute1, l - 1], (int)child1.ChromosomeGenes[position_of_attribute1, l] + 1);
                        }

                    }
                else if (count_fail_child1 == 0) { }
                else { status_child1 = false; }

                //fix condition child2
                if (count_fail_child2 == 1)
                    for (int l = 0; l < 4; l++)
                    {
                        if (problem_child2[l] == 1)
                        {
                            if (l == 0)
                                child2.ChromosomeGenes[position_of_attribute1, l] = rand.Next(0, (int)child2.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else if (l == 1)
                                child2.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child2.ChromosomeGenes[position_of_attribute1, l - 1], (int)child2.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else if (l == 2)
                                child2.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child2.ChromosomeGenes[position_of_attribute1, l - 1], (int)child2.ChromosomeGenes[position_of_attribute1, l + 1] + 1);
                            else
                                child2.ChromosomeGenes[position_of_attribute1, l] = rand.Next((int)child2.ChromosomeGenes[position_of_attribute1, l - 1], (int)child2.ChromosomeGenes[position_of_attribute1, l] + 1);
                        }

                    }
                else if (count_fail_child2 == 0) { }
                else { status_child2 = false; }

                // check condition to redo process or not.
                if (status_child1 == false || status_child2 == false)
                {
                    status_child1 = true;
                    status_child2 = true;
                    count_fail_child1 = 0;
                    count_fail_child2 = 0;
                    for (int l = 0; l < 4; l++)
                    { problem_child1[l] = 0; problem_child2[l] = 0; }
                }
                else { break; }

            }
        }
        //Mutates the chromosome genes by randomly switching them around 
        public void Mutate()
        {
            int position_of_attribute1 = rand.Next(0, 8);
            int position_gene = rand.Next(0, 4);

            if (rand.NextDouble() < ChromosomeMutationRate)
                for (int position = 0; position < ChromosomeLength; position++)
                {
                    if (position == position_of_attribute1)
                    {
                        if (position_gene == 0)
                            ChromosomeGenes[position, position_gene] = rand.Next(0, (int)ChromosomeGenes[position, position_gene + 1] + 1);
                        else if (position_gene == 1)
                            ChromosomeGenes[position, position_gene] = rand.Next((int)ChromosomeGenes[position, position_gene - 1], (int)ChromosomeGenes[position, position_gene + 1] + 1);
                        else if (position_gene == 2)
                            ChromosomeGenes[position, position_gene] = rand.Next((int)ChromosomeGenes[position, position_gene - 1], (int)ChromosomeGenes[position, position_gene + 1] + 1);
                        else
                            ChromosomeGenes[position, position_gene] = rand.Next((int)ChromosomeGenes[position, position_gene - 1], (int)ChromosomeGenes[position, position_gene] + 1);
                    }
                }
        }
    }
}
