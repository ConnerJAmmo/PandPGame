using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerCont : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] LayerMask ignoreLayer;

[Header("---- Stats ----")]
[Range(1,10)][SerializeField] int HP;
    [Range(1,10)][SerializeField] int speed;
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

    int jumpCount;
    int wallJumpCount;
    RaycastHit wallJumpHit;
    bool wallJumpPosib;
    bool wasGrounded;
    int HPOrig;
   UnityEngine.Vector3 moveDir;
   UnityEngine.Vector3 playerVel;

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

        wasGrounded = controller.isGrounded;
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
 
        jump();
        controller.Move(playerVel * Time.deltaTime);

        if(controller.isGrounded)
        {
          jumpCount = 0;  
          wallJumpCount = 0;
          playerVel.x = 0;
          playerVel.z = 0;
          playerVel.y = -2f;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
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

}
