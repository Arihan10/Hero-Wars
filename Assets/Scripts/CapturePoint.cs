using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime; 

public class CapturePoint : MonoBehaviour
{
    public bool T1, T2; 

    int playersT1, playersT2;

    [SerializeField] float chargePerSecond = 0.1f; 
    float T1charge = 0f;

    PhotonView PV;

    bool won = false; 
    
    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>(); 
    }

    // Update is called once per frame
    void Update()
    {
        if (GameUI.instance.roundEnd) return; 

        bool T1charging = T1 && playersT1 > 0;
        bool T2charging = T2 && playersT2 > 0; 

        if (T1charging && !T2charging) {
            T1charge += chargePerSecond * Time.deltaTime; 
        } else if (T2charging && !T1charging) {
            T1charge -= chargePerSecond * Time.deltaTime; 
        }

        GameUI.instance.T1charge = T1charge; 

        if (PhotonNetwork.LocalPlayer.IsMasterClient && !won) {
            if (T1charge >= 1f) TeamWinRound(0); 
            else if (T1charge <= -1f) TeamWinRound(1); 
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return; 

        if (other.tag == "Player") {
            if (other.GetComponentInParent<PlayerController>().playerManager.playerTeam == 0) ++playersT1;
            else ++playersT2; 
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return; 

        if (other.tag == "Player") {
            if (other.GetComponentInParent<PlayerController>().playerManager.playerTeam == 0) --playersT1; 
            else --playersT2; 
        }
    }

    IEnumerator ErrorCorrection(float interval) {
        while (true) {
            PV.RPC("RPC_CorrectError", RpcTarget.Others, T1charge);

            yield return new WaitForSeconds(interval); 
        }
    }

    [PunRPC]
    void RPC_CorrectError(float charge) {
        T1charge = charge; 
    }

    public void Cap(int _team) {
        if (_team == 0) T1 = true; 
        else T2 = true; 

        if (PhotonNetwork.LocalPlayer.IsMasterClient && !(T1 && T2)) StartCoroutine(ErrorCorrection(0.2f)); 
    }

    public void TeamWinRound(int _team) {
        won = true; 
        PV.RPC("RPC_TeamWinRound", RpcTarget.All, _team); 
    }

    [PunRPC]
    void RPC_TeamWinRound(int _team) {
        GameUI.instance.RoundEnd(_team); 
    }
}
