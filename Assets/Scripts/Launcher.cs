using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using Photon.Pun.UtilityScripts;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using UnityEngine.UI; 

public class Launcher : MonoBehaviourPunCallbacks {
    public static Launcher instance;

    [SerializeField] TMP_InputField roomNameInputField, playerNameInputField;

    [SerializeField] TMP_Text roomNameText, heroName; 

    [SerializeField] Transform[] playerListContents; 
    [SerializeField] Transform roomListContent; 

    [SerializeField] GameObject roomListItemPrefab, playerListItemPrefab, startGameButton;

    [SerializeField] PhotonTeamsManager teamManager;

    [SerializeField] HeroProfile[] heroes;

    [SerializeField] Image heroImg; 

    private void Awake() {
        instance = this;
    }

    // Start is called before the first frame update
    void Start() {
        // MenuManager.instance.OpenMenu("LoadingMenu"); 
        MenuManager.instance.OpenLoadingMenu("LoadingMenu"); 
        Debug.Log("Connecting to Master");
        PhotonNetwork.ConnectUsingSettings(); 
    }

    public override void OnConnectedToMaster() {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    public override void OnJoinedLobby() {
        Debug.Log("Joined Lobby");
        MenuManager.instance.OpenMenu("PlayMenu"); 

        PhotonNetwork.NickName = "Player " + Random.Range(0, 1000);
        // playerNameInputField.text = PhotonNetwork.NickName; 

        ChooseHero(0); 
    }

    public void CreateRoom() {
        if (string.IsNullOrEmpty(roomNameInputField.text)) return; 
        RoomOptions rmOptions = new RoomOptions() { CleanupCacheOnLeave = false };
        PhotonNetwork.CreateRoom(roomNameInputField.text, rmOptions); 
        MenuManager.instance.OpenLoadingMenu("LoadingMenu"); 
    }

    public void StartGame() {
        if (!PhotonNetwork.InRoom) {
            PhotonNetwork.JoinRandomOrCreateRoom(null, 16, MatchmakingMode.FillRoom, null, null, PhotonNetwork.NickName, null, null); 
            MenuManager.instance.OpenLoadingMenu("LoadingMenu"); 
            return; 
        }
        
        PhotonNetwork.LoadLevel(1); 
    }

    public void LeaveRoom() {
        PhotonNetwork.LeaveRoom();
        // MenuManager.instance.OpenMenu("LoadingMenu"); 
        MenuManager.instance.OpenLoadingMenu("LoadingMenu"); 
    }

    public override void OnJoinedRoom() {
        MenuManager.instance.OpenMenu("RoomMenu"); 
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        RoomManager.instance.ChooseTeam(); 

        foreach (Transform playerListContent in playerListContents) {
            foreach (Transform trans in playerListContent) {
                Destroy(trans.gameObject); 
            }
        }

        Player[] players = PhotonNetwork.PlayerList;
        for (int i = 0; i < players.Length; ++i) {
            int playerTeam = 0;
            if (players[i].CustomProperties.ContainsKey("team")) playerTeam = (int)players[i].CustomProperties["team"]; 

            Instantiate(playerListItemPrefab, playerListContents[playerTeam]).GetComponent<PlayerListItem>().Setup(players[i]); 
        }

        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
        for (int i = 0; i < playerListContents.Length; ++i) {
            foreach (Transform trans in playerListContents[i]) {
                if (trans.GetComponent<PlayerListItem>().text.text == targetPlayer.NickName) {
                    Destroy(trans.gameObject); 
                }
            }

            if (i == (int)changedProps["team"]) {
                Instantiate(playerListItemPrefab, playerListContents[(int)changedProps["team"]]).GetComponent<PlayerListItem>().Setup(targetPlayer); 
            }
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient) {
        startGameButton.SetActive(PhotonNetwork.IsMasterClient); 
    }

    public override void OnLeftRoom() {
        MenuManager.instance.OpenMenu("PlayMenu");

        startGameButton.SetActive(true); 
    }

    public void JoinRoom(RoomInfo info) {
        PhotonNetwork.JoinRoom(info.Name);
        // MenuManager.instance.OpenMenu("LoadingMenu"); 
        MenuManager.instance.OpenLoadingMenu("LoadingMenu"); 
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList) {
        foreach (Transform trans in roomListContent) {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; ++i) {
            if (roomList[i].RemovedFromList) continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().Setup(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) {
        int playerTeam = 0;
        if (newPlayer.CustomProperties.ContainsKey("team")) playerTeam = (int)newPlayer.CustomProperties["team"];

        Instantiate(playerListItemPrefab, playerListContents[playerTeam]).GetComponent<PlayerListItem>().Setup(newPlayer); 
    }

    public void OnUsernameInputValueChanged() {
        PhotonNetwork.NickName = playerNameInputField.text; 
    }

    public void ChooseHero(int heroIndex) {
        RoomManager.instance.ChooseHero(heroIndex); 

        heroName.text = heroes[heroIndex].name;
        heroImg.sprite = heroes[heroIndex].sprite; 
    }
}
