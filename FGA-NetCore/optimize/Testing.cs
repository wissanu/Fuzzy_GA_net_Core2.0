using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FGA_NetCore.optimize
{
    public class Testing
    {
        public Chromosome exact_rule;
        public int TP_value = 0;
        public int TN_value = 0;
        public int FP_value = 0;
        public int FN_value = 0;
        public int unknown = 0;
        public double result;
        public int A = 0;
        public int B = 0;
        public string check_status_output = "yes";

        public void ruletest(Chromosome rule, int test_num, string[,] testset, string[] testset_outputclass)
        {
            exact_rule = rule;// get rule.

            //process of get result from testset with rule.
            calculate(test_num,testset,testset_outputclass);

            Console.WriteLine("-----------------------Test-Solution-----------------------");
            Console.WriteLine("fitness_best : {0}", exact_rule.ChromosomeFitness);
            Console.WriteLine("Sensiticity : {0:0.00}%, Specificity : {1:0.00}%, Accuracy : {2:0.00}%", exact_rule.Sensitivity, exact_rule.Specificity, exact_rule.Accuracy);
            Console.WriteLine("TN_value : {0}, FN_value : {1}, TP_value : {2}, FP_value : {3}, A : {4}, B : {5}", exact_rule.TN_value_best, exact_rule.FN_value_best, exact_rule.TP_value_best, exact_rule.FP_value_best, exact_rule.A, exact_rule.B);
            for (int x = 0; x < exact_rule.ChromosomeLength; x++)
                Console.WriteLine("{0},{1},{2},{3}", exact_rule.ChromosomeGenes[x, 0], exact_rule.ChromosomeGenes[x, 1], exact_rule.ChromosomeGenes[x, 2], exact_rule.ChromosomeGenes[x, 3]);
            Console.WriteLine("---------------------------------------------------------------");

            TP_value = 0;
            TN_value = 0;
            FP_value = 0;
            FN_value = 0;
            unknown = 0;
            A = 0;
            B = 0;
        }

        private double fuzzy(string[] data)
        {
            double[] prob = new double[exact_rule.ChromosomeLength];
            double totalprob = 0;

            for (int i = 0; i < exact_rule.ChromosomeLength; i++)
            {
                if ((double.Parse(data[i]) > exact_rule.ChromosomeGenes[i, 0]) && (double.Parse(data[i]) < exact_rule.ChromosomeGenes[i, 1]))
                {
                    prob[i] = (double.Parse(data[i]) - exact_rule.ChromosomeGenes[i, 0]) / (exact_rule.ChromosomeGenes[i, 1] - exact_rule.ChromosomeGenes[i, 0]);
                }
                else if ((double.Parse(data[i]) >= exact_rule.ChromosomeGenes[i, 1]) && (double.Parse(data[i]) <= exact_rule.ChromosomeGenes[i, 2]))
                {
                    prob[i] = 1;
                }
                else if ((double.Parse(data[i]) > exact_rule.ChromosomeGenes[i, 2]) && (double.Parse(data[i]) < exact_rule.ChromosomeGenes[i, 3]))
                {
                    prob[i] = (exact_rule.ChromosomeGenes[i, 3] - double.Parse(data[i])) / (exact_rule.ChromosomeGenes[i, 3] - exact_rule.ChromosomeGenes[i, 2]);
                }
                else { prob[i] = 0; }
            }
            for (int i = 0; i < exact_rule.ChromosomeLength; i++)
            {
                totalprob = totalprob + prob[i];
            }
            totalprob = totalprob / exact_rule.ChromosomeLength;
            return totalprob;
        }

        public void calculate(int test_num, string[,] testset, string[] testset_outputclass)
        {
            // calculate probability using for fitness fucntion
            double threshold = 0.3;
            string[] datax = new string[exact_rule.ChromosomeLength];
            for (int i = 0; i < test_num; i++)
            {
                // using data from traning set

                for (int y = 0; y < exact_rule.ChromosomeLength; y++)
                    datax[y] = testset[i, y];

                result = fuzzy(datax);

                if ((result >= threshold) && testset_outputclass[i].Equals(check_status_output))
                {
                    TN_value = TN_value + 1; // yes as yes
                }
                else if ((result >= threshold) && !testset_outputclass[i].Equals(check_status_output))
                {
                    FN_value = FN_value + 1; // no as yes
                }
                else if ((result < threshold) && !testset_outputclass[i].Equals(check_status_output))
                {
                    TP_value = TP_value + 1; // classify no as no
                }
                else if ((result < threshold) && testset_outputclass[i].Equals(check_status_output))
                {
                    FP_value = FP_value + 1; // classify yes as no FN
                }
                else
                    unknown = unknown + 1;
                    
                if (!testset_outputclass[i].Equals(check_status_output))
                    B++;
                else { A++; }
            }

            //calculate result from this rule
            exact_rule.ChromosomeFitness = ((Convert.ToDouble(TN_value) / Convert.ToDouble(A)) - (Convert.ToDouble(FN_value) / Convert.ToDouble(B))) * 100;
            exact_rule.FN_value_best = FN_value;
            exact_rule.TN_value_best = TN_value;
            exact_rule.TP_value_best = TP_value;
            exact_rule.FP_value_best = FP_value;
            exact_rule.Sensitivity = (Convert.ToDouble(TN_value) / Convert.ToDouble(A)) * 100;
            exact_rule.Specificity = (Convert.ToDouble(TP_value) / Convert.ToDouble(B)) * 100;
            exact_rule.Accuracy = ((Convert.ToDouble(TN_value) + Convert.ToDouble(TP_value)) / (Convert.ToDouble(A) + Convert.ToDouble(B))) * 100;
            exact_rule.A = A;
            exact_rule.B = B;
        }
    }
}
