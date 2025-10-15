using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static Unity.VisualScripting.LudiqRootObjectEditor;

public class GenAlg : MonoBehaviour
{
    private int currentGeneration = 0;
    private List<Map> population;
    private Coroutine algorithm;

    [SerializeField]
    private int mu = 10;

    [SerializeField]
    private int lambda = 10;

    [SerializeField]
    private int maxGenerations = 10;

    [Tooltip("Weights must sum 1")]
    [Range(0f, 1f)][SerializeField]
    private float _connectionWeight = 0.4f;
    
    [Tooltip("Weights must sum 1")]
    [Range(0f, 1f)][SerializeField]
    private float _coverageWeight = 0.4f;
    
    [Tooltip("Weights must sum 1")]
    [Range(0f, 1f)][SerializeField]
    private float _startEndWeight = 0.2f;

    [Tooltip("Weights must sum 1")]
    [Range(0f, 1f)][SerializeField]
    private float _singlePathWeight = 0.2f;

    [Range(0f, 0.5f)][SerializeField]
    public float desiredCoverage = 0.33f;

    [Range(1, 100)][SerializeField]
    private int perturbationCells = 25;

    public float connectionWeight
    {
        get => _connectionWeight;
        set
        {
            _connectionWeight = value;
            NormalizeWeights();
        }
    }
    public float coverageWeight
    {
        get => _coverageWeight;
        set
        {
            _coverageWeight = value;
            NormalizeWeights();
        }
    }
    public float startEndWeight
    {
        get => _startEndWeight;
        set
        {
            _startEndWeight = value;
            NormalizeWeights();
        }
    }

    public float singlePathWeight
    {
        get => _singlePathWeight;
        set
        {
            _singlePathWeight = value;
            NormalizeWeights();
        }
    }

