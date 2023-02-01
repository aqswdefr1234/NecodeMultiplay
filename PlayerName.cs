using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Unity.Collections;
using UnityEngine.SceneManagement;
public class PlayerName : NetworkBehaviour//https://docs-multiplayer.unity3d.com/netcode/1.0.0/basics/networkvariable/index.html
{
    [SerializeField] private TMP_Text playerName3D;
    [SerializeField] private NetworkVariable<FixedString128Bytes> networkPlayersNames = new NetworkVariable<FixedString128Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private GameObject playerColliderObject;
    public NetworkVariable<int> networkPlayersSceneNumber = new NetworkVariable<int>(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //networkVariable을 static으로 선언했더니 에디터 상에 값이 예전에 들어갔던 값이 계속 남아있다. 추후에 static을 빼주더라도 그 변수에 저장되어있던 값이 그래도 나오는 경우가 생겨버리므로 static을 사용할때는 주의!
    public int scene_Number = -1; //오너의 현재 씬 값
    public int ownerPlayerClientId = -1; //오너가 생성될때 아이디 값을 넣어줌. 오너를 제외한 클라이언트는 -1
    private GameObject forOwnerDataLoad;
    void Awake()
    {
        forOwnerDataLoad = GameObject.Find("PlayerListData");
    }
    public override void OnNetworkSpawn()//원래 있던 객체에서도 돌아감.
    {
        if (IsOwner)
        {
            if(SceneManager.GetActiveScene().name == "Shopping")
                scene_Number = 0;
            else if (SceneManager.GetActiveScene().name == "Room1")
                scene_Number = 1;
            
            networkPlayersNames.Value = GameObject.Find("Canvas").transform.Find("Visible_Nickname").GetComponent<TMP_Text>().text;
            networkPlayersSceneNumber.Value = scene_Number;
            playerName3D.text = networkPlayersNames.Value.ToString();
            gameObject.name = networkPlayersNames.Value.ToString();// 생성된 플레이어의 이름 바꿈.
            playerColliderObject.name = gameObject.name;
            ownerPlayerClientId = Convert.ToInt32(OwnerClientId);
        }
        else
        {
            StartCoroutine(WaitLoadingName());
        }
        networkPlayersSceneNumber.OnValueChanged += OnSomeValueChanged; //값 변화시 이벤트 발생
    }
    private void OnSomeValueChanged(int previous, int current) //해당오브젝트의 내부값 가져옴.오너의 값을 가져오기 위해서는 외부에 있는 데이터를 가져오는 방식으로 해아함
    {
        Debug.Log($"{gameObject.name}이전씬: {previous} | 이동한 현재 씬: {current}");
        if (!IsOwner)
        {
            if (forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ == current)
            {
                gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                gameObject.SetActive(true);
            }
            else
            {
                gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                gameObject.SetActive(false);
            }
            if(current == 0)
            {
                forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Add(gameObject);
                forOwnerDataLoad.GetComponent<PlayerListData>().room1Players.Remove(gameObject);
            }
            else if (current == 1)
            {
                forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Remove(gameObject);
                forOwnerDataLoad.GetComponent<PlayerListData>().room1Players.Add(gameObject);
            }
        }
        else
        {
            if(current == 0)
            {
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers, true);
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().room1Players, false);
            }
            else if (current == 1)
            {
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers, false);
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().room1Players, true);
            }
        }
    }
    IEnumerator WaitLoadingName()//NetworkVariable(T value = default, NetworkVariableReadPermission readPerm = NetworkVariableReadPermission.Everyone,NetworkVariableWritePermission writePerm = NetworkVariableWritePermission.Server);
    {
        while (true)
        {
            Debug.Log("WaitLoadingName");
            if (networkPlayersNames.Value == "")
                yield return new WaitForSeconds(0.2f);
            else
                break;
            
            Debug.Log(networkPlayersNames.Value);
        }
        playerName3D.text = networkPlayersNames.Value.ToString();
        gameObject.name = playerName3D.text;// 생성된 플레이어의 이름 바꿈.
        forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Add(gameObject);
    }
    void PlayerListIsVisible(List<GameObject> list,bool isTrue)
    {
        foreach (GameObject obj in list)
        {
            obj.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = isTrue;
            obj.SetActive(isTrue);
        }
    }
}
