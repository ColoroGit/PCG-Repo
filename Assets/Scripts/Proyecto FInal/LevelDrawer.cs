using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelDrawer : MonoBehaviour
{
    public TextAsset levelFile; // Arrastra tu .txt aquí
    public float cellSize = 1f; // Tamaño de cada celda
    public GameObject cellPrefab; // Prefab de un cuadrado (puede ser un simple SpriteRenderer cuadrado)

    // Colores para cada tipo de celda
    public Color[] colors = { Color.gray, Color.green, Color.blue, Color.red, Color.yellow, Color.magenta, Color.cyan, Color.white, Color.black };

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // Limpia el nivel anterior
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }

            Draw();
        }
    }

    void Draw()
    {
        if (levelFile == null || cellPrefab == null)
        {
            Debug.LogError("Falta asignar el archivo de nivel o el prefab.");
            return;
        }

        string[] lines = levelFile.text.Replace("\r", "").Split('\n');
        int height = lines.Length;
        int width = lines[0].Length;

        // Dibuja la matriz
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                char c = lines[y][x];

                Debug.Log($"Dibujando celda en ({x},{y}) con valor '{c}'");

                int colorIndex;
                if (!int.TryParse(c.ToString(), out colorIndex) || colorIndex < 0 || colorIndex >= colors.Length)
                {
                    Debug.LogWarning($"Valor de celda inválido en ({x},{y}): '{c}'");
                    colorIndex = 0; // Color por defecto
                }

                GameObject cell = Instantiate(cellPrefab, new Vector3(x * cellSize, -y * cellSize, 0), Quaternion.identity, transform);
                var renderer = cell.GetComponent<SpriteRenderer>();
                if (renderer != null)
                    renderer.color = colors[colorIndex];
            }
        }
    }
}
