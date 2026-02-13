using UnityEngine;

public class TurretPlacement : MonoBehaviour, ITurret
{
    [SerializeField] GameObject[] STTowers;
    [SerializeField] GameObject[] AOETowers;
    [SerializeField] GameObject forceField;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] mined5Aud;
    [Range(0, 1)][SerializeField] float mined5AudVol;

    [Range(0, 3)] public int turretLevel;
    public bool hasShield = false;
    private int shieldHP;

    void ITurret.SpawnTower(char key)
    {
        Debug.Log("Key Registered: " + key);
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();

        if (key == 'z')
        {
            if (turretLevel == 0 && player.woodCount >= 5 && player.goldCount >= 5)
            {
                Instantiate(STTowers[turretLevel++], 
                    new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                    transform.rotation, transform);
                player.woodCount = player.woodCount - 5;
                gameManager.instance.removeGold(-5);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            }
            else if (turretLevel != 3 && player.woodCount >= 5 && player.goldCount >= 10)
            {
                Debug.Log("Upgrade Attempt");
                transform.GetComponent<ITurret>().UpgradeTower(key);
            }
        }
        else if (key == 'x')
        {
            if (turretLevel == 0 && player.stoneCount >= 5 && player.goldCount >= 5)
            {
                Instantiate(AOETowers[turretLevel++], 
                    new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                    transform.rotation, transform);
                player.stoneCount = player.stoneCount - 5;
                gameManager.instance.removeGold(-5);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            }
            else if (turretLevel != 3 && player.stoneCount >= 5 && player.goldCount >= 10)
            {
                Debug.Log("Upgrade Attempt");
                transform.GetComponent<ITurret>().UpgradeTower(key);
            }
        }
    }

    void ITurret.UpgradeTower(char key)
    {
        Debug.Log("Upgrade Registered");
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();

        if (key == 'z')
        {
            Debug.Log("Upgrading Gun");
            if (hasShield)
            {
                shieldHP = transform.GetComponentInChildren<ForceField>().HP;
                Debug.Log("Recording Shield Health: " + shieldHP);
            }
            Destroy(transform.GetChild(1).gameObject);
            Instantiate(STTowers[turretLevel++], 
                new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                transform.rotation, transform);
            if (hasShield)
            {
                Debug.Log("Adding Shield");
                Transform detectRadius = transform.GetChild(1).GetChild(0);
                Instantiate(forceField,
                    new Vector3(detectRadius.position.x, detectRadius.position.y, detectRadius.position.z),
                    detectRadius.rotation, detectRadius);
                Debug.Log("Shield Health Restored to: " + shieldHP);
                detectRadius.GetChild(0).GetComponent<ForceField>().HP = shieldHP;
            }
            player.woodCount = player.woodCount - 5;
            gameManager.instance.removeGold(-10);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
        }
        else if (key == 'x')
        {
            Debug.Log("Upgrading Rocket");
            if (hasShield)
            {
                shieldHP = transform.GetComponentInChildren<ForceField>().HP;
                Debug.Log("Recording Shield Health: " + shieldHP);
            }
            Destroy(transform.GetChild(1).gameObject);
            Instantiate(AOETowers[turretLevel++], 
                new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z),
                transform.rotation, transform);
            if (hasShield)
            {
                Transform detectRadius = transform.GetChild(1).GetChild(0);
                Instantiate(forceField,
                    new Vector3(detectRadius.position.x, detectRadius.position.y, detectRadius.position.z),
                    detectRadius.rotation, detectRadius);
                detectRadius.GetChild(0).GetComponent<ForceField>().HP = shieldHP;
            }
            player.stoneCount = player.stoneCount - 5;
            gameManager.instance.removeGold(-10);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
        }
    }

    void ITurret.ShieldGenerator()
    {
        Debug.Log("Key Registered: c");
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();
        Transform detectRadius = transform.GetChild(1).GetChild(0);

        if (turretLevel > 0 && player.goldCount >= 5 && !hasShield)
        {
            Instantiate(forceField,
                new Vector3(detectRadius.position.x, detectRadius.position.y, detectRadius.position.z),
                detectRadius.rotation, detectRadius);
            gameManager.instance.removeGold(-5);
            gameManager.instance.updateResourcesUI();
            //aud.PlayOneShot(mined5Aud[0], mined5AudVol);
            hasShield = true;
        }
        else if (turretLevel > 0 && player.goldCount >= 5 && hasShield && 
            transform.GetComponentInChildren<ForceField>().maxHP != transform.GetComponentInChildren<ForceField>().HP)
        {
            transform.GetChild(1).GetChild(0).GetComponent<ForceField>().HP = transform.GetChild(1).GetChild(0).GetComponent<ForceField>().maxHP;
        }
    }
}
