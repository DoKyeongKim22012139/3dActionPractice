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


    Vector3 moveVec;

    Animator anime;


    private void Awake()
    {
        
        anime=GetComponentInChildren<Animator>();
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");

        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        transform.position += moveVec * speed*(wDown ? 0.3f :1f)*Time.deltaTime;
        

        anime.SetBool("isRun", moveVec != Vector3.zero);
        anime.SetBool("isWalk", wDown);




        transform.LookAt(transform.position+moveVec);



    }
}
