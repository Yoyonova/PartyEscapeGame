using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public GameObject[] enemyPrefabs;
    public GameObject enemyPrefab;
    public TextAsset levelText;
    public int maxGridWidth = 9;
    public int maxGridHeight = 9;
    public float baseCellSize;
    public float cellSize;

    public string currentLevel = "AAA";
    public int gridHeight = 3;
    public int gridWidth = 3;
    public int spellCount = 3;
    public SpellManager spellManager;
    public PlayerBehavior player;

    public EnemyBehavior[,] enemies;
    public int exitX, exitY;
    public string[] spells;

    void Start()
    {
        instance = this;
        LoadLevel();
    }

    private void LoadLevel()
    {
        int levelIndex = levelText.text.IndexOf(currentLevel);

        spellCount = int.Parse(levelText.text.Substring(levelIndex + 10, 2));
        spells = new string[spellCount];
        for (int i = 0; i < spellCount; i++)
        {
            int nextSpellIndex = levelIndex + 14 + i * 4;
            spells[i] = levelText.text.Substring(nextSpellIndex, 3);
        }
        spellManager.LoadSpells(spells);

        gridWidth = int.Parse(levelText.text.Substring(levelIndex + 4, 2));
        gridHeight = int.Parse(levelText.text.Substring(levelIndex + 7, 2));
        cellSize = baseCellSize / Mathf.Max(gridWidth, gridHeight);
        transform.position = new Vector3(Camera.main.transform.position.x, Camera.main.transform.position.y);
        transform.position -= new Vector3(gridWidth * cellSize / 2, gridHeight * cellSize / 2);

        enemies = new EnemyBehavior[gridWidth, gridHeight];

        for (int i = 0; i < gridWidth; i++)
        {
            for (int j = 0; j < gridHeight; j++)
            {
                int nextEnemyIndex = levelIndex + 15 + spellCount * 4 + j * (gridWidth * 2 + 2) + i * 2;
                string nextEnemyAttribute1 = levelText.text.Substring(nextEnemyIndex, 2);


                if (nextEnemyAttribute1.Contains('x'))
                {
                    exitX = i;
                    exitY = gridHeight - j - 1;
                }
                else if (nextEnemyAttribute1.Contains('p'))
                {
                    player.x = i;
                    player.y = gridHeight - j - 1;
                    player.UpdatePosition();
                }
                else if (!nextEnemyAttribute1.Contains('.'))
                {
                    SpawnEnemy(i, gridHeight - j - 1, int.Parse(nextEnemyAttribute1));
                }
            }
        }
    }

    private void SpawnEnemy(int x, int y, int attribute1)
    {
        enemies[x, y] = Instantiate(enemyPrefab).GetComponent<EnemyBehavior>();
        enemies[x, y].transform.parent = transform;
        enemies[x, y].x = x;
        enemies[x, y].y = y;
        enemies[x, y].UpdatePosition();
        enemies[x, y].attributes[attribute1] = true;
        enemies[x, y].UpdateAttributeDisplay();
    }

    public bool ContainsEnemy(int x, int y)
    {
        return (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && enemies[x, y] != null && enemies[x, y].isAlive);
    }

    public bool IsObstructed(int x, int y)
    {
        return (ContainsEnemy(x, y) || x < 0 || x >= gridWidth || y < 0 || y >= gridHeight);
    }
}
