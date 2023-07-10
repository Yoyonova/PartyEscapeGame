using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerBehavior : MonoBehaviour
{
    public int x, y = 0;
    public EnemyManager enemyManager;

    [SerializeField] float escapeStepDuration;

    private bool hasEscaped = false;
    private Vector3 targetPosition;
    private float positionPrecision = 0.2f;
    private Rigidbody2D rigidBody;
    private List<(int, int)> steps;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (hasEscaped)
        {
            float proximity = (targetPosition - transform.localPosition).magnitude;

            if (proximity > positionPrecision)
            {
                Vector2 moveDirection = (targetPosition - transform.localPosition).normalized;
                bool isMovingAway = Vector2.Angle(moveDirection, rigidBody.velocity) > 90;

                if (isMovingAway)
                {
                    rigidBody.velocity -= rigidBody.velocity.normalized * 200 * Time.deltaTime;
                }

                rigidBody.velocity += moveDirection * 100 * Time.deltaTime;
            }
            else
            {
                if (rigidBody.velocity.magnitude <= 200 * Time.deltaTime)
                {
                    rigidBody.velocity = new Vector2(0, 0);
                }
                else
                {
                    rigidBody.velocity -= rigidBody.velocity.normalized * 200 * Time.deltaTime;
                }
            }
        }
    }

    public void TryEscape()
    {
        if (!hasEscaped)
        {
            steps = new List<(int, int)>();
            steps.Add((x, y));
            bool canEscape = EscapeStep(steps);

            if (canEscape)
            {
                StartCoroutine(Escape());
            }
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
        transform.localScale = new Vector3(enemyManager.cellSize, enemyManager.cellSize) * 0.05f;
    }

    private IEnumerator Escape()
    {
        hasEscaped = true;

        foreach((int, int) step in steps)
        {
            x = step.Item1;
            y = step.Item2;

            targetPosition = new Vector3((x + 0.5f) * EnemyManager.instance.cellSize, (y + 0.5f) * EnemyManager.instance.cellSize);

            yield return new WaitForSeconds(escapeStepDuration);
        }

        GetComponent<Renderer>().enabled = false;
        GetComponent<ParticleSystem>().Play();

        yield return new WaitForSeconds(4 * escapeStepDuration);

        SceneManager.LoadScene((SceneManager.GetActiveScene().buildIndex + 1) % SceneManager.sceneCountInBuildSettings);
    }
}