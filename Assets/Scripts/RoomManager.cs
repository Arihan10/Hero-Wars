using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

public class RoomManager : MonoBehaviourPunCallbacks {
    public static RoomManager instance;

    public int playerHero = 0; 
    int playerTeam = 110; 

    private void Awake() {
        if (instance) Destroy(gameObject);
        instance = this; 
        DontDestroyOnLoad(this); 
    }

    public void ChooseTeam() {
        // Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties; 
        Hashtable hash = new Hashtable(); 
        if (PhotonNetwork.CurrentRoom.PlayerCount <= 1) playerTeam = 0; 
        else playerTeam = 1;
        hash.Add("team", playerTeam); 
        // hash["team"] = playerTeam; 
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash); 
    }

    public void ChooseHero(int _heroIndex) {
        playerHero = _heroIndex;
        // Hashtable hash = PhotonNetwork.LocalPlayer.CustomProperties; 
        Hashtable hash = new Hashtable(); 
        hash.Add("hero", playerHero); 
        // hash["hero"] = playerHero; 
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash); 
    }

    public override void OnEnable() {
        base.OnEnable();
        SceneManager.sceneLoaded += OnSceneLoaded; 
    }

    public override void OnDisable() {
        base.OnDisable();
        SceneManager.sceneLoaded -= OnSceneLoaded; 
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode) {
        if (scene.buildIndex == 1) {
            Debug.Log("Creating player!"); 
            PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerManager"), Vector3.zero, Quaternion.identity); 
            // AudioManager.instance.Background(); 
        }
    }
}
