using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscoManager : MonoBehaviour
{
    [SerializeField] private float averageLightInterval, lightIntervalVariance;
    [SerializeField] private GameObject discoLightPrefab;
    private float lightInterval, timer;

    private void Start()
    {
        SetNewLightInterval();
    }
    private void Update()
    {
        timer += Time.deltaTime;

        if(timer >= lightInterval)
        {
            float randomX = (0.5f - Random.value) * EnemyManager.instance.GetLevelWidth();
            float randomY = (0.5f - Random.value) * EnemyManager.instance.GetLevelHeight();

            Instantiate(discoLightPrefab, new Vector3(randomX, randomY) + transform.position, Quaternion.identity, this.transform);

            SetNewLightInterval();
        }
    }

    private void SetNewLightInterval()
    {
        lightInterval = averageLightInterval * (1f - lightIntervalVariance + (2 * lightIntervalVariance * Random.value));
        timer = 0f;
    }
}
