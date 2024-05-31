using System;
using Unity.Netcode;
using UnityEngine;

public class ServerManager : MonoBehaviour
{
    private void Start() => TryStartServer();

    private void TryStartServer()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager is not null && !manager.IsServer)
        {
            manager.StartServer();
        }
    }
}