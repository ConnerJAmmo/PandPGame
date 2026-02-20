using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Data;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    [SerializeField] private GameObject[] spawnPoints;

    public Wave[] waves;
    public int currentWaveIndex = 0;

    private bool readyToCountDown;
    private bool waveComplete;
    private enemyAI[] preloadedEnemies;
    private int spawnPointIndex = 0;
    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        readyToCountDown = true;
        for (int i = 0; i < waves.Length; i++)
        {
            waves[i].enemiesLeft = waves[i].enemies.Length;
        }
        gameManager.instance.SetWaveCountUI(waves.Length);
        StartCoroutine(PreloadWave(currentWaveIndex));
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
            waveComplete = false;
            countdown = waves[currentWaveIndex].timeToNextWave;
            StartCoroutine(SpawnWave());
        }
        
        if (!waveComplete && waves[currentWaveIndex].enemiesLeft == 0)
        {
            waveComplete = true;
            if (currentWaveIndex >= waves.Length -1)
            {
                gameManager.instance.youWin();
            }
            else
            {
            readyToCountDown = true;
            currentWaveIndex++;
            StartCoroutine(PreloadWave(currentWaveIndex));
            }
        }
    }

    private IEnumerator PreloadWave(int waveIndex)
    {
        preloadedEnemies = new enemyAI[waves[waveIndex].enemies.Length];

        for (int i = 0; i < waves[waveIndex].enemies.Length; i++)
        {
            GameObject spawnPoint = spawnPoints[spawnPointIndex % spawnPoints.Length];
            spawnPointIndex++;
            var asyncOp = InstantiateAsync(waves[waveIndex].enemies[i], spawnPoint.transform);
            yield return asyncOp;

            preloadedEnemies[i] = asyncOp.Result[0];
            preloadedEnemies[i].transform.position = spawnPoint.transform.position;
            preloadedEnemies[i].transform.rotation = spawnPoint.transform.rotation;
            preloadedEnemies[i].gameObject.SetActive(false);
        }
    }

    private IEnumerator SpawnWave()
    {
        for (int i = 0; i < waves[currentWaveIndex].enemies.Length; i++)
        {
            preloadedEnemies[i].gameObject.SetActive(true);
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
