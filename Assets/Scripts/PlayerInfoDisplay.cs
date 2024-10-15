using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PlayerInfoDisplay : MonoBehaviour {
    [SerializeField] TextMeshProUGUI username;

    [SerializeField] PhotonView PV;

    PlayerController[] players;

    int localTeam;

    // Start is called before the first frame update
    void Start() {
        username.text = PV.Owner.NickName; 

        players = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController player in players) {
            PhotonView playerPV = player.GetComponent<PhotonView>();
            if (playerPV.Owner.NickName == PhotonNetwork.LocalPlayer.NickName) {
                localTeam = playerPV.GetComponent<PlayerController>().playerManager.playerTeam;
            }
        }

        if (localTeam == PV.GetComponent<PlayerController>().playerManager.playerTeam) username.color = Color.blue;
        else username.color = Color.red;
    }
}
