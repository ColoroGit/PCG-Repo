using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private int virusAmount = 2;

    // LISTA DE PREFABS DE VIRUS CON SUS RESISTENCIAS
    [SerializeField] private List<GameObject> virusPrefabs;

    private List<GameObject> selectedVirus;
    private Dictionary<string, GameObject> bestTurrets;
    private Dictionary<string, int> elementsAmount;

    private void Start()
    {
        selectedVirus ??= new();
        elementsAmount ??= new();
        bestTurrets ??= new();

        // Random selection
        for (int i = 0; i < virusAmount; i++)
        {
            int randomIndex = Random.Range(0, virusPrefabs.Count);

            if (!selectedVirus.Contains(virusPrefabs[randomIndex]))
            {
                selectedVirus.Add(virusPrefabs[randomIndex]);
                Debug.Log($"Selected virus is {selectedVirus[i].GetComponent<VirusTemp>().Name}");
            }
        }

        // Get best turrets
        for (int i = 0; i < selectedVirus.Count; i++)
        {
            List<string> debugTurrets = TurretsManager.Instance.GetBestTurrets(selectedVirus[i].GetComponent<VirusTemp>().ResistenceToElements);
            Debug.Log($"Best turret against {selectedVirus[i].GetComponent<VirusTemp>().Name} is {debugTurrets[0]}");
            bestTurrets.Add(debugTurrets[0], selectedVirus[i]);
        }

        // Decompose turrets
        elementsAmount = TurretsManager.Instance.GetElementsAmount(bestTurrets);

        foreach (KeyValuePair<string, int> element in elementsAmount)
        {
            Debug.Log($"Element: {element.Key}, Amount: {element.Value}");
        }
    }
}
