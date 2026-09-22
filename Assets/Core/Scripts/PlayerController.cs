using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private float groundCheckDistance = 0.08f;

    [SerializeField]
    private LayerMask groundLayer;

    [SerializeField]
    private LayerMask objectLayer;

    private Rigidbody2D rigidbody;
    private Collider2D bodyCollider;

    private float speed = 5f;
    private float jumpHeight = 3f;

    private float dashSpeed = 15f;
    private float dashTime = 0.2f;
    private float dashCooldown = 1f;

    private float pushDistance = 2f;
    private float pushMovementLockTime = 0.15f;

    private float moveDirection;

    private bool jumpRequested;
    private bool dashRequested;
    private bool isDashing;
    private bool showRays = true;

    private float dashDirection;
    private float dashTimer;
    private float dashCooldownTimer;

    private float pushMovementLockTimer;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();
    }


    private void FixedUpdate()
    {
        UpdateDashTimers();
        UpdatePushTimer();

        if (pushMovementLockTimer <= 0f)
        {
            if (dashRequested)
            {
                TryDash();
            }

            if (isDashing)
            {
                ApplyDash();
            }
            else
            {
                ApplyMovement();
            }
        }

        dashRequested = false;

        if (jumpRequested)
        {
            TryJump();
            jumpRequested = false;
        }
    }


    /// <summary>
    /// Устанавливает обычную горизонтальную скорость движения персонажа.
    /// </summary>
    /// <param name="value">
    /// Новая скорость движения в Unity units в секунду.
    /// Например, <c>5f</c> означает скорость 5 units в секунду.
    /// Значение автоматически ограничивается диапазоном от <c>0f</c> до <c>20f</c>.
    /// </param>
    /// <remarks>
    /// Метод можно безопасно вызывать каждый кадр.
    ///
    /// <code>
    /// public float Speed = 5f;
    ///
    /// void Update()
    /// {
    ///     Controller.SetSpeed(Speed);
    /// }
    /// </code>
    /// </remarks>
    public void SetSpeed(float value)
    {
        speed = Mathf.Clamp(value, 0f, 20f);
    }


    /// <summary>
    /// Устанавливает желаемую высоту прыжка персонажа.
    /// </summary>
    /// <param name="value">
    /// Высота прыжка в Unity units.
    /// Например, <c>3f</c> означает прыжок примерно на 3 Unity units вверх.
    /// Значение автоматически ограничивается диапазоном от <c>0.5f</c> до <c>10f</c>.
    /// </param>
    /// <remarks>
    /// PlayerController самостоятельно рассчитывает необходимую начальную
    /// вертикальную скорость с учётом гравитации объекта.
    ///
    /// <code>
    /// Controller.SetJumpHeight(3f);
    /// </code>
    /// </remarks>
    public void SetJumpHeight(float value)
    {
        jumpHeight = Mathf.Clamp(value, 0.5f, 10f);
    }


    /// <summary>
    /// Устанавливает скорость персонажа во время рывка <see cref="Dash"/>.
    /// </summary>
    /// <param name="value">
    /// Скорость рывка в Unity units в секунду.
    /// Значение автоматически ограничивается диапазоном от <c>1f</c> до <c>40f</c>.
    /// </param>
    /// <remarks>
    /// <code>
    /// Controller.SetDashSpeed(15f);
    /// </code>
    /// </remarks>
    public void SetDashSpeed(float value)
    {
        dashSpeed = Mathf.Clamp(value, 1f, 40f);
    }


    /// <summary>
    /// Устанавливает продолжительность одного рывка <see cref="Dash"/>.
    /// </summary>
    /// <param name="value">
    /// Продолжительность рывка в секундах.
    /// Например, <c>0.2f</c> означает 0.2 секунды.
    /// Значение автоматически ограничивается диапазоном
    /// от <c>0.05f</c> до <c>2f</c>.
    /// </param>
    public void SetDashTime(float value)
    {
        dashTime = Mathf.Clamp(value, 0.05f, 2f);
    }


    /// <summary>
    /// Устанавливает время перезарядки способности <see cref="Dash"/>.
    /// </summary>
    /// <param name="value">
    /// Время перезарядки в секундах.
    /// Например, <c>1f</c> означает, что новый рывок будет доступен через одну секунду.
    /// Значение автоматически ограничивается диапазоном от <c>0f</c> до <c>10f</c>.
    /// </param>
    public void SetDashCooldown(float value)
    {
        dashCooldown = Mathf.Clamp(value, 0f, 10f);
    }


    /// <summary>
    /// Устанавливает максимальное расстояние, на котором персонаж
    /// может обнаружить и толкнуть другой объект.
    /// </summary>
    /// <param name="value">
    /// Максимальная дистанция в Unity units.
    ///
    /// Например, <c>2f</c> означает, что лучи поиска будут иметь
    /// длину 2 Unity units от левой и правой границы персонажа.
    ///
    /// Это же расстояние дополнительно проверяется непосредственно
    /// при вызове <see cref="Push(GameObject, float)"/>.
    ///
    /// Значение автоматически ограничивается диапазоном
    /// от <c>0.1f</c> до <c>10f</c>.
    /// </param>
    /// <remarks>
    /// Пример:
    /// <code>
    /// public float PushDistance = 2f;
    ///
    /// void Update()
    /// {
    ///     Controller.SetPushDistance(PushDistance);
    /// }
    /// </code>
    /// </remarks>
    public void SetPushDistance(float value)
    {
        pushDistance = Mathf.Clamp(value, 0.1f, 10f);
    }


    /// <summary>
    /// Включает или выключает отображение служебных Raycast-лучей
    /// персонажа в окне Scene.
    /// </summary>
    /// <param name="value">
    /// Передайте <c>true</c>, чтобы отображать луч проверки земли
    /// и горизонтальные лучи поиска объектов.
    ///
    /// Передайте <c>false</c>, чтобы скрыть эти лучи.
    /// </param>
    /// <remarks>
    /// Лучи являются Gizmos и предназначены только для разработки.
    /// Они не отображаются в самой запущенной игре.
    ///
    /// Для их отображения кнопка Gizmos в окне Scene должна быть включена.
    ///
    /// <code>
    /// Controller.ShowRays(true);
    /// </code>
    ///
    /// или:
    ///
    /// <code>
    /// Controller.ShowRays(false);
    /// </code>
    /// </remarks>
    public void ShowRays(bool value)
    {
        showRays = value;
    }


    /// <summary>
    /// Задаёт персонажу направление обычного движения влево.
    /// </summary>
    /// <remarks>
    /// Метод можно вызывать каждый кадр, пока игрок удерживает клавишу.
    ///
    /// <code>
    /// if (Input.GetKey(KeyCode.A))
    /// {
    ///     Controller.MoveLeft();
    /// }
    /// </code>
    /// </remarks>
    public void MoveLeft()
    {
        moveDirection = -1f;
    }


    /// <summary>
    /// Задаёт персонажу направление обычного движения вправо.
    /// </summary>
    /// <remarks>
    /// Метод можно вызывать каждый кадр, пока игрок удерживает клавишу.
    ///
    /// <code>
    /// if (Input.GetKey(KeyCode.D))
    /// {
    ///     Controller.MoveRight();
    /// }
    /// </code>
    /// </remarks>
    public void MoveRight()
    {
        moveDirection = 1f;
    }


    /// <summary>
    /// Останавливает обычное горизонтальное движение персонажа.
    /// </summary>
    /// <remarks>
    /// Метод не отменяет уже начавшийся внешний толчок.
    ///
    /// <code>
    /// Controller.Stop();
    /// </code>
    /// </remarks>
    public void Stop()
    {
        moveDirection = 0f;
    }


    /// <summary>
    /// Просит персонажа выполнить прыжок.
    /// </summary>
    /// <remarks>
    /// Прыжок произойдёт только тогда, когда под персонажем находится земля.
    /// Проверка земли и вся работа с Rigidbody2D выполняются внутри PlayerController.
    ///
    /// <code>
    /// if (Input.GetKeyDown(KeyCode.Space))
    /// {
    ///     Controller.Jump();
    /// }
    /// </code>
    /// </remarks>
    public void Jump()
    {
        jumpRequested = true;
    }


    /// <summary>
    /// Просит персонажа выполнить быстрый горизонтальный рывок.
    /// </summary>
    /// <remarks>
    /// Рывок выполняется в текущем направлении движения.
    ///
    /// <code>
    /// if (Input.GetKeyDown(KeyCode.LeftShift))
    /// {
    ///     Controller.Dash();
    /// }
    /// </code>
    /// </remarks>
    public void Dash()
    {
        dashRequested = true;
    }


    /// <summary>
    /// Проверяет, находится ли персонаж сейчас на земле.
    /// </summary>
    /// <returns>
    /// <c>true</c>, если Raycast под персонажем обнаружил объект
    /// из слоя, указанного преподавателем в <c>Ground Layer</c>.
    ///
    /// <c>false</c>, если земли под персонажем нет.
    /// </returns>
    public bool IsGrounded()
    {
        return CheckGround();
    }


    /// <summary>
    /// Проверяет, готов ли рывок к использованию.
    /// </summary>
    /// <returns>
    /// <c>true</c>, если персонаж сейчас не выполняет Dash
    /// и время его перезарядки закончилось.
    ///
    /// В остальных случаях возвращает <c>false</c>.
    /// </returns>
    public bool CanDash()
    {
        return !isDashing && dashCooldownTimer <= 0f;
    }


    /// <summary>
    /// Ищет ближайший подходящий объект непосредственно слева от персонажа.
    /// </summary>
    /// <returns>
    /// GameObject первого объекта, обнаруженного слева на расстоянии,
    /// установленном через <see cref="SetPushDistance(float)"/>.
    ///
    /// Если подходящего объекта нет, возвращает <c>null</c>.
    /// </returns>
    /// <remarks>
    /// Метод самостоятельно использует Physics2D.Raycast.
    /// Студенту не требуется работать с RaycastHit2D.
    ///
    /// <code>
    /// GameObject leftObject = Controller.GetObjectLeft();
    ///
    /// if (leftObject != null)
    /// {
    ///     Controller.Push(leftObject, 10f);
    /// }
    /// </code>
    /// </remarks>
    public GameObject GetObjectLeft()
    {
        return GetObjectInDirection(Vector2.left);
    }


    /// <summary>
    /// Ищет ближайший подходящий объект непосредственно справа от персонажа.
    /// </summary>
    /// <returns>
    /// GameObject первого объекта, обнаруженного справа на расстоянии,
    /// установленном через <see cref="SetPushDistance(float)"/>.
    ///
    /// Если подходящего объекта нет, возвращает <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <code>
    /// GameObject rightObject = Controller.GetObjectRight();
    ///
    /// if (rightObject != null)
    /// {
    ///     Controller.Push(rightObject, 10f);
    /// }
    /// </code>
    /// </remarks>
    public GameObject GetObjectRight()
    {
        return GetObjectInDirection(Vector2.right);
    }


    /// <summary>
    /// Толкает переданный объект в сторону от текущего персонажа.
    /// </summary>
    /// <param name="target">
    /// GameObject, который требуется толкнуть.
    ///
    /// Обычно это объект, полученный из
    /// <see cref="GetObjectLeft"/> или <see cref="GetObjectRight"/>.
    ///
    /// Если передать <c>null</c>, метод ничего не сделает.
    /// </param>
    /// <param name="force">
    /// Сила физического импульса.
    ///
    /// Чем больше значение, тем сильнее будет толчок.
    /// Значение автоматически ограничивается диапазоном
    /// от <c>0f</c> до <c>50f</c>.
    /// </param>
    /// <remarks>
    /// Перед толчком PlayerController самостоятельно проверяет:
    ///
    /// 1. Существует ли переданный объект.
    ///
    /// 2. Есть ли у него Rigidbody2D.
    ///
    /// 3. Не пытается ли персонаж толкнуть самого себя.
    ///
    /// 4. Находится ли объект достаточно близко.
    ///
    /// Если объект дальше расстояния, установленного через
    /// <see cref="SetPushDistance(float)"/>, толчок не произойдёт.
    ///
    /// После толчка управление цели на короткое время перестаёт
    /// перезаписывать горизонтальную скорость, поэтому физический
    /// импульс действительно отбрасывает персонажа.
    ///
    /// <code>
    /// GameObject enemy = Controller.GetObjectRight();
    ///
    /// if (enemy != null)
    /// {
    ///     Controller.Push(enemy, 12f);
    /// }
    /// </code>
    /// </remarks>
    public void Push(GameObject target, float force)
    {
        if (target == null)
            return;

        PlayerController targetController =
            target.GetComponentInParent<PlayerController>();

        if (targetController == null)
            return;

        if (targetController == this)
            return;

        ColliderDistance2D distanceInfo =
            bodyCollider.Distance(targetController.bodyCollider);

        float distance = Mathf.Max(0f, distanceInfo.distance);

        if (distance > pushDistance)
            return;

        float safeForce = Mathf.Clamp(force, 0f, 50f);

        float direction = Mathf.Sign(
            targetController.transform.position.x - transform.position.x
        );

        if (Mathf.Abs(direction) < 0.01f)
        {
            direction = moveDirection;
        }

        if (Mathf.Abs(direction) < 0.01f)
        {
            direction = 1f;
        }

        targetController.ReceivePush(direction, safeForce);
    }


    /// <summary>
    /// Уничтожает GameObject текущего персонажа.
    /// </summary>
    /// <remarks>
    /// Unity удалит объект, на котором находится этот PlayerController.
    ///
    /// Например:
    /// <code>
    /// if (transform.position.y &lt; -10f)
    /// {
    ///     Controller.DestroyPlayer();
    /// }
    /// </code>
    /// </remarks>
    public void DestroyPlayer()
    {
        Destroy(gameObject);
    }


    private void ApplyMovement()
    {
        Vector2 velocity = rigidbody.linearVelocity;

        velocity.x = moveDirection * speed;

        rigidbody.linearVelocity = velocity;
    }


    private void TryJump()
    {
        if (!CheckGround())
            return;

        float gravity = Mathf.Abs(
            Physics2D.gravity.y * rigidbody.gravityScale
        );

        float jumpVelocity = Mathf.Sqrt(
            2f * gravity * jumpHeight
        );

        Vector2 velocity = rigidbody.linearVelocity;

        velocity.y = jumpVelocity;

        rigidbody.linearVelocity = velocity;
    }


    private void TryDash()
    {
        if (!CanDash())
            return;

        if (Mathf.Abs(moveDirection) < 0.01f)
            return;

        dashDirection = moveDirection;
        dashTimer = dashTime;
        dashCooldownTimer = dashCooldown;
        isDashing = true;
    }


    private void ApplyDash()
    {
        Vector2 velocity = rigidbody.linearVelocity;

        velocity.x = dashDirection * dashSpeed;

        rigidbody.linearVelocity = velocity;
    }


    private void ReceivePush(float direction, float force)
    {
        isDashing = false;
        dashRequested = false;

        pushMovementLockTimer = pushMovementLockTime;

        rigidbody.AddForce(
            Vector2.right * direction * force,
            ForceMode2D.Impulse
        );
    }


    private void UpdateDashTimers()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.fixedDeltaTime;
        }

        if (!isDashing)
            return;

        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
        {
            isDashing = false;
        }
    }


    private void UpdatePushTimer()
    {
        if (pushMovementLockTimer > 0f)
        {
            pushMovementLockTimer -= Time.fixedDeltaTime;
        }
    }


    private bool CheckGround()
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 rayOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y
        );

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        return hit.collider != null;
    }


    private GameObject GetObjectInDirection(Vector2 direction)
    {
        Bounds bounds = bodyCollider.bounds;

        Vector2 rayOrigin = bounds.center;

        if (direction.x < 0f)
        {
            rayOrigin.x = bounds.min.x - 0.01f;
        }
        else
        {
            rayOrigin.x = bounds.max.x + 0.01f;
        }

        RaycastHit2D hit = Physics2D.Raycast(
            rayOrigin,
            direction,
            pushDistance,
            objectLayer
        );

        if (hit.collider == null)
            return null;

        PlayerController targetController =
            hit.collider.GetComponentInParent<PlayerController>();

        if (targetController != null)
        {
            return targetController.gameObject;
        }

        Rigidbody2D hitRigidbody = hit.collider.attachedRigidbody;

        if (hitRigidbody != null)
        {
            return hitRigidbody.gameObject;
        }

        return hit.collider.gameObject;
    }


    private void OnDrawGizmosSelected()
    {
        if (!showRays)
            return;

        Collider2D col = GetComponent<Collider2D>();

        if (col == null)
            return;

        Bounds bounds = col.bounds;

        Vector2 groundRayOrigin = new Vector2(
            bounds.center.x,
            bounds.min.y
        );

        Gizmos.DrawLine(
            groundRayOrigin,
            groundRayOrigin + Vector2.down * groundCheckDistance
        );

        Vector2 leftRayOrigin = new Vector2(
            bounds.min.x - 0.01f,
            bounds.center.y
        );

        Vector2 rightRayOrigin = new Vector2(
            bounds.max.x + 0.01f,
            bounds.center.y
        );

        Gizmos.DrawLine(
            leftRayOrigin,
            leftRayOrigin + Vector2.left * pushDistance
        );

        Gizmos.DrawLine(
            rightRayOrigin,
            rightRayOrigin + Vector2.right * pushDistance
        );
    }


    private void OnValidate()
    {
        groundCheckDistance = Mathf.Clamp(
            groundCheckDistance,
            0.01f,
            0.5f
        );
    }
}
