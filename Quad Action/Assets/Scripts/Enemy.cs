using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public bool isChase;



    Rigidbody rigid;
    BoxCollider boxCollider;
    Material mat;
    NavMeshAgent nav;
    Animator anime;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        mat = GetComponentInChildren<MeshRenderer>().material;
        nav = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();

        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anime.SetBool("isWalk", true);
    }


    void Update()
    {
        if (target == null)
            return;

        if(isChase)
            nav.SetDestination(target.position);
    }


    void FreezeVelocity()
    {
        if(isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // angularveclciy 회전 물리속도

        }

       
    }
    
     void FixedUpdate()
    {
        FreezeVelocity();
    }



    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;

            Vector3 reactVec = transform.position - other.transform.position;
            //Debug.Log("Melee :" + curHealth);
            StartCoroutine(OnDamage(reactVec, false));
        }

        else if(other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            curHealth -= bullet.damamge;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);
            //Debug.Log("Range :" + curHealth);
            StartCoroutine (OnDamage(reactVec, false));
        }
    }

    public void HitByGrenade(Vector3 exposionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - exposionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }





    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        mat.color = Color.red;
        yield return new WaitForSeconds(0.1f);  

        if(curHealth >0)
        {
            mat.color = Color.white;
        }

        else
        {
            mat.color = Color.gray;
            gameObject.layer = 12;
            isChase = false;
            nav.enabled= false;
            anime.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up*3;
                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);

            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
            }

            Destroy(gameObject, 4);
        }
    }


}
