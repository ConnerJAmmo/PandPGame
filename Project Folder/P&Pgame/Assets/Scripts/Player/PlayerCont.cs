
using UnityEngine;
using System.Collections;
//using NUnit.Framework;

public class PlayerCont : MonoBehaviour, IStore, IDamage
{
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;
    
    [Header("---- Stats ----")]
    [Range(1,100)] [SerializeField] public int HP;
    [Range(1,10)]  [SerializeField] int speed;
    [Range(1,10)]  [SerializeField] int slopeSlideSpeed;
    [Range(2,5)]   [SerializeField] int sprintMod;
    
    [Header("---- Jump ----")]
    [Range(8,20)] [SerializeField] int jumpSpeed;
    [Range(1,4)]  [SerializeField] int jumpMax;
    [Range(8,20)] [SerializeField] int wallJumpSpeed;
    [Range(1,4)]  [SerializeField] int wallJumpPush;
    [Range(1,4)]  [SerializeField] int wallJumpMax;
    [Range(1,2)]  [SerializeField] float wallCheckDis;
    
    [Header("---- Physics ----")]
    [Range(1,100)][SerializeField] int gravity;
    
    [Header("---- Resources ----")]
    [Range(1,4)]  [SerializeField] float mineRate;
    [Range(5,15)] [SerializeField] int mineDist;
    [Range(1,4)]  [SerializeField] int mineDamage;

    [SerializeField] public int woodCount;
    [SerializeField] public int stoneCount;
    [SerializeField] public int goldCount;
    [Header("---- Tools ----")]
    [SerializeField] GameObject bullet;
    [SerializeField] GameObject STTower;
    [SerializeField] GameObject AOETower;
    [SerializeField] Transform shootPos;
    [SerializeField] float shootRate;

    [SerializeField] int shootDist;
    [SerializeField] int shootDamage;

    int jumpCount;
    int wallJumpCount;

    RaycastHit wallJumpHit;

    bool wallJumpPosib;
    bool wasGrounded;
    bool showSTHint = true;
    bool showAOEHint = true;

    private float _groundRayDis = 1;

    float shootTimer;
    float mineTimer;

    private RaycastHit slopeHit; 

    public int HPOrig;

   UnityEngine.Vector3 moveDir;
   UnityEngine.Vector3 playerVel;
   UnityEngine.Vector3 slideVel;
   UnityEngine.Vector3 PlayerBodyPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
        gameManager.instance.SetHPOirgUI();
        updatePlayerUI();
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
      
        if(OnSteepSlope())
        {
            SteepSlopeMovement();
        }
        else if(controller.isGrounded)
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
            if(slideVel.magnitude > 0.1f)
            {
                controller.Move(slideVel);
                slideVel = Vector3.Lerp(slideVel, Vector3.zero, 2f * Time.deltaTime);
            }
            playerVel.y -= gravity * Time.deltaTime;
        }

        if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
        if (Input.GetButton("Fire2") && mineTimer >= mineRate)
        {
            mine();
        }
        if(Input.GetButtonDown("z"))
        {
            if(wasGrounded)
            {
                SpawnSTTower();
            }
        }
        if(Input.GetButtonDown("x"))
        {
            if(wasGrounded)
            {
                SpawnAOETower();
            }
        }
    }

    void UpdateHints()
    {
        // Place hints for placing
        string placeHint = "";

        // how many can we place
        int stRemaining = woodCount / 5;
        int aoeRemaining = stoneCount / 5;

        // This will make our hints stay while we can afford them
        if (wasGrounded && stRemaining > 0)
            placeHint += $"Press Z to place ST Turret ({stRemaining} remaining)\n";
        if (wasGrounded && aoeRemaining > 0)
            placeHint += $"Press X to place AOE Turret ({aoeRemaining} remaining)\n";

        string mineHint = GetMineHint(); // I created separate method for minehint

        if (!string.IsNullOrEmpty(mineHint))
        {
            placeHint += mineHint + '\n';
        }

        gameManager.instance.SetHint(placeHint.Trim());
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
                return;
            }
        }
        if(Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void sprint()
    {
        if(Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    private void WallCheck()
    {
        wallJumpPosib = Physics.Raycast(transform.position, transform.right, out wallJumpHit, wallCheckDis) ||
        Physics.Raycast(transform.position, -transform.right, out wallJumpHit, wallCheckDis);
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
        shootTimer = 0;

        Instantiate(bullet, shootPos.position, shootPos.rotation);
        
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
                    changed = true;
                }
                else if (matType == "Stone")
                {
                    stoneCount = stoneCount + matAmount;
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
            return total;
    }

    void SpawnSTTower()
    {
        
        if(woodCount >= 5 && goldCount >= 5)
        {
            Instantiate(STTower, PlayerBodyPos, transform.rotation);
            woodCount = woodCount - 5;
            gameManager.instance.UpdateGold(-5);
            gameManager.instance.updateResourcesUI();

            showSTHint = false; // Hides Z key display after use
        }
        else return;
    }
    void SpawnAOETower()
    {
        if(stoneCount >= 5 && goldCount >= 5)
        {
            Instantiate(AOETower, PlayerBodyPos, transform.rotation);
            gameManager.instance.UpdateGold(-5);
            stoneCount = stoneCount - 5;
            gameManager.instance.updateResourcesUI();

            showSTHint = false; // Hides X key display after use

        }
        else return;
    }

    public void takeDamage(int amount)
    {
        //HP -= amount;
        updatePlayerUI();
        StartCoroutine(flashDamage());

        if (HP <= 0)
        {
            gameManager.instance.youLose();
        }

    }

    public void updatePlayerUI()
    {
        gameManager.instance.playerHPBar.fillAmount = (float)HP / HPOrig;
        gameManager.instance.SetHPUI();
    }

    IEnumerator flashDamage()
    {
        gameManager.instance.damageFlash.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gameManager.instance.damageFlash.SetActive(false);
    }
    
}