using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Enum representing current shadow type. If one isn't specified, it
//will be dynamic, which is a grayed-out and reduced y-scale (~0.65)
//of the object's current sprite on it's attached renderer (unless a special
//shadow sprite is given, since that will simply be used instead).
//Small - XLarge are also the end of the name of their sprite asset!
public enum ShadowType
{
    Dynamic = 0,
    Small = 1,
    Medium = 2,
    Large = 3,
    XLarge = 4
}


/**
 * This component can be attached to any GameObject to give it the illusion of
 * a fake z-axis towards the camera using a specified height. This height
 * will use/create a shadow child object (that is just a sprite renderer)
 * and to show the true y-axis position of the object in the air.
 * 
 * Any GameObject that uses this component MUST NOT use the built in Unity Monobehavior functions
 * for TRIGGERS! The fake height illusion will cause incorrect results from those.
 * YOU MUST make a class/component use the IFakeHeight interface and implement its trigger
 * methods to properly have the GameObject trigger things relative to the shadow.
 */
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer), typeof(Collider2D))]
public class FakeHeightObject : MonoBehaviour
{
    public const float shadowSpriteYScale = 0.75f;
    public const float shadowColliderReduction = 0.8f;
    public const string spritePath = "Sprites/Shadows/DefaultShadow";

    [SerializeField] private ShadowType shadowType;
    [SerializeField] private bool zeroShadowRotation = true;
    [SerializeField] private bool triggerIgnoreShadow;

    [SerializeField] private byte shadowOpacity = 40;
    [SerializeField] private bool useCustomOpacity;
    [SerializeField] private float height = 0f;
    [SerializeField] private Sprite customShadowSprite;

    private ContactFilter2D shadowContactFilter;
    private Collider2D mainObjCollider;             //<- used to check if other shadows collide w/ this.
    private SpriteRenderer mainSpriteRenderer;
    private Rigidbody2D shadowRB;
    private bool initialized = false;

    //Color of shadow as black with correct opacity. Last two characters 
    //of parsed string = opacity value of shadow.
    public Color GetShadowColor
    {
        get
        {
            Color retCol;
            byte opacity = 40;
            if (useCustomOpacity)
                opacity = shadowOpacity;
            ColorUtility.TryParseHtmlString("#000000" + opacity.ToString(), out retCol);
            return retCol;
        } 
    }

    public bool TriggerIgnoreShadow { get => triggerIgnoreShadow; }
    public Vector3 ShadowPosition { get => ChildShadow.transform.position; }
    public GameObject ChildShadow { get; private set; }
    public Rigidbody2D ShadowRB { get; private set; }
    public SpriteRenderer ShadowRenderer { get; private set; }
    public CapsuleCollider2D ShadowCollider { get; private set; }

    public IFakeHeight[] FakeHeightComponents { get; private set; }
    public HashSet<Collider2D> CollidingObjects { get; private set; }
    public HashSet<Collider2D> ValidTriggers { get; private set; }
    public bool IsGrounded { get => !(height > 0); }

    //TODO: 
    //---> ADD FUNCTIONALITY TO MOVE THE OBJECT UP IN Y-AXIS A CERTAIN AMOUNT
    //WHILE KEEPING THE SHADOW FROM FOLLOWING THAT MOVEMENT EACH FRAME. SHOULD JUST BE
    //A METHOD TO MOVE THE OBJECT UP A CERTAIN AMOUNT (GIVEN AS A FLOAT PARAMETER)
    //ANY OBJECT WITH THIS SCRIPT SHOULD CONTROL THE UP AND DOWN MOVEMENT ON ITS OWN EACH FRAME
    //MAYBE ADD SOME FUNCTIONALITY FOR THIS IN THE IFAKEHEIGHT INTERFACE.

