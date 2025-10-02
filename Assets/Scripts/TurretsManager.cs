using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretsManager : MonoBehaviour
{
    // B -> Cl
    // N -> Na

    private List<string> turrets = new()
    {
        "HHO",
        "COO",
        "NB",
        "HB",
        "NOH",
        "CHHHH"
    };

    private List<int> elementsAmount = new()
    {
        3,
        3,
        2,
        2,
        3,
        5
    };

    public static TurretsManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private int CalculateFitness(string turret, List<string> elementResistence)
    {
        int index = turrets.IndexOf(turret);
        int fitness = 0;

        foreach (string element in elementResistence)
        {
            if (!turret.Contains(element))
            {
                fitness++;
            }
        }

        Debug.Log($"Turret: {turret} fitness is {fitness}");

        return fitness;
    }

    private List<string> SortFitnessTurrets(Dictionary<string, int> fitnessTurrets)
    //private void SortFitnessTurrets(Dictionary<string, int> fitnessTurrets)
    {
        List<KeyValuePair<string, int>> sortedList = new(fitnessTurrets);
        sortedList.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value));
        fitnessTurrets.Clear();
        foreach (KeyValuePair<string, int> pair in sortedList)
        {
            fitnessTurrets.Add(pair.Key, pair.Value);
        }

        List<string> sortedTurrets = new();

        foreach (KeyValuePair<string, int> pair in fitnessTurrets)
        {
            sortedTurrets.Add(pair.Key);
        }

        return sortedTurrets;
    }

    public List<string> GetBestTurrets(List<string> elementResistence)
    //public Dictionary<string, int> GetBestTurrets(List<string> elementResistence)
    {
        Debug.Log($"Element resistence: {elementResistence.ToString()}");
        Dictionary<string, int> fitnessTurrets = new();

        foreach (string turret in turrets)
        {
            int fitness = CalculateFitness(turret, elementResistence);
            fitnessTurrets.Add(turret, fitness);
        }

        return SortFitnessTurrets(fitnessTurrets);

        //SortFitnessTurrets(fitnessTurrets);

        //return fitnessTurrets;
    }

    public Dictionary<string, int> GetElementsAmount(Dictionary<string, GameObject> bestTurrets)
    {
        Dictionary<string, int> elementsAmount = new()
        {
            { "H", 0 },
            { "O", 0 },
            { "C", 0 },
            { "N", 0 },
            { "B", 0 }
        };

        foreach (string turret in bestTurrets.Keys)
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
