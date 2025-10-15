using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalMap
{
    public List<Direction> dMap = new();
    public List<Vector2> path = new();
    public Vector2 startPos;
    public Vector2 currentPos;
    public int limit = 11; //values from 0 to 11; those that have 0 or 11 are boundaries (walls), only used for starting and ending points
    public float fitness;

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    };

    public Vector2 DirectionToVector2(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return new Vector2(0, 1);
            case Direction.Down:
                return new Vector2(0, -1);
            case Direction.Left:
                return new Vector2(-1, 0);
            case Direction.Right:
                return new Vector2(1, 0);
            default: 
                return new Vector2(0, 0);
        }
    }

    public DirectionalMap()
    {
        // Select a random starting position on the boundary
        int XorY = Random.Range(0, 2);
        startPos = currentPos = (XorY == 0) ? (Random.Range(0, 2) == 0) ? new Vector2(0, Random.Range(1, limit)) : new Vector2(11, Random.Range(1, limit))
                                            : (Random.Range(0, 2) == 0) ? new Vector2(Random.Range(1, limit), 0) : new Vector2(Random.Range(1, limit), 11);
        path.Add(startPos);

        if (startPos.x == 0) dMap.Add(Direction.Right);
        else if (startPos.y == 0) dMap.Add(Direction.Up);
        else if (startPos.x == limit) dMap.Add(Direction.Left);
        else if (startPos.y == limit) dMap.Add(Direction.Down);

        ActualizeCurrentPosInMap(dMap[dMap.Count - 1]);

        while (currentPos.x != 0 && currentPos.y != 0 && currentPos.x != limit && currentPos.y != limit)
        {
            Direction nm = PickNextMove();

            if (nm == Direction.None)
            {
                Debug.Log("No possible moves left, stopping path generation.");
                break;
            }

            dMap.Add(nm);
            ActualizeCurrentPosInMap(dMap[dMap.Count - 1]);
        }
    }

    private Direction PickNextMove()
    {
        List<Direction> possibleMoves = new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };
        
        while (true)
        {
            if (possibleMoves.Count == 0)
                return Direction.None;
            Direction nextMove = possibleMoves[Random.Range(0, possibleMoves.Count)];
            if (path.Contains(currentPos + DirectionToVector2(nextMove)))
            {
                possibleMoves.Remove(nextMove);
            }
            else
            {
                return nextMove;
            }
        }
    }

    private void ActualizeCurrentPosInMap(Direction dir)
    {
        currentPos += DirectionToVector2(dir);
        path.Add(currentPos);
    }

    public void CalculateFitness()
    {
        float neighborsFitness = NeighborsFitness();
        float extensionFitness = ExtensionFitness(); /*Agregarle wieght para jugar con los resultados*/
        float coverageFitness = CoverageFitness(); /*Agregarle wieght para manipular la dificultad*/
        fitness = (neighborsFitness + extensionFitness + coverageFitness) / 3f;
    }

    // Fitnsess de los vecinos
    private float NeighborsFitness()
    {
        int maxCellScore = 2;
        int maxPossibleScore = path.Count * maxCellScore - 2;
        int totalScore = 0;
        foreach (Vector2 cell in path) 
        { 
            int connections = 0;
            // Up
            if (path.Contains(cell + DirectionToVector2(Direction.Up))) connections++;
            // Down
            if (path.Contains(cell + DirectionToVector2(Direction.Down))) connections++;
            // Left
            if (path.Contains(cell + DirectionToVector2(Direction.Left))) connections++;
            // Right
            if (path.Contains(cell + DirectionToVector2(Direction.Right))) connections++;

            if (connections == 2)
                totalScore += maxCellScore; // Maximum score
            else if (connections == 1 || connections == 3)
                totalScore += 1; // Medium score
            // 0 or 4 connections: minimum score (0)
        }

        float fitness = (float)totalScore / maxPossibleScore;
        return fitness;
    }

    // Fitness de extensión
    private float ExtensionFitness()
    {
        List<int> x = new List<int>();
        List<int> y = new List<int>();

        foreach (Vector2 cell in path)
        {
            if (!x.Contains((int)cell.x)) x.Add((int)cell.x);
            if (!y.Contains((int)cell.y)) y.Add((int)cell.y);
        }

        int xExtension = x.Count;
        int yExtension = y.Count;
        int maxExtension = limit * 2;
        return (float)(xExtension + yExtension) / maxExtension;
    }

    // Fitness del coverage
    private float CoverageFitness()
    {
        int maxExtension = (limit - 1) % 2 == 0 ? (limit - 1) / 2 * (limit - 1) + (limit - 1) / 2
                                                : limit / 2 * (limit - 1) + (limit) / 2 - 1;
        return (float)path.Count / maxExtension;
    }

    public void Perturbate(int pos)
    {
        currentPos = startPos;
        path = new List<Vector2> { startPos };

        foreach (Direction dir in dMap.GetRange(0, pos - 1))
        {
            ActualizeCurrentPosInMap(dir);
        }

        dMap[pos] = PickNextMove();
        int curentPosIndex = pos;

        foreach (Direction dir in dMap.GetRange(pos + 1, dMap.Count - (pos + 1)))
        {
            curentPosIndex++;
            if (currentPos.x != 0 && currentPos.y != 0 && currentPos.x != limit && currentPos.y != limit)
            {
                dMap.RemoveRange(curentPosIndex, dMap.Count - (curentPosIndex + 1));
                break;
            }
            ActualizeCurrentPosInMap(dir);
        }

        while (currentPos.x != 0 && currentPos.y != 0 && currentPos.x != limit && currentPos.y != limit)
        {
            Direction nm = PickNextMove();
            dMap.Add(nm);
            ActualizeCurrentPosInMap(dMap[dMap.Count - 1]);
        }
    }
}
