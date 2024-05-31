using System;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;


public delegate void OnLobbyChange(MultiplayerLobby lobby);

public partial class LobbiesManager : NetworkBehaviour
{
    private static LobbiesManager instance;
    private static ulong LocalClientID => NetworkManager.Singleton.LocalClientId;
    public static bool IsConnectedClient => NetworkManager.Singleton != null && NetworkManager.Singleton.IsConnectedClient;

    /// <summary>
    /// Returns true if there is a local lobby while not being connected anymore. That lobby is not synchronized with the server, thus is dirty.
    /// </summary>
    private bool IsLobbyDirty => localLobby != null && !NetworkManager.IsConnectedClient;

    protected virtual void Start()
    {
        if (instance)
        {
            throw new InvalidOperationException("There can only be one LobbiesManager.");
        }

        NetworkManager.OnClientDisconnectCallback += OnDisconnect;
        instance = this;
    }

    private void Update()
    {
        if (IsLobbyDirty) Client_ExitLobby();
    }

    public override void OnDestroy()
    {
        instance = null;
        if (NetworkManager)
        {
            NetworkManager.OnClientDisconnectCallback -= OnDisconnect;
        }

        localLobby = null;
        onLocalLobbyChange = null;
    }

    private void OnDisconnect(ulong playerID)
    {
        if (IsServer)
        {
            Server_ClientExitLobby(playerID, false);
        }
        else
        {
            Client_ExitLobby();
        }
    }
}