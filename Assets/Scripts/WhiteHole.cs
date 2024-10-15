using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class WhiteHole : MonoBehaviour
{
    public PhotonView playerPV; 

    [SerializeField] float whiteHoleTime = 3f; 

    List<PlayerController> players = new List<PlayerController>(); 
    
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(DestroyHole(whiteHoleTime)); 
    }

    public void Setup(PhotonView PV) {
        playerPV = PV; 
    }

    IEnumerator DestroyHole(float time) {
        if (!playerPV.IsMine) yield break; 

        yield return new WaitForSeconds(time); 

        Debug.Log("hello daadu"); 
        foreach (PlayerController player in players) player.RadioactiveEnter(playerPV.ViewID, false, 1); 
        Destroy(transform.parent.gameObject); 
    }

    private void OnTriggerEnter(Collider other) {
        if (other.GetComponentInParent<PlayerController>()) {
            players.Add(other.GetComponentInParent<PlayerController>()); 
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.GetComponentInParent<PlayerController>()) {
            players.Remove(other.GetComponentInParent<PlayerController>()); 
        }
    }
}
