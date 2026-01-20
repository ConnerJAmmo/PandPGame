using UnityEngine;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour, IDamage
{
    [Header("Stats")]
    [SerializeField] int maxHP = 200;
    int HP;

    [Header("World UI")]
    [SerializeField] GameObject healthBarPrefab; // World - space canvas prefab, not regular
    [SerializeField] Vector3 barOffset = new Vector3(0, 2.2f, 0);

    Transform barRoot;
    Image fillImage;
    Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HP = maxHP;
        cam = Camera.main;

        if (healthBarPrefab != null)
        {
            var barObject = Instantiate(healthBarPrefab, transform.position + barOffset, Quaternion.identity);

            // Just incase the fill is an image child
            fillImage = barObject.GetComponentInChildren<Image>();
            updateBar();
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!barRoot) return;

        // and this is to make sure it follows position
        barRoot.position = transform.position + barOffset;

        // make sure the healthBar always faces canera
        if (cam)
            barRoot.forward = cam.transform.forward;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP < 0) HP = 0;

        updateBar();

        if (HP <= 0)
        {
            gameManager.instance.youLose();

            Destroy(gameObject);
        }
    }

    void updateBar()
    {
        if (fillImage)
            fillImage.fillAmount = (float)HP / maxHP;
    }
}
