using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;
using Hashtable = ExitGames.Client.Photon.Hashtable;
using Photon.Realtime; 

public class PlayerManager : MonoBehaviour
{
    PhotonView PV;

    public int playerTeam, playerHero = 0, deaths = 0; 

    public float maxHealth = 100f, playerHealth = 100f;
    [SerializeField] float respawnTime = 5f; 

    GameObject controller; 

    private void Awake() {
        PV = GetComponent<PhotonView>();

        playerTeam = (int)PV.Owner.CustomProperties["team"];
        playerHero = (int)PV.Owner.CustomProperties["hero"]; 

        playerHealth = maxHealth; 
    }

    // Start is called before the first frame update
    void Start()
    {
        if (PV.IsMine) {
            CreateController(); 
        }
    }

    private void Update() {
        if (controller && playerHealth >= 0) controller.GetComponent<PlayerController>().healthBarFill.transform.localScale = new Vector3(playerHealth / maxHealth, controller.GetComponent<PlayerController>().healthBarFill.transform.localScale.y, controller.GetComponent<PlayerController>().transform.localScale.z); 
    }

    public void CreateController() {
        if (!PV.IsMine) return;
        Transform spawnpoint = SpawnManager.instance.GetSpawnpoint(playerTeam); 
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "PlayerController"), spawnpoint.position, Quaternion.identity, 0, new object[] { PV.ViewID }); 
    }

    public void CreateControllerReference(GameObject _controller) {
        controller = _controller; 
        maxHealth = controller.GetComponent<PlayerController>().heroes[playerHero].maxHealth; 
        playerHealth = maxHealth; 
    }

    public void TakeDamage(float damage, int viewID) {
        if (!controller) return; 

        PV.RPC("RPC_TakeDamage", RpcTarget.All, damage, viewID); 
    }

    [PunRPC]
    public void RPC_TakeDamage(float damage, int viewID) {
        if (playerHealth <= 0f) return; 

        playerHealth -= damage; 

        if (PV.IsMine && playerHealth <= 0f) {
            if (viewID != -1) {
                Player killer = PhotonView.Find(viewID).Owner;

                Hashtable hash = new Hashtable();
                if (killer.CustomProperties.TryGetValue("kills", out object kills)) hash.Add("kills", (int)kills + 1);
                else hash.Add("kills", 1); 
                killer.SetCustomProperties(hash);
            }

            Die(); 
        }
    }

    public void Die() {
        if (!PV.IsMine || !controller) return; 
        StartCoroutine(DieRespawn(respawnTime)); 
    }

    IEnumerator DieRespawn(float duration) {
        Destroy(controller.GetComponent<PlayerController>().UI.gameObject); 
        PhotonNetwork.Destroy(controller);
        ++deaths; 

        Hashtable hash = new Hashtable(); 
        hash.Add("deaths", deaths);
        PhotonNetwork.LocalPlayer.SetCustomProperties(hash); 

        yield return new WaitForSeconds(duration); 

        CreateController(); 
    }

    public void HealHP(float heal) {
        PV.RPC("RPC_HealHP", RpcTarget.All, heal); 
    }

    [PunRPC]
    public void RPC_HealHP(float damage) {
        playerHealth += damage;
        playerHealth = Mathf.Clamp(playerHealth, 0f, controller.GetComponent<PlayerController>().hero.maxHealth); 
    }

    public void Tase(float duration) {
        controller.GetComponent<PlayerController>().Tase(duration); 
    }

    public static PlayerManager Find(Player player) {
        PlayerManager[] players = FindObjectsOfType<PlayerManager>(); 
        foreach (PlayerManager _player in players) {
            if (player == _player.GetComponent<PhotonView>().Owner) return _player; 
        }

        Debug.Log("no manager found for player" + player.NickName); 
        return players[0]; 
    }
}
