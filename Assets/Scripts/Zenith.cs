using System.Collections;
using System.Collections.Generic;
using UnityEngine; 
using UnityEngine.UI; 
using Photon.Pun; 

public class Zenith : Hero {
    Rigidbody rb;

    [SerializeField] GameObject bomb, bombLocation, warningField;
    List<PlayerController> decayingPlayers = new List<PlayerController>(), decayingPlayers2 = new List<PlayerController>(); 

    [SerializeField] float throwMin = 0.1f, throwMax = 5f, holeRadius = 5f, holeGravity = 10f, holeDamage = 7.5f, radioactiveRadius = 5f, radioactiveRadius2 = 3f, radioActiveStart = 10f, radioactiveEnd = 1f, teleportDuration = 0.5f; 
    float charge = 0f; 

    // Start is called before the first frame update
    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    public override void Move(Vector3 movement) {
        rb.AddForce(movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public override void Jump() {
        rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
    }

    public override void Primary() {
        if (!primaryUse && Time.time - lastPrimary > 1f / primaryRate) {
            lastPrimary = Time.time;
            primaryUse = true;
        }

        if (primaryUse) {
            charge += Time.deltaTime;
        }
    }

    public override void PrimaryClicked() {
        if (primaryUse) {
            PV.RPC("RPC_ThrowBomb", RpcTarget.All, transform.forward, charge);

            charge = 0f; 
            primaryUse = false; 
            lastPrimary = Time.time; 
        }
    }

    [PunRPC]
    void RPC_ThrowBomb(Vector3 forward, float _charge) {
        GameObject _bomb = Instantiate(bomb, bombLocation.transform.position, bomb.transform.rotation);
        _bomb.GetComponent<Rigidbody>().velocity = rb.velocity * 1f; 
        _bomb.GetComponent<Rigidbody>().AddForce(forward * Mathf.Lerp(throwMin, throwMax, Mathf.Max(_charge / primaryDuration, 0f))); 
        _bomb.GetComponent<BlackHoleBomb>().Setup(PV, this, holeRadius); 
    }

    public override void PrimaryHit(PlayerController con, Vector3 pos) {
        float _strength = (holeRadius -  Vector3.Distance(con.transform.position, pos)) / holeRadius; 
        con.GravitateTowards(pos, holeGravity * _strength); 
        con.playerManager.TakeDamage(holeDamage * _strength, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
    }

    public override void Secondary() {
        if (Time.time - lastSecondary > 1f / secondaryRate && !secondaryUse) {
            Debug.Log("Used secondary");

            PV.RPC("RPC_RadioactiveField", RpcTarget.All, secondaryDuration); 

            lastSecondary = Time.time;
        }
    }

    [PunRPC]
    void RPC_RadioactiveField(float duration) {
        StartCoroutine(RadioactiveField(duration)); 
    }

    IEnumerator RadioactiveField(float duration) {
        secondaryUse = true; 
        StartCoroutine(FadeField(true, 0.5f, 0.02f)); 

        yield return new WaitForSeconds(duration);

        StartCoroutine(FadeField(false, 0.5f, 0.02f)); 
        secondaryUse = false; 

        lastSecondary = Time.time; 

        /*yield return new WaitForSeconds(0.05f); 
        if (PV.IsMine) PoisonPlayers(false);*/
        foreach (PlayerController _player in decayingPlayers) {
            Debug.Log("removing decay player " + _player.name); 
            --_player.decays[1]; 
        }
        decayingPlayers.Clear(); 
    }

    IEnumerator FadeField(bool fadeIn, float duration, float increment) {
        if (fadeIn) warningField.SetActive(true); 

        for (float i = 0f; i <= duration; i += increment) {
            Color col = warningField.GetComponentInChildren<Image>().color;
            col.a = fadeIn ? i / duration : 1f - i / duration;
            warningField.GetComponentInChildren<Image>().color = col; 

            yield return new WaitForSeconds(increment); 
        }

        if (!fadeIn) warningField.SetActive(false); 
    }

    public override void SecondaryClicked() { }
    
    public override void SecondaryHit(PlayerController controller, bool toggle, int layer) {
        Debug.Log("secondary hit");

        if (toggle) {
            if (layer == 0) decayingPlayers.Add(controller);
            else decayingPlayers2.Add(controller); 
            ++controller.decays[1]; 
        } else {
            if (layer == 0) decayingPlayers.Remove(controller);
            else decayingPlayers2.Remove(controller); 
            --controller.decays[1]; 
        }
    }

    public override void Special() {
        if (Time.time - lastSpecial > 1f / specialRate && !specialUse) {
            Debug.Log("Used special");

            StartCoroutine(ShowTeli(specialDuration)); 

            lastSpecial = Time.time;
        }
    }

    IEnumerator ShowTeli(float duration) {
        GetComponentInParent<PlayerController>().UI.teliBtn.SetActive(true);
        specialUse = true; 

        yield return new WaitForSeconds(duration);

        if (!specialUse) yield break; 

        GetComponentInParent<PlayerController>().UI.teliBtn.SetActive(false);
        specialUse = false; 
    }

    public override void SpecialClicked() { }

    public override void SpecialHit(Vector2 touchPos) {
        Debug.Log("special hit");

        Ray ray = Camera.main.ScreenPointToRay(touchPos); 

        RaycastHit hit; 
        if (Physics.Raycast(ray, out hit)) {
            Vector3 newPos = new Vector3(hit.point.x, GetComponentInParent<PlayerController>().transform.position.y, hit.point.z);
            if (hit.collider.tag == "Ground") {
                PV.RPC("RPC_Teleport", RpcTarget.All, newPos, teleportDuration); 
                GetComponentInParent<PlayerController>().UI.teliBtn.SetActive(false);

                specialUse = false; 
                lastSpecial = Time.time; 
            }
        }
    }

    [PunRPC]
    void RPC_Teleport(Vector3 pos, float duration) {
        StartCoroutine(Teleport(pos, duration)); 
    }

    IEnumerator Teleport(Vector3 pos, float duration) {
        GameObject _bomb = Instantiate(bomb, transform.position + new Vector3(0f, 2f, 0f), bomb.transform.rotation); 
        _bomb.GetComponent<BlackHoleBomb>().Setup(PV, this, holeRadius); 
        if (PV.IsMine) GetComponentInParent<PlayerController>().transform.position = pos; 
        ToggleEnabled(false); 

        yield return new WaitForSeconds(duration); 

        ToggleEnabled(true); 
    }

    void ToggleEnabled(bool toggle) {
        MeshRenderer[] meshes = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mesh in meshes) mesh.enabled = toggle;

        Collider[] cols = GetComponentsInChildren<Collider>();
        foreach (Collider col in cols) col.enabled = toggle; 

        GetComponentInParent<Rigidbody>().isKinematic = !toggle; 

        GetComponentInParent<PlayerController>().enabled = toggle; 
    }

    public override void Ultimate() {
        Debug.Log("Used ultimate");
    }

    public override void UltimateClicked() { }

    public override void UltimateHit() {
        Debug.Log("ultimate hit");
    }

    public void Update() {
        base.Update();
        if (!PV.IsMine) return;

        if (rb.velocity.magnitude >= 0.25f) {
            SetAnimProp("walking", true);
        } else {
            SetAnimProp("walking", false);
        }
    }

    private void FixedUpdate() {
        if (!PV.IsMine) return; 

        if (secondaryUse) {
            // PoisonPlayers(true); 

            foreach (PlayerController _player in decayingPlayers) {
                _player.playerManager.TakeDamage(Mathf.Lerp(radioActiveStart, radioactiveEnd, (1f - Vector3.Distance(transform.position, _player.transform.position) / radioactiveRadius)) * Time.fixedDeltaTime, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
            }
        }

        foreach (PlayerController _player in decayingPlayers2) {
            _player.playerManager.TakeDamage(Mathf.Lerp(radioActiveStart, radioactiveEnd, (1f - Vector3.Distance(transform.position, _player.transform.position) / radioactiveRadius)) * Time.fixedDeltaTime, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID);
        }
    }

    /*void PoisonPlayers(bool toggle) {
        PlayerController[] _players = FindObjectsOfType<PlayerController>();

        foreach (PlayerController _player in _players) {
            if (_player != GetComponentInParent<PlayerController>()) {
                float dist = Vector3.Distance(GetComponentInParent<PlayerController>().transform.position, _player.transform.position); 
                if (toggle) {
                    if (dist <= radioactiveRadius) {
                        // _player.playerManager.TakeDamage(Mathf.Lerp(radioActiveStart, radioactiveEnd, (1f - dist / radioactiveRadius)) * Time.fixedDeltaTime); 
                        _player.decayPerSecs[1] = Mathf.Lerp(radioActiveStart, radioactiveEnd, (1f - dist / radioactiveRadius)); 
                        // Debug.Log(dist + " " + Mathf.Lerp(radioActiveStart, radioactiveEnd, (1f - dist / radioactiveRadius)) + " " +  Time.fixedDeltaTime); 
                        // _player.ToggleDecay(true); 
                        _player.decays[1] = 1; 
                    // } else _player.ToggleDecay(false); 
                    } else _player.decays[1] = 0; 
                } else {
                    // _player.ToggleDecay(false); 
                    _player.decays[1] = 0; 
                }
            }
        }
    }*/

    public void SetAnimProp(string propertyName, bool value) {
        if (animator.GetBool(propertyName) == value) return; 
        PV.RPC("RPC_SetAnimProp", RpcTarget.All, propertyName, value); 
    }

    [PunRPC]
    void RPC_SetAnimProp(string propertyName, bool value) {
        animator.SetBool(propertyName, value);
    }
}
