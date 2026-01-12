using UnityEditor;
using UnityEngine;

public class PlayerCont : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] LayerMask ignoreLayer;
//
[Header("---- Stats ----")]
[Range(1,10)][SerializeField] int HP;
    [Range(1,10)][SerializeField] int speed;
    [Range(1,10)][SerializeField] int slopeSlideSpeed;
    [Range(2,5)][SerializeField] int sprintMod;
    [Header("---- Jump ----")]
    [Range(8,20)][SerializeField] int jumpSpeed;
    [Range(1,4)][SerializeField] int jumpMax;
    [Range(8,20)][SerializeField] int wallJumpSpeed;
    [Range(1,4)][SerializeField] int wallJumpPush;
    [Range(1,4)][SerializeField] int wallJumpMax;
     [Range(1,2)][SerializeField] float wallCheckDis;
    [Header("---- Physics ----")]
    [Range(1,100)][SerializeField] int gravity;
    [Header("---- Tools ----")]
    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform spawnPoint;
    

    int jumpCount;
    int wallJumpCount;
    RaycastHit wallJumpHit;
    bool wallJumpPosib;
    bool wasGrounded;
    private float _groundRayDis = 1;
    float shootTimer;
    private RaycastHit slopeHit; 

    int HPOrig;
   UnityEngine.Vector3 moveDir;
   UnityEngine.Vector3 playerVel;
   UnityEngine.Vector3 slideVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrig = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        sprint();
    }

     void Movement()
    {
        wasGrounded = controller.isGrounded; //storing this at the top to prevent walljumping off the ground
        shootTimer += Time.deltaTime;
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        jump();
        controller.Move(playerVel * Time.deltaTime);
      
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
        if(Input.GetButtonDown("Fire1") && shootTimer >= shootRate)
        {
            Debug.Log("shoot");
            shoot();
        }
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
        Instantiate(bullet, spawnPoint.position, transform.rotation);
    }

}
