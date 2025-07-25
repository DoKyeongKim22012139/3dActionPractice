using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Boss : Enemy
{
    public GameObject missile;
    public Transform missilePortA;
    public Transform missilePortB;

    Vector3 lookVec;
    Vector3 tauntVec;
    public bool isLook; //플레이어 바라보는 변수

    // 상속의 awake는 젤 밑의 자식만 실행된다.
    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anime = GetComponentInChildren<Animator>();

        nav.isStopped = true;
        StartCoroutine(Think());
    }


    void Update()
    {
        if(isDead)
        {
            StopAllCoroutines(); //죽으면 모든 코루틴 멈추기
            return;
        }
       if(isLook)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            lookVec = new Vector3(h, 0, v) * 5f;
            transform.LookAt(target.position + lookVec);
        }
       else
        {
            if (target == null)
                return;

            nav.SetDestination(tauntVec);
        }
    }

    IEnumerator Think()
    {
        yield return new WaitForSeconds(0.1f);

        int ranAction = Random.Range(0, 5);

        switch (ranAction)
        { 
            case 0:
            case 1:
                StartCoroutine(MisilleShot());

                break;
            case 2:
            case 3:

                StartCoroutine(RockShot());
                break;

            case 4:
                StartCoroutine(Taunt());
                break;
       
        }

    }


    IEnumerator MisilleShot()
    {
        anime.SetTrigger("doShot");
        yield return new WaitForSeconds(0.2f);
        GameObject instantMissileA = Instantiate(missile, missilePortA.position, missilePortA.rotation);
        BossMisslie bossMisslieA = instantMissileA.GetComponent<BossMisslie>();
        bossMisslieA.target= target;

        yield return new WaitForSeconds(0.3f);
        GameObject instantMissileB = Instantiate(missile, missilePortB.position, missilePortB.rotation);
        BossMisslie bossMisslieB = instantMissileB.GetComponent<BossMisslie>();
        bossMisslieB.target = target;

        yield return new WaitForSeconds(2f);
        StartCoroutine(Think());
    }

    IEnumerator RockShot()
    {
        isLook = false;
        anime.SetTrigger("doBigShot");
        Instantiate(bullet, transform.position,transform.rotation);
        yield return new WaitForSeconds(3f);
        isLook = true;
        StartCoroutine(Think());
    }

    IEnumerator Taunt()
    {
        tauntVec =target.position+ lookVec;
        isLook = false;
        nav.isStopped = false;
        boxCollider.enabled= false;
        anime.SetTrigger("doTaunt");

        yield return new WaitForSeconds(1.5f);
        meleeArea.enabled = true;
        yield return new WaitForSeconds(0.5f);
        meleeArea.enabled = false;

        yield return new WaitForSeconds(1f);
        isLook = true;
        nav.isStopped = true;
        boxCollider.enabled = true;
        StartCoroutine(Think());
    }
}
