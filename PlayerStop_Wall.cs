using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStop_Wall : MonoBehaviour//벽과 부딪치면 멈추도록
{
    private int trigger = 0;
    void OnCollisionEnter(Collision other)
    {
        if(trigger == 0)
        {
            if (other.collider.CompareTag("Player")) //트리거에서는 other.tga == 로 간단하게 태그 사용가능
            {
                other.transform.parent.GetComponent<PlayerMoveAndCamera>().speedSetting = 1f;
                trigger = 1;
            }
        }
    }
    void OnCollisionExit(Collision other)
    {
        Debug.Log("콜리전exit 작동중");
        if (trigger == 1)
        {
            if (other.collider.CompareTag("Player"))
            {
                other.transform.parent.GetComponent<PlayerMoveAndCamera>().speedSetting = 10f;
                trigger = 0;
            }
        }
    }
}
