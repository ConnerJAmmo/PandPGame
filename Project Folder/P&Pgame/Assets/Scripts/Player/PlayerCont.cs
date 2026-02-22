using bullet.fx.pack;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCont : MonoBehaviour, IStore, IDamage, IPickup, IPickupKeys, IPickupGeneric
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("---- Stats ----")]
    [Range(1,100)] [SerializeField] public int HP;
    [Range(5,10)]  [SerializeField] int speed;
    [Range(3,10)]  [SerializeField] int slopeSlideSpeed;
    [Range(2,5)]   [SerializeField] int sprintMod;
    [SerializeField] public int regenAmount;
    [SerializeField] public float regenRate;

    [Header("---- Jump ----")]
    [Range(8,20)] [SerializeField] int jumpSpeed;
    [Range(1,4)]  [SerializeField] int jumpMax;
    [Range(8,20)] [SerializeField] int wallJumpSpeed;
    [Range(1,4)]  [SerializeField] int wallJumpPush;
    [Range(1,4)]  [SerializeField] int wallJumpMax;
    [SerializeField] float wallCheckDis;
    
    [Header("---- Physics ----")]
    [Range(1,100)][SerializeField] int gravity;
    private int speedBoostTotal = 0;
    private int baseSpeed;
    private int jumpBoostTotal = 0;
    private int baseJumpMax;
    private float miningSpeedBoostTotal = 0f;
    private float baseMineRate;

    [Header("---- Resources ----")]
    [SerializeField] float mineRate;
    [Range(5,15)] [SerializeField] int mineDist;
    [Range(1,4)]  [SerializeField] int mineDamage;
    [SerializeField] public GameObject pickModel;


    [SerializeField] public int woodCount;
    [SerializeField] public int stoneCount;
    [SerializeField] public int metalCount;
    [SerializeField] public int goldCount;
    [SerializeField] public int powerCrystals;
    [Header("---- Tools ----")]
    [SerializeField] public GameObject bullet;
    [Range(5, 15)][SerializeField] int buildDist;
    [SerializeField] public Transform shootPos;
    [SerializeField] public Transform machineGunShootPos;
    [SerializeField] public Transform m1GarandShootPos;
    [SerializeField] public Transform m1918BarShootPos;
    [SerializeField] cameraContr camScript;

    [Header("Guns")]
    [SerializeField] public List<GunStats> gunList = new List<GunStats>();
    [SerializeField] public GameObject gunModel;
    [SerializeField] public float shootRate;
    [SerializeField] public int shootDist;
    [SerializeField] public int shootDamage;

    [Header("Keys")]
    [SerializeField] public List<string> keyRing = new List<string>();

    [Header("--------Audio---------")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] jumpAud;
    [Range(0, 1)] [SerializeField] float jumpAudVol;
    [SerializeField] AudioClip[] shootAud;
    [Range(0, 1)] [SerializeField] float shootAudVol;
    [SerializeField] AudioClip[] hurtAud;
    [Range(0, 1)] [SerializeField] float hurtAudVol;
    [SerializeField] AudioClip[] reloadAud;
    [Range(0, 1)] [SerializeField] float reloadAudVol;
    [SerializeField] AudioClip[] mineWoodAud;
    [Range(0, 1)] [SerializeField] float mineWoodVol;
    [SerializeField] AudioClip[] mineSteelAud;
    [Range(0, 1)] [SerializeField] float mineSteelAudVol;
    [SerializeField] AudioClip[] mined5Aud;
    [Range(0, 1)] [SerializeField] float mined5AudVol;
    [SerializeField] AudioClip[] gunSelectUpAud;
    [Range(0, 1)] [SerializeField] float gunSelectUpAudVol;
    [SerializeField] AudioClip[] gunSelectDownAud;
    [Range(0, 1)] [SerializeField] float gunSelectDownAudVol;
    [SerializeField] AudioClip[] keyGetAud;
    [Range(0, 1)][SerializeField] float keyGetAudVol;

    [Header("--------------------------")]
    public string gunName;

    int jumpCount;
    int wallJumpCount;

    RaycastHit wallJumpHit;

    bool wallJumpPosib;
    bool wasGrounded;
    bool showSTHint = true;
    bool showAOEHint = true;

    private float _groundRayDis = 1;

    public int gunListPos;
    public float shootTimer;
    float mineTimer;
    bool pickRotated;
    public float healTimer;

    private RaycastHit slopeHit; 

    public int HPOrig;

   UnityEngine.Vector3 moveDir;
   UnityEngine.Vector3 playerVel;
   UnityEngine.Vector3 slideVel;
   UnityEngine.Vector3 PlayerBodyPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //ResetSpeedBoosts();
        baseSpeed = speed;
        baseJumpMax = jumpMax;
        baseMineRate = mineRate;
        HPOrig = HP;
        gameManager.instance.SetPlayerHPOirgUI();
        updatePlayerUI();
        shootDamage = 0;
        shootRate = 0;
        shootDist = 0;
        pickRotated = false;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        sprint();
        UpdateHints();
    }

    void Movement()
    {
        wasGrounded = controller.isGrounded; //storing this at the top to prevent walljumping off the ground
        shootTimer += Time.deltaTime;
        mineTimer += Time.deltaTime;
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        goldCount = gameManager.instance.GetGold();
        jump();
        controller.Move(playerVel * Time.deltaTime);
        PlayerBodyPos = transform.position + Vector3.down;

        if (pickRotated == false && mineTimer >= mineRate)
        {
            //nothing
        }
        else if (pickRotated == true && mineTimer >= mineRate)
        {
            pickModel.transform.Rotate(0, 0, 90);
            pickRotated = false;
        }


        if (OnSteepSlope())
        {
            SteepSlopeMovement();
        }
        else if (controller.isGrounded)
        {
            slideVel = Vector3.zero;
            jumpCount = 0;
            wallJumpCount = 0;
            playerVel.x = 0;
            playerVel.z = 0;
            playerVel.y = -2f;
        }
        else
        {
            if (slideVel.magnitude > 0.1f)
            {
                controller.Move(slideVel);
                slideVel = Vector3.Lerp(slideVel, Vector3.zero, 2f * Time.deltaTime);
            }
            playerVel.y -= gravity * Time.deltaTime;
        }

        if (HP >= HPOrig)
        {
            healTimer = 0;
        }
        else if (HP < HPOrig && HP > 0)
        {
            healTimer += Time.deltaTime;
        }

        if (healTimer >= regenRate)
        {
            healTimer = 0;
            HP += regenAmount;
            if (HP > HPOrig)
            {
                HP = HPOrig;
            }
            updatePlayerUI();
        }

        if (Input.GetButton("Fire1") && gunList.Count > 0 && gunList[gunListPos].ammoCur > 0 && shootTimer >= shootRate)
        {
            shoot();
        }
        if (Input.GetButton("Fire2") && mineTimer >= mineRate)
        {
            pickModel.transform.Rotate(0, 0, -90);
            pickRotated = true;
            mine();
        }
        if (Input.GetButtonDown("z"))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, buildDist, ~ignoreLayer))
            {
                //Debug.Log("Raycast hit object: " + hit.collider.gameObject.name, hit.collider.gameObject);
                if (hit.collider.gameObject.GetComponentInParent<ITurret>() != null)
                    hit.collider.gameObject.GetComponentInParent<ITurret>().SpawnTower('z');
            } 
        }
        if (Input.GetButtonDown("x"))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, buildDist, ~ignoreLayer))
            {
                //Debug.Log("Raycast hit object: " + hit.collider.gameObject.name, hit.collider.gameObject);
                if (hit.collider.gameObject.GetComponentInParent<ITurret>() != null)
                    hit.collider.gameObject.GetComponentInParent<ITurret>().SpawnTower('x');
            }
        }
        if (Input.GetButtonDown("c"))
        {
            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, buildDist, ~ignoreLayer))
            {
                //Debug.Log("Raycast hit object: " + hit.collider.gameObject.name, hit.collider.gameObject);
                if (hit.collider.gameObject.GetComponentInParent<ITurret>() != null)
                    hit.collider.gameObject.GetComponentInParent<ITurret>().ShieldGenerator();
            }
        }
        SelectGun();
        reload();
    }

    void reload()
    {
        if (Input.GetButtonDown("Reload") && gunList.Count > 0)
        {
            aud.PlayOneShot(reloadAud[0], reloadAudVol);
            gunList[gunListPos].ammoCur = gunList[gunListPos].ammoMax;
            gameManager.instance.UpdateAmmoUI(gunList[gunListPos].ammoCur, gunList[gunListPos].ammoMax);
        }
    }

    void UpdateHints()
    {
        // Place hints for placing
        string placeHint = "";

        // how many can we place
        int stRemaining = woodCount / gameManager.instance.towerWoodCost;
        int aoeRemaining = stoneCount / gameManager.instance.towerStoneCost;

        // This will make our hints stay while we can afford them
        if (wasGrounded && stRemaining > 0 && goldCount >= gameManager.instance.towerGoldCost)
            placeHint += $"Press Z at an empty marker to place ST Turret ({stRemaining} remaining)\n";
        if (wasGrounded && aoeRemaining > 0 && goldCount >= gameManager.instance.towerGoldCost)
            placeHint += $"Press X at an empty marker to place AOE Turret ({aoeRemaining} remaining)\n";

        string mineHint = GetMineHint(); // I created separate method for minehint

        if (!string.IsNullOrEmpty(mineHint))
        {
            placeHint += mineHint + '\n';
        }

        gameManager.instance.SetBaseHint(placeHint.Trim());
    }

    string GetMineHint()
    {
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, mineDist, ~ignoreLayer))
        {
            IMaterial mat = hit.collider.GetComponent<IMaterial>();
            if (mat != null)
            {
                string type = mat.materialType();
                return $"Press E to mine {type}";
            }
        }

        return "";
    }

    void jump()
    {
        if(Input.GetButtonDown("Jump") && !wasGrounded && wallJumpCount < wallJumpMax)
        {
           WallCheck();
           if(wallJumpPosib)
            {
                playerVel.y = wallJumpSpeed;
                playerVel.x = wallJumpHit.normal.x * wallJumpPush;
                wallJumpCount++;

                float tiltDir = Vector3.Dot(wallJumpHit.normal, transform.right) > 0 ? -1f : 1f;
                camScript.SetWallJumpTilt(tiltDir);
                StartCoroutine(ResetTiltAfterDelay());
                return;
            }
        }
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
            aud.PlayOneShot(jumpAud[Random.Range(0, jumpAud.Length)], jumpAudVol);
        }
    }

    void sprint()
    {
        /*if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
            camScript.SetSprintFOV();
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
            camScript.ResetFOV();
        } This sprint is for hold to sprint */
        if(Input.GetButtonDown("Sprint"))
        {
            if(speed == baseSpeed)
            {
                speed *= sprintMod;
                camScript.SetSprintFOV();
            }
            else
            {
               speed /= sprintMod;
               camScript.ResetFOV(); 
            }
        }
    }

    private void WallCheck()
    {
        wallJumpPosib = Physics.Raycast(transform.position, transform.right, out wallJumpHit, wallCheckDis, ~ignoreLayer) ||
                        Physics.Raycast(transform.position, -transform.right, out wallJumpHit, wallCheckDis, ~ignoreLayer) ||
                        Physics.Raycast(transform.position, transform.forward, out wallJumpHit, wallCheckDis, ~ignoreLayer) ||
                        Physics.Raycast(transform.position, -transform.forward, out wallJumpHit, wallCheckDis, ~ignoreLayer);
    }

    private bool OnSteepSlope()
    {
        
        if (!controller.isGrounded) return false;
        float castDis = (controller.height / 2) + _groundRayDis;
        if(Physics.SphereCast(transform.position, controller.radius, Vector3.down, out slopeHit, castDis , ~ignoreLayer))
        {
            float slopeAngle = Vector3.Angle(slopeHit.normal, Vector3.up);
            if(slopeAngle > controller.slopeLimit)
            {
                return true;
            }
        }
        return false;
    }

    private void SteepSlopeMovement()
    {
        Vector3 SlopeDirection = Vector3.ProjectOnPlane(Vector3.down, slopeHit.normal).normalized;
        float slidespeed = (speed + slopeSlideSpeed) * Time.deltaTime;
        slideVel = SlopeDirection * slidespeed;
        controller.Move(slideVel);
        moveDir.y = -gravity * Time.deltaTime;
    }

    void shoot()
    {
        gunList[gunListPos].ammoCur--;

        shootTimer = 0;

        aud.PlayOneShot(gunList[gunListPos].shootSound[Random.Range(0, shootAud.Length)]);
       
        Instantiate(bullet, shootPos.transform.position, shootPos.transform.rotation);
        Instantiate(gunList[gunListPos].muzzleFlashEffect, shootPos.transform.position, shootPos.transform.rotation);

        gameManager.instance.UpdateAmmoUI(gunList[gunListPos].ammoCur, gunList[gunListPos].ammoMax);
    }

    void mine()
    {
        mineTimer = 0;

        

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, mineDist, ~ignoreLayer))
        {
            

            IMaterial mat = hit.collider.GetComponent <IMaterial>();
            
            if (mat != null)
            {
                bool changed = false;
                string matType = mat.materialType();
                int matAmount = mat.materialDamage(mineDamage);

                if (matType == "Wood")
                {
                    woodCount = woodCount + matAmount;
                    aud.PlayOneShot(mineWoodAud[0], mineWoodVol);
                    changed = true;
                }
                else if (matType == "Stone")
                {
                    stoneCount = stoneCount + matAmount;
                    aud.PlayOneShot(mineSteelAud[0], mineSteelAudVol);
                    changed = true;
                }
                else if (matType == "Metal")
                {
                    metalCount = metalCount + matAmount;
                    aud.PlayOneShot(mineSteelAud[0], mineSteelAudVol);
                    changed = true;
                }
                if (changed)
                {
                    gameManager.instance.updateResourcesUI();
                }
            }
        }
    }

    public int grabMaterial(int amount, string type)
    {
        int finalAmount = 0;

        if(type == "Wood")
        {
            if (woodCount >= amount)
            {
                finalAmount = finalAmount + woodCount;
                
            }
        }
        else if (type == "Stone")
        {
            if (stoneCount >= amount)
            {
                finalAmount = finalAmount + stoneCount;
              
            }
        }
        else if (type == "Metal")
        {
            if (stoneCount >= amount)
            {
                finalAmount = finalAmount + metalCount;

            }
        }

        return finalAmount;
    }

    public int displayMaterial(string type)
    {
        int total = 0;

        if (type == "Wood")
        {
            total = woodCount;
        }
        else if (type == "Stone")
        {
            total = stoneCount;
        }
        else if (type == "Metal")
        {
            total = metalCount;
        }
        return total;
    }

    public void resetGunStatsToOrig()
    {
        gunList[gunListPos].shootDist = gunList[gunListPos].shootDistOrig;
        gunList[gunListPos].shootDamage = gunList[gunListPos].shootDamageOrig;
        gunList[gunListPos].shootRate = gunList[gunListPos].shootRateOrig;
        gunList[gunListPos].damageLevel = 0;
        gunList[gunListPos].fireRateLevel = 0;
        gunList[gunListPos].DistLevel = 0;
    }

    public void takeDamage(int amount, DamageType type)
    {
        HP -= amount;
        aud.PlayOneShot(hurtAud[Random.Range(0, hurtAud.Length)],hurtAudVol);
        updatePlayerUI();
        StartCoroutine(flashDamage());

        if (HP <= 0)
        {
            gameManager.instance.youLosePlayer();
        }

    }

    public void updatePlayerUI()
    {
        if (HP > 0)
        {
            gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
            gameManager.instance.SetPlayerHPUI();
        }
        else if (HP < 0)
        {
            HP = 0;
            gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
            gameManager.instance.SetPlayerHPUI();
        }
    }

    IEnumerator flashDamage()
    {
        gameManager.instance.damageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.damageFlash.SetActive(false);
    }

    public void getGunStats(GunStats gun)
    {
        gunList.Add(gun);
        gunListPos = gunList.Count - 1;

        ChangeGun();

    }

    public void ChangeGun()
    {
        shootDamage = gunList[gunListPos].shootDamage;
        shootDist = gunList[gunListPos].shootDist;
        shootRate = gunList[gunListPos].shootRate;
        shootPos = gunList[gunListPos].shootPos;

        gunName = gunList[gunListPos].gunName;
        gameManager.instance.damageLevel = gunList[gunListPos].damageLevel;
        gameManager.instance.rangeLevel = gunList[gunListPos].DistLevel;
        gameManager.instance.fireRateLevel = gunList[gunListPos].fireRateLevel;


        gunModel.GetComponent<MeshFilter>().sharedMesh = gunList[gunListPos].gunModel.GetComponent<MeshFilter>().sharedMesh;
        gunModel.GetComponent<MeshRenderer>().sharedMaterial = gunList[gunListPos].gunModel.GetComponent<MeshRenderer>().sharedMaterial;
        gameManager.instance.UpdateAmmoUI(gunList[gunListPos].ammoCur, gunList[gunListPos].ammoMax);
        gameManager.instance.SetGunNameText();
    }

    void SelectGun()
    {
        if (Input.GetAxis("Mouse ScrollWheel") > 0 && gunListPos < gunList.Count - 1)
        {
            gunListPos++;
            aud.PlayOneShot(gunSelectUpAud[0], gunSelectUpAudVol);
            ChangeGun();
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 && gunListPos > 0)
        {
            gunListPos--;
            aud.PlayOneShot(gunSelectDownAud[0], gunSelectDownAudVol);
            ChangeGun();
        }
    }

    public void getKey(string key)
    {
        keyRing.Add(key);
        aud.PlayOneShot(keyGetAud[0], keyGetAudVol);
    }

    public void ApplySpeedBoost(int boostAmount)
    {
        speed += boostAmount;
        speedBoostTotal += boostAmount;
    }
    /*
    private void LoadSpeedBoosts()
    {
        speedBoostTotal = GameData.instance.PlayerSpeedBoost;
        speed = baseSpeed + speedBoostTotal;
    }

    public void ResetSpeedBoosts()
    {
        GameData.instance.PlayerSpeedBoost = 0;
        speed = baseSpeed;
        speedBoostTotal = 0;
    }*/

    public void ApplyJumpBoost(int jumpAmount)
    {
        jumpMax += jumpAmount;
        jumpBoostTotal += jumpAmount;
    }
    
    public void ApplyMiningSpeedBoost(float miningSpeedBoost)
    {
        mineRate -= miningSpeedBoost;
        miningSpeedBoostTotal += miningSpeedBoost;

        if (mineRate < 0.3f)
        {
            mineRate = 0.3f;
        }
    }

    public void getGeneric(string name, int amount)
    {
        if (name == "Power Crystal")
        {
            powerCrystals = powerCrystals + amount;
        }
    }

    IEnumerator ResetTiltAfterDelay()
    {
        yield return new WaitForSeconds(0.15f);
        camScript.ResetTilt();
    }
}