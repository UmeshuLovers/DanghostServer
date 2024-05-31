using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public class ActiveLobbiesEditor : EditorWindow
{
    private enum LobbyDisplayMode
    {
        Local,
        Server
    }

    public ActiveLobbiesEditor()
    {
        titleContent = new GUIContent("Lobbies Tracker");
        LobbiesManager.onLocalLobbyChange += TriggerRepaint;
    }

    private float CurrentTime => (float)EditorApplication.timeSinceStartup;
    private const float fetchInterval = 1f;
    private bool autoFetch = true;
    private float lastFetchTime;
    private bool ShouldFetch => autoFetch && CurrentTime - lastFetchTime > fetchInterval;
    private bool IsNetworkManagerActive => NetworkManager.Singleton != null;
    private bool IsServer => IsNetworkManagerActive && NetworkManager.Singleton.IsServer;
    private bool IsClient => IsNetworkManagerActive && NetworkManager.Singleton.IsClient;
    private bool IsHost => IsNetworkManagerActive && NetworkManager.Singleton.IsHost;
    private ulong? LocalClientID => IsNetworkManagerActive ? NetworkManager.Singleton.LocalClientId : null;
    private LobbiesCollection Lobbies => LobbiesManager.Lobbies;
    private LobbiesCollection ServerLobbies => LobbiesManager.ServerLobbies;
    private MultiplayerLobby LocalLobby => LobbiesManager.LocalLobby;
    private LobbyDisplayMode displayMode = LobbyDisplayMode.Local;

    [MenuItem("Multiplayer/Lobbies Tracker")]
    public static void ShowWindow() => GetWindow<ActiveLobbiesEditor>();

    private void TriggerRepaint(MultiplayerLobby lobby)
    {
        Repaint();
        EditorUtility.SetDirty(this);
    }

    private void TryFetchServerLobbies()
    {
        if (!ShouldFetch) return;
        LobbiesManager.RequestLobbiesFetch();
        lastFetchTime = CurrentTime;
    }

    private void CustomLabel(string label, int width)
    {
        GUILayout.Label(label, EditorStyles.boldLabel, new[] { GUILayout.Width(width) });
    }

    public void OnGUI()
    {
        GUILayout.Space(15);
        GUILayout.Label("Lobbies Tracker", EditorStyles.boldLabel);
        OnGUINetworkManagement();
        if (IsServer)
        {
            OnGUIServer(Lobbies);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            CustomLabel("Tracking", 100);
            displayMode = (LobbyDisplayMode)EditorGUILayout.EnumPopup((Enum)displayMode, new[] { GUILayout.Width(150) });
            if (EditorGUI.EndChangeCheck() && displayMode == LobbyDisplayMode.Server)
            {
                LobbiesManager.RequestLobbiesFetch();
            }

            if (displayMode == LobbyDisplayMode.Local)
            {
                EditorGUILayout.EndHorizontal();
                OnGUIClient();
            }
            else
            {
                CustomLabel("Auto Fetch", 80);
                autoFetch = EditorGUILayout.Toggle(autoFetch, new[] { GUILayout.Width(20) });
                if (!autoFetch)
                {
                    if (GUILayout.Button("Fetch"))
                    {
                        LobbiesManager.RequestLobbiesFetch();
                    }
                }

                EditorGUILayout.EndHorizontal();
                OnGUIServer(ServerLobbies);
                TryFetchServerLobbies();
            }
        }
    }

    private void OnGUINetworkManagement()
    {
        if (!IsNetworkManagerActive)
        {
            GUILayout.Label("NetworkManager is not active", EditorStyles.helpBox);
        }
        else if (!IsClient && !IsServer && !IsHost)
        {
            GUILayout.Label("NetworkManager is active, but the local instance is neither client, server nor host.", EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Server"))
            {
                NetworkManager.Singleton.StartServer();
            }

            if (GUILayout.Button("Start Client"))
            {
                NetworkManager.Singleton.StartClient();
            }

            if (GUILayout.Button("Start Host"))
            {
                NetworkManager.Singleton.StartHost();
            }

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            string mode = IsClient ? "Client" : IsServer ? "Server" : "Host";
            GUILayout.Label($"NetworkManager is active and the local instance is {mode}.", EditorStyles.helpBox);
            if (GUILayout.Button($"Stop {mode}"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }
    }

    private void OnGUIClient()
    {
        GUILayout.Space(15);
        string clientID = LocalClientID.HasValue ? $" ({LocalClientID})" : "";
        GUILayout.Label($"Client{clientID}'s Tracker", EditorStyles.boldLabel);
        if (LocalLobby == null)
        {
            GUILayout.Label("No active lobby", EditorStyles.helpBox);
            return;
        }

        DisplayLobby(0, LocalLobby);
    }

    private void OnGUIServer(LobbiesCollection lobbies)
    {
        GUILayout.Space(15);
        GUILayout.Label("Server's Tracker", EditorStyles.boldLabel);
        if (lobbies == null)
        {
            GUILayout.Label("No active lobbies", EditorStyles.helpBox);
        }
        else
        {
            GUILayout.Space(15);
        }

        for (int i = 0; i < lobbies?.Count; i++)
        {
            DisplayLobby(i, lobbies[i]);
        }
    }

    private void DisplayLobby(int index, MultiplayerLobby lobby)
    {
        GUILayout.Label($"Lobby {index}", EditorStyles.helpBox, new[] { GUILayout.Width(260) });
        DrawValue("Lobby ID", lobby.LobbyID.ToString(), 60, 80);
        DrawValue("Seed", lobby.GameplaySeed.ToString(), 40, 60);
        DrawValue("Private Lobby", (option => GUILayout.Toggle(lobby.IsPrivate, "", option)), 100, 40);
        for (int i = 0; i < lobby.Count; i++)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(15);
            DrawColumnItem($"Player", $"{i + 1} / 4", 40, 40);
            DrawColumnItem("Client ID", lobby[i].PlayerID.ToString(), 70, 80);
            DrawColumnItem("Client Name", lobby[i].PlayerName, 85, 80);
            GUILayout.EndHorizontal();
        }
    }

    private void DrawValue(string label, string value, float labelWidth, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        DrawColumnItem(label, value, labelWidth, fieldWidth);
        GUILayout.EndHorizontal();
    }

    private void DrawColumnItem(string label, string value, float labelWidth, float fieldWidth)
    {
        GUILayout.Label(label, EditorStyles.boldLabel, new[] { GUILayout.Width(labelWidth), GUILayout.ExpandWidth(false) });
        GUILayout.Label(value, EditorStyles.textField, new[] { GUILayout.Width(fieldWidth), GUILayout.ExpandWidth(false) });
        GUILayout.Space(5);
    }

    private void DrawValue(string label, Action<GUILayoutOption> drawValue, float labelWidth, float fieldWidth)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(15);
        GUILayout.Label(label, EditorStyles.boldLabel, new[] { GUILayout.Width(labelWidth), GUILayout.ExpandWidth(false) });
        drawValue(GUILayout.Width(fieldWidth));
        GUILayout.Space(5);
        GUILayout.EndHorizontal();
    }
}