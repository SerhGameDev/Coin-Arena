using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Coin : MonoBehaviour
{
    [Header("Coin")]

    [SerializeField]
    [Tooltip(
        "Сколько монет получит игрок при подборе.\n\n" +
        "Примеры:\n" +
        "1 — обычная монета\n" +
        "3 — золотая монета\n" +
        "5 — редкая монета"
    )]
    private int value = 1;


    [Header("Respawn")]

    [SerializeField]
    [Tooltip(
        "Если включено, монета появится снова через некоторое время после подбора."
    )]
    private bool respawn = true;


    [SerializeField]
    [Tooltip(
        "Через сколько секунд монета снова появится после подбора.\n\n" +
        "Например, 3 означает три секунды."
    )]
    private float respawnTime = 3f;


    [Header("Idle Animation")]

    [SerializeField]
    [Tooltip(
        "Насколько высоко монета будет плавать вверх и вниз.\n\n" +
        "Небольшие значения вроде 0.1–0.25 обычно выглядят лучше всего."
    )]
    private float floatingHeight = 0.15f;


    [SerializeField]
    [Tooltip(
        "Сколько секунд занимает одно движение монеты вверх или вниз.\n\n" +
        "Чем меньше значение, тем быстрее монета плавает."
    )]
    private float floatingTime = 0.7f;


    [SerializeField]
    [Tooltip(
        "Скорость вращения монеты вокруг своей оси в градусах в секунду.\n\n" +
        "Установите 0, если вращение не требуется."
    )]
    private float rotationSpeed = 90f;


    [Header("Collect Animation")]

    [SerializeField]
    [Tooltip(
        "Продолжительность анимации исчезновения монеты после подбора."
    )]
    private float collectAnimationTime = 0.18f;


    [SerializeField]
    [Tooltip(
        "Насколько сильно монета увеличится непосредственно перед исчезновением.\n\n" +
        "Например, 1.3 означает увеличение до 130% размера."
    )]
    private float collectPunchScale = 1.3f;


    [Header("Spawn Animation")]

    [SerializeField]
    [Tooltip(
        "Продолжительность анимации появления монеты."
    )]
    private float spawnAnimationTime = 0.3f;


    private Collider2D coinCollider;
    private SpriteRenderer spriteRenderer;

    private Vector3 startScale;
    private Vector3 startLocalPosition;

    private bool collected;

    private Tween floatingTween;


    private void Awake()
    {
        coinCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        coinCollider.isTrigger = true;

        startScale = transform.localScale;
        startLocalPosition = transform.localPosition;
    }


    private void Start()
    {
        StartIdleAnimation();
    }


    private void Update()
    {
        if (collected)
            return;

        transform.Rotate(
            0f,
            0f,
            rotationSpeed * Time.deltaTime
        );
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected)
            return;

        PlayerIdentity player =
            other.GetComponentInParent<PlayerIdentity>();

        if (player == null)
            return;

        Collect(player);
    }


    private void Collect(PlayerIdentity player)
    {
        collected = true;

        PlayerInfo.AddMoney(
            player.GetPlayer(),
            value
        );

        coinCollider.enabled = false;

        StopIdleAnimation();

        transform.DOKill();

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            transform.DOScale(
                startScale * collectPunchScale,
                collectAnimationTime * 0.4f
            )
            .SetEase(Ease.OutBack)
        );

        sequence.Append(
            transform.DOScale(
                Vector3.zero,
                collectAnimationTime * 0.6f
            )
            .SetEase(Ease.InBack)
        );

        sequence.OnComplete(
            OnCollectAnimationFinished
        );
    }


    private void OnCollectAnimationFinished()
    {
        spriteRenderer.enabled = false;

        if (respawn)
        {
            StartCoroutine(
                RespawnRoutine()
            );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }


    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(
            respawnTime
        );

        Respawn();
    }


    private void Respawn()
    {
        transform.DOKill();

        transform.localPosition =
            startLocalPosition;

        transform.localScale =
            Vector3.zero;

        spriteRenderer.enabled = true;

        collected = false;


        transform
            .DOScale(
                startScale,
                spawnAnimationTime
            )
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                coinCollider.enabled = true;

                StartIdleAnimation();
            });
    }


    private void StartIdleAnimation()
    {
        StopIdleAnimation();

        transform.localPosition =
            startLocalPosition;

        floatingTween =
            transform
                .DOLocalMoveY(
                    startLocalPosition.y + floatingHeight,
                    floatingTime
                )
                .SetEase(Ease.InOutSine)
                .SetLoops(
                    -1,
                    LoopType.Yoyo
                );
    }


    private void StopIdleAnimation()
    {
        if (floatingTween != null)
        {
            floatingTween.Kill();
            floatingTween = null;
        }
    }


    private void OnDestroy()
    {
        transform.DOKill();

        if (floatingTween != null)
        {
            floatingTween.Kill();
        }
    }


    private void OnValidate()
    {
        value =
            Mathf.Clamp(
                value,
                1,
                100
            );

        respawnTime =
            Mathf.Clamp(
                respawnTime,
                0f,
                60f
            );

        floatingHeight =
            Mathf.Clamp(
                floatingHeight,
                0f,
                2f
            );

        floatingTime =
            Mathf.Clamp(
                floatingTime,
                0.1f,
                5f
            );

        rotationSpeed =
            Mathf.Clamp(
                rotationSpeed,
                -720f,
                720f
            );

        collectAnimationTime =
            Mathf.Clamp(
                collectAnimationTime,
                0.05f,
                2f
            );

        collectPunchScale =
            Mathf.Clamp(
                collectPunchScale,
                1f,
                3f
            );

        spawnAnimationTime =
            Mathf.Clamp(
                spawnAnimationTime,
                0.05f,
                2f
            );
    }
}