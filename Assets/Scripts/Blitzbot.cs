using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun; 

public class Blitzbot : Hero
{
    Rigidbody rb; 

    [SerializeField] GameObject primaryBulletPrefab, primaryTip1, primaryTip2, protectiveBarrier; 

    [SerializeField] float primaryDamage = 5f, stimMoveBoostFactor = 1.5f, stimFireRateBoostFactor = 1.5f; 
    
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
        if (Time.time - lastPrimary > 1f / primaryRate) {
            Debug.Log("Used primary"); 

            RaycastHit? hit = GetComponentInParent<RayCaster>().Shoot(eyeRaycastPoint);
            if (hit.HasValue) {
                Vector3 target = hit.Value.point; 
                if (hit.Value.collider.tag != "Player") target = GetComponentInParent<RaycastCapsule>().Shoot(target); 
                PV.RPC("RPC_ShootBullets", RpcTarget.All, target); 
            }

            lastPrimary = Time.time; 
        }
    }

    [PunRPC]
    void RPC_ShootBullets(Vector3 target) {
        GameObject bulletGO = Instantiate(primaryBulletPrefab, primaryTip1.transform.position, primaryBulletPrefab.transform.rotation); 
        bulletGO.GetComponent<SimpleProjectile>().Setup(transform.root.GetComponent<PhotonView>(), GetComponentInParent<PlayerController>().transform.forward, target, this); 
        bulletGO.GetComponent<Bullet>().damage = primaryDamage; 

        bulletGO = Instantiate(primaryBulletPrefab, primaryTip2.transform.position, primaryBulletPrefab.transform.rotation);
        bulletGO.GetComponent<SimpleProjectile>().Setup(transform.root.GetComponent<PhotonView>(), GetComponentInParent<PlayerController>().transform.forward, target, this); 
        bulletGO.GetComponent<Bullet>().damage = primaryDamage; 
    }

    public override void PrimaryClicked() { }

    public override void PrimaryHit(Collision col, float dist) {
        col.collider.GetComponentInParent<PlayerController>().Tase(0.5f); 
        col.collider.GetComponentInParent<PlayerController>().playerManager.TakeDamage(primaryDamage, GetComponentInParent<PlayerController>().playerManager.GetComponent<PhotonView>().ViewID); 
    }

    public override void Secondary() {
        if (Time.time - lastSecondary > 1f / secondaryRate && !protectiveBarrier.activeSelf) {
            Debug.Log("Used secondary");

            PV.RPC("LaunchBarrier", RpcTarget.All, secondaryDuration); 

            lastSecondary = Time.time; 
        }
    }

    [PunRPC]
    public void LaunchBarrier(float duration) {
        StartCoroutine(ProtectiveBarrier(duration)); 
    }

    IEnumerator ProtectiveBarrier(float duration) {
        protectiveBarrier.SetActive(true); 
        protectiveBarrier.GetComponent<ProtectiveBarrier>().playerTeam = GetComponentInParent<PlayerController>().playerManager.playerTeam; 
        secondaryUse = true; 
        SetAnimProp("shield", true); 

        yield return new WaitForSeconds(duration);

        SetAnimProp("shield", false); 
        protectiveBarrier.SetActive(false); 
        lastSecondary = Time.time; 
        secondaryUse = false; 
    }

    public override void SecondaryClicked() { }

    public override void SecondaryHit() {
        Debug.Log("secondary hit"); 
    }

    public override void Special() {
        if (Time.time - lastSpecial > 1f / specialRate) {
            Debug.Log("Used special");

            StartCoroutine(StimBoost(specialDuration)); 

            lastSpecial = Time.time; 
        }
    }

    public override void SpecialClicked() { }

    public override void SpecialHit() {
        Debug.Log("special hit"); 
    }

    IEnumerator StimBoost(float duration) {
        moveSpeed *= stimMoveBoostFactor;
        primaryRate *= stimFireRateBoostFactor;
        secondaryRate *= stimFireRateBoostFactor;
        specialUse = true; 

        yield return new WaitForSeconds(duration);

        moveSpeed /= stimMoveBoostFactor;
        primaryRate /= stimFireRateBoostFactor;
        secondaryRate /= stimFireRateBoostFactor;
        specialUse = false; 
        lastSpecial = Time.time; 
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

    public void SetAnimProp(string propertyName, bool value) {
        PV.RPC("RPC_SetAnimProp", RpcTarget.All, propertyName, value); 
    }

    [PunRPC]
    void RPC_SetAnimProp(string propertyName, bool value) {
        animator.SetBool(propertyName, value); 
    }
}
