/*
Author:   Romaine A. Carter
Created:  Thursday, January 06. 2011
Rivision: Null
Descrip:  Genetic algorithm inspired timetable generation engine version 1.0
*/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;
using System.Threading;

namespace simpleGA
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();           
        }

        private Thread _thread;
        private const int epochs             = 1000000;         // epoch = number of iterations. 
        private const int populationSize     = 1000;            // size of the poulation in an epoch.
        private const int initPopulationSize = 10000;           // initial population size
        private const int survivorThreshold  = 6;               // chromosomes with fitness >= survivorThreshold will survive.
        private const int mutantThreshold    = 3;               // chromosomes with fitness >= mutantThreshold will be mutated.
        private const int crossoverThreshold = 6;               // chromosomes with fitness >= crossoverThreshold will be mated.
        private const int fitnessUBound      = 9;               // end algo if fitness between fitnessUBound and fitnessLBound.
        private const int fitnessLBound      = 9;               // end algo if fitness between fitnessUBound and fitnessLBound.

        readonly List<sequence> survivors  = new List<sequence>();          

           
        private void button1_Click(object sender, EventArgs e)
        {
            _thread = new Thread(new ThreadStart(Start));
            _thread.Start();
        }

        private void Start()
        {
            var population = new List<sequence>();
        
            InitGA(population, initPopulationSize);

            var count = 0;
            while (population.Count(i => i.fitness >= fitnessLBound && i.fitness <= fitnessUBound ) <= 0 && count <= epochs)
            {                
                Epoch(population);
                count++;
                SetIteration(count.ToString());

                var found = population.Where(i => i.fitness >= 8).FirstOrDefault();
                if(found != null)
                    SetSample(string.Join("", found.buffer));
            }            
        }

        private void Epoch(List<sequence> population)
        {
            //Elitism
            survivors.AddRange(population.Where(i => i.fitness >= survivorThreshold)); 
            
            Mutate(population.Where(i => i.fitness >= mutantThreshold));
               
            CrossOver(population.Where(i => i.fitness >= crossoverThreshold) as IEnumerable<sequence>);           
          
            population.Clear();
            population.AddRange(survivors);

            for (int i = 0; i < populationSize - survivors.Count; i++) 
            {
                var temp = GenerateSequence();
                CalculateFitness(temp);
                population.Add(temp);
            }
            survivors.Clear();
        }

        private static void InitGA(List<sequence> generation, int size)
        {
            for (int i = 0; i < size; i++) // generate population
            {
                var temp = GenerateSequence();
                CalculateFitness(temp);
                generation.Add(temp);                
            }
        }

        private static void CalculateFitness(sequence seq)
        {
            var ordered = new sequence{buffer = new List<string>() {"1", "2", "3", "4", "5", "6", "7", "8", "9"}};
                                                                                              
            int result=0;
            for (int i = 0; i < 9; i++)
            {
                if (ordered.buffer[i] == seq.buffer[i]) result++;                
            }
            seq.fitness = result;
        }

        private readonly Random rn = new Random();
        private void Mutate(IEnumerable<sequence> candidates)
        {
            try
            {                
                foreach (var individual in candidates)   // mutate not so fit chromosomes in population.
                {
                    int index = rn.Next(0, 9);
                    individual.buffer[index] = rn.Next(0, 10).ToString();
                    CalculateFitness(individual);
                    survivors.Add(individual);
                }
            }
            catch (Exception)  {}                    
        }

        private void CrossOver(IEnumerable<sequence> candidates)
        {
           if(candidates.Count() >= 2)
            {
                var temp = new sequence();
                temp.buffer = new List<string>();

                temp.buffer.AddRange(candidates.ElementAt(0).buffer.GetRange(0,3));
                temp.buffer.AddRange(candidates.ElementAt(1).buffer.GetRange(3, 6));
                CalculateFitness(temp);
                survivors.Add(temp);               
            }                               
        }

        static readonly Random rand = new Random();
        private static sequence GenerateSequence()
        {   
            var str = new List<string>();                               
            for(int i = 1; i < 10; i++)
            {                
                str.Add(rand.Next(0, 10).ToString());              
            }
            var temp = new sequence();
            temp.buffer = str;
            return temp ;
        }

        delegate void SetTextCallback(string text);
        private void SetIteration(string text)
        {           
            if (lblIterations.InvokeRequired)
            {
                var d = new SetTextCallback(SetIteration);
                Invoke(d, new object[] { text });
            }
            else
            {
                lblIterations.Text = text;
            }
        }        

        private void SetSample(string text)
        {          
            if (lblSample.InvokeRequired)
            {
                var d = new SetTextCallback(SetSample);
                Invoke(d, new object[] { text });
            }
            else
            {
                lblSample.Text = text;
            }
        }
    }

    public class sequence
    {
        public int fitness;
        public List<string> buffer;
    }
}
