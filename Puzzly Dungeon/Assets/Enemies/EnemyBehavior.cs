using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public int x, y;
    public int xProspective, yProspective;

    public bool[] attributes;
    public SpriteRenderer[] attributeIcons;

    public bool isAlive = true;

    private List<Vector2Int> recentPositions = new();

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
                    isAlive = false;
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
        transform.localPosition = new Vector3((x + 0.5f) * EnemyManager.instance.cellSize, (y + 0.5f) * EnemyManager.instance.cellSize);
        transform.localScale = new Vector3(EnemyManager.instance.cellSize, EnemyManager.instance.cellSize);
        xProspective = x;
        yProspective = y;
    }

    public void ConfirmEffect()
    {
        if(x != xProspective || y != yProspective) // Log movement
        {
            EnemyManager.instance.movementHistory[xProspective, yProspective] = new Vector2Int(xProspective - x, yProspective - y);
            recentPositions.Add(new Vector2Int(x, y));
        }

        x = xProspective;
        y = yProspective;

        UpdatePosition();
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
    }

    private bool IsRepeatMove(int x, int y)
    {
        return recentPositions.Contains(new Vector2Int(x, y));
    }
}
