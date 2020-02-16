using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionDamage : MonoBehaviour {

    public float baseDamage = 10f;
    public float reductionByDistance = 1; // how much the damage is reduced per unit of distance
    public float reductionByVolume = 1; // how much the damage is reduced per unit of volume of obstacles between the explosion and player/enemy

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" || other.tag == "Enemy")
        {   float damageReductionByDistance = reductionByDistance * Vector3.Distance(transform.position, other.transform.position);
            float damageReductionByVolume = 0;
            RaycastHit[] hits;
            hits = Physics.RaycastAll(transform.position, other.transform.position - transform.position, GetComponent<SphereCollider>().radius);
            foreach (RaycastHit hit in hits)
            {
                Vector3 hitObjectSize = hit.collider.gameObject.transform.localScale;
                damageReductionByVolume += reductionByVolume * Mathf.Clamp(((hitObjectSize.x + hitObjectSize.z) / 2 + Mathf.Clamp(hitObjectSize.y, 0, 2)), 0, 5); // need to refine logic here, 0,2 and 0,5 are arbitrary
            }
            // other takes damage: "Math.Clamp(baseDamage - damageReductionByDistance - damageReductionByVolume, 0, baseDamage)"
        }
    }
}
