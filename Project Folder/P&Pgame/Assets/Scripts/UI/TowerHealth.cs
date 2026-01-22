using bullet.fx.pack;
using UnityEngine;
using UnityEngine.UI;

public class TowerHealth : MonoBehaviour
{
    
    // I removed all the HP code and the IDamage implementation because some build
    // the base script and added it to the base and both script was implemnting TakeDamage
    // So this script is for UI tower health only

    [Header("World UI")]
    [SerializeField] GameObject healthBarPrefab; // World - space canvas prefab, not regular
    [SerializeField] Vector3 barOffset = new Vector3(0, 3f, 0);

    Transform barRoot;
    TowerHealthBarUI ui;
    Camera cam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.main;

        Debug.Log($"TowerHealth UI: ui null? {ui == null}, fill null? {(ui != null && ui.fillImage == null)}");

        if (healthBarPrefab != null)
        {
            var barObject = Instantiate(healthBarPrefab, transform);
            barRoot = barObject.transform;
            barRoot.localPosition = barOffset;

            // Just incase the fill is an image child
            ui = barObject.GetComponentInChildren<TowerHealthBarUI>();
            
        }
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (!barRoot || !cam) return;
        barRoot.forward = cam.transform.forward;
        
            
    }

    public void updateBar(float normalized)
    {
        if (ui != null && ui.fillImage != null)
            ui.fillImage.fillAmount = Mathf.Clamp01(normalized);
    }

   
}
