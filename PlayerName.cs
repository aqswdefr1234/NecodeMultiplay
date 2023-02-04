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
            {
                forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ = 0;
            }
            else if (SceneManager.GetActiveScene().name == "Room1")
            {
                forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ = 1;
            }
                
            networkPlayersNames.Value = GameObject.Find("NetManager").GetComponent<RelayServerManager>().onNetworkSpawn_PlayerName;
            networkPlayersSceneNumber.Value = forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_;
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
        if (!IsOwner)//게임 시작시 코루틴에서 같은 씬에 없는 플레이어 비활성화 및 리스트 추가를 진행하므로 previous != -1 조건을 추가한다
        {           //그렇게 해야 유저 입장시 유저 정보 가저오기도 전에 비활성화 시키는걸 막을 수 있다.
            if (previous != -1 && forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ == current)
            {
                gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
                gameObject.SetActive(true);
            }
            else if(previous != -1 && forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ != current)
            {
                gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                gameObject.SetActive(false);
            }
            if(previous != -1 && current == 0) //새로들어온 플레이어가 WaitLoadingName 코루틴에서 리스트에 추가되고, 여기서도 추가되면 안되므로 previous != -1 조건 넣어줌
            {
                Debug.Log("current : " + current);
                forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Add(gameObject);
                forOwnerDataLoad.GetComponent<PlayerListData>().room1Players.Remove(gameObject);
            }
            else if (previous != -1 && current == 1)
            {
                Debug.Log("current : " + current);
                forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Remove(gameObject);
                forOwnerDataLoad.GetComponent<PlayerListData>().room1Players.Add(gameObject);
            }
        }
        else
        {
            if(previous != -1 && current == 0)
            {
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers, true);
                PlayerListIsVisible(forOwnerDataLoad.GetComponent<PlayerListData>().room1Players, false);
            }
            else if (previous != -1 && current == 1)
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
            if (forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ == -1 || networkPlayersNames.Value == "" || networkPlayersSceneNumber.Value == -1)
                yield return new WaitForSeconds(0.2f);
            else
            {
                playerName3D.text = networkPlayersNames.Value.ToString();
                gameObject.name = playerName3D.text;// 생성된 플레이어의 이름 바꿈. 실제로 바뀌는데 시간이 걸린다. 그래서 오브젝트를 비활성화 시킬때 오브젝트 이름이 바뀌었는지 확인 후 비활성화 시켜야한다.
                if (networkPlayersSceneNumber.Value == 0)
                    forOwnerDataLoad.GetComponent<PlayerListData>().shoppingPlayers.Add(gameObject);
                else if (networkPlayersSceneNumber.Value == 1)
                    forOwnerDataLoad.GetComponent<PlayerListData>().room1Players.Add(gameObject);
                if (forOwnerDataLoad.GetComponent<PlayerListData>().ownerSceneNumber_ != networkPlayersSceneNumber.Value)//오너의 현재씬과 기존플레이어와의 위치가 같다면 활성화 아니면 비활성화
                {//OnSomeValueChanged 에서는 포탈을 통해 씬을 이동할 때(+새로운 유저가 들어올 때)만 작동되기때문에 기존에 있었던 플레이어들을 로딩할 때 활성화/비활성화 작업이 필요하다.
                    Debug.Log(gameObject.name + " : false");
                    gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
                    gameObject.SetActive(false);
                }
                break;
            }
        }
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
