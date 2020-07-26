using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * PLEASE NOTE: This and PlayerControl were one of the first scripts made in this
 *  project and are not designed well in many ways. If you are reading this, it still
 *  hasn't been refactored and may have aspects that seem inefficient/redundant!!
 */
//This script stores all player's control events and methods, among other attributes, like animators
//Acts as the main script for the player, as control is the most important. Coupled with PlayerStats and PlayerInventory.
[RequireComponent(typeof(FakeHeightObject))]
public class PlayerControl : MonoBehaviour, IFakeHeight
{ 
    [SerializeField] private bool hasWeapons;

    //Components
    [SerializeField] private Animator pAnimator;

    [SerializeField] private SpriteRenderer pSpriteRenderer;

    [SerializeField] private Transform hands;
    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;

    [SerializeField] private Weapon currWeapon;

    [SerializeField] private Transform weaponPosition;
    [SerializeField] private Transform aimTransform;
    [SerializeField] private Rigidbody2D rb2d;

    [SerializeField] private Vector2 screenMousePosition;

    [SerializeField] private Vector2 movement;
    
    //References to separate player scripts:
    //This player's current inventory and stats
    [SerializeField] private PlayerInventory pInventory;
    [SerializeField] private PlayerStats pStats;

    //Crosshair reference to move it during top down play.
    [SerializeField] private Crosshair crosshair;

    public PlayerActions pActions;

    private Camera mainCamera;

    //Events
    public event Action<IInteractable> playerInteraction;

    public Collider2D MainCollider { get; private set; }
    public Vector2 Movement { get => movement; }
    public FakeHeightObject FakeHeight { get; set; }
    public Vector2 LastValidPosition { get; private set; }

