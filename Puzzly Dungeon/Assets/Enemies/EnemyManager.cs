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
        spellManager.maxMana = int.Parse(levelText.text.Substring(levelIndex + 13, 2));
        spells = new string[spellCount];
        for (int i = 0; i < spellCount; i++)
        {
            int nextSpellIndex = levelIndex + 17 + i * 4;
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
                int nextEnemyIndex = levelIndex + 18 + spellCount * 4 + j * (gridWidth * 2 + 2) + i * 2;
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
        enemies[x, y].InitializePosition();
        enemies[x, y].attributes[attribute1] = true;
        enemies[x, y].UpdateAttributeDisplay();
        enemies[x, y].InitializeHistory();
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

    public List<EnemyBehavior> GetAdjacentEnemies(int x, int y)
    {
        List<EnemyBehavior> adjacentEnemies = new();

        if (ContainsEnemy(x + 1, y)) adjacentEnemies.Add(enemies[x + 1, y]);
        if (ContainsEnemy(x - 1, y)) adjacentEnemies.Add(enemies[x - 1, y]);
        if (ContainsEnemy(x, y + 1)) adjacentEnemies.Add(enemies[x, y + 1]);
        if (ContainsEnemy(x, y - 1)) adjacentEnemies.Add(enemies[x, y - 1]);

        return adjacentEnemies;
    }

    public IEnumerator ProcessSpellEffect()
    {
        isProcessing = true;

        while (true)
        {

            bool isValidBoard = false;

            while (!isValidBoard)
            {
                isValidBoard = true;

                EnemyBehavior[,] newEnemies = new EnemyBehavior[gridWidth, gridHeight];

                foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
                {
                    int enemyX = enemy.xProspective;
                    int enemyY = enemy.yProspective;

                    if (enemy.willBonk)
                    {
                        enemyX = enemy.x;
                        enemyY = enemy.y;
                    }

                    if (enemy.willBeAlive)
                    {
                        bool enemyIsOutsideX = enemyX < 0 || enemyX >= gridWidth;
                        bool enemyIsOutsideY = enemyY < 0 || enemyY >= gridHeight;
                        bool enemyIsOnPlayer = (enemyX == player.x) && (enemyY == player.y);
                        if (enemyIsOutsideX || enemyIsOutsideY || enemyIsOnPlayer)
                        {
                            enemy.willBonk = true;
                            isValidBoard = false;
                        } else if (newEnemies[enemyX, enemyY] != null) // Collision occurred, so cancel movement
                        {
                            EnemyBehavior collidedEnemy = newEnemies[enemyX, enemyY];

                            collidedEnemy.willBonk = true;
                            enemy.willBonk = true;
                            isValidBoard = false;
                        }
                        else
                        {
                            newEnemies[enemyX, enemyY] = enemy;
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

            bool willContinue = false;

            foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
            {
                if (enemy.didSomething) willContinue = true;

                enemy.ResetEffectIteration();
            }

            if (!willContinue) break;

            yield return new WaitForSeconds(effectDuration);
        }

        foreach (EnemyBehavior enemy in transform.GetComponentsInChildren<EnemyBehavior>())
        {
            enemy.FinishEffect();
        }

        player.TryEscape();

        isProcessing = false;
    }

    public bool JustEmptied(int x, int y)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return false;

        return (positionHistory[x, y] && ((enemies[x, y] == null) || !enemies[x, y].isAlive)) ;
    }

    public float GetLevelWidth()
    {
        return cellSize * gridWidth;
    }

    public float GetLevelHeight()
    {
        return cellSize * gridHeight;
    }
}
