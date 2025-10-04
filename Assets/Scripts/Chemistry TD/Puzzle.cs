using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private VirusSpawnManager vsm;

    [SerializeField] private int virusAmount = 2;


    List<string> virusTypes = new()
    {
        "H", "O", "C", "N", "B",
        "HO", "HC", "HN", "HB", "OC", "ON", "OB", "CN", "CB", "NB",
        "HOC", "HON", "HOB", "HCN", "HCB", "HNB",
        "OCN", "OCB", "ONB",
        "CNB",
    };

    private List<string> selectedVirus;
    private List<string> bestTurrets;
    private Dictionary<string, int> elementsAmount;

    private void Start()
    {
        selectedVirus ??= new();
        elementsAmount ??= new();
        bestTurrets ??= new();

        // Random selection
        // Change this for Hill Climbing (search for the most optimal virus combination based on the dificulty)
        // For Hill climbing, we select a random set of virus, calculate their difficulty based on how much damage the best turrets can deal to them
        // And to adjust and search a better set of virus that fits the given dificulty, the "movements" we do to traverse the solution space is to add, remove, or swap** an atom of the virus set
        for (int i = 0; i < virusAmount; i++)
        {
            int randomIndex = Random.Range(0, virusTypes.Count);

            if (!selectedVirus.Contains(virusTypes[randomIndex]))
            {
                selectedVirus.Add(virusTypes[randomIndex]);
            }
            else
                i--;
        }

        Debug.Log($"Selected virus: {string.Join(", ", selectedVirus)}");

        bestTurrets = TurretsManager.Instance.GetBestTurrets(selectedVirus);

        List<string> selectedBestTurrets = new();
        for (int i = 0; i < virusAmount && i < bestTurrets.Count; i++)
        {
            selectedBestTurrets.Add(bestTurrets[i]);
        }

        // Decompose turrets
        elementsAmount = TurretsManager.Instance.GetElementsAmount(selectedBestTurrets);
        AddConfusingAtoms(selectedBestTurrets, 2, true);

        ui.InitializeCounters(elementsAmount);
        vsm.StartRound(selectedVirus);
    }

    private void AddConfusingAtoms(List<string> selectedBestTurrets, int extraTurrets = 1, bool useLeastOptimal = true)
    {
        HashSet<string> usedTurrets = new(selectedBestTurrets);
        List<string> candidateTurrets = new();

        // Buscar torretas candidatas que no sean las óptimas ya seleccionadas
        foreach (var virus in selectedVirus)
        {
            if (useLeastOptimal)
            {
                // Agregar la menos óptima (última de la lista)
                if (bestTurrets.Count > 1)
                {
                    for (int i = bestTurrets.Count - 1; i >= 0; i--)
                    {
                        if (!usedTurrets.Contains(bestTurrets[i]))
                        {
                            candidateTurrets.Add(bestTurrets[i]);
                            break;
                        }
                    }
                }
            }
            else
            {
                // Agregar la más óptima que no esté ya usada
                for (int i = selectedBestTurrets.Count - 1; i < bestTurrets.Count; i++)
                {
                    if (!usedTurrets.Contains(bestTurrets[i]))
                    {
                        candidateTurrets.Add(bestTurrets[i]);
                        break;
                    }
                }
            }
        }

        int added = 0;
        foreach (string turret in candidateTurrets)
        {
            if (added >= extraTurrets) break;
            foreach (char kvp in turret)
            {
                elementsAmount[kvp.ToString()]++;
            }
            added++;
        }
    }
}