    /**
     * UNITY ENGINE FUNCTIONS RELATING TO SCENE
     */
    private void Awake()
    {
        //TODO: When creating camera functionality, set this to static instance of current main camera for performance.
        mainCamera = Camera.main;

        //Assign references to important variables and objects w/o inspector.
        pInventory = GetComponentInChildren<PlayerInventory>();
        pStats = GetComponentInChildren<PlayerStats>();

        //TODO: Instantiate new crosshair instead of finding one in the scene; make it UI based later...
        crosshair = FindObjectOfType<Crosshair>();
        pActions = new PlayerActions();

        hasWeapons = (pInventory.GetWeaponCount() > 0);
        pInventory.NewWeaponEquipped = false;

        //Set weapon and hand positions to defaults positions on Player prefab.
        hands = gameObject.transform.Find("hands");
        leftHand = hands.Find("leftHand").gameObject;
        rightHand = hands.Find("rightHand").gameObject;

        pSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        MainCollider = GetComponent<Collider2D>();

        //Aim transfrom -> position for aimed weapon to rotate around.
        //weaponPosition -> child position of aimTransform where current weapon is instantiated.
        aimTransform = gameObject.transform.Find("Aim");
        weaponPosition = aimTransform.Find("gunSlot");

        rb2d = gameObject.transform.GetComponent<Rigidbody2D>();

        //Subscribe respective variables or functions to PlayerActions control events.
        pActions.MainActions.Aim.performed += ctx => screenMousePosition = ctx.ReadValue<Vector2>();
        pActions.MainActions.Interact.performed += ctx => CheckInteractables();
        pActions.MainActions.SwapWeapon.performed += ctx => CheckWeaponSwap();
        pActions.MainActions.Attack.performed += ctx => WeaponAttack();
        //pActions.MainActions.Move.performed += ctx => Move();

    }
    private void Update()
    {
        hasWeapons = (pInventory.GetWeaponCount() > 0);

        //Set mouse position in 3d gameworld to mouse position on screen, then set z to zero.
        Vector3 worldMousePosition = mainCamera.ScreenToWorldPoint(screenMousePosition);
        worldMousePosition.z = 0f;


        //If crosshair found in scene, set its position to worldMousePosition.
        if(crosshair != null)
            crosshair.transform.position = worldMousePosition;

        aimTransform.eulerAngles = AimTowardsCrosshair(worldMousePosition);

        if(hasWeapons)
        {
            if (pInventory.NewWeaponEquipped)
            {
                EquipNewWeapon();

                pInventory.NewWeaponEquipped = false;
            }
        }
        else
        {
            currWeapon = null;
            if(!leftHand.activeSelf)
                leftHand.gameObject.SetActive(true);
            if(!rightHand.activeSelf)
                rightHand.gameObject.SetActive(true);
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    //Called when object is ENABLED in scene.
    private void OnEnable()
    {
        //Enable player actions for input
        pActions.MainActions.Enable();
    }

    //Called when object is DISABLED in scene.
    private void OnDisable()
    {
        //Disable player actions to disallow input.
        pActions.MainActions.Disable();
    }

    /**
     * HELPER METHODS; MISCELLANEOUS
     */

    //Called when weapon is swapped or new weapon is picked up.
    private void EquipNewWeapon()
    {
        currWeapon = pInventory.GetCurrWeapon();
        currWeapon.transform.parent = weaponPosition.transform;
        currWeapon.transform.position = weaponPosition.position;
        Vector3 weaponRotation = weaponPosition.transform.eulerAngles;
        currWeapon.transform.eulerAngles = weaponRotation;

        //If pointing left, set weapon's y-scale to positive to avoid scale issues from aimTransform's scale.
        if(weaponRotation.z > 90 || weaponRotation.z < -90)
        {
            currWeapon.transform.localScale = new Vector3(currWeapon.transform.localScale.x, Mathf.Abs(currWeapon.transform.localScale.y), currWeapon.transform.localScale.z);
        }
        //Move hand sprites to current weapon pivot after weapon is in default position.
        //If no pivot exits on current weapon, set hand to default position on player.
        rightHand.SetActive(currWeapon.RightHandPivot == null);
        leftHand.SetActive(currWeapon.LeftHandPivot == null);

        currWeapon.Equip(this);
    }
    
    /**
     * PLAYER INPUT METHODS
     * 
     * The following functions perform the actions in PlayerActions.
     * Subscription of these functions to input events from PlayerActions is done in 
     * the Player's animator state machine.
     */

        
    //Up, Down, Left, and Right Movement.
    protected virtual void Move()
    {
        //Read vector 2 from PlayerActions input system as normalized vector.
        movement = pActions.MainActions.Move.ReadValue<Vector2>().normalized;
        rb2d.MovePosition(rb2d.position + movement * pStats.ChangeMoveSpeed(0) * Time.deltaTime);
    }
    
    //Cycles through weapons in order in weapon inventory
    //MAY LATER have functionality for 1-4 keys and/or scroll wheel equipping.
    protected virtual void CheckWeaponSwap()
    {
        currWeapon.Unequip();
        pInventory.SwapWeapon();
        EquipNewWeapon();
    }

    //Checks for player input to do current nearby interaction.
    //If input matches, fire playerInteraction event.
    protected virtual void CheckInteractables()
    {
        playerInteraction(pInventory.GetCurrInteractable());
    }

    //Returns Vector3 representing euler angle of 2D object (player's gun)
    //that will point towards the mouse cursor (crosshair) position.
    protected virtual Vector3 AimTowardsCrosshair(Vector3 crosshairPos)
    {
        //Get angle to aim towards by getting normalized vector from
        //current transform to crosshairPos. Then, convert direction vector to
        //an angle by taking Tan(y/x) of direction and converting to degrees.
        Vector3 aimDirection = (crosshairPos - transform.position).normalized;
        float angle = Mathf.Atan2(aimDirection.y, aimDirection.x) * Mathf.Rad2Deg;

        //Flip player's sprite & aim child transform if angle to point to is left
        Vector3 flipAimScale = Vector3.one;
        Vector3 flipHandScale = Vector3.one;
        if (angle > 90 || angle < -90)
        {
            flipAimScale.y = -1f;
            flipHandScale.x = -1f;
            if(!pSpriteRenderer.flipX)
                pSpriteRenderer.flipX = true;
        }
        else
        {
            flipAimScale.y = +1f;
            flipHandScale.x = +1f;
            if(pSpriteRenderer.flipX)
                pSpriteRenderer.flipX = false;
        }

        //Flip aim and hand's scales separately, since hands don't rotate with aim transform.
        aimTransform.localScale = flipAimScale;
        hands.localScale = flipHandScale;
        
        return new Vector3(0, 0, angle);
    }

    //If player has a weapon, call its firing method and
    //reduce ammo count by returned ammo specific weapon expends
    protected virtual void WeaponAttack()
    {
        if(currWeapon != null)
        {
            float shotSpeedMultiplier = pStats.ChangeShotSpeed(0f);
            currWeapon.FireWeapon(shotSpeedMultiplier, crosshair.transform.position);
        }
    }

    public PlayerStats GetPlayerStats()
    {
        return pStats;
    }

    /**
     * TRIGGER METHODS FROM IFAKEHEIGHT.
     */
    public void OnCollisionEnter2D(Collision2D collision)
    {
        
    }

    public void OnCollisionExit2D(Collision2D collision)
    {

    }

    public void OnCollisionStay2D(Collision2D collision)
    {
        
    }

    public void OnFakeHeightTriggerEnter(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<IInteractable>() != null)
        {
            IInteractable interactable = collider.gameObject.GetComponent<IInteractable>();
            Debug.Log("Player interaction. Press E to use.");
            pInventory.AddInteractable(interactable);
        }
    }

    public void OnFakeHeightTriggerExit(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<IInteractable>() != null)
        {
            IInteractable interactable = collider.gameObject.GetComponent<IInteractable>();
            if (interactable is Lootable && interactable == pInventory.GetCurrInteractable())
            {
                if (TooltipPopup.GetTooltipObject().activeSelf)
                    TooltipPopup.HideInfo();
            }
            pInventory.RemoveInteractable(interactable);
        }
    }

    public void OnFakeHeightTriggerStay(Collider2D collider)
    {
        if (collider.gameObject.GetComponent<IInteractable>() != null)
        {
            IInteractable interactable = collider.gameObject.GetComponent<IInteractable>();
            if (interactable == pInventory.GetCurrInteractable())
            {
                if (interactable is Lootable && !TooltipPopup.GetTooltipObject().activeSelf)
                {
                    TooltipPopup.DisplayInfo((Lootable)interactable);
                }
            }
        }
    }
}
