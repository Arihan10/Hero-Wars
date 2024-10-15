using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable; 

public class PlayerUI : MonoBehaviourPunCallbacks
{
    [SerializeField] GameObject player, joystick; 
    public GameObject primaryBtn, secondaryBtn, specialBtn, ultimateBtn, primaryBtnFill, secondaryBtnFill, specialBtnFill, teliBtn, scoreboard;

    [SerializeField] TextMeshProUGUI scoreHomeText, scoreAwayText; 

    int scoreHome, scoreAway; 
    
    // Start is called before the first frame update
    void Start()
    {
        transform.parent = null; 
        joystick.SetActive(true);

        UpdateScoreCount(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ToggleScoreboard(); 
        }
    }

    public void OnClick() {
        GetComponentInChildren<TextMeshProUGUI>().text = "CLICKED"; 
    }

    public void TeleportClicked() {
        Vector2 localPoint; 
        for (int i = 0; i < Input.touchCount; ++i) {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.touches[0].position, null, out localPoint); 
            Debug.Log(localPoint);

            if (Vector3.Distance(Vector3.zero, localPoint) <= teliBtn.GetComponent<RectTransform>().rect.width / 2f) {
                player.GetComponent<PlayerController>().TeleportClicked(Input.touches[0].position); 
                return; 
            }
        }
        
        RectTransformUtility.ScreenPointToLocalPointInRectangle(GetComponent<RectTransform>(), Input.mousePosition, null, out localPoint);
        Debug.Log(localPoint); 

        if (Vector3.Distance(Vector3.zero, localPoint) <= teliBtn.GetComponent<RectTransform>().rect.width / 2f) {
            player.GetComponent<PlayerController>().TeleportClicked(Input.mousePosition); 
            return;
        }
    }

    public void ToggleScoreboard() {
        // scoreboard.SetActive(!scoreboard.activeSelf); 
        if (scoreboard.GetComponent<CanvasGroup>().alpha == 0) scoreboard.GetComponent<CanvasGroup>().alpha = 1;
        else scoreboard.GetComponent<CanvasGroup>().alpha = 0; 
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps) {
        if (changedProps.ContainsKey("kills")) {
            // int _team = PlayerManager.Find(targetPlayer).playerTeam; 
            if (PlayerManager.Find(targetPlayer).playerTeam == player.GetComponent<PlayerController>().playerManager.playerTeam) {
                ++scoreHome;
                scoreHomeText.text = scoreHome.ToString(); 
            } else {
                ++scoreAway; 
                scoreAwayText.text = scoreAway.ToString(); 
            }
        }
    }

    public void UpdateScoreCount() {
        PlayerManager[] players = FindObjectsOfType<PlayerManager>(); 
        foreach (PlayerManager manager in players) {
            int _kills = 0;
            if (manager.GetComponent<PhotonView>().Owner.CustomProperties.TryGetValue("kills", out object kills)) _kills = (int)kills; 
            if (manager.playerTeam == player.GetComponent<PlayerController>().playerManager.playerTeam) {
                scoreHome += _kills; 
                scoreHomeText.text = scoreHome.ToString(); 
            } else {
                scoreAway += _kills; 
                scoreAwayText.text = scoreAway.ToString(); 
            }
        }
    }
}
