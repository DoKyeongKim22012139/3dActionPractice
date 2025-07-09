using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range};
    public Type type;
    public int damage;
    public float rate;

    public int maxAmmo; //최대 총알 개수
    public int curAmmo; //지금 총알개수


    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public Transform bulletPos;
    public GameObject bullet;
    public Transform bulletCasePos;
    public GameObject bulletCase;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴을 작동중에도 멈춘다.
            StartCoroutine("Swing");
        }

        else if (type ==Type.Range && curAmmo >0)
        {
            curAmmo--;
            StartCoroutine("Shot");
        }
      
    }
    
    IEnumerator Swing()
    {
        
        //1
        yield return new WaitForSeconds(0.1f); //0.1초 대기 //yield return null; //1프레임 대기
        meleeArea.enabled = true;
        trailEffect.enabled = true;
        //2
        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;

        //3
        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;

    }
    //use가 메인루틴 -> swing() 서브 루틴  ->다시 메인루틴으로 *여태까지한것
    //코루틴은 use()에서 swing이 같이 실행된다 -> 코루틴(협동)

    IEnumerator Shot()
    {
        //1 총알발사
        GameObject instantBullet = Instantiate(bullet, bulletPos.position, bulletPos.rotation);
        Rigidbody bulletRigid = instantBullet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPos.forward * 50;
      
        yield return null;

        //2 탄피 배출
        GameObject instantCase = Instantiate(bulletCase, bulletCasePos.position, bulletCasePos.rotation);
        Rigidbody CaseRigid = instantCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePos.forward * Random.Range(-3, -2)+ Vector3.up*Random.Range(2,3);
        CaseRigid.AddForce(caseVec, ForceMode.Impulse);
        CaseRigid.AddTorque(Vector3.up * 10,ForceMode.Impulse);

    }
}
