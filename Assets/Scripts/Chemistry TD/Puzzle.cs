using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Android.Types;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    [SerializeField] private UIManager ui;
    [SerializeField] private VirusSpawnManager vsm;

    [SerializeField, Range(2, 4)] private int virusAmount = 2;
    [SerializeField, Range(1f, 3f)] private float resistanceAmount = 1f; // esto x virusamount = la cantidad de elementos totales a los que los virus son resistentes
    [SerializeField, Range(0f, 1f)] private float DMGOutputDifficulty;

    List<string> virusTypes = new()
    {
        "H", "O", "C", "N", "B",
        "HO", "HC", "HN", "HB", "OC", "ON", "OB", "CN", "CB", "NB",
        "HOC", "HON", "HOB", "HCN", "HCB", "HNB",
        "OCN", "OCB", "ONB",
        "CNB",
    };

    private List<string> selectedVirus;
    private Dictionary<string, int> bestTurrets;
    private Dictionary<string, int> selectedBestTurrets;
    private Dictionary<string, int> elementsAmount;

    private int maxTrys = 50;
    private float tolerance = 0.01f;

    private void OnValidate()
    {
        float floor = Mathf.Floor(resistanceAmount);
        if (resistanceAmount - floor > 0.5f)
            resistanceAmount = floor + 0.5f;
        else
            resistanceAmount = floor;
    }

    private void Start()
    {
        selectedVirus ??= new();
        elementsAmount ??= new();
        bestTurrets ??= new();
        selectedBestTurrets ??= new();

        SelectRandomVirus();
        //AdjustAmountOfResistances();

        float maxDMGOutput = 3 + (2 * (virusAmount - 1));
        float desiredDMGOutput = maxDMGOutput * DMGOutputDifficulty;

        while (maxTrys > 0)
        {
            float currentDifficulty = CalculateDifficulty();
            if (Mathf.Abs(currentDifficulty - desiredDMGOutput) < tolerance)
                break;
            else if (maxTrys-- > 0)
                Move(currentDifficulty - desiredDMGOutput);
        }

        Debug.Log($"Selected virus: {string.Join(", ", selectedVirus)}");

        // Decompose turrets
        elementsAmount = TurretsManager.Instance.GetElementsAmount(selectedBestTurrets.Keys.ToList());
        AddConfusingAtoms(2, true);

        ui.InitializeCounters(elementsAmount);
        vsm.StartRound(selectedVirus);
    }

    private void AdjustAmountOfResistances()
    {
        Debug.Log("AdjustingResistances");

        int finalAmount = (int)(virusAmount * resistanceAmount);
        int currentAmount = selectedVirus.Sum(v => v.Length);

        while (currentAmount != finalAmount)
        {
            Debug.Log("Inside Big Loop");

            if (currentAmount < finalAmount)
            {
                string randomV;
                //faltan resistencias, agarrar un random, ver si le puedo agregar y hacerlo, sino re seleccionar
                while (true)
                {
                    Debug.Log("Inside Little Loop - Adding");

                    randomV = selectedVirus[Random.Range(0, selectedVirus.Count)];
                    if (randomV.Length < 3)
                        break;
                }

                Debug.Log("Outside Little Loop - Adding");

                char newAtom;
                do
                {
                    newAtom = virusTypes[Random.Range(0, virusTypes.Count)][0];
                } while (randomV.Contains(newAtom));

                randomV += newAtom;
                selectedVirus[selectedVirus.IndexOf(randomV)] = randomV;
            }
            else
            {
                string randomV;
                //sobran resistencias, agarrar un random, ver si le puedo restar y hacerlo, sino re seleccionar
                while (true)
                {
                    Debug.Log("Inside Little Loop - Removing");

                    randomV = selectedVirus[Random.Range(0, selectedVirus.Count)];
                    if (randomV.Length > 1)
                        break;
                }

                Debug.Log("Outside Little Loop - Removing");

                string previousV = randomV;
                char atomToRemove = randomV[Random.Range(0, randomV.Length)];
                randomV = randomV.Replace(atomToRemove.ToString(), "");
                selectedVirus[selectedVirus.IndexOf(previousV)] = randomV;
            }
        }
    }

    private void SelectRandomVirus()
    {
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
    }

    private float CalculateDifficulty()
    {
        float maxDMGOutput = 3 + (2 * (virusAmount - 1));

        bestTurrets = TurretsManager.Instance.GetBestTurrets(selectedVirus);
        selectedBestTurrets.Clear();

        foreach (KeyValuePair<string, int> t in bestTurrets)
        {
            selectedBestTurrets.Add(t.Key, t.Value);
            if (selectedBestTurrets.Count == virusAmount)
                break;
        }

        float fit = 0;

        foreach (var turret in selectedBestTurrets)
        {
            fit += turret.Value;
        }

        return fit / maxDMGOutput;
    }

    private void Move(float difficultyDelta)
    {
        float maxDMGOutput = 3 + (2 * (virusAmount - 1));
        float desiredDMGOutput = maxDMGOutput * DMGOutputDifficulty;
        float currentDifficulty = CalculateDifficulty();
        float bestDelta = Mathf.Abs(currentDifficulty - desiredDMGOutput);
        List<string> originalVirus = new List<string>(selectedVirus);

        for (int attempt = 0; attempt < 20; attempt++)
        {
            if (selectedVirus.Count == 0)
                break;

            int virusIndex = Random.Range(0, selectedVirus.Count);
            string virus = selectedVirus[virusIndex];

            if (virus.Length == 0)
                continue;

            int charIndex = Random.Range(0, virus.Length);
            char oldChar = virus[charIndex];

            // Get all possible new chars not in the virus
            List<char> possibleChars = virusTypes
                .SelectMany(vt => vt.ToCharArray())
                .Distinct()
                .Where(c => !virus.Contains(c))
                .ToList();

            if (possibleChars.Count == 0)
                continue;

            char newChar = possibleChars[Random.Range(0, possibleChars.Count)];
            char[] virusChars = virus.ToCharArray();
            virusChars[charIndex] = newChar;
            string newVirus = new string(virusChars);

            // Ensure no duplicate viruses
            if (selectedVirus.Contains(newVirus))
                continue;

            selectedVirus[virusIndex] = newVirus;
            float newDifficulty = CalculateDifficulty();
            float newDelta = Mathf.Abs(newDifficulty - desiredDMGOutput);

            if (newDelta < bestDelta)
            {
                // Found a better solution, exit
                break;
            }
            else
            {
                // Revert and try again
                selectedVirus = new List<string>(originalVirus);
            }
        }
    }

    private void AddConfusingAtoms(int extraTurrets = 1, bool useLeastOptimal = true)
    {
        HashSet<string> usedTurrets = new(selectedBestTurrets.Keys.ToList());
        List<string> candidateTurrets = bestTurrets.Keys.Except(selectedBestTurrets.Keys).ToList();

        // Buscar torretas candidatas que no sean las óptimas ya seleccionadas
        
        if (useLeastOptimal)
            candidateTurrets.Reverse(); // Usar las menos óptimas primero

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
