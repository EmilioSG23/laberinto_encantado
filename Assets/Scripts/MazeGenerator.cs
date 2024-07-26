using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator2D : MonoBehaviour
{
    public int mazeWidth = 10;
    public int mazeHeight = 10;
    public float cellSize = 1f;
    public GameObject wallPrefab;

    private int[,] maze;
    private List<Vector2Int> stack = new List<Vector2Int>();

    void Start()
    {
        maze = new int[mazeHeight, mazeWidth];
        GenerateMaze();
        DrawMaze();
    }

    void GenerateMaze()
    {
        for (int y = 0; y < mazeHeight; y++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                maze[y, x] = 1; // 1 means wall, 0 means path
            }
        }

        Vector2Int currentCell = new Vector2Int(0, 0);
        stack.Add(currentCell);
        maze[currentCell.y, currentCell.x] = 0;

        while (stack.Count > 0)
        {
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(currentCell);
            if (neighbors.Count > 0)
            {
                Vector2Int nextCell = neighbors[Random.Range(0, neighbors.Count)];
                stack.Add(currentCell);
                RemoveWall(currentCell, nextCell);
                currentCell = nextCell;
                maze[currentCell.y, currentCell.x] = 0;
            }
            else
            {
                currentCell = stack[stack.Count - 1];
                stack.RemoveAt(stack.Count - 1);
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        Vector2Int[] directions = {
            new Vector2Int(0, 1),
            new Vector2Int(0, -1),
            new Vector2Int(1, 0),
            new Vector2Int(-1, 0)
        };

        foreach (var direction in directions)
        {
            Vector2Int neighbor = cell + direction;
            if (IsWithinBounds(neighbor) && maze[neighbor.y, neighbor.x] == 1)
            {
                neighbors.Add(neighbor);
            }
        }
        return neighbors;
    }

    bool IsWithinBounds(Vector2Int cell)
    {
        return cell.x >= 0 && cell.x < mazeWidth && cell.y >= 0 && cell.y < mazeHeight;
    }

    void RemoveWall(Vector2Int currentCell, Vector2Int nextCell)
    {
        Vector2Int wallPosition = (currentCell + nextCell) / 2;
        maze[wallPosition.y, wallPosition.x] = 0;
    }

    void DrawMaze()
    {
        for (int y = 0; y < mazeHeight; y++)
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                if (maze[y, x] == 1)
                {
                    Instantiate(wallPrefab, new Vector3(x * cellSize, y * cellSize, 0), Quaternion.identity);
                }
            }
        }
    }
}