    public void NormalizeWeights()
    {
        float total = _connectionWeight + _coverageWeight + _startEndWeight + _singlePathWeight;
        if (total != 1f && total > 0f)
        {
            _connectionWeight /= total;
            _coverageWeight /= total;
            _startEndWeight /= total;
            _singlePathWeight /= total;
        }
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

    private void Start()
    {
        population ??= new();
        InitializePopulation();

        algorithm = StartCoroutine(PlayAlgorithm());
    }

    private void InitializePopulation()
    {
        for (int i = 0; i < mu + lambda; i++)
        {
            Map individual = new Map(connectionWeight, coverageWeight, startEndWeight, desiredCoverage, singlePathWeight);
            population.Add(individual);
        }
    }

    private void ShufflePopulation()
    {
        for (int i = 0; i < population.Count; i++)
        {
            Map temp = population[i];
            int randomIndex = Random.Range(i, population.Count);
            population[i] = population[randomIndex];
            population[randomIndex] = temp;
        }
    }

    private void CalculateMapsFitness()
    {
        foreach (Map individual in population)
        {
            individual.CalculateFinalFitness();
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

    private void GenerateOffspring()
    {
        //List<Map> offspring = MakeEliteIndividualCopies();
        List<Map> offspring = CrossoverMu();

        //RandomOffspringPerturbation(offspring);
        CalculatedOffspringPerturbation(offspring);

        population.AddRange(offspring);
    }

    private List<Map> MakeEliteIndividualCopies()
    {
        List<Map> offspring = new();

        for (int i = 0; i < mu; i++)
        {
            offspring.Add(population[i]);
        }

        return offspring;
    }

    private List<Map> CrossoverMu()
    {
        List<Map> offspring = new List<Map>();

        // Crossover the top mu individuals to produce lambda offspring
        for (int i = 0; i < lambda; i++)
        {
            // Select two parents from the elite mu individuals
            int parent1Idx = Random.Range(0, mu);
            int parent2Idx = Random.Range(0, mu);
            while (parent2Idx == parent1Idx && mu > 1)
            {
                parent2Idx = Random.Range(0, mu);
            }

            Map parent1 = population[parent1Idx];
            Map parent2 = population[parent2Idx];

            Map child = Crossover(parent1, parent2);
            offspring.Add(child);
        }

        return offspring;
    }

    private Map Crossover(Map parent1, Map parent2)
    {
        // Create a new child map with the same dimensions as the parents
        Map child = new Map(parent1.connectionWeight, parent1.coverageWeight, parent1.startEndWeight, parent1.desiredCoverage, singlePathWeight)
        {
            width = parent1.width,
            height = parent1.height,
            map = new int[parent1.width][]
        };

        // Combine the maps of the parents to create the child's map
        for (int x = 0; x < parent1.width; x++)
        {
            child.map[x] = new int[parent1.height];
            for (int y = 0; y < parent1.height; y++)
            {
                // Randomly choose a cell value from one of the parents
                child.map[x][y] = Random.Range(0, 2) == 0 ? parent1.map[x][y] : parent2.map[x][y];
            }
        }

        return child;
    }

    private void RandomOffspringPerturbation(List<Map> offspring)
    {
        foreach (Map individual in offspring)
        {
            HashSet<(int, int)> changed = new HashSet<(int, int)>();
            int attempts = 0;
            while (changed.Count < perturbationCells && attempts < perturbationCells * 5)
            {
                int x = Random.Range(0, individual.width);
                int y = Random.Range(0, individual.height);
                if (changed.Add((x, y)))
                {
                    individual.map[x][y] = 1 - individual.map[x][y]; // Flip the cell value
                }
                attempts++;
            }
        }
    }

    private void CalculatedOffspringPerturbation(List<Map> offspring)
    {
        foreach (Map individual in offspring)
        {
            // Find all cells that can be flipped to increase fitness
            List<(int, int)> candidates = new List<(int, int)>();
            for (int x = 0; x < individual.width; x++)
            {
                for (int y = 0; y < individual.height; y++)
                {
                    int original = individual.map[x][y];
                    // Try flipping the cell
                    individual.map[x][y] = 1 - original;
                    float newFitness = TestFitness(individual);
                    individual.map[x][y] = original;
                    if (newFitness > individual.fitness)
                    {
                        candidates.Add((x, y));
                    }
                }
            }

            HashSet<(int, int)> changed = new HashSet<(int, int)>();
            int attempts = 0;
            // Prefer beneficial flips, fallback to random if not enough
            while (changed.Count < perturbationCells && attempts < perturbationCells * 5)
            {
                (int x, int y) cell;
                if (candidates.Count > 0)
                {
                    cell = candidates[Random.Range(0, candidates.Count)];
                }
                else
                {
                    cell = (Random.Range(0, individual.width), Random.Range(0, individual.height));
                }
                if (changed.Add(cell))
                {
                    individual.map[cell.x][cell.y] = 1 - individual.map[cell.x][cell.y];
                }
                attempts++;
            }
        }
    }

    // Helper to test fitness after a hypothetical flip
    private float TestFitness(Map individual)
    {
        float oldFitness = individual.fitness;
        individual.CalculateFinalFitness();
        float newFitness = individual.fitness;
        individual.fitness = oldFitness; // Restore
        return newFitness;
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
            GenerateOffspring();

            GetHighestFitness();

            currentGeneration++;

            yield return null;
        }

        PrintTop5();
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
            DrawMapInScene(population[i], new Vector3(i * 12, 0, 0)); // Offset each map horizontally
        }
    }

    private void DrawMapInScene(Map mapObj, Vector3 origin)
    {
        float cellSize = 1.0f;

        GameObject mapParent = new GameObject($"Map_{mapObj.fitness:F2}");
        mapParent.transform.position = origin;
        mapParent.transform.parent = this.transform;

        for (int x = 0; x < mapObj.width; x++)
        {
            for (int y = 0; y < mapObj.height; y++)
            {
                int cell = mapObj.map[x][y]; // Road   // Empty
                Color color = cell == 1 ? Color.cyan : Color.gray;

                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = origin + new Vector3(x * cellSize, y * cellSize, 0);
                cube.GetComponent<Renderer>().material.color = color;
                cube.name = $"MapBlock_{x}_{y}";

                cube.transform.parent = mapParent.transform;
            }
        }
    }
}