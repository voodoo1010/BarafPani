using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    private int spawnIndex = 0;

    public override void OnNetworkSpawn()
    {
        Debug.Log($"OnNetworkSpawn fired. IsServer: {IsServer}, IsHost: {IsHost}, IsClient: {IsClient}");
        if (!IsServer) return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientFullyConnected;

        SpawnPlayer(NetworkManager.Singleton.LocalClientId);
    }


    private void OnClientFullyConnected(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId) return;
        Debug.Log($"Client fully connected: {clientId}");
        SpawnPlayer(clientId);
    }

    public override void OnNetworkDespawn()
    {
        if (!IsServer) return;
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientFullyConnected;
    }

    private void SpawnPlayer(ulong clientId)
    {
        Debug.Log($"SpawnPlayer called for clientId: {clientId}");
        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogWarning($"ClientId {clientId} not found in ConnectedClients");
            return;
        }
        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        if (client.PlayerObject != null)
        {
            Debug.Log($"ClientId {clientId} already has a PlayerObject, skipping");
            return;
        }
        Transform spawnPoint = spawnPoints[spawnIndex % spawnPoints.Length];
        spawnIndex++;
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject netObj = player.GetComponent<NetworkObject>();

        Debug.Log($"NetworkObject for clientId {clientId} | IsSpawned before: {netObj.IsSpawned} | Observers would include client: {NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId)}");

        netObj.SpawnAsPlayerObject(clientId, destroyWithScene: true);

        Debug.Log($"After spawn | IsSpawned: {netObj.IsSpawned} | Observer count: {netObj.GetObservers()}");
    }
}