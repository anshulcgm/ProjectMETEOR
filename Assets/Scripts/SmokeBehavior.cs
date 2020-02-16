using UnityEngine;
using HoloToolkit.Unity.InputModule;

public class SmokeBehavior : MonoBehaviour
{
    public GameObject smokeEffect;

    private Vector3 initialPosition;
    private Vector3 destination;

    private Rigidbody rb;

    public float speed;

    private void Awake()
    {
        initialPosition = Camera.main.transform.position;
        destination = GazeManager.Instance.HitPosition;
        rb = GetComponent<Rigidbody>();
        rb.velocity = (destination - initialPosition).normalized * speed;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Instantiate(smokeEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
