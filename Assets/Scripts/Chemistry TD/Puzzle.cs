using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private UIManager ui;

    [SerializeField] private int virusAmount = 2;


    List<string> virusTypes = new()
    {
        "HNB",
        "HOC",
        "ONB",
        "CNB",
        "CON",
        "CB"
    };

    private List<string> selectedVirus;
    private Dictionary<string, string> bestTurrets;
    private Dictionary<string, int> elementsAmount;

    private void Start()
    {
        selectedVirus ??= new();
        elementsAmount ??= new();
        bestTurrets ??= new();

        // Random selection
        for (int i = 0; i < virusAmount; i++)
        {
            int randomIndex = Random.Range(0, virusTypes.Count);

            if (!selectedVirus.Contains(virusTypes[randomIndex]))
            {
                selectedVirus.Add(virusTypes[randomIndex]);
            }
        }

        // Get best turrets
        for (int i = 0; i < selectedVirus.Count; i++)
        {
            List<string> debugTurrets = TurretsManager.Instance.GetBestTurrets(selectedVirus[i]);
            Debug.Log($"Best turret against {selectedVirus[i]} is {debugTurrets[0]}");
            bestTurrets.Add(debugTurrets[0], selectedVirus[i]);
        }

        // Decompose turrets
        elementsAmount = TurretsManager.Instance.GetElementsAmount(bestTurrets);

        // Falta Agregarle otros átomos random que confundan

        ui.InitializeCounters(elementsAmount);

        //Desde el VirusSpawnManager spawnear virus pasándole la lsita de virus seleccionada.
    }
}