    private void Awake()
    {
        //Initialize at zero height
        Initialize();
        CollidingObjects = new HashSet<Collider2D>();
        ValidTriggers = new HashSet<Collider2D>();
        shadowContactFilter.NoFilter();

        mainObjCollider = GetComponent<Collider2D>();
        mainSpriteRenderer = GetComponent<SpriteRenderer>();

        //Initialize all fake height varaibles in components.
        FakeHeightComponents = GetComponents<IFakeHeight>();
        for(int i = 0; i < FakeHeightComponents.Length; i++)
        {
            FakeHeightComponents[i].FakeHeight = this;
        }

        //Create shadow object and initialize its sprite renderer
        ChildShadow = new GameObject(gameObject.name + " shadow");
        ShadowRB = ChildShadow.AddComponent<Rigidbody2D>();
        ShadowRenderer = ChildShadow.AddComponent<SpriteRenderer>();
        ShadowCollider = ChildShadow.AddComponent<CapsuleCollider2D>();
        ShadowRB.bodyType = RigidbodyType2D.Kinematic;

        ShadowRB.isKinematic = true;
        if (zeroShadowRotation) ShadowRB.freezeRotation = true;

        if (customShadowSprite != null)
        {
            ShadowRenderer.sprite = customShadowSprite;
            shadowType = ShadowType.Dynamic;
        }

        if (shadowType == ShadowType.Dynamic)
        {
            if (ShadowRenderer.sprite == null)
                ShadowRenderer.sprite = mainSpriteRenderer.sprite;

            Vector3 scale = transform.localScale;
            transform.localScale = new Vector3(scale.x, shadowSpriteYScale * scale.y, scale.z);
        }
        else
        {
            //Load circular shadow sprite since shadowType is not dynamic
            string pathSuffix = Enum.GetName(typeof(ShadowType), shadowType);
            ShadowRenderer.sprite = Resources.Load<Sprite>(spritePath + pathSuffix) as Sprite;
        }

        ShadowRenderer.color = GetShadowColor;
        ShadowRenderer.sortingOrder = mainSpriteRenderer.sortingOrder - 1;

        ResetShadowPosition();
        ResetShadowCollider();
    }

    private void FixedUpdate()
    {
        //Get update contacts for shadow.
        List<Collider2D> overlapColliders = new List<Collider2D>();

        CollidingObjects.Clear();
        Physics2D.OverlapCollider(ShadowCollider, shadowContactFilter.NoFilter(), overlapColliders);

        for(byte i = 0; i < overlapColliders.Count; i++)
        {
            if (overlapColliders[i] != mainObjCollider)
            {
                CollidingObjects.Add(overlapColliders[i]);
                Debug.Log(gameObject.name + overlapColliders[i]);
            }
        }
    }
    public void SetShadowContactFilter(ContactFilter2D newFilter)
    {
        this.shadowContactFilter = newFilter;
    }

    public void SetShadowVisibility(bool enable)
    {
        ChildShadow.gameObject.SetActive(enable);
    }

    public void Initialize()
    {
        height = 0f;
        initialized = true;
    }

    public void Initialize(float height)
    {
        this.height = height;
        ResetShadowPosition();
        initialized = true;
    }

    public void ResetShadowCollider()
    {
        ShadowCollider.direction = CapsuleDirection2D.Horizontal;
        ShadowCollider.size = ShadowRenderer.size * shadowColliderReduction;
        ShadowCollider.isTrigger = true;
    }

    public void ResetShadowPosition()
    {
        ChildShadow.transform.parent = null;
        ChildShadow.transform.localEulerAngles = Vector3.zero;
        ChildShadow.transform.position = transform.position;
        ChildShadow.transform.position -= new Vector3(0, transform.position.y - mainSpriteRenderer.bounds.min.y - height);
        ChildShadow.transform.parent = this.transform;
    }
    private bool IsTriggerValid(Collider2D collision)
    {
        if (CollidingObjects.Contains(collision))
            return true;

        FakeHeightObject possibleFH;
        if(collision.TryGetComponent<FakeHeightObject>(out possibleFH))
        {
            if (possibleFH.triggerIgnoreShadow)
                return true;
            if (possibleFH.CollidingObjects.Contains(mainObjCollider))
                return true;
        }
        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsTriggerValid(collision))
            return;
        for (int i = 0; i < FakeHeightComponents.Length; i++)
        {
            FakeHeightComponents[i].OnFakeHeightTriggerEnter(collision);
        }
        ValidTriggers.Add(collision);
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!ValidTriggers.Contains(collision))
            return;
        for (int i = 0; i < FakeHeightComponents.Length; i++)
        {
            FakeHeightComponents[i].OnFakeHeightTriggerExit(collision);
        }
        ValidTriggers.Remove(collision);
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsTriggerValid(collision))
            return;
        for (int i = 0; i < FakeHeightComponents.Length; i++)
        {
            FakeHeightComponents[i].OnFakeHeightTriggerStay(collision);
        }
    }
    
}
