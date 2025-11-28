using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;

public class WFC : MonoBehaviour
{
    public TextAsset sampleFile; // Arrastra tu .txt sample aquí
    public int patternSize = 2; // Tamaño del patrón cuadrado (ej: 2 para 2x2)
    public int outputWidth = 10;
    public int outputHeight = 10;
    public string outputFileName = "GeneratedLevel.txt";

    [Header("Seed (0 = aleatoria)")]
    public int manualSeed = 0; // Ingresa la seed manualmente aquí
    [ReadOnly] public int usedSeed; // Última seed utilizada (solo lectura)

    private int[,] sampleGrid;
    private int sampleWidth, sampleHeight;

    // Estructura de patrón
    class Pattern
    {
        public int[,] data;
        public int hash;
        public List<int> compatibleUp = new();
        public List<int> compatibleDown = new();
        public List<int> compatibleLeft = new();
        public List<int> compatibleRight = new();
        public int count = 1;
    }

    private List<Pattern> patterns = new();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            GenerateMap();
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Start();
        }
    }

    void Start()
    {
        if (sampleFile == null)
        {
            Debug.LogError("No se ha asignado un archivo de muestra.");
            return;
        }

        ParseSample(sampleFile.text);
        ExtractPatterns();
        FindCompatibilities();
        GenerateMap();
        Debug.Log("WFC terminado. Archivo generado: " + outputFileName + " (Seed: " + usedSeed + ")");
    }

    void GenerateMap()
    {
        int seedToUse = manualSeed != 0 ? manualSeed : UnityEngine.Random.Range(1, int.MaxValue);
        usedSeed = seedToUse;
        int[,] outputGrid = RunWFC(seedToUse);
        SaveGridToTxt(outputGrid, outputFileName);
        Debug.Log("Mapa generado con seed: " + usedSeed);
    }

    // 1. Parsear el sample a matriz
    void ParseSample(string text)
    {
        string[] lines = text.Replace("\r", "").Split('\n');
        sampleHeight = lines.Length;
        sampleWidth = lines[0].Length;
        sampleGrid = new int[sampleWidth, sampleHeight];
        for (int y = 0; y < sampleHeight; y++)
            for (int x = 0; x < sampleWidth; x++)
                sampleGrid[x, y] = lines[y][x] - '0';
    }

    // 2. Extraer patrones únicos y contar ocurrencias
    void ExtractPatterns()
    {
        patterns.Clear();
        Dictionary<int, Pattern> patternDict = new();

        for (int y = 0; y <= sampleHeight - patternSize; y++)
        {
            for (int x = 0; x <= sampleWidth - patternSize; x++)
            {
                int[,] pat = new int[patternSize, patternSize];
                for (int dy = 0; dy < patternSize; dy++)
                    for (int dx = 0; dx < patternSize; dx++)
                        pat[dx, dy] = sampleGrid[x + dx, y + dy];

                int hash = GetPatternHash(pat);
                if (!patternDict.ContainsKey(hash))
                {
                    patternDict[hash] = new Pattern { data = pat, hash = hash, count = 1 };
                }
                else
                {
                    patternDict[hash].count++;
                }
            }
        }
        patterns.AddRange(patternDict.Values);
    }

    // 3. Calcular compatibilidades de adyacencia
    void FindCompatibilities()
    {
        for (int i = 0; i < patterns.Count; i++)
        {
            for (int j = 0; j < patterns.Count; j++)
            {
                if (IsCompatible(patterns[i], patterns[j], "up"))
                    patterns[i].compatibleUp.Add(j);
                if (IsCompatible(patterns[i], patterns[j], "down"))
                    patterns[i].compatibleDown.Add(j);
                if (IsCompatible(patterns[i], patterns[j], "left"))
                    patterns[i].compatibleLeft.Add(j);
                if (IsCompatible(patterns[i], patterns[j], "right"))
                    patterns[i].compatibleRight.Add(j);
            }
        }
    }

    // 4. Ejecutar WFC
    int[,] RunWFC(int seed)
    {
        int w = outputWidth;
        int h = outputHeight;
        // Cada celda tiene un set de patrones posibles        
        List<int>[,] wave = new List<int>[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                wave[x, y] = new List<int>();
                for (int i = 0; i < patterns.Count; i++)
                    wave[x, y].Add(i);
            }

        System.Random rng = new System.Random(seed);

        while (true)
        {
            // Encuentra la celda con menor entropía (>1 posibilidades)            
            int minCount = int.MaxValue;
            List<(int x, int y)> minCells = new();
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    int count = wave[x, y].Count;
                    if (count > 1 && count < minCount)
                    {
                        minCount = count;
                        minCells.Clear();
                        minCells.Add((x, y));
                    }
                    else if (count == minCount)
                        minCells.Add((x, y));
                }
            if (minCount == int.MaxValue) break; // Todas colapsadas

            // Selecciona una celda aleatoria de las de menor entropía            
            var (cx, cy) = minCells[rng.Next(minCells.Count)];

            // Colapsa: elige un patrón posible según frecuencia
            List<int> possible = wave[cx, cy];
            int chosen = WeightedRandomPattern(possible, rng);
            wave[cx, cy] = new List<int> { chosen };

            // Propaga restricciones a vecinos            
            Propagate(wave, cx, cy, w, h);
        }

        // Construye la grilla final usando el patrón colapsado en cada celda
        int[,] grid = new int[w, h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                int patIdx = wave[x, y].Count > 0 ? wave[x, y][0] : 0;
                int[,] pat = patterns[patIdx].data;
                grid[x, y] = pat[0, 0]; // Solo toma el valor superior izquierdo del patrón
            }
        return grid;
    }

    // 5. Guardar la grilla en un .txt    
    void SaveGridToTxt(int[,] grid, string fileName)
    {
        string path = Path.Combine(Application.dataPath, fileName);
        using (StreamWriter sw = new StreamWriter(path))
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                    sw.Write(grid[x, y]);
                sw.WriteLine();
            }
        }
    }

    int GetPatternHash(int[,] pat)
    {
        int hash = 17;
        for (int dy = 0; dy < patternSize; dy++)
            for (int dx = 0; dx < patternSize; dx++)
                hash = hash * 31 + pat[dx, dy];
        return hash;
    }

    bool IsCompatible(Pattern a, Pattern b, string dir)
    {
        // Compara los bordes según la dirección        
        if (dir == "up")
        {
            for (int i = 0; i < patternSize; i++)
                if (a.data[i, 0] != b.data[i, patternSize - 1])
                    return false;
        }
        else if (dir == "down")
        {
            for (int i = 0; i < patternSize; i++)
                if (a.data[i, patternSize - 1] != b.data[i, 0])
                    return false;
        }
        else if (dir == "left")
        {
            for (int i = 0; i < patternSize; i++)
                if (a.data[0, i] != b.data[patternSize - 1, i])
                    return false;
        }
        else if (dir == "right")
        {
            for (int i = 0; i < patternSize; i++)
                if (a.data[patternSize - 1, i] != b.data[0, i])
                    return false;
        }
        return true;
    }

    int WeightedRandomPattern(List<int> possible, System.Random rng)
    {
        // Elige un patrón según su frecuencia en el sample        
        int total = 0;
        foreach (int idx in possible)
            total += patterns[idx].count;
        int r = rng.Next(total);
        int sum = 0;
        foreach (int idx in possible)
        {
            sum += patterns[idx].count;
            if (r < sum)
                return idx;
        }
        return possible[0];
    }

    void Propagate(List<int>[,] wave, int x, int y, int w, int h)
    {
        Queue<(int x, int y)> queue = new();
        queue.Enqueue((x, y));
        while (queue.Count > 0)
        {
            var (cx, cy) = queue.Dequeue();
            foreach (var dir in new[] { (0, -1, "up"), (0, 1, "down"), (-1, 0, "left"), (1, 0, "right") })
            {
                int nx = cx + dir.Item1;
                int ny = cy + dir.Item2;
                if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;

                var neighbor = wave[nx, ny];
                int before = neighbor.Count;
                neighbor.RemoveAll(idx => !HasCompatible(wave[cx, cy], idx, dir.Item3));
                if (neighbor.Count == 0)
                {
                    // Si se queda sin opciones, restaura todos los patrones posibles
                    for (int i = 0; i < patterns.Count; i++)
                        neighbor.Add(i);
                    Debug.LogWarning($"Celda ({nx},{ny}) quedó vacía, se restauran todos los patrones.");
                }
                if (neighbor.Count < before)
                    queue.Enqueue((nx, ny));
            }
        }
    }

    bool HasCompatible(List<int> sourcePatterns, int neighborIdx, string dir)
    {
        foreach (int srcIdx in sourcePatterns)
        {
            Pattern src = patterns[srcIdx];
            Pattern neighbor = patterns[neighborIdx];
            if (dir == "up" && neighbor.compatibleDown.Contains(srcIdx)) return true;
            if (dir == "down" && neighbor.compatibleUp.Contains(srcIdx)) return true;
            if (dir == "left" && neighbor.compatibleRight.Contains(srcIdx)) return true;
            if (dir == "right" && neighbor.compatibleLeft.Contains(srcIdx)) return true;
        }
        return false;
    }
}
