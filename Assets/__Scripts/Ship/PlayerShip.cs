#define DEBUG_PlayerShip_RespawnNotifications

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerShip : MonoBehaviour
{
    
    static private PlayerShip   _S;
    static public PlayerShip    S
    {
        get
        {
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogWarning("Second attempt to set PlayerShip singleton _S.");
            }
            _S = value;
        }
    }

    static public int   JUMPS = 3;
    static public float	LAST_COLLISION = -1000;
    static public float COLLISION_DELAY = 1;


    [Header("Set in Inspector")]
    public float        shipSpeed = 10f;
    public GameObject   bulletPrefab;
    [Tooltip("The amount of time that the ship disappears during jump/teleport.")]
    public float        respawnDelay = 2;
    [Tooltip("The number of Jumps that the ship start the game with.")]
    public int          startingJumps = 3;
    [Tooltip("The particle effect to show when the ship disappears for a Jump.")]
    public GameObject   jumpDisappearParticlesPrefab;
	[Tooltip("The particle effect to show when the ship reappears from a Jump.")]
    public GameObject	jumpAppearParticlesPrefab;
    Rigidbody           rigid;
    PlayerInput playerInput;
    Vector3 movement;
    Vector3 mousePostion;

    void Awake()
    {
        S = this;

        JUMPS = startingJumps;
        
        // NOTE: We don't need to check whether or not rigid is null because of [RequireComponent()] above
        rigid = GetComponent<Rigidbody>();
        playerInput = new PlayerInput();
        
    }

    private void OnEnable()
    {
        playerInput.Enable();
        playerInput.Player.Fire.performed += FireBullet;
    }

    private void FireBullet(InputAction.CallbackContext obj)
    {
        mousePostion = (playerInput.Player.MousePostion.ReadValue<Vector2>());
        TurretPointAtMouse turret = GetComponentInChildren<TurretPointAtMouse>();
        turret.mousePoint3D = Camera.main.ScreenToWorldPoint(mousePostion + Vector3.back * Camera.main.transform.position.z);
        float f = obj.ReadValue<float>();
        if ( f>.1f&&!AsteraX.PAUSED)
        {
            Fire();
            Debug.Log("fire");
      
        }
    }

    private void OnDisable()
    {
        playerInput.Disable();
    }
   
    void Update()
    {
        // Using Horizontal and Vertical axes to set velocity

        movement = playerInput.Player.Move.ReadValue<Vector2>();
        if (movement.magnitude > 1)
        {
            // Avoid speed multiplying by 1.414 when moving at a diagonal
            movement.Normalize();
        }

        rigid.velocity = movement * shipSpeed;

        

    }


    void Fire()
    {
        // Get direction to the mouse
        Vector3 mPos = mousePostion;
        mPos.z = -Camera.main.transform.position.z;
        Vector3 pos = Camera.main.ScreenToWorldPoint(mPos);
        // Instantiate the Bullet and set its direction
        GameObject go = Instantiate<GameObject>(bulletPrefab);
        go.transform.position = transform.position;
        go.transform.LookAt(pos);
    }

    void OnCollisionEnter(Collision collision)
    {
        Asteroid a = collision.gameObject.GetComponent<Asteroid>();
        if (a == null) {
            return;
        }

        if (Time.time < LAST_COLLISION + COLLISION_DELAY) {
            return;
        } else {
            LAST_COLLISION = Time.time;
        }

        JUMPS--;
        if (JUMPS < 0) {
            gameObject.SetActive(false);
            AsteraX.GameOver();
            return;
        }

        // Respawn in a new location
        Respawn();
    }

    void Respawn() {
#if DEBUG_PlayerShip_RespawnNotifications
        Debug.Log("PlayerShip:Respawn()");
#endif
        StartCoroutine(AsteraX.FindRespawnPointCoroutine(transform.position, RespawnCallback)); 

        OffScreenWrapper wrapper = GetComponent<OffScreenWrapper>();
        if (wrapper != null) {
            wrapper.enabled = false;
        }
        transform.position = new Vector3(10000,10000,0);
    }

    void RespawnCallback(Vector3 newPos) {
#if DEBUG_PlayerShip_RespawnNotifications
        Debug.Log("PlayerShip:RespawnCallback( "+newPos+" )");
#endif
        transform.position = newPos;

        OffScreenWrapper wrapper = GetComponent<OffScreenWrapper>();
        if (wrapper != null) {
            wrapper.enabled = true;
        }
    }


    static public float MAX_SPEED
    {
        get
        {
            return S.shipSpeed;
        }
    }
    
	static public Vector3 POSITION
    {
        get
        {
            return S.transform.position;
        }
    }

    static public float RESPAWN_DELAY
    {
        get
        {
            return S.respawnDelay;
        }
    }

    static public GameObject DISAPPEAR_PARTICLES
    {
        get 
        {
            return S.jumpDisappearParticlesPrefab;
        }
    }

    static public GameObject APPEAR_PARTICLES
    {
        get 
        {
            return S.jumpAppearParticlesPrefab;
        }
    }
}
