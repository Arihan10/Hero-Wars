using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.SceneManagement; 

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public Image captureFill;

    public float T1charge = 0f; 

    int localTeam;

    [SerializeField] GameObject homeBoxes, awayBoxes; 

    public bool roundEnd = false;

    [SerializeField] GameObject winText, loseText;

    int homeScore, awayScore; 
    
    // Start is called before the first frame update
    void Start()
    {
        if (instance) Destroy(gameObject); 
        else instance = this;
        DontDestroyOnLoad(this); 

        localTeam = (int)PhotonNetwork.LocalPlayer.CustomProperties["team"]; 
    }

    private void Update() {
        if (T1charge > 0f) {
            if (localTeam == 0) captureFill.color = Color.blue; 
            else captureFill.color = Color.red; 
        } else if (T1charge < 0f) {
            if (localTeam == 0) captureFill.color = Color.red; 
            else captureFill.color = Color.blue; 
        }

        captureFill.transform.localScale = new Vector3(Mathf.Abs(T1charge), captureFill.transform.localScale.y, captureFill.transform.localScale.z); 
    }

    public void RoundEnd(int team) {
        StartCoroutine(NewRound(team, 3.5f)); 
    }

    IEnumerator NewRound(int team, float delay) {
        roundEnd = true;
        if (team == localTeam) {
            winText.SetActive(true); 
            homeBoxes.transform.GetChild(homeScore).GetChild(0).GetComponent<Image>().color = Color.blue; 
            if (++homeScore == 3) {
                Debug.Log("GAME WIN!"); 
            }
        } else {
            loseText.SetActive(true); 
            awayBoxes.transform.GetChild(awayScore).GetChild(0).GetComponent<Image>().color = Color.red; 
            if (++awayScore == 3) {
                Debug.Log("GAME LOSE!"); 
            }
        }
        FindObjectOfType<PlayerUI>().gameObject.SetActive(false); 

        yield return new WaitForSeconds(delay);

        winText.SetActive(false); 
        loseText.SetActive(false); 
        roundEnd = false;
        if (PhotonNetwork.LocalPlayer.IsMasterClient) {
            // Reload(); 
            PhotonNetwork.DestroyAll(); 
            PhotonNetwork.AutomaticallySyncScene = true; 
            PhotonNetwork.LoadLevel(2); 
            yield return new WaitForSeconds(1f); 
            PhotonNetwork.LoadLevel(1); 
        }
    }
}
