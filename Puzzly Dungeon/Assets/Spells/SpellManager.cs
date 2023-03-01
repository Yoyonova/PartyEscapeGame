using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SpellManager : MonoBehaviour
{
    public float SpellPositionGap = 3f;
    public float SpellPositionWidth = 5f;

    public EnemyManager enemyManager;
    public PlayerBehavior player;
    public GameObject spellPrefab;
    public GameObject selector;

    private int spellCount;
    private SpellBehavior[] spells;

    public int selectedSpell = -1;
    public int mana = 3;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            float gridX = mousePosition.x - enemyManager.transform.position.x;
            float gridY = mousePosition.y - enemyManager.transform.position.y;
            bool clickedWithinGridX = gridX > 0 && gridX < enemyManager.cellSize * enemyManager.maxGridWidth;
            bool clickedWithinGridY = gridY > 0 && gridY < enemyManager.cellSize * enemyManager.maxGridHeight;

            float spellsX = mousePosition.x - transform.position.x;
            float spellsY = -1 * (mousePosition.y - transform.position.y);
            bool clickedOnSpell = spellsX > 0 && spellsX < SpellPositionWidth && spellsY > 0 && spellsY < spellCount * SpellPositionGap;

            if (clickedWithinGridX && clickedWithinGridY)
            {
                int enemyX = Mathf.FloorToInt(gridX / enemyManager.cellSize);
                int enemyY = Mathf.FloorToInt(gridY / enemyManager.cellSize);

                bool targetExists = enemyManager.enemies[enemyX, enemyY] != null && enemyManager.enemies[enemyX, enemyY].isAlive;
                bool spellIsCastable = selectedSpell >= 0 && spells[selectedSpell].cost <= mana;

                if (targetExists && spellIsCastable)
                {
                    mana -= spells[selectedSpell].cost;
                    spells[selectedSpell].castSpell(enemyX, enemyY);
                    player.TryEscape();
                }
            } else if (clickedOnSpell)
            {
                selectedSpell = Mathf.FloorToInt(spellsY / SpellPositionGap);
                selector.transform.localPosition = new Vector3(0, -1 * selectedSpell * SpellPositionGap - 0.5f);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void LoadSpells(string[] spellIds)
    {
        spellCount = spellIds.Length;
        spells = new SpellBehavior[spellCount];

        for (int i = 0; i < spellCount; i++)
        {
            spells[i] = Instantiate(spellPrefab).GetComponent<SpellBehavior>();
            spells[i].LoadSpell(spellIds[i]);

            spells[i].transform.parent = transform;
            spells[i].transform.localPosition = new Vector3(0, i * -1 * SpellPositionGap);
        }
    }
}
