using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Hands : MonoBehaviour
{
    [SerializeField]
    private Transform body;

    [SerializeField]
    private Vector2 handPosition = new Vector2(0.6f, 0f);

    [SerializeField]
    private List<Sprite> handSprites = new List<Sprite>();

    [Header("Movement")]

    [SerializeField]
    private float followSmoothTime = 0.1f;

    [SerializeField]
    private float maxFollowSpeed = 30f;

    [Header("Rotation")]

    [SerializeField]
    private float rotationSmoothSpeed = 10f;

    private SpriteRenderer spriteRenderer;

    private Transform target;

    private Vector3 storedWorldPosition;
    private Vector3 velocity;

    private Quaternion storedRotation;


    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (body == null)
        {
            Debug.LogWarning(
                "Hands: Body не назначен.",
                this
            );

            enabled = false;
            return;
        }

        CreateTarget();

        SetRandomSprite();

        storedWorldPosition = target.position;
        storedRotation = target.rotation;

        transform.position = storedWorldPosition;
        transform.rotation = storedRotation;
    }


    private void LateUpdate()
    {
        if (target == null)
            return;

        storedWorldPosition =
            Vector3.SmoothDamp(
                storedWorldPosition,
                target.position,
                ref velocity,
                followSmoothTime,
                maxFollowSpeed
            );

        transform.position =
            storedWorldPosition;


        storedRotation =
            Quaternion.Lerp(
                storedRotation,
                target.rotation,
                rotationSmoothSpeed * Time.deltaTime
            );

        transform.rotation =
            storedRotation;
    }


    private void CreateTarget()
    {
        GameObject targetObject =
            new GameObject(gameObject.name + " Target");

        target =
            targetObject.transform;

        target.SetParent(body);

        target.localPosition =
            handPosition;

        target.localRotation =
            Quaternion.identity;

        target.localScale =
            Vector3.one;
    }


    private void SetRandomSprite()
    {
        if (handSprites == null)
            return;

        if (handSprites.Count == 0)
            return;

        int index =
            Random.Range(0, handSprites.Count);

        spriteRenderer.sprite =
            handSprites[index];
    }


    private void OnValidate()
    {
        followSmoothTime =
            Mathf.Clamp(
                followSmoothTime,
                0.01f,
                1f
            );

        maxFollowSpeed =
            Mathf.Clamp(
                maxFollowSpeed,
                1f,
                100f
            );

        rotationSmoothSpeed =
            Mathf.Clamp(
                rotationSmoothSpeed,
                0.1f,
                50f
            );
    }


    private void OnDestroy()
    {
        if (target != null)
        {
            Destroy(target.gameObject);
        }
    }
}