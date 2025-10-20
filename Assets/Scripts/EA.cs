using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
// using UnityEditor.U2D.Aseprite;
using UnityEngine;

public class EA : MonoBehaviour
{
    private int currentGeneration = 0;
    private List<DirectionalMap> population;

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

    [SerializeField]
    private GameObject vsm;

    public void SetExtensionWeight(float weight) { extensionWeight = weight; }
    public void SetCoverageWeight(float weight) { coverageWeight = weight; }
    public void SetTurnsWeight(float weight) { turnsWeight = weight; }
    public void SetTurnsDensity(float density) { turnsDensity = density; }

    private void Start()
    {
        population ??= new();
    }

    public void Generate()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        population.Clear();
        currentGeneration = 0;
        InitializePopulation();
        PlayAlgorithm();
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
        }
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

    private void PlayAlgorithm()
    {
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
            currentGeneration++;
        }

        PrintBest();
    }

    private void GetHighestFitness()
    {
        for (int i = 0; i < 3; i++)
        {
            Debug.Log($"Top {i + 1} Fitness: {population[i].fitness}");
        }
    }

    private void PrintBest()
    {
        Debug.Log("Best Map:");
        Debug.Log($"Fitness: {population[0].fitness}");
        DrawMapInScene(population[0], Vector3.zero);
    }

    private void DrawMapInScene(DirectionalMap mapObj, Vector3 origin)
    {
        float cellSize = 3.0f; 
        List<Vector3> pathAsVector3 = new List<Vector3>();

        GameObject mapParent = new GameObject($"Map_{mapObj.fitness:F2}");
        mapParent.transform.position = origin;
        mapParent.transform.parent = this.transform;


        foreach (Vector2 cell in mapObj.path)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.transform.position = origin + new Vector3(cell.x * cellSize, cell.y * cellSize, 0);
            cube.transform.localScale = new Vector3(cellSize, cellSize, 0);
            cube.GetComponent<Renderer>().material.color = Color.cyan;
            cube.name = $"MapRoad_{cell.x}_{cell.y}";

            cube.transform.parent = mapParent.transform;

            pathAsVector3.Add(cube.transform.position);
        }

        vsm.transform.position = mapObj.startPos * cellSize + origin;
        vsm.GetComponentInParent<VirusSpawnManager>().path = pathAsVector3;
    }
}
