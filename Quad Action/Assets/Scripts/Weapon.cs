using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range};
    public Type type;
    public int damage;
    public float rate;
    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing"); // 코루틴을 작동중에도 멈춘다.
            StartCoroutine("Swing");
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
}
