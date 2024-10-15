using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Chimera : Hero {
    Rigidbody rb;

    [SerializeField] float primaryDamageBegin = 2f, primaryDamagePerSecond = 2f, primaryInfectDuration = 5f, healAmount = 75f; 

    [SerializeField] GameObject primaryBulletPrefab, primaryTip, player;
    GameObject healthBar; 

    [SerializeField] List<PlayerController> decayingPlayers = new List<PlayerController>();

    [SerializeField] Material invisibleMatLocal, invisibleMatRemote;

    PlayerController mainController; 

    // Start is called before the first frame update
    public void Awake() {
        rb = GetComponentInParent<Rigidbody>();
        PV = GetComponent<PhotonView>();
        mainController = GetComponentInParent<PlayerController>(); 
        healthBar = mainController.healthBarFill.GetComponentInParent<Canvas>().gameObject; 
    }

    public override void Move(Vector3 movement) {
        rb.AddForce(movement * (moveSpeed * Time.fixedDeltaTime));
    }

    public override void Jump() {
        rb.AddForce(Vector3.up * 100f, ForceMode.VelocityChange);
    }

    public override void Primary() {
        if (Time.time - lastPrimary > 1f / primaryRate && !secondaryUse) {
            Debug.Log("Used primary");

            RaycastHit? hit = GetComponentInParent<RayCaster>().Shoot(eyeRaycastPoint);
            if (hit.HasValue) {
                Vector3 target = hit.Value.point;
                if (hit.Value.collider.tag != "Player") target = GetComponentInParent<RaycastCapsule>().Shoot(target);
                PV.RPC("RPC_ShootBullet", RpcTarget.All, target);
            }

            lastPrimary = Time.time; 
        }
    }

    [PunRPC]
    void RPC_ShootBullet(Vector3 target) {
        GameObject bulletGO = Instantiate(primaryBulletPrefab, primaryTip.transform.position, primaryBulletPrefab.transform.rotation);
        bulletGO.GetComponent<SimpleProjectile>().Setup(transform.root.GetComponent<PhotonView>(), GetComponentInParent<PlayerController>().transform.forward, target, this); 
        bulletGO.GetComponent<Bullet>().damage = primaryDamageBegin; 
    }

    public override void PrimaryClicked() { }

    public override void PrimaryHit(Collision col, float dist) {
        col.collider.GetComponentInParent<PlayerController>().playerManager.TakeDamage(primaryDamageBegin, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
        // StartCoroutine(PoisonPlayer(col.collider.GetComponentInParent<PlayerController>(), primaryInfectDuration)); 
        PV.RPC("RPC_PoisonPlayer", RpcTarget.All, col.collider.GetComponentInParent<PlayerController>().GetComponent<PhotonView>().ViewID, primaryInfectDuration); 
    }

    [PunRPC]
    void RPC_PoisonPlayer(int viewID, float duration) {
        StartCoroutine(PoisonPlayer(PhotonView.Find(viewID).GetComponent<PlayerController>(), duration)); 
    }

    IEnumerator PoisonPlayer(PlayerController _player, float duration) {
        ++_player.decays[2];
        decayingPlayers.Add(_player); 

        yield return new WaitForSeconds(duration); 

        decayingPlayers.Remove(_player); 
        --_player.decays[2]; 
    }

    public override void Secondary() {
        if (Time.time - lastSecondary > 1f / secondaryRate && !secondaryUse) {
            Debug.Log("Used secondary");

            PV.RPC("RPC_Invisible", RpcTarget.All, secondaryDuration); 

            lastSecondary = Time.time; 
        }
    }

    [PunRPC]
    void RPC_Invisible(float duration) {
        StartCoroutine(Invisible(duration)); 
    }

    IEnumerator Invisible(float duration) {
        healthBar.SetActive(false); 
        if (PV.IsMine) StartCoroutine(SetMatsRecursively(player.transform, duration, invisibleMatLocal)); 
        else StartCoroutine(SetMatsRecursively(player.transform, duration, invisibleMatRemote)); 
        secondaryUse = true; 

        yield return new WaitForSeconds(duration);

        secondaryUse = false;
        healthBar.SetActive(true); 

        lastSecondary = Time.time; 
    }

    public override void SecondaryClicked() { }

    public override void SecondaryHit() {
        Debug.Log("secondary hit"); 
    }

    public override void Special() {
        if (Time.time - lastSpecial > 1f / specialRate && !secondaryUse) {
            Debug.Log("Used special");

            StartCoroutine(Heal(specialDuration, 0.02f, healAmount)); 

            lastSpecial = Time.time; 
        }
    }

    IEnumerator Heal(float duration, float interval, float totalHeal) {
        mainController.enabled = false;
        specialUse = true; 

        for (float i = 0f; i <= duration; i += interval) {
            mainController.playerManager.HealHP(totalHeal / (duration / interval));
            yield return new WaitForSeconds(interval); 
        }

        specialUse = false; 
        mainController.enabled = true;

        lastSpecial = Time.time; 
    }

    public override void SpecialClicked() { }

    public override void SpecialHit() {
        Debug.Log("special hit");
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
            SetAnimProp("running", true);
        } else {
            SetAnimProp("running", false);
        }
    }

    public void FixedUpdate() {
        if (!PV.IsMine) return; 

        foreach (PlayerController controller in decayingPlayers) {
            controller.playerManager.TakeDamage(primaryDamagePerSecond * Time.fixedDeltaTime, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
        }
    }

    public void SetAnimProp(string propertyName, bool value) {
        PV.RPC("RPC_SetAnimProp", RpcTarget.All, propertyName, value);
    }

    [PunRPC]
    void RPC_SetAnimProp(string propertyName, bool value) {
        animator.SetBool(propertyName, value);
    }

    IEnumerator SetMatsRecursively(Transform parent, float duration, Material mat) {
        Material[] mats = new Material[0]; 
        Debug.Log(parent.name); 
        if (parent.gameObject.GetComponent<SkinnedMeshRenderer>()) {
            // Debug.Log(parent.name); 
            mats = parent.gameObject.GetComponent<SkinnedMeshRenderer>().materials;
            Material[] _mats = new Material[mats.Length];
            for (int i = 0; i < _mats.Length; ++i) _mats[i] = mat;
            parent.gameObject.GetComponent<SkinnedMeshRenderer>().materials = _mats;
            parent.gameObject.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off; 
        }

        foreach (Transform child in parent) {
            StartCoroutine(SetMatsRecursively(child, duration, mat)); 
        }

        yield return new WaitForSeconds(duration);

        if (parent.gameObject.GetComponent<SkinnedMeshRenderer>()) {
            parent.gameObject.GetComponent<SkinnedMeshRenderer>().materials = mats; 
            parent.gameObject.GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On; 
        }
    }
}
