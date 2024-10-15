using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class PlayerController : MonoBehaviour
{
    #region Variables
    [SerializeField] Joystick joystick;

    [SerializeField] float downForce = 50f, stepSmooth = 2f, lowerDist = 0.05f, upperDist = 0.1f, stepHeight = 0.75f; 
    float lastFired; 

    Rigidbody rb;

    Vector3 movement; 

    PhotonView PV;

    public PlayerUI UI; 

    public PlayerManager playerManager; 

    public Hero[] heroes;
    public Hero hero;

    public GameObject healthBarFill;

    public bool[] effectsEnabled; 
    bool dazed = false, primary, secondary, special, ultimate; 

    public int[] decays;

    string radioactiveCanvasName = "IndicatorCanvas"; 
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();

        playerManager = PhotonView.Find((int)PV.InstantiationData[0]).GetComponent<PlayerManager>(); 
        // playerManager = PlayerManager.Find(PV.Owner); 
        playerManager.CreateControllerReference(gameObject); 

        // hero = heroes[playerManager.playerHero];
        // hero.gameObject.SetActive(true); 
        SetHero(playerManager.playerHero); 

        if (!PV.IsMine) {
            Destroy(UI.gameObject);
            // Destroy(rb); 
            foreach (Rigidbody _rb in GetComponentsInChildren<Rigidbody>()) {
                _rb.isKinematic = true; 
                _rb.interpolation = RigidbodyInterpolation.None; 
            }
            SetLayerRecursively(transform, "RemotePlayer"); 
        } else {
            SetLayerRecursively(transform, "LocalPlayer"); 
        }

        name = PV.Owner.NickName;

        effectsEnabled = new bool[decays.Length];
        for (int i = 0; i < effectsEnabled.Length; ++i) effectsEnabled[i] = false; 
    }

    // Update is called once per frame
    void Update()
    {
        if (!PV.IsMine || !hero || !rb || GameUI.instance.roundEnd) return; 

        if (Input.GetKey(KeyCode.A) || primary) Primary();
        if (Input.GetKey(KeyCode.S) || secondary) Secondary();
        if (Input.GetKey(KeyCode.LeftShift) || special) Special();
        if (Input.GetKey(KeyCode.X) || ultimate) Ultimate(); 

        if (dazed) return; 

        movement = new Vector3(joystick.Horizontal, 0f, joystick.Vertical);
        movement = movement.magnitude * movement.normalized; 

        if (Input.GetKeyDown(KeyCode.Space)) {
            hero.Jump();
        }
    }

    private void FixedUpdate() {
        if (!PV.IsMine || !hero || !rb || GameUI.instance.roundEnd) return; 
        if (dazed) return; 

        hero.Move(movement); 

        Vector3 moveDir = new Vector3(movement.x, 0f, movement.z); 
        if (moveDir != Vector3.zero) transform.forward = Vector3.Lerp(transform.forward, moveDir, 30f * Time.fixedDeltaTime);

        bool climbing = false; 
        RaycastHit lower; 
        hero.upperRaycast.transform.position = new Vector3(hero.lowerRaycast.transform.position.x, hero.lowerRaycast.transform.position.y + stepHeight, hero.lowerRaycast.transform.position.z); 
        if (Physics.Raycast(hero.lowerRaycast.transform.position, transform.TransformDirection(Vector3.forward), out lower, lowerDist) && rb.velocity.magnitude >= 0.25f) {
            RaycastHit upper; 
            if (!Physics.Raycast(hero.upperRaycast.transform.position, transform.TransformDirection(Vector3.forward), out upper, upperDist)) {
                Debug.Log("climbing"); 
                rb.position -= new Vector3(0f, -hero.stepSmooth * Time.fixedDeltaTime, 0f);
                climbing = true; 
            }
        }

        if (!climbing) rb.AddForce(Vector3.down * (downForce * Time.fixedDeltaTime)); 

        for (int i = 0; i < decays.Length; ++i) {
            if (decays[i] > 0 && !effectsEnabled[i]) PV.RPC("RPC_ToggleDecay", RpcTarget.All, i, true); 
            else if (decays[i] <= 0 && effectsEnabled[i]) PV.RPC("RPC_ToggleDecay", RpcTarget.All, i, false); 
        }
    }

    #region Own Abilities
    public void StartHold(int btn) {
        if (btn == 0) primary = true; 
        else if (btn == 1) secondary = true; 
        else if (btn == 2) special = true; 
        else ultimate = true; 
    }

    public void Click(int btn) {
        /*if (btn == 0) hero.PrimaryClicked(); 
        else if (btn == 1) hero.SecondaryClicked(); 
        else if (btn == 2) hero.SpecialClicked(); 
        else hero.UltimateClicked();*/
    }

    public void StopHold(int btn) {
        if (btn == 0) {
            hero.PrimaryClicked();
            primary = false;
        } else if (btn == 1) {
            hero.SecondaryClicked();
            secondary = false;
        } else if (btn == 2) {
            hero.SpecialClicked();
            special = false;
        } else {
            hero.UltimateClicked(); 
            ultimate = false;
        }
    }

    public void Primary() {
        hero.Primary(); 
    }

    public void Secondary() {
        hero.Secondary(); 
    }

    public void Special() {
        hero.Special(); 
    }

    public void Ultimate() {
        hero.Ultimate(); 
    }
    #endregion

    #region Enemy Abilities
    public void Tase(float duration) {
        PV.RPC("RPC_Tase", RpcTarget.All, duration); 
    }

    [PunRPC]
    void RPC_Tase(float duration) {
        StartCoroutine(TaseRoutine(duration)); 
        // Destroy(Instantiate(hero.taseEffect, hero.taseEffect.transform.position, hero.taseEffect.transform.rotation), duration); 
    }

    IEnumerator TaseRoutine(float duration) {
        dazed = true;
        hero.effects[0].SetActive(true); 

        yield return new WaitForSeconds(duration);

        hero.effects[0].SetActive(false); 
        dazed = false; 
    }

    public void GravitateTowards(Vector3 pos, float strength) {
        PV.RPC("RPC_GravitateTowards", RpcTarget.All, pos, strength); 
    }

    [PunRPC]
    void RPC_GravitateTowards(Vector3 pos, float strength) {
        rb.AddForce((pos - transform.position).normalized * strength); 
    }

    /*public void ToggleDecay(int decayIndex) {
        if (decays[decayIndex] == 0) PV.RPC("RPC_ToggleeDecay", RpcTarget.All, toggle); 
    }*/

    [PunRPC]
    void RPC_ToggleDecay(int decayIndex, bool toggle) {
        Debug.Log("trying to decay " + decayIndex + " " + hero.effects[decayIndex].name + " " + toggle); 
        hero.effects[decayIndex].SetActive(toggle);
        effectsEnabled[decayIndex] = toggle; 
    }

    public void TeleportClicked(Vector2 touchPos) {
        hero.SpecialHit(touchPos); 
    }

    public void RadioactiveEnter(int viewID, bool hit, int layer) {
        if (layer == 1 && PhotonView.Find(viewID).Owner == PV.Owner) return; 
        PV.RPC("RPC_RadioactiveEnter", RpcTarget.All, viewID, hit, layer); 
    }

    [PunRPC]
    void RPC_RadioactiveEnter(int viewID, bool hit, int layer) {
        PhotonView.Find(viewID).GetComponentInParent<PlayerController>().hero.SecondaryHit(this, hit, layer); 
    }

    private void OnTriggerEnter(Collider other) {
        if (!PV.IsMine) return; 

        if (other.name == radioactiveCanvasName) {
            // other.GetComponentInParent<PlayerController>().hero.SecondaryHit(this, true); 
            // if (other.GetComponentInParent<PlayerController>()) PV.RPC("RPC_RadioactiveEnter", RpcTarget.All, other.GetComponentInParent<PlayerController>().GetComponent<PhotonView>().ViewID, true, 0); 
            if (other.GetComponentInParent<PlayerController>()) RadioactiveEnter(other.GetComponentInParent<PlayerController>().GetComponent<PhotonView>().ViewID, true, 0); 
            // else PV.RPC("RPC_RadioactiveEnter", RpcTarget.All, other.GetComponentInParent<WhiteHole>().playerPV.ViewID, true, 1); 
            else RadioactiveEnter(other.GetComponentInChildren<WhiteHole>().playerPV.ViewID, true, 1); 
            Debug.Log("trigger enter"); 
        } else if (other.tag == "Death") {
            playerManager.TakeDamage(10000f, -1); 
        }
    }

    private void OnTriggerExit(Collider other) {
        if (!PV.IsMine) return; 

        if (other.name == radioactiveCanvasName) {
            // other.GetComponentInParent<PlayerController>().hero.SecondaryHit(this, false); 
            if (other.GetComponentInParent<PlayerController>()) RadioactiveEnter(other.GetComponentInParent<PlayerController>().GetComponent<PhotonView>().ViewID, false, 0);
            // else PV.RPC("RPC_RadioactiveEnter", RpcTarget.All, other.GetComponentInParent<WhiteHole>().playerPV.ViewID, true, 1); 
            else RadioactiveEnter(other.GetComponentInChildren<WhiteHole>().playerPV.ViewID, false, 1); 
            Debug.Log("trigger exit"); 
        }
    }
    #endregion

    #region Utility
    public void SetHero(Hero _hero) {
        hero = _hero; 
        foreach (Hero __hero in heroes) {
            __hero.gameObject.SetActive(false); 
        }
        hero.gameObject.SetActive(true); 
    }

    public void SetHero(int _heroIndex) {
        hero = heroes[_heroIndex]; 
        foreach (Hero _hero in heroes) {
            _hero.gameObject.SetActive(false);
        }
        hero.gameObject.SetActive(true); 
    }

    public void SetLayerRecursively(Transform parent, string layer) {
        parent.gameObject.layer = LayerMask.NameToLayer(layer);

        foreach (Transform child in parent) {
            SetLayerRecursively(child, layer);
        }
    }
    #endregion
}
