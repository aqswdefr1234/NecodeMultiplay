using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerListData : MonoBehaviour //호스트가아닌 클라이언트 오너 입장에서는 리스트가 처음에 비어있다 이러면안된다.
{
    public List<GameObject> shoppingPlayers = new List<GameObject>();
    public List<GameObject> room1Players = new List<GameObject>();
    public int ownerSceneNumber_;

    void Awake()
    {
        GameObject[] playerListObjects = GameObject.FindGameObjectsWithTag("PlayerListData");
        if (playerListObjects.Length > 1)
            Destroy(gameObject);
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
