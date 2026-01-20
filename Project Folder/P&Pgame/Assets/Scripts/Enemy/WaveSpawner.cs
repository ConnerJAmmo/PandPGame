using UnityEngine;
using System.Collections;
using System.Data;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public GameObject wavePrefab;
        public float timeBetweenSpawns = 30;
    }

    public Wave[] waves; 
    private int currentWaveIndex = 0;

    private void Start()
    {
        gameManager.instance.SetWaveCountUI(waves.Length);
        gameManager.instance.SetActiveWaveUI(currentWaveIndex + 1);
    }

    void Update()
    {
        if (!gameManager.instance.waveActive && currentWaveIndex < waves.Length)
        {
            StartCoroutine(SpawnWaveRoutine(waves[currentWaveIndex]));
        }
    }

    private IEnumerator SpawnWaveRoutine(Wave wave)
    {
        Debug.Log("Spawning Wave: " + (currentWaveIndex + 1));
        gameManager.instance.waveActive = true;
        gameManager.instance.SetActiveWaveUI(currentWaveIndex + 1);

        foreach (Transform childContainer in wave.wavePrefab.transform)
        {
            // 1. Instantiate the container
            GameObject containerInstance = Instantiate(childContainer.gameObject, transform.position + childContainer.localPosition, childContainer.rotation);

            // 2. Use a while loop to avoid skipping children when unparenting
            // This ensures every single child is moved out safely
            while (containerInstance.transform.childCount > 0)
            {
                Transform enemy = containerInstance.transform.GetChild(0);
                enemy.SetParent(null); // Move enemy to world space
            }

            // 3. Destroy the now-empty container
            Destroy(containerInstance);

            yield return new WaitForSeconds(wave.timeBetweenSpawns);
        }

        // Wait until all enemies are dead
        yield return new WaitUntil(() => gameManager.instance.enemyCount <= 0);

        gameManager.instance.enemyCountOrig = 0;
        gameManager.instance.updateEnemyCountTotal(0);

        currentWaveIndex++;
        gameManager.instance.waveActive = false;
    }
}


/*
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
            readyToCountDown = true;
            currentWaveIndex++;
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

    public void SpawnNextWave()
    {
        if (currentWaveIndex >= waveTemplates.Length) return;

        // 1. Instantiate the entire container at the spawner's location
        GameObject waveInstance = Instantiate(waveTemplates[currentWaveIndex], transform.position, Quaternion.identity);

        currentWaveIndex++;
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
*/
