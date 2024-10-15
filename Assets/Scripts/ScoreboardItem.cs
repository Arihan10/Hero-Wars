using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Realtime;
using Photon.Pun;
using Hastable = ExitGames.Client.Photon.Hashtable; 

public class ScoreboardItem : MonoBehaviourPunCallbacks {
    [SerializeField] TextMeshProUGUI usernameText, killsText, deathsText;

    Player player; 

    public void Initialize(Player player) {
        usernameText.text = player.NickName;
        this.player = player;

        UpdateStats(); 
    }

    public void UpdateStats() {
        if (player.CustomProperties.TryGetValue("kills", out object kills)) {
            killsText.text = kills.ToString(); 
        }

        if (player.CustomProperties.TryGetValue("deaths", out object deaths)) {
            deathsText.text = deaths.ToString(); 
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hastable changedProps) {
        Debug.Log("props changed"); 
        if (targetPlayer == player) {
            if (changedProps.ContainsKey("kills") || changedProps.ContainsKey("deaths")) {
                UpdateStats();
            }
        }
    }
}
