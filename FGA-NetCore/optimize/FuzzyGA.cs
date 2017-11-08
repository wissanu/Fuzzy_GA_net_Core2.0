using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FGA_NetCore.optimize
{
    
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

            public double percent_train = 0.4;
            public int train_num;
            public int test_num;
            public string[,] temp_data;
            public string[,] trainingset; // use this origin data variable as inputdata by ignore the last column when used.
            public string[,] testset;
            public string[] trainingset_classoutput; // collect classoutput of traniningset data
            public string[] testset_classoutput;
            public int num_rec = 635; // change this when your original data has increase or decrease.
            public string check_status_output = "yes";
            public double MutationRate;
            public double CrossoverRate;
            public int ChromosomeLength;
            public int PopulationSize;
            public int GenerationSize;
            public bool Elitism;
            public double best_fitness = 0.0;
            public double[,] best_chromosome;
            public Chromosome best_chro;
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
                TN_value = new int[PopulationSize];
                TP_value = new int[PopulationSize];
                FP_value = new int[PopulationSize];
                FN_value = new int[PopulationSize];
                unknown = new int[PopulationSize];
                result = new double[PopulationSize];
                best_chro = new Chromosome(4,false);

                // change percen_train variable for full-train or percentage split.
                train_num = (Type_demonslation.Equals("fulltrain"))? num_rec : (Type_demonslation.Equals("percentage")) ? (int)(Math.Floor(percent_train * num_rec)) : (int)(num_rec * 0.2); // set percent_train = 1.0 for full-train. otherwise, for percentage split.
                test_num = (Type_demonslation.Equals("fulltrain"))? num_rec : (Type_demonslation.Equals("percentage")) ? (num_rec - train_num) : (int)(num_rec * 0.8);

                // read data from CSV and collect to variable
                temp_data = Stroedata_file.LoadCsv();

                //define capacity of trainingset and testset.
                trainingset = new string[train_num, ChromosomeLength];
                trainingset_classoutput = new string[train_num];
                testset = new string[test_num, ChromosomeLength];
                testset_classoutput = new string[test_num];   

                // split train & output class data
                if (Type_demonslation.Equals("percentage") || Type_demonslation.Equals("fulltrain"))
                {
                    int x = 0;
                    for (int i = 0; i < train_num; i++)
                    {
                        for (int l = 0; l < ChromosomeLength; l++)
                            trainingset[i, l] = temp_data[i, l];
                        trainingset_classoutput[i] = temp_data[i, ChromosomeLength];
                    }

                    // split test & output class data
                    for (int i = train_num; i < test_num; i++)
                    {
                        for (int l = 0; l < ChromosomeLength; l++)
                            testset[x, l] = temp_data[i, l];
                        testset_classoutput[x] = temp_data[i, ChromosomeLength];
                        x++;
                    }
                }

                // train process consist of "5-crossfold", "percentage", "fulltrain"
                train_test(Type_demonslation);

            }

            public void train_test(string status)
            {
                Testing getrule = new Testing();
                int count_number = 1;
                int num_cross_fold = 1;
                int loop = 1;

                //initial create chromosome
                while (true)
                {
                    if (status.Equals("5-crossfold"))
                    {
                        if (num_cross_fold <= 5 && loop == 6)
                            loop = 1;
                        if (num_cross_fold == 6)
                            break;

                        // split data into 5 part in-use.
                        if (num_cross_fold <= 5 && loop == 1)
                        {
                            int x = 0, y = 0;
                            while (count_number <= 5)
                            {
                                if (count_number == num_cross_fold)
                                    for (int i = train_num * (count_number - 1); i < train_num * count_number; i++)
                                    {
                                        for (int l = 0; l < ChromosomeLength; l++)
                                            trainingset[y, l] = temp_data[i, l];
                                        trainingset_classoutput[y] = temp_data[i, ChromosomeLength];
                                        y++;
                                    }
                                if (count_number != num_cross_fold)
                                    for (int i = train_num * (count_number - 1); i < train_num * count_number; i++)
                                    {
                                        for (int l = 0; l < ChromosomeLength; l++)
                                            testset[x, l] = temp_data[i, l];
                                        testset_classoutput[x] = temp_data[i, ChromosomeLength];
                                        x++;
                                    }
                                count_number++;
                            }
                            count_number = 1;
                        }
                    }
                        
                    CurrentGenerationList.Clear();
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
                    }

                    if (loop == 5)
                    {
                        Console.WriteLine("-----------------------Best - Solution-----------------------");
                        Console.WriteLine("fitness_best : {0}", best_chro.ChromosomeFitness);
                        Console.WriteLine("Sensiticity : {0:0.00}%, Specificity : {1:0.00}%, Accuracy : {2:0.00}%", best_chro.Sensitivity, best_chro.Specificity, best_chro.Accuracy);
                        Console.WriteLine("TN_value : {0}, FN_value : {1}, TP_value : {2}, FP_value : {3}, A : {4}, B : {5}", best_chro.TN_value_best, best_chro.FN_value_best, best_chro.TP_value_best, best_chro.FP_value_best, best_chro.A, best_chro.B);
                        for (int x = 0; x < ChromosomeLength; x++)
                            Console.WriteLine("{0},{1},{2},{3}", best_chro.ChromosomeGenes[x, 0], best_chro.ChromosomeGenes[x, 1], best_chro.ChromosomeGenes[x, 2], best_chro.ChromosomeGenes[x, 3]);
                        Console.WriteLine("---------------------------------------------------------------");
                        num_cross_fold++; // for ending each folds of cross-validaton
                        loop++;

                        // test process
                        //getrule.ruletest(best_chro,testset,testset_classoutput);

                        if (status.Equals("fulltrain") || status.Equals("percentage"))
                            break;
                    }
                    else
                    {
                        if (loop == 1)
                            best_chro = (Chromosome)CurrentGenerationList[PopulationSize - 1];

                        //get the best solution
                        if (best_chro.ChromosomeFitness <= ((Chromosome)CurrentGenerationList[PopulationSize - 1]).ChromosomeFitness)
                            best_chro = (Chromosome)CurrentGenerationList[PopulationSize - 1];

                        loop++;
                    }
                }    
                
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
                for (int i = 0; i < train_num; i++)
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
            GA ga = new GA(0.9, 0.8, 100, 300, 8);
            ga.Elitism = true;
            ga.LaunchGA("5-crossfold");

            var elapsedMs = DateTime.Now.Subtract(timeStarted).TotalSeconds;
            var x = DateTime.Now.Subtract(timeStarted);
            Console.WriteLine($"Time passed {Math.Floor(x.TotalMinutes)} min {x.Seconds}.{x.Milliseconds} sec");
            Console.ReadLine();

        }
        public static void Main(string[] args)
        {
            run();
        }

    }
}
