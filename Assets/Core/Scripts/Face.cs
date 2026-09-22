using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Face : MonoBehaviour
{
    [SerializeField]
    private List<Sprite> faces = new List<Sprite>();

    [SerializeField]
    private Sprite deathFace;

    [SerializeField]
    private Rigidbody2D targetRigidbody;

    [Header("Auto Face")]

    [SerializeField]
    private float speedChangeToReact = 4f;

    [SerializeField]
    private float highSpeedToReact = 8f;

    [SerializeField]
    private float autoChangeCooldown = 0.5f;

    [Header("Animation")]

    [SerializeField]
    private float changeAnimationTime = 0.12f;

    [SerializeField]
    private float changeScale = 1.25f;

    private SpriteRenderer spriteRenderer;

    private Vector3 startLocalScale;

    private Vector2 previousVelocity;

    private float autoChangeTimer;

    private bool autoFaceEnabled = true;
    private bool isDead;

    private Tween faceTween;


    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        startLocalScale = transform.localScale;

        if (targetRigidbody == null)
        {
            targetRigidbody = GetComponentInParent<Rigidbody2D>();
        }

        if (targetRigidbody != null)
        {
            previousVelocity = targetRigidbody.linearVelocity;
        }

        if (faces.Count > 0 && spriteRenderer.sprite == null)
        {
            spriteRenderer.sprite = faces[0];
        }
    }


    private void Update()
    {
        if (!autoFaceEnabled)
            return;

        if (isDead)
            return;

        if (targetRigidbody == null)
            return;

        UpdateAutoFace();
    }


    private void LateUpdate()
    {
        transform.rotation = Quaternion.identity;
    }


    /// <summary>
    /// Показывает лицо из списка <c>Faces</c>.
    /// </summary>
    /// <param name="index">
    /// Номер лица в списке.
    ///
    /// Нумерация начинается с нуля:
    /// <c>0</c> — первое лицо,
    /// <c>1</c> — второе лицо,
    /// <c>2</c> — третье лицо и так далее.
    ///
    /// Если передать номер, которого нет в списке,
    /// метод ничего не сделает.
    /// </param>
    /// <remarks>
    /// Лицо меняется с небольшой анимацией через DOTween.
    ///
    /// Пример:
    /// <code>
    /// Face.SetFace(0);
    /// </code>
    ///
    /// Или:
    /// <code>
    /// Face.SetFace(2);
    /// </code>
    /// </remarks>
    public void SetFace(int index)
    {
        if (isDead)
            return;

        if (index < 0 || index >= faces.Count)
            return;

        ChangeFace(faces[index]);
    }


    /// <summary>
    /// Выбирает случайное лицо из списка <c>Faces</c>
    /// и показывает его с небольшой анимацией.
    /// </summary>
    /// <remarks>
    /// Если список лиц пуст, метод ничего не сделает.
    ///
    /// После смерти персонажа случайная смена лица отключена,
    /// чтобы лицо смерти не могло случайно поменяться.
    ///
    /// Пример:
    /// <code>
    /// Face.RandomFace();
    /// </code>
    /// </remarks>
    public void RandomFace()
    {
        if (isDead)
            return;

        if (faces.Count == 0)
            return;

        int index = Random.Range(0, faces.Count);

        ChangeFace(faces[index]);
    }


    /// <summary>
    /// Показывает специальное лицо смерти персонажа.
    /// </summary>
    /// <remarks>
    /// После вызова этого метода автоматическая смена лиц прекращается.
    /// Обычные вызовы <see cref="SetFace(int)"/> и <see cref="RandomFace"/>
    /// также больше не меняют лицо.
    ///
    /// Это позволяет гарантировать, что лицо смерти останется
    /// на персонаже до его уничтожения.
    ///
    /// Пример:
    /// <code>
    /// Face.ShowDeathFace();
    /// </code>
    /// </remarks>
    public void ShowDeathFace()
    {
        if (deathFace == null)
            return;

        isDead = true;
        autoFaceEnabled = false;

        ChangeFace(deathFace);
    }


    /// <summary>
    /// Включает или выключает автоматическую смену выражения лица.
    /// </summary>
    /// <param name="value">
    /// Передайте <c>true</c>, чтобы Face самостоятельно реагировал
    /// на резкие изменения скорости персонажа.
    ///
    /// Передайте <c>false</c>, чтобы остановить автоматическую смену.
    ///
    /// При выключенной автоматике лицо всё ещё можно менять вручную
    /// через <see cref="SetFace(int)"/> или <see cref="RandomFace"/>.
    /// </param>
    /// <remarks>
    /// Например, чтобы временно отключить автоматические эмоции:
    /// <code>
    /// Face.SetAutoFace(false);
    /// </code>
    ///
    /// Чтобы снова включить:
    /// <code>
    /// Face.SetAutoFace(true);
    /// </code>
    ///
    /// Если уже было показано лицо смерти через
    /// <see cref="ShowDeathFace"/>, автоматическую смену
    /// повторно включить нельзя.
    /// </remarks>
    public void SetAutoFace(bool value)
    {
        if (isDead)
            return;

        autoFaceEnabled = value;

        if (targetRigidbody != null)
        {
            previousVelocity = targetRigidbody.linearVelocity;
        }
    }


    private void UpdateAutoFace()
    {
        if (autoChangeTimer > 0f)
        {
            autoChangeTimer -= Time.deltaTime;
        }

        Vector2 currentVelocity = targetRigidbody.linearVelocity;

        float velocityChange = (
            currentVelocity - previousVelocity
        ).magnitude;

        float currentSpeed = currentVelocity.magnitude;

        if (autoChangeTimer <= 0f)
        {
            if (velocityChange >= speedChangeToReact)
            {
                RandomFaceInternal();

                autoChangeTimer = autoChangeCooldown;
            }
            else if (currentSpeed >= highSpeedToReact)
            {
                RandomFaceInternal();

                autoChangeTimer = autoChangeCooldown;
            }
        }

        previousVelocity = currentVelocity;
    }


    private void RandomFaceInternal()
    {
        if (faces.Count == 0)
            return;

        int index = Random.Range(0, faces.Count);

        ChangeFace(faces[index]);
    }


    private void ChangeFace(Sprite newFace)
    {
        if (newFace == null)
            return;

        if (spriteRenderer.sprite == newFace)
            return;

        if (faceTween != null && faceTween.IsActive())
        {
            faceTween.Kill();
        }

        transform.DOKill();

        transform.localScale = startLocalScale;

        Sequence sequence = DOTween.Sequence();

        sequence.Append(
            transform.DOScale(
                startLocalScale * changeScale,
                changeAnimationTime
            )
            .SetEase(Ease.OutBack)
        );

        sequence.AppendCallback(() =>
        {
            spriteRenderer.sprite = newFace;
        });

        sequence.Append(
            transform.DOScale(
                startLocalScale,
                changeAnimationTime
            )
            .SetEase(Ease.OutBack)
        );

        faceTween = sequence;
    }


    private void OnDestroy()
    {
        if (faceTween != null && faceTween.IsActive())
        {
            faceTween.Kill();
        }

        transform.DOKill();
    }
}