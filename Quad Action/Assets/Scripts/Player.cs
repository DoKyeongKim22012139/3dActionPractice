using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    public float speed;
    public GameObject[] weapons;
    public bool[] hasWeapons;
    public GameObject[] grenades;
    public int hasGrenade;
    public GameObject grenadeObj;

    public GameManager gameManager;
    public Camera followCamera; //마우스로 카메라 회전

    public AudioSource jumpSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;

    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxHasGrenade;

    float hAxis;
    float vAxis;


    bool wDown;
    bool jDown;
    bool gDown;
    bool iDown;
    bool fDown;
    bool rDown;
    bool sDown1;
    bool sDown2;
    bool sDown3;

    bool isShop;
    bool isSwap;
    bool isJump;
    bool isDodge;
    bool isReload;
    bool isFireReady =true;
    bool isBorder;
    bool isDamaged;
    bool isDead;
    Rigidbody rigid;
    Vector3 moveVec;
    Vector3 dodgeVec;

    Animator anime;
    GameObject nearObject;
    public Weapon equipWeapon;
    MeshRenderer[] meshs;

    float fireDelay;
    int equipWeaponIndex = -1;

    private void Awake()
    {
        
        anime=GetComponentInChildren<Animator>();
        rigid=GetComponent<Rigidbody>();
        meshs=GetComponentsInChildren<MeshRenderer>();

        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
        
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
        Grenade();
        Attack();
        Reload();
        Dodge();
        Interaction();
        Swap();
    }


    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        wDown = Input.GetButton("Walk");
        jDown = Input.GetButtonDown("Jump");
        fDown = Input.GetButton("Fire1");
        gDown = Input.GetButtonDown("Fire2");
        rDown = Input.GetButtonDown("Reload");
        iDown = Input.GetButtonDown("Interaction");
        sDown1 = Input.GetButtonDown("Swap1");
        sDown2 = Input.GetButtonDown("Swap2");
        sDown3 = Input.GetButtonDown("Swap3");
    }

     void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        

        if(isDodge)
        {
            moveVec = dodgeVec;
        }

        if (isSwap || !isFireReady ||isReload ||isDead)
        {
            moveVec = Vector3.zero;
        }

        if(!isBorder)
             transform.position += moveVec * speed * (wDown ? 0.3f : 1f) * Time.deltaTime;


        anime.SetBool("isRun", moveVec != Vector3.zero);
        anime.SetBool("isWalk", wDown);
    }

    void Turn()
    {
        //키보드에 의한 회전
        transform.LookAt(transform.position + moveVec);

        //마우스에 의한 회전
        if(fDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); //마우스가 가리키는 방향으로 ray발사
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) //out return처럼 반환값을 주어진 변수에 저장하는 키워드 ray가 읽은거를 rayhit에 저장
            {
                Vector3 nextVec = rayHit.point - transform.position; //rayhit포인트는 충돌지점의 월드 좌표
                nextVec.y = 0; 
                transform.LookAt(transform.position + nextVec);
            }
        }
       
    }


    void Jump()
    {
        if(jDown && moveVec == Vector3.zero && !isJump &&!isDodge &&!isSwap && !isShop &&!isDead)
        {
            rigid.AddForce(Vector3.up*20, ForceMode.Impulse);
            anime.SetBool("isJump", true);
            anime.SetTrigger("doJump");
            isJump = true;

            jumpSound.Play();
        }
    }

    void Grenade()
    {
        if (hasGrenade == 0)
            return;

        if(gDown && !isReload &&!isSwap && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition); //마우스가 가리키는 방향으로 ray발사
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100)) //out return처럼 반환값을 주어진 변수에 저장하는 키워드 ray가 읽은거를 rayhit에 저장
            {
                Vector3 nextVec = rayHit.point - transform.position; //rayhit포인트는 충돌지점의 월드 좌표
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObj, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back *10, ForceMode.Impulse);
                hasGrenade--;

                grenades[hasGrenade].SetActive(false);
            }
        }
    }

    void Attack()
    {
        if (equipWeapon == null) return;

        fireDelay += Time.deltaTime;
        isFireReady = equipWeapon.rate < fireDelay;

        if(fDown && isFireReady &&!isDodge &&!isSwap &&!isShop && !isDead)
        {
            equipWeapon.Use();
            anime.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            fireDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null) return; //손에든게 없으면 리턴

        if (equipWeapon.type == Weapon.Type.Melee) return; //근접무기면 리턴

        if (ammo == 0) return; // 가진 총알이 없으면 리턴

        if(rDown &&!isJump &&!isDodge && !isSwap && isFireReady && !isShop && !isDead)
        {
            anime.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 2.5f);
        }
        
    }

    void ReloadOut()
    {
        int needAmmo = equipWeapon.maxAmmo - equipWeapon.curAmmo; //필요한 총알 개수 
        int reAmmo = ammo < needAmmo ? ammo : needAmmo; //가진 총알이 총의 최대 장전 총알보다 작으면 가진 총알을 넣고 많으면 최대치 넣기
        equipWeapon.curAmmo +=reAmmo; //그리고 현재총알을 채워주자
        ammo -= reAmmo; //그리고 가진총알 빼자
        isReload = false;
    }
    void Dodge()
    {
        if (jDown && moveVec != Vector3.zero && !isJump && !isDodge &&!isSwap && !isShop && !isDead)
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

    void Swap()
    {

        if (sDown1 && (!hasWeapons[0] || equipWeaponIndex == 0))
            return;
        if (sDown2 && (!hasWeapons[1] || equipWeaponIndex == 1))
            return;
        if (sDown3 && (!hasWeapons[2] || equipWeaponIndex == 2))
            return;

        int weaponIndex = -1;

        if (sDown1) { weaponIndex = 0; }
        if (sDown2) weaponIndex = 1;
        if (sDown3) weaponIndex = 2;


        if ( (sDown1 || sDown2 || sDown3) && !isJump && !isDodge && !isShop && !isDead)
        {
            
            if (equipWeapon != null)
                equipWeapon.gameObject.SetActive(false);


            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            anime.SetTrigger("doSwap");
            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }


    void SwapOut()
    {
        isSwap = false;
    }

    void Interaction()
    {
        if(iDown && nearObject!=null && !isJump && !isDodge && !isDead)
        {
            if(nearObject.tag=="Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject );
            }



            else if (nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
                
            }


        }
    }


    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero; // angularveclciy 회전 물리속도
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green); //레이어 발사
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }


    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag =="Floor")
        {
            anime.SetBool("isJump", false);
            isJump = false;
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();

            switch (item.type)
            {
                case Item.Type.Ammo:

                    ammo += item.value;
                    if(ammo>maxAmmo)
                        ammo = maxAmmo;

                    break;
                case Item.Type.Coin:

                    coin += item.value;
                    if(coin >maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    grenades[hasGrenade].SetActive(true);
                    hasGrenade += item.value;
                    if(hasGrenade > maxHasGrenade)
                        hasGrenade = maxHasGrenade;
                    break;
                   
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet")
        {
            
            if (!isDamaged)
            {
                
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damamge;
               
                bool isBossAtk = other.name == "Boss Melee Area";
                StartCoroutine(OnDamage(isBossAtk));
            }

            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAtk)
    {
        isDamaged = true;

        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }

        if (isBossAtk)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);
        yield return new WaitForSeconds(1f);
        isDamaged = false;
        foreach (MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        if (isBossAtk)
            rigid.velocity= Vector3.zero;

        if (health <= 0 && !isDead)
            OnDie();
    }


    void OnDie()
    {
        anime.SetTrigger("doDie");
        isDead = true;
        gameManager.GameOver();
    }

    private void OnTriggerStay(Collider other)
    {
        if(other.tag=="Weapon" || other.tag== "Shop")
        {
            nearObject = other.gameObject;
            //Debug.Log(nearObject.name);
        }

       
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag=="Weapon")
        {
            nearObject = null;
        }
        else if (other.tag == "Shop")
        {
            Shop shop = other.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
            
        }
    }
}
