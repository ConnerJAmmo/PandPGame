using UnityEngine;

public class PlayerCont : MonoBehaviour
{
    [SerializeField] CharacterController controller;

    [SerializeField] LayerMask ignoreLayer;


[Range(1,10)][SerializeField] int HP;
    [Range(1,10)][SerializeField] int speed;
    [Range(2,5)][SerializeField] int sprintMod;
    [Range(8,20)][SerializeField] int jumpSpeed;
    [Range(1,4)][SerializeField] int jumpMax;
     [Range(1,100)][SerializeField] int gravity;

    int jumpCount;
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
        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);
        
        jump();
        controller.Move(playerVel * Time.deltaTime);

        if(controller.isGrounded)
        {
          jumpCount = 0;  
          playerVel = UnityEngine.Vector3.zero;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }
    }

    void jump()
    {
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
}
