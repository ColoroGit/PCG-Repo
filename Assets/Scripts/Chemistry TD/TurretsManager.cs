using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TurretsManager : MonoBehaviour
{
    // B -> Cl
    // N -> Na

    private List<string> turretsTypes = new()
    {
        "HHO",
        "COO",
        "NB",
        "HB",
        "NOH",
        "CHHHH"
    };

    public static TurretsManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private float CalculateFitness(string turret, List<string> elementResistences)
    {
        float allKillerFitness = CalculateAllKillerFitness(turret, elementResistences);
        float oneShotFitness = CalculateMaxSingleVirusDamageFitness(turret, elementResistences);

        Debug.Log($"Turret: {turret} fitness is {allKillerFitness + oneShotFitness}");
        return allKillerFitness + oneShotFitness;
    }

    // Daño máximo posible a un solo virus (cantidad de elementos de la torreta que no están en la resistencia)
    private float CalculateMaxSingleVirusDamageFitness(string turret, List<string> elementResistences)
    {
        float maxDamage = 0f;

        foreach (string resistance in elementResistences)
        {
            float damage = 0f;
            foreach (char element in turret)
            {
                if (!resistance.Contains(element))
                {
                    damage += 1f;
                }
            }
            if (damage > maxDamage)
            {
                maxDamage = damage;
            }
        }

        return maxDamage;
    }

    // Cantidad de virus distintos que puede dañar la torreta (al menos un elemento de la torreta le hace daño)
    private float CalculateAllKillerFitness(string turret, List<string> elementResistences)
    {
        float fitness = 0f;

        foreach (string resistance in elementResistences)
        {
            bool canHit = false;
            foreach (char element in turret)
            {
                if (!resistance.Contains(element))
                {
                    canHit = true;
                    break;
                }
            }
            if (canHit)
            {
                fitness += 1f;
            }
        }

        return fitness;
    }

    private Dictionary<string, float> SortFitnessTurrets(Dictionary<string, float> fitnessTurrets)
    {
        List<KeyValuePair<string, float>> sortedList = new(fitnessTurrets);
        sortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        
        return sortedList.ToDictionary(pair => pair.Key, pair => pair.Value);
    }

    public Dictionary<string, float> GetBestTurrets(List<string> elementResistences)
    {
        Debug.Log($"Element resistence: {string.Join(", ", elementResistences)}");
        Dictionary<string, float> fitnessTurrets = new(); 

        foreach (string turret in turretsTypes)
        {
            float fitness = CalculateFitness(turret, elementResistences);
            fitnessTurrets.Add(turret, fitness);
        }

        return SortFitnessTurrets(fitnessTurrets);
    }

    public Dictionary<string, int> GetElementsAmount(List<string> bestTurrets)
    {
        Dictionary<string, int> elementsAmount = new()
        {
            { "H", 0 },
            { "O", 0 },
            { "C", 0 },
            { "N", 0 },
            { "B", 0 }
        };

        foreach (string turret in bestTurrets)
        {
            for (int i = 0; i < turret.Length; i++)
            {
                string element = turret[i].ToString();

                if (elementsAmount.ContainsKey(element))
                {
                    elementsAmount[element]++;
                }
            }
        }

        return elementsAmount;
    }
}

