using UnityEngine;

public class TurretPlacement : MonoBehaviour, ITurret
{
    [SerializeField] GameObject STTower;
    [SerializeField] GameObject AOETower;
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] mined5Aud;
    [Range(0, 1)][SerializeField] float mined5AudVol;

    public bool hasTurret = false;
    public bool hasShield = false;

    void ITurret.SpawnTower(char key)
    {
        Debug.Log("Key Registered: " + key);
        PlayerCont player = gameManager.instance.player.GetComponent<PlayerCont>();

        if (key == 'z')
        {
            if (!hasTurret && player.woodCount >= 5 && player.goldCount >= 5)
            {
                Instantiate(STTower, new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), transform.rotation);// *= Quaternion.Euler(0, -22.5f, 0));
                player.woodCount = player.woodCount - 5;
                gameManager.instance.removeGold(-5);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);

                hasTurret = true;
            }

        }
        else if (key == 'x')
        {
            if (!hasTurret && player.stoneCount >= 5 && player.goldCount >= 5)
            {
                Instantiate(AOETower, new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z), transform.rotation);// *= Quaternion.Euler(0, -22.5f, 0));
                player.stoneCount = player.stoneCount - 5;
                gameManager.instance.removeGold(-5);
                gameManager.instance.updateResourcesUI();
                //aud.PlayOneShot(mined5Aud[0], mined5AudVol);

                hasTurret = true;
            }
        }
    }

    void ITurret.ShieldGenerator()
    {
        throw new System.NotImplementedException();
    }
}
