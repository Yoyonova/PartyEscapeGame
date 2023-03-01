using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerBehavior : MonoBehaviour
{
    public int x, y = 0;
    public EnemyManager enemyManager;

    public void TryEscape()
    {
        List<(int, int)> steps = new List<(int, int)>();
        steps.Add((x, y));
        bool canEscape = EscapeStep(steps);

        if(canEscape)
        {
            Destroy(transform.gameObject);
        }
    }

    private bool EscapeStep(List<(int, int)> steps)
    {
        (int, int) position = steps.Last();

        // Exit found
        if(position == (enemyManager.exitX, enemyManager.exitY))
        {
            return true;
        }

        // Obstacle Encountered OR Intersected with previous step
        if(enemyManager.IsObstructed(position.Item1, position.Item2) || steps.GetRange(0, steps.Count - 1).Contains(position))
        {
            return false;
        }

        // Go Right
        steps.Add((position.Item1 + 1, position.Item2));
        if (EscapeStep(steps)) return true;
        steps.RemoveAt(steps.Count - 1);

        // Go Down
        steps.Add((position.Item1, position.Item2 - 1));
        if (EscapeStep(steps)) return true;
        steps.RemoveAt(steps.Count - 1);

        // Go Left
        steps.Add((position.Item1 - 1, position.Item2));
        if (EscapeStep(steps)) return true;
        steps.RemoveAt(steps.Count - 1);

        // Go Up
        steps.Add((position.Item1, position.Item2 + 1));
        if (EscapeStep(steps)) return true;
        steps.RemoveAt(steps.Count - 1);

        return false;
    }

    public void UpdatePosition()
    {
        transform.localPosition = new Vector3((x + 0.5f) * enemyManager.cellSize, (y + 0.5f) * enemyManager.cellSize);
        transform.localScale = new Vector3(enemyManager.cellSize, enemyManager.cellSize);
    }
}
