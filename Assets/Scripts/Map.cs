using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map
{
    public int[][] map; // 0 for empty, 1 for road
    public int width = 10;
    public int height = 10;
    public float fitness;

    public float connectionWeight;
    public float coverageWeight;
    public float startEndWeight;
    public float singlePathWeight;
    public float desiredCoverage;

    public Map(float _connectionWeight, float _coverageWeight, float _startEndWeight, float _singlePathWeight, float _desiredCoverage)
    {
        connectionWeight = _connectionWeight;
        coverageWeight = _coverageWeight;
        startEndWeight = _startEndWeight;
        desiredCoverage = _desiredCoverage;
        singlePathWeight = _singlePathWeight;

        map = new int[width][];
        for (int i = 0; i < width; i++)
        {
            map[i] = new int[height];
            for (int j = 0; j < height; j++)
            {
                map[i][j] = Random.Range(0, 1);
            }
        }

        this.singlePathWeight = singlePathWeight;
    }

    public float CalculateConnectionFitness()
    {
        int maxCellScore = 2;
        int maxPossibleScore = width * height * maxCellScore;

        int totalScore = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i][j] == 1)
                {
                    int connections = 0;
                    // Up
                    if (i > 0)
                    {
                        if (map[i - 1][j] == 1) connections++;
                    }
                    else connections++;

                    // Down
                    if (i < width - 1)
                    {
                        if (map[i + 1][j] == 1) connections++;
                    }
                    else connections++;

                    // Left
                    if (j > 0)
                    {
                        if (map[i][j - 1] == 1) connections++;
                    }
                    else connections++;

                    // Right
                    if (j < height - 1)
                    {
                        if (map[i][j + 1] == 1) connections++;
                    }
                    else connections++;

                    if (connections == 2)
                        totalScore += maxCellScore; // Maximum score
                    else if (connections == 1 || connections == 3)
                        totalScore += 1; // Medium score
                    // 0 or 4 connections: minimum score (0)
                }
            }
        }

        float baseFitness = maxPossibleScore == 0 ? 0f : (float)totalScore / maxPossibleScore;
        
        if (HasCycle())
            baseFitness *= 0.5f; // Reduce fitness by 50% if a cycle is present
        
        return baseFitness;
    }

    private bool HasCycle()
    {
        bool[,] visited = new bool[width, height];

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i][j] == 1 && !visited[i, j])
                {
                    if (DFSHasCycle(i, j, -1, -1, visited))
                        return true;
                }
            }
        }
        return false;
    }

    private bool DFSHasCycle(int x, int y, int parentX, int parentY, bool[,] visited)
    {
        visited[x, y] = true;
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int dir = 0; dir < 4; dir++)
        {
            int nx = x + dx[dir];
            int ny = y + dy[dir];

            if (nx >= 0 && nx < width && ny >= 0 && ny < height && map[nx][ny] == 1)
            {
                if (!visited[nx, ny])
                {
                    if (DFSHasCycle(nx, ny, x, y, visited))
                        return true;
                }
                else if (nx != parentX || ny != parentY)
                {
                    // Found a cycle
                    return true;
                }
            }
        }
        return false;
    }

    public float CalculateCoverageFitness()
    {
        int roadCells = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (map[i][j] == 1)
                {
                    roadCells++;
                }
            }
        }
        float actualCoverage = (float)roadCells / (width * height);

        // Fitness is highest when actualCoverage == desiredCoverage, decreases as it deviates
        return 1f - Mathf.Abs(actualCoverage - desiredCoverage) / desiredCoverage;
    }

    public float CalculateStartAndEndPointsFitness()
    {
        int borderRoadCells = 0;

        for (int i = 0; i < width; i++)
        {
            // Top border
            if (map[i][0] == 1) borderRoadCells++;
            // Bottom border
            if (map[i][height - 1] == 1) borderRoadCells++;
        }
        for (int j = 1; j < height - 1; j++)
        {
            // Left border
            if (map[0][j] == 1) borderRoadCells++;
            // Right border
            if (map[width - 1][j] == 1) borderRoadCells++;
        }

        int ideal = 2;
        int diff = Mathf.Abs(borderRoadCells - ideal);

        // Fitness is highest at ideal, decreases as it moves away, symmetric for values above or below ideal
        float maxDiff = Mathf.Max(ideal, Mathf.Abs((width + height) * 2 - ideal)); // max possible border cells
        return 1f - (float)diff / maxDiff;
    }

    private float CalculateSinglePathFitness()
    {
        bool[,] visited = new bool[width, height];
        int roadCells = 0;
        int connectedCells = 0;
        bool foundStart = false;
        int startX = 0, startY = 0;

        // Busca el primer road cell como punto de inicio
        for (int i = 0; i < width && !foundStart; i++)
        {
            for (int j = 0; j < height && !foundStart; j++)
            {
                if (map[i][j] == 1)
                {
                    startX = i;
                    startY = j;
                    foundStart = true;
                }
            }
        }

        // Si no hay caminos, retorna falso
        if (!foundStart)
            return 0f;

        // Cuenta road cells totales
        for (int i = 0; i < width; i++)
            for (int j = 0; j < height; j++)
                if (map[i][j] == 1)
                    roadCells++;

        // DFS para contar los conectados y detectar ciclos
        bool hasCycle = false;
        void DFS(int x, int y, int px, int py)
        {
            visited[x, y] = true;
            connectedCells++;
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int dir = 0; dir < 4; dir++)
            {
                int nx = x + dx[dir];
                int ny = y + dy[dir];
                if (nx >= 0 && nx < width && ny >= 0 && ny < height && map[nx][ny] == 1)
                {
                    if (!visited[nx, ny])
                    {
                        DFS(nx, ny, x, y);
                    }
                    else if (nx != px || ny != py)
                    {
                        hasCycle = true;
                    }
                }
            }
        }

        DFS(startX, startY, -1, -1);

        // Debe estar todo conectado y sin ciclos
        return (connectedCells == roadCells && !hasCycle) ? 1f : 0f;
    }


    public void CalculateFinalFitness()
    {
        float connectionFitness = CalculateConnectionFitness();
        float coverageFitness = CalculateCoverageFitness();
        float startEndFitness = CalculateStartAndEndPointsFitness();
        float singlePathFitness = CalculateSinglePathFitness();

        fitness = connectionWeight * connectionFitness + coverageWeight * coverageFitness + startEndWeight * startEndFitness + singlePathWeight * singlePathFitness;
    }
}
