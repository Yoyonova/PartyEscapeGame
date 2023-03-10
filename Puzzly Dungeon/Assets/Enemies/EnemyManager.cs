using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;

    public GameObject[] enemyPrefabs;
    public GameObject enemyPrefab;
    public TextAsset levelText;
    public int maxGridWidth, maxGridHeight, gridHeight, gridWidth, spellCount;
    public float baseCellSize, cellSize, effectDuration;

    public string currentLevel;
    public SpellManager spellManager;
    public PlayerBehavior player;

    public EnemyBehavior[,] enemies;
    public int exitX, exitY;
    public string[] spells;

    public bool isProcessing;
    private bool[,] positionHistory;
    public Vector2Int[,] movementHistory;

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

    public void UpdateEnemyPositions()
    {
        enemies = new EnemyBehavior[gridWidth, gridHeight];

        foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
        {
            if(enemy.isAlive) enemies[enemy.x, enemy.y] = enemy;
        }
    }

    public bool ContainsEnemy(int x, int y)
    {
        return (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight && enemies[x, y] != null && enemies[x, y].isAlive);
    }

    public bool IsObstructed(int x, int y)
    {
        return (ContainsEnemy(x, y) || x < 0 || x >= gridWidth || y < 0 || y >= gridHeight);
    }

    public IEnumerator ProcessSpellEffect()
    {
        isProcessing = true;

        while (isProcessing)
        {

            bool isValidBoard = false;

            while (!isValidBoard)
            {
                isValidBoard = true;

                EnemyBehavior[,] newEnemies = new EnemyBehavior[gridWidth, gridHeight];

                foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
                {
                    if (enemy.isAlive)
                    {
                        bool enemyIsOutsideX = enemy.xProspective < 0 || enemy.xProspective >= gridWidth;
                        bool enemyIsOutsideY = enemy.yProspective < 0 || enemy.yProspective >= gridHeight;
                        bool enemyIsOnPlayer = (enemy.xProspective == player.x) && (enemy.yProspective == player.y);
                        if (enemyIsOutsideX || enemyIsOutsideY || enemyIsOnPlayer)
                        {
                            enemy.xProspective = enemy.x;
                            enemy.yProspective = enemy.y;

                            isValidBoard = false;
                        }
                        else if (newEnemies[enemy.xProspective, enemy.yProspective] != null) // Collision occurred, so cancel movement
                        {
                            EnemyBehavior collidedEnemy = newEnemies[enemy.xProspective, enemy.yProspective];
                            collidedEnemy.xProspective = collidedEnemy.x;
                            collidedEnemy.yProspective = collidedEnemy.y;
                            enemy.xProspective = enemy.x;
                            enemy.yProspective = enemy.y;

                            isValidBoard = false;
                        }
                        else
                        {
                            newEnemies[enemy.xProspective, enemy.yProspective] = enemy;
                        }
                    }
                }
            }

            movementHistory = new Vector2Int[gridWidth, gridHeight];
            positionHistory = new bool[gridWidth, gridHeight];
            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    positionHistory[i, j] = (enemies[i, j] != null) && enemies[i, j].isAlive;
                }
            }

            foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
            {
                enemy.ConfirmEffect();
            }

            UpdateEnemyPositions();

            foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
            {
                enemy.CheckKnockonEffects();
            }

            isProcessing = false;

            for (int i = 0; i < gridWidth; i++)
            {
                for (int j = 0; j < gridHeight; j++)
                {
                    bool positionChanged = (positionHistory[i, j] != ((enemies[i, j] != null) && enemies[i, j].isAlive));
                    bool moved = (movementHistory[i, j] != new Vector2Int(0,0));

                    if (positionChanged || moved)
                    {
                        isProcessing = true;
                    }
                }
            }

            yield return new WaitForSeconds(effectDuration);
        }

        foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
        {
            enemy.FinishEffect();
        }

        player.TryEscape();
    }

    public bool JustEmptied(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;

        return (positionHistory[x, y] && ((enemies[x, y] == null) || !enemies[x, y].isAlive)) ;
    }
}
