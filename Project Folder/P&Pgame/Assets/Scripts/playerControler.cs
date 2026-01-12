using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class playerControler : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("Stats")]
    [Range(1,10)] [SerializeField] int HP;
    [Range(1, 10)] [SerializeField] int speed;
    [Range(2, 5)] [SerializeField] int sprintMod;
    [Range(8, 20)] [SerializeField] int jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpMax;

    [Header("Physics")]
    [Range(15, 40)] [SerializeField] int gravity;

    [Header("Guns")]
    [Range(1, 1000)][SerializeField] int shootDist;
    [Range(0, 2)][SerializeField] float shootRate;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPos;

    int jumpCount;
    int HPOrigin;

    float shootTimer;

    Vector3 moveDir;
    Vector3 playerVel;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        HPOrigin = HP;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Sprint();
    }

    void Movement() 
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red);

        shootTimer += Time.deltaTime;

        moveDir = Input.GetAxis("Horizontal") * transform.right + Input.GetAxis("Vertical") * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        Jump();
        controller.Move(playerVel * Time.deltaTime);

        if (controller.isGrounded)
        {
            jumpCount = 0;
            playerVel = Vector3.zero;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        if(Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            Shoot();
        }

    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void Sprint()
    {
        if (Input.GetButtonDown("Sprint"))
        {
            speed *= sprintMod;
        }
        else if (Input.GetButtonUp("Sprint"))
        {
            speed /= sprintMod;
        }
    }

    void Shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, Camera.main.transform.rotation);
        /*
        RaycastHit hit;

        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }
        */

    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        //check if player is dead
        if(HP <= 0)
        {
            gameManager.instance.youLose();
        }
    }
}
