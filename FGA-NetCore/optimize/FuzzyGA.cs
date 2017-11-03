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

    class FuzzyGA
    {
        public delegate double GAFunction(double[] values);
        private static Random rand = new Random();
        public class GA
        {
            // progress in training process for full-training and percentage-split is done. the rest is testset.
            // full-traning set = 635. fitness = 82.28
            // percentage-split 20% = 127. fitness = 92.53
            // percentage-split 60% = 381. fitness = 83.xx
            // percentage-split 70% = 444. fitness = 82.98

            public string[,] trainingset; // use this origin data variable as inputdata by ignore the last column when used.
            public string[,] testset;
            public string[] trainingset_classoutput; // collect classoutput of traniningset data
            public string[] testset_classoutput;
            public int num_rec = 127; // change this when your original data has increase or decrease.
            public string check_status_output = "yes";
            public double MutationRate;
            public double CrossoverRate;
            public int ChromosomeLength;
            public int PopulationSize;
            public int GenerationSize;
            public bool Elitism;
            public double best_fitness = 0.0;
            public double[,] best_chromosome;
            public int[] TN_value;
            public int[] TP_value;
            public int[] FP_value;
            public int[] FN_value;
            public int[] unknown;
            public double[] result;
            public int A = 0;
            public int B = 0;
            private ArrayList CurrentGenerationList;
            private ArrayList NextGenerationList;
            private ArrayList elitism_array;
            private ArrayList FitnessList;
            private ArrayList temp_best_fitness;

            private IComparer chromosomeComparer = new ChromosomeComparer();

            public GA(double XoverRate, double mutRate, int popSize, int genSize, int ChromLength)
            {
                Elitism = false;
                MutationRate = mutRate;
                CrossoverRate = XoverRate;
                PopulationSize = popSize;
                GenerationSize = genSize;
                ChromosomeLength = ChromLength;
            }

            public void LaunchGA(String Type_demonslation)
            {
                FitnessList = new ArrayList();
                CurrentGenerationList = new ArrayList(PopulationSize);
                NextGenerationList = new ArrayList(PopulationSize);
                temp_best_fitness = new ArrayList(1);
                best_chromosome = new double[ChromosomeLength, 4];
                Chromosome.ChromosomeMutationRate = MutationRate;
                Chromosome.ChromosomeCrossoverRate = CrossoverRate;
                trainingset_classoutput = new string[num_rec];
                TN_value = new int[PopulationSize];
                TP_value = new int[PopulationSize];
                FP_value = new int[PopulationSize];
                FN_value = new int[PopulationSize];
                unknown = new int[PopulationSize];
                result = new double[PopulationSize];

                // read data from CSV and collect to variable
                trainingset = Stroedata_file.LoadCsv();
                for (int i = 0; i < num_rec; i++)
                    trainingset_classoutput[i] = trainingset[i, ChromosomeLength];

                // "5-crossfold", "percentage", "fulltrain"

                // full- training set here.
                if(Type_demonslation.Equals("fulltrain"))  
                     traint_without_crossfold();

                if(Type_demonslation.Equals("5-crossfold"))
                {}

                if(Type_demonslation.Equals("percentage"))
                    traint_without_crossfold();

            }

            // usage in full-trainingset and percentage-split
            public void traint_without_crossfold()
            {
                //initial create chromosome
                while (true)
                {
                    for (int count = 0; count < PopulationSize; count++)
                    {
                        Chromosome g = new Chromosome(ChromosomeLength, true);
                        CurrentGenerationList.Add(g);
                    }

                    //doing rank for sort fitness value in population
                    for (int y = 0; y < PopulationSize; y++)
                    {
                        TN_value[y] = 0;
                        TP_value[y] = 0;
                        FP_value[y] = 0;
                        FN_value[y] = 0;
                        unknown[y] = 0;
                        A = 0;
                        B = 0;
                    }
                    calculatefitness();
                    rankpop();

                    for (int i = 0; i < GenerationSize; i++)
                    {
                        for (int y = 0; y < PopulationSize; y++)
                        {
                            TN_value[y] = 0;
                            TP_value[y] = 0;
                            FP_value[y] = 0;
                            FN_value[y] = 0;
                            unknown[y] = 0;
                            A = 0;
                            B = 0;
                        }
                        CreateNextGeneration();
                        calculatefitness();
                        rankpop();

                        Console.WriteLine("-----------------------generation : {0}-----------------------", i+1);
                        Console.WriteLine("fitness_best : {0}", ((Chromosome)CurrentGenerationList[PopulationSize - 1]).ChromosomeFitness);
                        Console.WriteLine("Sensiticity : {0:0.00}%, Specificity : {1:0.00}%, Accuracy : {2:0.00}%", ((Chromosome)CurrentGenerationList[PopulationSize - 1]).Sensitivity, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).Specificity, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).Accuracy);
                        Console.WriteLine("TN_value : {0}, FN_value : {1}, TP_value : {2}, FP_value : {3}, A : {4}, B : {5}", ((Chromosome)CurrentGenerationList[PopulationSize - 1]).TN_value_best, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).FN_value_best, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).TP_value_best, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).FP_value_best, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).A, ((Chromosome)CurrentGenerationList[PopulationSize - 1]).B);
                        Console.WriteLine("---------------------------------------------------------------");

                    }
                    break;
                }
            }

            // using in 5-fold crossvalidation
            public void _5foldcross()
            {
                // create the process of array training set divide by 1/5

                // reproduct generation in GA process
            }

            private double fuzzy(string[] data, int num)
            {
                Chromosome g = ((Chromosome)CurrentGenerationList[num]);

                double[] prob = new double[ChromosomeLength];
                double totalprob = 0;

                for (int i = 0; i < ChromosomeLength; i++)
                {
                    if ((double.Parse(data[i]) > g.ChromosomeGenes[i, 0]) && (double.Parse(data[i]) < g.ChromosomeGenes[i, 1]))
                    {
                        prob[i] = (double.Parse(data[i]) - g.ChromosomeGenes[i, 0]) / (g.ChromosomeGenes[i, 1] - g.ChromosomeGenes[i, 0]);
                    }
                    else if ((double.Parse(data[i]) >= g.ChromosomeGenes[i, 1]) && (double.Parse(data[i]) <= g.ChromosomeGenes[i, 2]))
                    {
                        prob[i] = 1;
                    }
                    else if ((double.Parse(data[i]) > g.ChromosomeGenes[i, 2]) && (double.Parse(data[i]) < g.ChromosomeGenes[i, 3]))
                    {
                        prob[i] = (g.ChromosomeGenes[i, 3] - double.Parse(data[i])) / (g.ChromosomeGenes[i, 3] - g.ChromosomeGenes[i, 2]);
                    }
                    else { prob[i] = 0; }
                }
                for (int i = 0; i < ChromosomeLength; i++)
                {
                    totalprob = totalprob + prob[i];
                }
                totalprob = totalprob / ChromosomeLength;
                return totalprob;
            }

            public void calculatefitness()
            {
                // calculate probability using for fitness fucntion
                double threshold = 0.3;
                string[] datax = new string[ChromosomeLength];
                for (int i = 0; i < num_rec; i++)
                {
                    // using data from traning set

                    Enumerable.Range(0, PopulationSize).AsParallel().ForAll(x =>
                    {
                        for (int y = 0; y < ChromosomeLength; y++)
                            datax[y] = trainingset[i, y];

                        result[x] = fuzzy(datax, x);
                    });

                    for (int x = 0; x < PopulationSize; x++)
                    {
                        if ((result[x] >= threshold) && trainingset_classoutput[i].Equals(check_status_output))
                        {
                            TN_value[x] = TN_value[x] + 1; // yes as yes
                        }
                        else if ((result[x] >= threshold) && !trainingset_classoutput[i].Equals(check_status_output))
                        {
                            FN_value[x] = FN_value[x] + 1; // no as yes
                        }
                        else if ((result[x] < threshold) && !trainingset_classoutput[i].Equals(check_status_output))
                        {
                            TP_value[x] = TP_value[x] + 1; // classify no as
                                                           // no
                        }
                        else if ((result[x] < threshold) && trainingset_classoutput[i].Equals(check_status_output))
                        {
                            FP_value[x] = FP_value[x] + 1; // classify yes as
                                                           // no FN
                        }
                        else
                            unknown[x] = unknown[x] + 1;
                    }
                    if (!trainingset_classoutput[i].Equals(check_status_output))
                        B++;
                    else { A++; }
                }

                //calculate fitness to every chromosome or one chromosome in population
                for (int i = 0; i < PopulationSize; i++)
                {
                    ((Chromosome)CurrentGenerationList[i]).ChromosomeFitness = ((Convert.ToDouble(TN_value[i]) / Convert.ToDouble(A)) - (Convert.ToDouble(FN_value[i]) / Convert.ToDouble(B))) * 100;
                    ((Chromosome)CurrentGenerationList[i]).FN_value_best = FN_value[i];
                    ((Chromosome)CurrentGenerationList[i]).TN_value_best = TN_value[i];
                    ((Chromosome)CurrentGenerationList[i]).TP_value_best = TP_value[i];
                    ((Chromosome)CurrentGenerationList[i]).FP_value_best = FP_value[i];
                    ((Chromosome)CurrentGenerationList[i]).Sensitivity = (Convert.ToDouble(TN_value[i]) / Convert.ToDouble(A)) * 100;
                    ((Chromosome)CurrentGenerationList[i]).Specificity = (Convert.ToDouble(TP_value[i]) / Convert.ToDouble(B)) * 100;
                    ((Chromosome)CurrentGenerationList[i]).Accuracy = ((Convert.ToDouble(TN_value[i]) + Convert.ToDouble(TP_value[i])) / (Convert.ToDouble(A) + Convert.ToDouble(B))) * 100;
                    ((Chromosome)CurrentGenerationList[i]).A = A;
                    ((Chromosome)CurrentGenerationList[i]).B = B;
                }
            }

            private void rankpop()
            {
                CurrentGenerationList.Sort(chromosomeComparer);
            }

            private Chromosome TournamentSelection(double versus)
            {
                ArrayList dump = new ArrayList();
                Chromosome g = null;

                for (int i = 0; i < versus; i++)
                    dump.Add(((Chromosome)CurrentGenerationList[rand.Next(0, PopulationSize)]));

                g = (Chromosome)dump[0];

                for (int i = 0; i < versus; i++)
                    if (i != 0)
                        if (g.ChromosomeFitness < ((Chromosome)dump[i]).ChromosomeFitness)
                            g = (Chromosome)dump[i];

                return g;

            }

            private bool duplicate(Chromosome g, int number_of_nextgen)
            {
                int flag = 0;

                if (number_of_nextgen == 0)
                    return false;

                for (int x = 0; x < number_of_nextgen; x++)
                {
                    for (int i = 0; i < ChromosomeLength; i++)
                        for (int j = 0; j < 4; j++)
                        {
                            if (g.ChromosomeGenes[i, j] == ((Chromosome)NextGenerationList[x]).ChromosomeGenes[i, j])
                                flag++;
                        }

                    if (flag >= (ChromosomeLength * 4 - 3)) // consider the non-duplicate that greater than 3 bit of gene. for reduce the duplicate fitness value.
                        return true;
                    else
                        flag = 0;
                }

                return false;
            }

            private void CreateNextGeneration()
            {
                NextGenerationList.Clear();
                int pop = 3;
                elitism_array = new ArrayList(pop);

                //elitism process pharse I

                for (int l = 0; l < pop; l++)
                {
                    elitism_array.Add(CurrentGenerationList[PopulationSize - (1 + l)]);
                }

                foreach (var l in elitism_array)
                {
                    NextGenerationList.Add(l);
                }
                //end elitism pharse I

                // clear elitise array
                elitism_array.Clear();

                //random swap
                ArrayList g = new ArrayList(1);
                for (int x = 0; x < PopulationSize; x++)
                {
                    int swap1 = rand.Next(PopulationSize / 2, PopulationSize);
                    int swap2 = rand.Next(0, PopulationSize / 2);
                    g.Add(CurrentGenerationList[swap1]);
                    CurrentGenerationList[swap1] = CurrentGenerationList[swap2];
                    CurrentGenerationList[swap2] = g[0];
                    g.Clear();
                }

                //start crossover
                bool dup_child1 = false;
                bool dup_child2 = false;
                int count_num_nextgen = NextGenerationList.Count; // for an initial of 1o is coming from elitism equal to 10 members.
                int i = 0;
                while (i < (int)(0.9 * PopulationSize)) //(PopulationSize - pop)
                {
                    Chromosome parent1, parent2, child1, child2;
                    parent1 = TournamentSelection(5);
                    parent2 = TournamentSelection(2);

                    if (parent1.ChromosomeFitness != parent2.ChromosomeFitness)
                    {
                        parent1.Crossover(ref parent2, out child1, out child2);
                        child1.Mutate();
                        child2.Mutate();


                        dup_child1 = duplicate(child1, count_num_nextgen);
                        dup_child2 = duplicate(child2, count_num_nextgen);

                        if (!dup_child1 && !dup_child2)
                        {
                            NextGenerationList.Add(child1);
                            NextGenerationList.Add(child2);
                            i += 2;
                            count_num_nextgen = NextGenerationList.Count;

                        }
                        dup_child1 = false;
                        dup_child2 = false;
                    }
                }
                for (int l = 0; l < (PopulationSize - (int)(PopulationSize * 0.9)) - pop; l++)
                {
                    Chromosome h = new Chromosome(ChromosomeLength, true);
                    NextGenerationList.Add(h);
                    //NextGenerationList.Add(((Chromosome)CurrentGenerationList[rand.Next(0,PopulationSize)]));
                }

                //copy generation
                CurrentGenerationList.Clear();
                for (int o = 0; o < PopulationSize; o++)
                {
                    CurrentGenerationList.Add(NextGenerationList[o]);
                }
            }

        }


        public sealed class ChromosomeComparer : IComparer
        {
            public int Compare(object x, object y)
            {
                if (!(x is Chromosome) || !(y is Chromosome))
                    throw new ArgumentException("Not of type Chromosome");
                if (((Chromosome)x).ChromosomeFitness > ((Chromosome)y).ChromosomeFitness)
                    return 1;
                else if (((Chromosome)x).ChromosomeFitness == ((Chromosome)y).ChromosomeFitness)
                    return 0;
                else
                    return -1;
            }
        }

        public static void run()
        {
            var timeStarted = DateTime.Now;
            double min = 0.0;
            double sec = 0.0;
            int count_time = 1;
            GA ga = new GA(0.9, 0.8, 100, 300, 8);
            ga.Elitism = true;
            ga.LaunchGA("fulltrain");
            var elapsedMs = DateTime.Now.Subtract(timeStarted).TotalSeconds;

            var x = DateTime.Now.Subtract(timeStarted);
            Console.WriteLine($"Time passed {Math.Floor(x.TotalMinutes)} min {x.Seconds}.{x.Milliseconds} sec");
            Console.ReadLine();

            // calculate the min of run time
            while (true)
            {
                if ((60 * count_time) <= elapsedMs)
                {
                    min = min + 1.0;
                    count_time++;
                }
                else
                {
                    sec = elapsedMs - (60 * (count_time - 1));
                    break;
                }
            }
            Console.WriteLine("Time passed {0} min {1} sec", min, sec);
            Console.ReadLine();
        }
        public static void Main(string[] args)
        {
            run();
        }

    }
}
