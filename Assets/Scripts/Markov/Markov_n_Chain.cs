using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

public class Markov_n_Chain : MonoBehaviour
{
    public TextAsset textFile; // Arrastra tu archivo .txt aquí desde el editor
    public int n = 3; // Tamaño del n-grama (en columnas)
    public int levelSize = 10; // Cantidad de columnas a generar

    [Header("Seed (dejar en 0 para aleatoria)")]
    public int manualSeed = 0; // Ingresa la seed manualmente aquí
    [ReadOnly] public int usedSeed; // Última seed utilizada (solo lectura)

    // Diccionario: patrón (n columnas) -> (columna siguiente -> ocurrencias)
    private Dictionary<string, Dictionary<string, int>> nGramCounts = new();
    // Diccionario: patrón (n columnas) -> (columna siguiente -> probabilidad)
    private Dictionary<string, Dictionary<string, float>> nGramProbs = new();

    private int width;
    private int height;
    private List<string> columns = new();

    private TextAsset lastTextFile;
    private int lastN;

    private void Start()
    {
        lastTextFile = textFile;
        lastN = n;

        if (textFile != null)
        {
            ParseTextToColumns(textFile.text);
            DetectPatrons(n);
            CalculateProbabilities();
            GenerateColumns(levelSize, manualSeed != 0 ? manualSeed : null);
        }
        else
        {
            Debug.LogError("No se ha asignado un archivo de texto en el Inspector.");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            bool textFileChanged = textFile != lastTextFile;
            bool nChanged = n != lastN;

            if (textFileChanged || nChanged)
            {
                if (textFile != null)
                {
                    ParseTextToColumns(textFile.text);
                    DetectPatrons(n);
                    CalculateProbabilities();
                    lastTextFile = textFile;
                    lastN = n;
                }
                else
                {
                    Debug.LogError("No se ha asignado un archivo de texto en el Inspector.");
                    return;
                }
            }

            // Si no hay seed manual, genera una aleatoria y la almacena
            int? seedToUse = manualSeed != 0 ? manualSeed : (int?)UnityEngine.Random.Range(1, int.MaxValue);
            GenerateColumns(levelSize, seedToUse);
        }
    }

    private void ParseTextToColumns(string text)
    {
        string[] lines = text.Replace("\r", "").Split('\n');
        height = lines.Length;
        width = lines[0].Length;
        columns.Clear();

        for (int col = 0; col < width; col++)
        {
            char[] colChars = new char[height];
            for (int row = 0; row < height; row++)
            {
                if (col < lines[row].Length)
                    colChars[row] = lines[row][col];
                else
                    colChars[row] = ' ';
            }

            columns.Add(new string(colChars));
        }
    }

    private void DetectPatrons(int n)
    {
        nGramCounts.Clear();
        if (columns.Count < n + 1) return;

        for (int i = 0; i <= columns.Count - n - 1; i++)
        {
            // El patrón es la concatenación de n columnas
            string key = string.Join("|", columns.GetRange(i, n));
            string nextCol = columns[i + n];

            if (!nGramCounts.ContainsKey(key))
                nGramCounts[key] = new Dictionary<string, int>();

            if (!nGramCounts[key].ContainsKey(nextCol))
                nGramCounts[key][nextCol] = 0;

            nGramCounts[key][nextCol]++;
        }
    }

    private void CalculateProbabilities()
    {
        nGramProbs.Clear();
        foreach (var kvp in nGramCounts)
        {
            string key = kvp.Key;
            Dictionary<string, int> nextCounts = kvp.Value;
            int total = 0;
            foreach (var count in nextCounts.Values)
                total += count;

            nGramProbs[key] = new Dictionary<string, float>();
            foreach (var pair in nextCounts)
            {
                nGramProbs[key][pair.Key] = (float)pair.Value / total;
            }
        }
    }

    public void GenerateColumns(int numColumns, int? seed = null)
    {
        List<string> keys = new List<string>(nGramProbs.Keys);
        if (keys.Count == 0)
            return;

        // Seed management
        int actualSeed = seed ?? UnityEngine.Random.Range(1, int.MaxValue);
        usedSeed = actualSeed;
        System.Random rng = new System.Random(actualSeed);

        // Elige un patrón inicial aleatorio
        string current = keys[rng.Next(keys.Count)];
        List<string> pattern = new List<string>(current.Split('|'));

        while (pattern.Count < numColumns)
        {
            string key = string.Join("|", pattern.GetRange(pattern.Count - n, n));
            if (!nGramProbs.ContainsKey(key) || nGramProbs[key].Count == 0)
                break;

            string nextCol = GetRandomNextColumn(nGramProbs[key], rng);
            pattern.Add(nextCol);
        }

        string levelString = ColumnsToString(pattern);

        string filePath = Path.Combine(Application.dataPath, "GeneratedLevel.txt");
        try
        {
            File.WriteAllText(filePath, levelString);
            Debug.Log($"Nivel guardado en: {filePath} (Seed: {usedSeed})");
        }
        catch (IOException ex)
        {
            Debug.LogError("Error al guardar el archivo: " + ex.Message);
        }
    }

    // Selecciona una columna según la probabilidad y la seed
    private string GetRandomNextColumn(Dictionary<string, float> probs, System.Random rng)
    {
        double rand = rng.NextDouble();
        double cumulative = 0.0;
        foreach (var pair in probs)
        {
            cumulative += pair.Value;
            if (rand <= cumulative)
                return pair.Key;
        }
        // Fallback
        foreach (var pair in probs)
            return pair.Key;
        return "";
    }

    // Convierte la lista de columnas a un string para visualizar el nivel
    private string ColumnsToString(List<string> cols)
    {
        if (cols == null || cols.Count == 0) return "";
        int h = cols[0].Length;
        System.Text.StringBuilder sb = new();
        for (int row = 0; row < h; row++)
        {
            for (int col = 0; col < cols.Count; col++)
            {
                sb.Append(cols[col][row]);
            }
            sb.AppendLine();
        }
        return sb.ToString();
    }
}
