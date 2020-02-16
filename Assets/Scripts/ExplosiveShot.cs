using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveShot : MonoBehaviour {

    public GameObject explosionArea;
    public float shotExplosionRadius = 5;
    public float explosionDurationInSeconds = 1;

    private void OnCollisionEnter(Collision collision)
    {
        GameObject shotExplosion = Instantiate(explosionArea, transform.position, Quaternion.identity);
        shotExplosion.GetComponent<SphereCollider>().radius = shotExplosionRadius;
        StartCoroutine(explosionDuration());
        explosionDuration();
        Destroy(shotExplosion);
        Destroy(this);
    }

    IEnumerator explosionDuration()
    {
        yield return new WaitForSeconds(explosionDurationInSeconds);
    }
}
