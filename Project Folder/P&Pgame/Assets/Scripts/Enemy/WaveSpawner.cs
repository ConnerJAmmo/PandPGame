using UnityEngine;
using System.Collections;
using System.Data;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private float countdown;
    [SerializeField] private GameObject spawnPoint;

    public Wave[] waves;
    [SerializeField] private GameObject[] waveTemplates;
    public int currentWaveIndex = 0;

    private bool readyToCountDown;
    private bool waveComplete;
    

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
            waveComplete = false;
            countdown = waves[currentWaveIndex].timeToNextWave;
            StartCoroutine(SpawnWave());
        }
        
        if (!waveComplete && waves[currentWaveIndex].enemiesLeft == 0)
        {
            waveComplete = true;
            if (currentWaveIndex >= waves.Length -1)
            {
                //gameManager.instance.youWin();
                LevelComplete();
            }
            else
            {
            readyToCountDown = true;
            currentWaveIndex++;
            }
        }
    }

    void LevelComplete()
    {
        int current = SceneManager.GetActiveScene().buildIndex;

        int next;

        if (current == 3) //Outpost
        {
            next = 4;     //Gorge
        }
        else if (current == 4)//Gorge
            next = 5;     //Mothership
        else
            next = 1;     // Back to mainmenu

        GameSession.instance.CompleteLevelAndLoadNext(next);
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
