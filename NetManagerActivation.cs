using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetManagerActivation : MonoBehaviour
{
    public GameObject NetworkManagerObject;
    public GameObject BtnPanel;
    void Awake()
    {
        GameObject[] netManagerObject = GameObject.FindGameObjectsWithTag("NetworkManager");//비활성화 된 오브젝트는 찾을 수 없다. 네트워크 매니저 오브젝트는 시작할 때 비활성화 상태로 시작하기 때문에 검색된
                                                                                            //게임오브젝트는 0개이다.
        if (netManagerObject.Length > 0)
        {
            Debug.Log("네트워크 매니저가 이미 존재합니다.");
            BtnPanel.SetActive(false);
        }
        else
            NetworkManagerObject.SetActive(true);
    }
}
