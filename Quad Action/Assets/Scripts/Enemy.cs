using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;
using UnityEngine.Assertions.Must;

public class Enemy : MonoBehaviour
{
    public enum Type { A,B,C,D};
    public Type enemyType;
    public int maxHealth;
    public int curHealth;
    public int score;

    public GameManager manager;
    public Transform target;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public GameObject[] coins;
    public GameObject bullet;
    public BoxCollider meleeArea;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator anime;
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
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

        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(target.position);
            nav.isStopped = !isChase;
        }
    }


    void FreezeVelocity()
    {
        if(isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero; // angularveclciy 회전 물리속도

        }

       
    }
    
    void Targeting()
    {
        if (!isDead &&enemyType != Type.D)
        {
            float targetRadius = 0;
            float targetRange = 0;

            switch (enemyType)
            {

                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;

                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;

                case Type.C:
                    targetRadius = 0.5f;
                    targetRange = 25f;
                    break;
            }

            RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
        
    }


    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anime.SetBool("isAttack", true);

        switch (enemyType)
        {

            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;
                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;
                break;

            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;

            case Type.C:
                yield return new WaitForSeconds(0.5f);
                GameObject instantBullet =Instantiate(bullet, transform.position,transform.rotation);
                instantBullet.transform.Rotate(0, -90, 0);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward*20;

                yield return new WaitForSeconds(2f);
                break;
        }


        

        isChase = true;
        isAttack = false;
        anime.SetBool("isAttack", false);
    }

     void FixedUpdate()
    {
        Targeting();
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
        foreach(MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;
        
        yield return new WaitForSeconds(0.1f);  

        if(curHealth >0)
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }

        else
        {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            gameObject.layer = 12;
            isDead = true;
            isChase = false;
            nav.enabled= false;
            anime.SetTrigger("doDie");
            Player player = target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    manager.enemyCntA--;
                    break;
                case Type.B:
                    manager.enemyCntB--;
                    break;
                case Type.C:
                    manager.enemyCntC--;
                    break;
                case Type.D:
                    manager.enemyCntD--;
                    break;



            }

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
