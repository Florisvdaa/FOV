using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float speed = 6f;
    [SerializeField] private GameObject playerVisual;
    private Rigidbody rb;
    private Camera mainCam;

    private Vector3 velocity;
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCam = Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = mainCam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, mainCam.transform.position.y));

        playerVisual.transform.LookAt(mousePos + Vector3.up * transform.position.y);

        velocity = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized * speed;
    }

    public Transform GetPlayerVisualTransform()
    {
        return playerVisual.transform;
    }

    private void FixedUpdate()
    {
        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);
    }
}
