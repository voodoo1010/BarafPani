using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform[] spawnPoints;
    private int spawnIndex = 0;
    
    private HashSet<ulong> _spawnedClients = new HashSet<ulong>();
    private int spawnedPlayers = 0;

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadComplete += OnClientConnected;

        // foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        // {
        //     SpawnPlayer(clientId);
        // }
    }

    public override void OnNetworkDespawn()
    {
        // if (!IsServer) return;
        // _spawnedClients.Clear();
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadComplete -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId, string sceneName, LoadSceneMode loadSceneMode)
    {
        // if (!IsServer) return;
        // StartCoroutine(SpawnAfterDelay(clientId, 0.2f));
        SpawnPlayer(clientId);
    }

    private IEnumerator SpawnAfterDelay(ulong clientId, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        {
            Debug.LogWarning($"ClientId {clientId} disconnected before spawn");
            yield break;
        }

        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!IsServer) return;
        Debug.Log($"Spawned Clients Count: {_spawnedClients.Count} | Attempting to spawn player for clientId: {clientId}");
        if (_spawnedClients.Contains(clientId))
        {
            Debug.Log($"ClientId {clientId} already spawned, skipping");
            return;
        }

        // if (spawnedPlayers >= )
        

        // if (!NetworkManager.Singleton.ConnectedClients.ContainsKey(clientId))
        // {
        //     Debug.LogWarning($"ClientId {clientId} not found in ConnectedClients");
        //     return;
        // }

        // var client = NetworkManager.Singleton.ConnectedClients[clientId];
        // if (client.PlayerObject != null)
        // {
        //     Debug.Log($"ClientId {clientId} already has a PlayerObject, skipping");
        //     return;
        // }

        Transform spawnPoint = spawnPoints[spawnIndex % spawnPoints.Length];
        spawnIndex++;

        GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        NetworkObject netObj = player.GetComponent<NetworkObject>();
        netObj.SpawnAsPlayerObject(clientId, true);

        _spawnedClients.Add(clientId);
        spawnedPlayers++;
        Debug.Log($"Post Spawned Clients Count: {_spawnedClients.Count}");
        Debug.Log($"Successfully spawned player for clientId: {clientId}");
    }
}