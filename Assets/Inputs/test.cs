using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class test : MonoBehaviour
{
    PlayerInput playerInput;


    private void OnEnable()
    {
        playerInput.Enable();
    }
    private void OnDisable()
    {
        playerInput.Disable();
    }

    private void Awake()
    {
        playerInput = new PlayerInput();
    }
    private void Start()
    {
        playerInput.Player.Fire.performed += FireB;
    }

    private void FireB(InputAction.CallbackContext obj)
    {
        Debug.Log("fire");
        Vector3 vec = Camera.main.ScreenToWorldPoint(playerInput.Player.MousePostion.ReadValue<Vector2>());
        vec.z = 0;
        transform.position = vec;
    }

    private void Update()
    {
        
    }
}
