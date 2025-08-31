using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;

public class PlayerSpawner : MonoBehaviourPun
{
    public GameObject playerPrefab;
    public Transform[] spawnPoints;

    public void SpawnPlayer()
    {
        int spawnIndex = (PhotonNetwork.LocalPlayer.ActorNumber - 1) % spawnPoints.Length;
        Transform spawnPoint = spawnPoints[spawnIndex];
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, Quaternion.identity);

        // 스폰 완료 세팅
        Hashtable props = new Hashtable { { "Spawned", true } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }

    private void Start()
    {
        SpawnPlayer();
    }
}
