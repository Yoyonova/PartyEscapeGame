using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float acceleration = 50f;
    [SerializeField] private float positionPrecision = 0.2f;
    [SerializeField] private float deceleration = 50f;
    [SerializeField] private float randomSpeedVariation = 0.2f;
    [SerializeField] private float InstantSpeed = 15f;
    private float randomSpeedFactor = 1f;
    private Rigidbody2D rigidBody;

    public int x, y, xProspective, yProspective;
    private int xBonk, yBonk;
    public bool willBonk = false;
    private Vector3 targetPosition;

    public bool[] attributes;
    public SpriteRenderer[] attributeIcons;
    private bool hasBeenAss = false;
    private bool hasBeenRomanced = false;

    public bool isAlive = true;
    public bool willBeAlive = true;

    private List<Vector2Int> recentPositions = new();
    public bool didSomething = false;

    private List<Vector2Int> positionHistory;
    private List<bool> lifeHistory;
    private List<bool[]> attributeHistory;

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody2D>();

        randomSpeedFactor = 1f - randomSpeedVariation + (2 * Random.value * randomSpeedVariation);
    }

    private void FixedUpdate()
    {
        float proximity = (targetPosition - transform.localPosition).magnitude;

        if (proximity > positionPrecision)
        {
            Vector2 moveDirection = (targetPosition - transform.localPosition).normalized;
            bool isMovingAway = Vector2.Angle(moveDirection, rigidBody.velocity) > 90;

            if (isMovingAway) {
                rigidBody.velocity -= rigidBody.velocity.normalized * deceleration * Time.deltaTime * randomSpeedFactor;
            }

            rigidBody.velocity += moveDirection * acceleration * Time.deltaTime * randomSpeedFactor;
        } else
        {
            if (rigidBody.velocity.magnitude <= deceleration * Time.deltaTime)
            {
                rigidBody.velocity = new Vector2(0, 0);
            } else
            {
                rigidBody.velocity -= rigidBody.velocity.normalized * deceleration * Time.deltaTime * randomSpeedFactor;
            }
        }
    }

    public void UpdateAttributeDisplay()
    {
        for(int i = 1; i < attributes.Length; i++)
        {
            attributeIcons[i].enabled = attributes[i] && isAlive;
        }
    }

    public void ApplyEffect(char effect)
    {
        didSomething = true;

        if (attributes[1]) // Has a shield
        {
            attributes[1] = false;
        }
        else
        {
            switch (effect)
            {
                case 'X':
                    willBeAlive = false;
                    break;
                case 'R':
                    xProspective += 1;
                    break;
                case 'L':
                    xProspective -= 1;
                    break;
                case 'U':
                    yProspective += 1;
                    break;
                case 'D':
                    yProspective -= 1;
                    break;
                default:
                    break;
            }
        }
    }

    private void UpdateVisibility()
    {
        foreach (SpriteRenderer sprite in transform.GetComponentsInChildren<SpriteRenderer>())
        {
            sprite.enabled = isAlive;
        }

        UpdateAttributeDisplay();
    }

    public void UpdatePosition()
    {
        targetPosition = new Vector3((x + 0.5f) * EnemyManager.instance.cellSize, (y + 0.5f) * EnemyManager.instance.cellSize);
    }

    public void InitializePosition()
    {
        UpdatePosition();
        transform.localScale = new Vector3(EnemyManager.instance.cellSize, EnemyManager.instance.cellSize);
        transform.localPosition = targetPosition;
        xProspective = x;
        yProspective = y;
    }

    public void ConfirmEffect()
    {
        StartMove();

        if (!willBonk)
        {
            if (x != xProspective || y != yProspective) // Log movement
            {
                EnemyManager.instance.movementHistory[xProspective, yProspective] = new Vector2Int(xProspective - x, yProspective - y);
                recentPositions.Add(new Vector2Int(x, y));
            }

            x = xProspective;
            y = yProspective;
            xBonk = x;
            yBonk = y;
        }
        else
        {
            xBonk = xProspective;
            yBonk = yProspective;

            xProspective = x;
            yProspective = y;
        }

        isAlive = willBeAlive;

        UpdateVisibility();
        UpdatePosition();
    }

    private void StartMove()
    {
        Vector2 moveDirection = new Vector2(xProspective, yProspective) - new Vector2(x, y);

        if (moveDirection != new Vector2(0, 0)) {
            rigidBody.velocity += moveDirection.normalized * InstantSpeed;
        }
    }

    public void CheckKnockonEffects()
    {
        if (isAlive)
        {
            if (attributes[2]) // Has Party Hat
            {
                int placesToGo = 0;
                char effect = '.';

                if (EnemyManager.instance.JustEmptied(x + 1, y) && !IsRepeatMove(x + 1, y))
                {
                    placesToGo++;
                    effect = 'R';
                }

                if (EnemyManager.instance.JustEmptied(x - 1, y) && !IsRepeatMove(x - 1, y))
                {
                    placesToGo++;
                    effect = 'L';
                }

                if (EnemyManager.instance.JustEmptied(x, y + 1) && !IsRepeatMove(x, y + 1))
                {
                    placesToGo++;
                    effect = 'U';
                }

                if (EnemyManager.instance.JustEmptied(x, y - 1) && !IsRepeatMove(x, y - 1))
                {
                    placesToGo++;
                    effect = 'D';
                }

                if (placesToGo == 1)
                {
                    ApplyEffect(effect);
                }
            }

            if (attributes[3]) // Has Asshole Cap
            {
                bool hasBonked = xBonk != x || yBonk != y;
                bool bonkedIntoPerson = EnemyManager.instance.ContainsEnemy(xBonk, yBonk);
                if (!hasBeenAss && hasBonked && bonkedIntoPerson)
                {
                    hasBeenAss = true;
                    char effect;

                    if (xBonk > x)
                    {
                        effect = 'R';
                    }
                    else if (xBonk < x)
                    {
                        effect = 'L';
                    }
                    else if (yBonk > y)
                    {
                        effect = 'U';
                    }
                    else
                    {
                        effect = 'D';
                    }

                    EnemyManager.instance.enemies[xBonk, yBonk].ApplyEffect(effect);
                }
            }

            if (attributes[4]) // Has Heart
            {
                List<EnemyBehavior> romantics = new();
                FindConnectedRomantics(romantics);

                if (romantics.Count >= 2 && !hasBeenRomanced)
                {
                    foreach (EnemyBehavior romantic in romantics)
                    {
                        romantic.hasBeenRomanced = true;
                        romantic.ApplyEffect('X');
                    }
                }
            }
        }
    }

    private void FindConnectedRomantics(List<EnemyBehavior> romantics)
    {
        if (!attributes[4] || romantics.Contains(this)) return;

        romantics.Add(this);

        foreach(EnemyBehavior enemy in EnemyManager.instance.GetAdjacentEnemies(x, y))
        {
            enemy.FindConnectedRomantics(romantics);
        }
    }

    public void ResetEffectIteration()
    {
        willBonk = false;
        didSomething = false;
    }

    public void FinishEffect()
    {
        recentPositions = new();
        hasBeenAss = false;
        hasBeenRomanced = false;

        positionHistory.Add(new Vector2Int(x, y));
        lifeHistory.Add(isAlive);
        attributeHistory.Add(new bool[attributes.Length]);
        attributes.CopyTo(attributeHistory[attributeHistory.Count - 1], 0);
    }

    private bool IsRepeatMove(int x, int y)
    {
        return recentPositions.Contains(new Vector2Int(x, y));
    }

    public void Reset()
    {
        x = positionHistory[0].x;
        y = positionHistory[0].y;
        xProspective = x;
        yProspective = y;

        isAlive = lifeHistory[0];
        willBeAlive = isAlive;

        attributeHistory[0].CopyTo(attributes, 0);

        UpdatePosition();
        UpdateVisibility();
        InitializeHistory();
    }

    public void UndoMove()
    {
        x = positionHistory[positionHistory.Count - 2].x;
        y = positionHistory[positionHistory.Count - 2].y;
        xProspective = x;
        yProspective = y;

        isAlive = lifeHistory[lifeHistory.Count - 2];
        willBeAlive = isAlive;

        attributeHistory[attributeHistory.Count - 2].CopyTo(attributes, 0);

        positionHistory.RemoveAt(positionHistory.Count - 1);
        lifeHistory.RemoveAt(lifeHistory.Count - 1);
        attributeHistory.RemoveAt(attributeHistory.Count - 1);

        UpdatePosition();
        UpdateVisibility();
    }

    public void InitializeHistory()
    {
        positionHistory = new();
        lifeHistory = new();
        attributeHistory = new();

        positionHistory.Add(new Vector2Int(x, y));
        lifeHistory.Add(isAlive);
        attributeHistory.Add(new bool[attributes.Length]);
        attributes.CopyTo(attributeHistory[0], 0);
    }
}
