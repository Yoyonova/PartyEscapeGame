using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class SpellBehavior : MonoBehaviour
{
    public Vector3 iconOffset;
    public float iconSize = .2f;
    public float iconGap = .3f;
    public Sprite[] iconSprites = new Sprite[26];
    public int maxSpellSize = 5;
    public TextAsset spellText;

    public int cost;

    private char[,] effects;
    private SpriteRenderer[] icons;

    public void LoadSpell(string spellId)
    {
        int spellIndex = spellText.text.IndexOf(spellId);
        effects = new char[maxSpellSize, maxSpellSize];

        cost = int.Parse(spellText.text.Substring(spellIndex + 4, 2));

        for (int i = 0; i < maxSpellSize; i++)
        {
            for (int j = 0; j < maxSpellSize; j++)
            {
                int nextEffectIndex = spellIndex + 8 + j * (maxSpellSize + 2) + i;
                effects[i,j] = spellText.text[nextEffectIndex];

                if(effects[i,j] != '.')
                {
                    SpriteRenderer newIcon = new GameObject().AddComponent<SpriteRenderer>();
                    newIcon.transform.parent = transform;
                    newIcon.transform.localPosition = iconOffset + new Vector3(i * iconGap, -1 * j * iconGap);
                    int spriteIndex = effects[i,j] - 'A';
                    newIcon.sprite = iconSprites[spriteIndex];
                    newIcon.transform.localScale = new Vector3(iconSize, iconSize);
                }
            }
        }
    }

    public void castSpell(int x, int y)
    {
        int spellRadius = maxSpellSize / 2;
        EnemyManager enemyManager = EnemyManager.instance;
        EnemyBehavior[,] enemies = enemyManager.enemies;

        for (int i = Math.Max(0, x - spellRadius); i < Math.Min(enemyManager.gridWidth, x + spellRadius); i++)
        {
            for (int j = Math.Max(0, y - spellRadius); j < Math.Min(enemyManager.gridHeight, y + spellRadius); j++)
            {
                if (enemies[i, j] != null)
                {
                    enemies[i, j].ApplyEffect(effects[i - x + spellRadius, y - j + spellRadius]);
                }
            }
        }

        StartCoroutine(enemyManager.ProcessSpellEffect());
    }
}
