using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public int x, y;
    public int xProspective, yProspective;

    public bool[] attributes;
    public Sprite[] attributeSprites;
    private List<GameObject> attributeObjects = new List<GameObject>();

    public bool isAlive = true;

    public void UpdateAttributeDisplay()
    {
        foreach(GameObject sprite in attributeObjects)
        {
            Destroy(sprite);
        }

        attributeObjects = new List<GameObject>();

        for (int i = 1; i < attributes.Length; i++)
        {
            if (attributes[i]) {
                SpriteRenderer newIcon = new GameObject().AddComponent<SpriteRenderer>();
                newIcon.transform.parent = transform;
                newIcon.transform.localPosition = new Vector3(0, 0, -1);
                newIcon.sprite = attributeSprites[i];
                newIcon.transform.localScale = new Vector3(.2f,.2f);
                newIcon.sortingOrder = 10;
                attributeObjects.Add(newIcon.gameObject);
            }
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
        x = xProspective;
        y = yProspective;

        UpdatePosition();
    }
}
