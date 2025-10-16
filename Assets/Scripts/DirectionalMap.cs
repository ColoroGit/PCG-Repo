using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DirectionalMap
{
    public List<Direction> dMap = new();
    public List<Vector2> path = new();
    public Vector2 startPos;
    public Vector2 currentPos;
    public int turns;
    public int limit = 11; //values from 0 to 11; those that have 0 or 11 are boundaries (walls), only used for starting and ending points
    
    public float extensionWeight;
    public float coverageWeight;
    public float turnsWeight;
    public float turnsDensity;
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

    public Direction OppositeDirection(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            case Direction.Right:
                return Direction.Left;
            default:
                return Direction.None;
        }
    }

    public DirectionalMap(float _extensionWeight, float _coverageWeight, float _turnsWeigt, float _turnsDensity)
    {
        extensionWeight = _extensionWeight;
        coverageWeight = _coverageWeight;
        turnsWeight = _turnsWeigt;
        turnsDensity = _turnsDensity;

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
            dMap.Add(nm);
            ActualizeCurrentPosInMap(dMap[dMap.Count - 1]);

            if (nm == Direction.None)
            {
                Debug.Log("No possible moves left, stopping path generation.");
                fitness = 0;
                break;
            }
        }
    }

    private Direction PickNextMove()
    {
        List<Direction> possibleMoves = new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        possibleMoves.Remove(OppositeDirection(dMap[dMap.Count - 1]));

        if (dMap.Count > 1 && dMap[dMap.Count - 1] != dMap[dMap.Count - 2])
        {
            possibleMoves.Remove(OppositeDirection(dMap[dMap.Count - 2]));            
        }

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
                if (SimulateNextMove(nextMove))
                    return nextMove;
                else
                    possibleMoves.Remove(nextMove);
            }
        }
    }

    public bool SimulateNextMove(Direction nm)
    {
        Vector2 simulatedPos = currentPos + DirectionToVector2(nm);

        List<Direction> possibleMoves = new List<Direction> { Direction.Up, Direction.Down, Direction.Left, Direction.Right };

        possibleMoves.Remove(OppositeDirection(nm));

        if (nm != dMap[dMap.Count - 1])
        {
            possibleMoves.Remove(OppositeDirection(dMap[dMap.Count - 1]));
        }

        foreach (Direction dir in possibleMoves)
        {
            Vector2 newSimulatedPos = simulatedPos + DirectionToVector2(dir);
            if (path.Contains(newSimulatedPos))
            {
                return false;
            }
        }

        return true;
    }

    private void ActualizeCurrentPosInMap(Direction dir)
    {
        currentPos += DirectionToVector2(dir);
        path.Add(currentPos);
    }

    public void CalculateFitness()
    {
        if (dMap[dMap.Count - 1] == Direction.None) { fitness = 0; return; }

        float extensionFitness = ExtensionFitness() * extensionWeight;
        float coverageFitness = CoverageFitness() * coverageWeight;
        float turnsFitness = TurnsFitness() * turnsWeight; 
        fitness = (extensionFitness + coverageFitness + turnsFitness) / 3f;
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

    private float TurnsFitness()
    {
        turns = 0;
        for (int i = 1; i < dMap.Count; i++)
        {
            if (dMap[i] != dMap[i - 1])
                turns++;
        }

        float maxDiff = dMap.Count;
        float amplifiedDensity = turnsDensity * maxDiff;
        float diff = Mathf.Abs(turns - amplifiedDensity);

        // Mientras diff sea más pequeño, más fitness
        return 1f - Mathf.Clamp01(diff / maxDiff);
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
            // Si ya llegamos a un borde, salimos
            if (currentPos.x == 0 || currentPos.y == 0 || currentPos.x == limit || currentPos.y == limit)
            {
                dMap.RemoveRange(curentPosIndex, dMap.Count - curentPosIndex);
                break;
            }
            // Si la dirección no es válida (por restricciones de PickNextMove), salimos
            if (path.Contains(currentPos + DirectionToVector2(dir)) || !SimulateNextMove(dir))
            {
                dMap.RemoveRange(curentPosIndex, dMap.Count - curentPosIndex);
                break;
            }
            ActualizeCurrentPosInMap(dir);
        }

        while (currentPos.x != 0 && currentPos.y != 0 && currentPos.x != limit && currentPos.y != limit)
        {
            Direction nm = PickNextMove();
            dMap.Add(nm);
            ActualizeCurrentPosInMap(dMap[dMap.Count - 1]);

            if (nm == Direction.None)
            {
                Debug.Log("No possible moves left, stopping path generation.");
                fitness = 0;
                break;
            }
        }
    }
}
