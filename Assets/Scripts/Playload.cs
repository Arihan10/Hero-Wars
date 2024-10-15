using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Playload : MonoBehaviour
{
    [SerializeField] Transform points; 

    [SerializeField] int team;
    int index = -1; 
    // int homePlayers, awayPlayers; 

    [SerializeField] float homeSpeedPerPlayer = 0.334f, awaySpeedPerPlayer = 0.15f; 
    // float dist, lastTime; 

    Transform target;

    Quaternion lookAtTarget;

    [SerializeField] Material homeFluid, awayFluid; 

    [SerializeField] GameObject fluid;

    bool doneMovement = false;

    [SerializeField] CapturePoint capturePoint; 

    List<PlayerController> homePlayers = new List<PlayerController>(), awayPlayers = new List<PlayerController>(); 
    
    // Start is called before the first frame update
    void Start()
    {
        if (team == (int)PhotonNetwork.LocalPlayer.CustomProperties["team"]) fluid.GetComponent<MeshRenderer>().material = homeFluid; 
        else fluid.GetComponent<MeshRenderer>().material = awayFluid; 
        
        if (!PhotonNetwork.LocalPlayer.IsMasterClient) return; 

        MoveNext();
    }

    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient || doneMovement || GameUI.instance.roundEnd) return;

        foreach (PlayerController player in homePlayers) if (!player) homePlayers.Remove(player); 
        foreach (PlayerController player in awayPlayers) if (!player) awayPlayers.Remove(player); 

        /* float elapsedTransit = Time.time - lastTime; 
        float speed = homePlayers * homeSpeedPerPlayer - awayPlayers * awaySpeedPerPlayer;
        transform.position = Vector3.Lerp(transform.position, target.position, (elapsedTransit * speed) / dist); */

        Quaternion _rot = transform.rotation; 
        transform.LookAt(target); 
        lookAtTarget = transform.rotation; 
        transform.rotation = _rot; 

        transform.rotation = Quaternion.Lerp(transform.rotation, lookAtTarget, 1.35f * Time.deltaTime); 

        float speed = homePlayers.Count * homeSpeedPerPlayer - awayPlayers.Count * awaySpeedPerPlayer; 
        transform.Translate(transform.forward * (speed * Time.deltaTime), Space.World); 

        if (Vector3.Distance(new Vector3(transform.position.x, target.position.y, transform.position.z), target.position) <= 0.1f) MoveNext(); 
    }

    void MoveNext() {
        ++index; 
        if (index == points.childCount) {
            doneMovement = true;
            capturePoint.Cap(team);
            transform.rotation = target.rotation; 
            return; 
        }
        target = points.GetChild(index); 
    }

    private void OnTriggerEnter(Collider other) {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient || other.tag != "Player") return; 

        PlayerController player = other.GetComponentInParent<PlayerController>(); 
        if (player.playerManager.playerTeam == team) homePlayers.Add(player); 
        else awayPlayers.Add(player); 
    }

    private void OnTriggerExit(Collider other) {
        if (!PhotonNetwork.LocalPlayer.IsMasterClient || other.tag != "Player") return;

        PlayerController player = other.GetComponentInParent<PlayerController>(); 
        if (player.playerManager.playerTeam == team) homePlayers.Remove(player); 
        else awayPlayers.Remove(player); 
    }
}
