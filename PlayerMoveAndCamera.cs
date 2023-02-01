using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerMoveAndCamera : NetworkBehaviour 
{
    private GameObject followCamera;
    public float speedSetting;
    public float turnSpeed;
    private Rigidbody myRigid;
    private float xRotate;
    private float yRotateSize;
    private float yRotate;
    private float xRotateSize;
    public GameObject myLocalObject;
    void Start()
    {
        followCamera = GameObject.Find("FollowCamera");
        myRigid = myLocalObject.GetComponent<Rigidbody>();
        DontDestroyOnLoad(followCamera);
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (!IsOwner) return;
        FollowCamera();
        PlayerMove_keybord();
        Look();
        //이것은 플레이어 오브젝트 마다 붙어있으므로 방을 만들었을 때 카메라가 해당 플레이어를 따라 온다고 해도 , 다른 유저가 들어오면 로컬플레이어의 씬에서도 다른유저에 붙어있는 스크립트가 작동된다.
        //각 유저의 씬마다 카메라가 하나씩 있다고 해도 오너플레이어 입장에서는 2명으로 취급되기때문에!IsOwner가 필요하다.
    }
    void FollowCamera()
    {
        followCamera.transform.position = new Vector3(myLocalObject.transform.position.x, myLocalObject.transform.position.y + 0.5f, myLocalObject.transform.position.z);
    }
    void PlayerMove_keybord()
    {
        float hAxis = Input.GetAxisRaw("Horizontal");
        float vAxis = Input.GetAxisRaw("Vertical");
        Vector3 moveVec = (hAxis * myLocalObject.transform.right + vAxis * myLocalObject.transform.forward).normalized;//transform.right 는 바라보는 방향 기준 오른편                                                                                   //transform.forward는 바라보는 방향 기준 정면
        myLocalObject.transform.position = (myLocalObject.transform.position + moveVec * speedSetting * Time.deltaTime);//https://docs.unity3d.com/ScriptReference/Rigidbody.MovePosition.html
    }
    private void Look()//https://itadventure.tistory.com/390
    {
        yRotateSize = Input.GetAxis("Mouse X") * turnSpeed;
        yRotate = myLocalObject.transform.eulerAngles.y + yRotateSize;
        xRotateSize = -Input.GetAxis("Mouse Y") * turnSpeed; //플레이어가 하늘을 바라보려면 Rotation x가 음수가 되어야 하므로 Input.GetAxis("Mouse Y") 값을 음수로 바꿔준다.
        xRotate = Mathf.Clamp(xRotate + xRotateSize, -45, 80);
        followCamera.transform.eulerAngles = new Vector3(xRotate, yRotate, 0);
        myLocalObject.transform.eulerAngles = new Vector3(0, yRotate, 0); //바라보는 방향의 y값만 받아와 플레이어가 바라보는 방향과 똑같이 회전시켜준다.
    }
}
