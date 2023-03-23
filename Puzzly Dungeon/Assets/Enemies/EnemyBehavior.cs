using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float positionPrecision = 0.2f;
    [SerializeField] private float deceleration = 200f;
    [SerializeField] private float randomSpeedVariation = 0.2f;
    [SerializeField] private float bonkSpeed = 10f;
    private float randomSpeedFactor = 1f;
    private Rigidbody2D rigidBody;

    public int x, y;
    public int xProspective, yProspective;
    public bool willBonk = false;
    private Vector3 targetPosition;

    public bool[] attributes;
    public SpriteRenderer[] attributeIcons;

    public bool isAlive = true;
    private bool willBeAlive = true;

    private List<Vector2Int> recentPositions = new();

    private List<Vector2Int> positionHistory;
    private List<bool> lifeHistory;

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
        switch (effect)
        {
            case 'X':
                if (attributes[1]) // Has a shield
                {
                    attributes[1] = false;
                    UpdateAttributeDisplay();
                }
                else
                {
                    willBeAlive = false;
                    UpdateVisibility();
                }
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
        xProspective = x;
        yProspective = y;
    }

    public void InitializePosition()
    {
        UpdatePosition();
        transform.localScale = new Vector3(EnemyManager.instance.cellSize, EnemyManager.instance.cellSize);
        transform.localPosition = targetPosition;
    }

    public void ConfirmEffect()
    {
        if (!willBonk)
        {
            if (x != xProspective || y != yProspective) // Log movement
            {
                EnemyManager.instance.movementHistory[xProspective, yProspective] = new Vector2Int(xProspective - x, yProspective - y);
                recentPositions.Add(new Vector2Int(x, y));
            }

            x = xProspective;
            y = yProspective;
        } else
        {
            Bonk();
            willBonk = false;

            xProspective = x;
            yProspective = y;
        }

        if(isAlive != willBeAlive)
        {
            isAlive = willBeAlive;
            UpdateVisibility();
        }

        UpdatePosition();
    }

    private void Bonk()
    {
        Vector2 bonkDirection = new Vector2(xProspective, yProspective) - new Vector2(x, y);

        if (bonkDirection != new Vector2(0, 0)) {
            rigidBody.velocity += bonkDirection.normalized * bonkSpeed;
        }
    }

    public void CheckKnockonEffects()
    {
        if(attributes[2]) // Has Party Hat
        {
            int placesToGo = 0;

            if (EnemyManager.instance.JustEmptied(x + 1, y) && !IsRepeatMove(x + 1, y))
            {
                placesToGo++;
                xProspective++;
            }

            if (EnemyManager.instance.JustEmptied(x - 1, y) && !IsRepeatMove(x - 1, y))
            {
                placesToGo++;
                xProspective--;
            }

            if (EnemyManager.instance.JustEmptied(x, y + 1) && !IsRepeatMove(x, y + 1))
            {
                placesToGo++;
                yProspective++;
            }

            if (EnemyManager.instance.JustEmptied(x, y - 1) && !IsRepeatMove(x, y - 1))
            {
                placesToGo++;
                yProspective--;
            }

            if(placesToGo != 1)
            {
                xProspective = x; // THIS MIGHT MESS THINGS UP LATER BY RESETTING MOVEMENT!!!
                yProspective = y;
            }
        }
    }

    public void FinishEffect()
    {
        recentPositions = new();
        positionHistory.Add(new Vector2Int(x, y));
        lifeHistory.Add(isAlive);
    }

    private bool IsRepeatMove(int x, int y)
    {
        return recentPositions.Contains(new Vector2Int(x, y));
    }

    public void Reset()
    {
        x = positionHistory[0].x;
        y = positionHistory[0].y;
        isAlive = lifeHistory[0];
        willBeAlive = isAlive;

        UpdatePosition();
        UpdateVisibility();
        InitializeHistory();
    }

    public void UndoMove()
    {
        x = positionHistory[positionHistory.Count - 2].x;
        y = positionHistory[positionHistory.Count - 2].y;
        isAlive = lifeHistory[lifeHistory.Count - 2];
        willBeAlive = isAlive;

        positionHistory.RemoveAt(positionHistory.Count - 1);
        lifeHistory.RemoveAt(lifeHistory.Count - 1);

        UpdatePosition();
        UpdateVisibility();
    }

    public void InitializeHistory()
    {
        positionHistory = new();
        lifeHistory = new();

        positionHistory.Add(new Vector2Int(x, y));
        lifeHistory.Add(isAlive);
    }
}
