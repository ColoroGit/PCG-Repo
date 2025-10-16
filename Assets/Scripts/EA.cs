using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class EA : MonoBehaviour
{
    private int currentGeneration = 0;
    private List<DirectionalMap> population;
    private Coroutine algorithm;

    [SerializeField]
    private int mu = 10;

    [SerializeField]
    private int lambda = 10;

    [SerializeField]
    private int maxGenerations = 50;

    [SerializeField, Range (0, 1)]
    private float extensionWeight;

    [SerializeField, Range(0, 1)]
    private float coverageWeight;

    [SerializeField, Range(0, 1)]
    private float turnsWeight;

    [SerializeField, Range(0, 1)]
    private float turnsDensity;

    private void Start()
    {
        population ??= new();
        InitializePopulation();

        algorithm = StartCoroutine(PlayAlgorithm());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (algorithm != null)
            {
                StopCoroutine(algorithm);
            }

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            population = new();
            currentGeneration = 0;
            InitializePopulation();
            algorithm = StartCoroutine(PlayAlgorithm());
        }
    }

    private void InitializePopulation()
    {
        for (int i = 0; i < mu + lambda; i++)
        {
            DirectionalMap individual = new DirectionalMap(extensionWeight, coverageWeight, turnsWeight, turnsDensity);
            population.Add(individual);
        }
    }

    private void ShufflePopulation()
    {
        for (int i = 0; i < population.Count; i++)
        {
            DirectionalMap temp = population[i];
            int randomIndex = Random.Range(i, population.Count);
            population[i] = population[randomIndex];
            population[randomIndex] = temp;
        }
    }

    private void CalculateMapsFitness()
    {
        foreach (DirectionalMap individual in population)
        {
            individual.CalculateFitness();
        }
    }

    private void SortPopulation()
    {
        population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
    }

    private void KeepElitePopulation()
    {
        if (population.Count > mu)
        {
            population = population.GetRange(0, mu);
        }
    }

    private void GenerateRandomOffspring()
    {
        //List<DirectionalMap> offspring = new();
        for (int i = 0; i < lambda; i++)
        {
            DirectionalMap individual = new DirectionalMap(extensionWeight, coverageWeight, turnsWeight, turnsDensity);
            population.Add(individual);

            //DirectionalMap parent1 = population[Random.Range(0, mu)];
            //DirectionalMap parent2 = population[Random.Range(0, mu)];
            //DirectionalMap child = parent1.Crossover(parent2);
            //child.Perturbate(Random.Range(1, child.dMap.Count));
            //offspring.Add(child);
        }
        //population.AddRange(offspring);
    }

    private void GenerateOffspring()
    {
        List<DirectionalMap> offspring = MakeEliteIndividualCopies();

        RandomOffspringPerturbation(offspring);

        population.AddRange(offspring);
    }

    private List<DirectionalMap> MakeEliteIndividualCopies()
    {
        List<DirectionalMap> offspring = new();

        for (int i = 0; i < mu; i++)
        {
            offspring.Add(population[i]);
        }

        return offspring;
    }

    private void RandomOffspringPerturbation(List<DirectionalMap> offspring)
    {
        foreach (DirectionalMap individual in offspring)
        {
            individual.Perturbate(Random.Range(1, individual.dMap.Count));
        }
    }

    private IEnumerator PlayAlgorithm()
    {
        Debug.Log($"Starting Genetic Algorithm with population size {population.Count}");

        while (currentGeneration < maxGenerations)
        {
            Debug.Log($"Generation {currentGeneration}");

            ShufflePopulation();
            CalculateMapsFitness();
            SortPopulation();
            KeepElitePopulation();
            GenerateRandomOffspring();
            //GenerateOffspring();

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            GetHighestFitness();
            PrintTop5();

            currentGeneration++;

            yield return new WaitForSeconds(0.5f);
        }
    }

    private void GetHighestFitness()
    {
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Top {i + 1} Fitness: {population[i].fitness}");
        }
    }

    private void PrintTop5()
    {
        Debug.Log("Top 5 Maps:");
        for (int i = 0; i < Mathf.Min(5, population.Count); i++)
        {
            Debug.Log($"Rank {i + 1} - Fitness: {population[i].fitness}");
            DrawMapInScene(population[i], new Vector3(i * 13, 0, 0)); // Offset each map horizontally
        }
    }

    private void DrawMapInScene(DirectionalMap mapObj, Vector3 origin)
    {
        float cellSize = 1.0f;

        GameObject mapParent = new GameObject($"Map_{mapObj.fitness:F2}");
        mapParent.transform.position = origin;
        mapParent.transform.parent = this.transform;

        GameObject background = GameObject.CreatePrimitive(PrimitiveType.Cube);
        background.transform.position = origin + new Vector3(((mapObj.limit + 1) / 2f) * cellSize - (cellSize / 2f), ((mapObj.limit + 1) / 2f) * cellSize - (cellSize / 2f), 1);
        background.transform.localScale = new Vector3((mapObj.limit + 1) * cellSize, (mapObj.limit + 1) * cellSize, 0.1f);
        background.GetComponent<Renderer>().material.color = Color.gray;
        background.name = "MapBackground";
        background.transform.parent = mapParent.transform;

        foreach (Vector2 cell in mapObj.path)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = origin + new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
            cube.GetComponent<Renderer>().material.color = Color.cyan;
            cube.name = $"MapRoad_{cell.x}_{cell.y}";

            cube.transform.parent = mapParent.transform;
        }
    }
}
