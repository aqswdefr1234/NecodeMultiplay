using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Http;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using NetworkEvent = Unity.Networking.Transport.NetworkEvent;
using TMPro;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RelayServerManager : MonoBehaviour //https://docs.unity.com/relay/en/manual/relay-and-ngo
{
    public TMP_InputField inputField_Nickname;
    public TMP_Text UI_Nickname;
    const int m_MaxConnections = 30;
    private string RelayJoinCode;
    public TMP_Text ShowingJoinCode;
    public TMP_InputField inputField;
    
    public GameObject joinUserObject;
    public GameObject BtnPanel;
    
    void Start()
    {
        Debug.Log("start");
        AuthenticatingAPlayer();
    }
    public async void RelayHostStart()
    {
        if(inputField_Nickname.text == "")
        {
            Debug.Log("Enter Nickname");
        }
        else
        {
            UI_Nickname.text = inputField_Nickname.text;
            BtnPanel.SetActive(false);
            await AllocateRelayServerAndGetJoinCode(m_MaxConnections);
            StartCoroutine(ConfigureTransportAndStartNgoAsHost());
        }
    }
    public void JoinCodeInputFieldActivation() //조인유저 오브젝트의 엔터버튼 이벤트
    {
        RelayJoinCode = inputField.text;
        if (RelayJoinCode != "")
        {
            RelayClinetStart();
        }
        else
        {
            ShowingJoinCode.text = "This is incorrect information.";
        }
    }
    public void JoinCodeBtn()//버튼 패널의 조인 버튼 이벤트
    {
        if (inputField_Nickname.text == "")
        {
            Debug.Log("Enter Nickname");
        }
        else
        {
            UI_Nickname.text = inputField_Nickname.text;
            joinUserObject.SetActive(true);
            BtnPanel.SetActive(false);
        }
    }
    public async void RelayClinetStart()
    {
        await JoinRelayServerFromJoinCode(RelayJoinCode);
        StartCoroutine("ConfigreTransportAndStartNgoAsConnectingPlayer");
    }

    async void AuthenticatingAPlayer()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            var playerID = AuthenticationService.Instance.PlayerId;
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[] key, string joinCode)> AllocateRelayServerAndGetJoinCode(int maxConnections, string region = null)
    {
        Allocation allocation;
        string createJoinCode;
        try
        {
            allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections, region);
        }
        catch (Exception e)
        {
            Debug.LogError($"Relay create allocation request failed {e.Message}");
            throw;
        }

        Debug.Log($"server: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"server: {allocation.AllocationId}");

        try
        {
            createJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch
        {
            Debug.LogError("Relay create join code request failed");
            throw;
        }

        var dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        return (dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.Key, createJoinCode);
    }
    IEnumerator ConfigureTransportAndStartNgoAsHost()
    {
        Debug.Log("Before while");
        Debug.Log("ConfigureTransportAndStartNgoAsHost()");
        var serverRelayUtilityTask = AllocateRelayServerAndGetJoinCode(m_MaxConnections);
        while (!serverRelayUtilityTask.IsCompleted)//접속완료되면 false, 아직 안됐으면 true (while문 조건 검사->while 문 실행   ----> 무한반복)
        {
            yield return null;
        }
        if (serverRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to start Relay Server. Server not started. Exception: " + serverRelayUtilityTask.Exception.Message);
            yield break;
        }
        Debug.Log("End while");
        var (ipv4address, port, allocationIdBytes, connectionData, key, joinCode) = serverRelayUtilityTask.Result;

        // Display the join code to the user.

        // The .GetComponent method returns a UTP NetworkDriver (or a proxy to it)
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(ipv4address, port, allocationIdBytes, key, connectionData, true);
        NetworkManager.Singleton.StartHost();
        //Instantiate(playerPrefab);
        ShowingJoinCode.text = joinCode;
        RelayJoinCode = joinCode;
        Debug.Log(joinCode);
        
        yield return null;
    }
    public static async Task<(string ipv4address, ushort port, byte[] allocationIdBytes, byte[] connectionData, byte[] hostConnectionData, byte[] key)> JoinRelayServerFromJoinCode(string joinCode)
    {
        JoinAllocation allocation;
        try
        {
            allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch
        {
            Debug.LogError("Relay join request failed");
            throw;
        }

        Debug.Log($"client connection data: {allocation.ConnectionData[0]} {allocation.ConnectionData[1]}");
        Debug.Log($"host connection data: {allocation.HostConnectionData[0]} {allocation.HostConnectionData[1]}");
        Debug.Log($"client allocation ID: {allocation.AllocationId}");

        var dtlsEndpoint = allocation.ServerEndpoints.First(e => e.ConnectionType == "dtls");
        return (dtlsEndpoint.Host, (ushort)dtlsEndpoint.Port, allocation.AllocationIdBytes, allocation.ConnectionData, allocation.HostConnectionData, allocation.Key);
    }
    IEnumerator ConfigreTransportAndStartNgoAsConnectingPlayer()
    {
        Debug.Log("ConfigreTransportAndStartNgoAsConnectingPlayer()");
        // Populate RelayJoinCode beforehand through the UI
        var clientRelayUtilityTask = JoinRelayServerFromJoinCode(RelayJoinCode);

        while (!clientRelayUtilityTask.IsCompleted)
        {
            joinUserObject.SetActive(false);
            yield return null;
        }

        if (clientRelayUtilityTask.IsFaulted)
        {
            Debug.LogError("Exception thrown when attempting to connect to Relay Server. Exception: " + clientRelayUtilityTask.Exception.Message);
            yield break;
        }

        var (ipv4address, port, allocationIdBytes, connectionData, hostConnectionData, key) = clientRelayUtilityTask.Result;

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(ipv4address, port, allocationIdBytes, key, connectionData, hostConnectionData, true);
        NetworkManager.Singleton.StartClient();
        
        yield return null;
    }
    
}
