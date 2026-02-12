using UnityEngine;
using System.Collections;
using System.Data;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    [SerializeField] private GameObject spawnPoint;

    public Wave[] waves;
    [SerializeField] private GameObject[] waveTemplates;
    public int currentWaveIndex = 0;

    private bool readyToCountDown;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToCountDown = true;
        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].enemiesLeft = waves[i].enemies.Length;
        }
        gameManager.instance.SetWaveCountUI(waves.Length);
    }

    // Update is called once per frame
    void Update()
    {
        if (readyToCountDown == true)
        {
            countdown -= Time.deltaTime;
        }

        if (countdown <= 0)
        {
            readyToCountDown = false;
            countdown = waves[currentWaveIndex].timeToNextWave;
            StartCoroutine(SpawnWave());
        }
        
        if (waves[currentWaveIndex].enemiesLeft == 0)
        {
            if (currentWaveIndex >= waves.Length -1)
            {
                gameManager.instance.youWin();
            }
            else
            {
            readyToCountDown = true;
            currentWaveIndex++;
            }
        }
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < waves[currentWaveIndex].enemies.Length; i++)
        {
            enemyAI Enemy = Instantiate(waves[currentWaveIndex].enemies[i], spawnPoint.transform);

            Enemy.transform.SetParent(spawnPoint.transform);

            yield return new WaitForSeconds(waves[currentWaveIndex].timeToNextEnemy);
        }
    }
}

[System.Serializable]
public class Wave
{
    public enemyAI[] enemies;
    public float timeToNextEnemy;
    public float timeToNextWave;

    [HideInInspector] public int enemiesLeft;
}
