using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    float hAxis;
    float vAxis;
    bool wDown;
    bool jDown;


    bool isJump;
    bool isDodge;

    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anime;


    private void Awake()
    {
        
        anime=GetComponentInChildren<Animator>();
        rigid=GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
    }


    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
    }

     void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if(isDodge)
        {
            moveVec = dodgeVec;
        }
        transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;


        anime.SetBool("isRun", moveVec != Vector3.zero);
        anime.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);
    }


    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump &&!isDodge)
        {
            rigid.AddForce(Vector3.up*20, ForceMode.Impulse);
            anime.SetBool("isJump", true);
            anime.SetTrigger("doJump");
            isJump = true;
        }
    }

    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge)
        {
            dodgeVec = moveVec;
            speed *= 2;
           
            anime.SetTrigger("doDodge");
            isDodge = true;

            Invoke("DodgeOut", 0.4f);
        }
    }

    void DodgeOut()
    {
        speed  *= 0.5f;
        isDodge = false;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag =="Floor")
        {
            anime.SetBool("isJump", false);
            isJump = false;
        }
    }

}
