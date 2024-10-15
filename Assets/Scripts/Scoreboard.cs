using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun; 

public class Scoreboard : MonoBehaviour
{
    [SerializeField] Transform containerHome, containerAway;
    [SerializeField] GameObject scoreboardItemPrefab;

    Player[] players;

    int localTeam;

    private void Awake() {
        // localTeam = GetComponentInParent<PlayerController>().playerManager.playerTeam; 
    }

    // Start is called before the first frame update
    void Start()
    {
        localTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"]; 

        players = PhotonNetwork.PlayerList; 
        foreach (Player player in players) {
            AddScoreboardItem(player); 
        }
    }

    void AddScoreboardItem(Player player) {
        ScoreboardItem item;
        if ((int)player.CustomProperties["team"] == localTeam) {
            item = Instantiate(scoreboardItemPrefab, containerHome).GetComponent<ScoreboardItem>(); 
        } else item = Instantiate(scoreboardItemPrefab, containerAway).GetComponent<ScoreboardItem>();

        item.Initialize(player); 
    }
}
