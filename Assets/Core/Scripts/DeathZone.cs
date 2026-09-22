using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathZone : MonoBehaviour
{
    [Header("Respawn Settings")]

    [SerializeField]
    [Tooltip(
        "Сколько секунд игрок будет отсутствовать после попадания в зону смерти.\n\n" +
        "Например:\n" +
        "1 — почти мгновенное возвращение\n" +
        "2 — короткая пауза\n" +
        "3 — заметное наказание за падение"
    )]
    private float respawnDelay = 2f;


    [SerializeField]
    [Tooltip(
        "Список возможных точек возрождения игрока.\n\n" +
        "Когда игрок погибает, одна из этих точек выбирается случайно.\n\n" +
        "Создайте в сцене пустые GameObject, например:\n" +
        "Spawn Point 1\n" +
        "Spawn Point 2\n" +
        "Spawn Point 3\n\n" +
        "После этого перетащите их сюда."
    )]
    private List<Transform> spawnPoints = new List<Transform>();


    [Header("Player Detection")]

    [SerializeField]
    [Tooltip(
        "Tag объектов, которые считаются игроками.\n\n" +
        "Обычно всем игрокам нужно назначить Tag = Player.\n\n" +
        "Если в зону попадёт объект с другим Tag, зона его проигнорирует."
    )]
    private string playerTag = "Player";


    [Header("Respawn Behaviour")]

    [SerializeField]
    [Tooltip(
        "Если включено, после возрождения горизонтальная и вертикальная скорость игрока будет обнулена.\n\n" +
        "Рекомендуется оставить включённым, чтобы игрок не продолжал лететь после телепортации."
    )]
    private bool resetVelocityOnRespawn = true;


    [SerializeField]
    [Tooltip(
        "Если включено, при возрождении вращение игрока будет сброшено.\n\n" +
        "Полезно, если персонаж может вращаться после столкновений."
    )]
    private bool resetRotationOnRespawn = true;


    [SerializeField]
    [Tooltip(
        "Угол по Z, который будет установлен игроку после возрождения.\n\n" +
        "Обычно используется 0."
    )]
    private float respawnRotationZ = 0f;


    private Collider2D zoneCollider;


    private void Awake()
    {
        zoneCollider = GetComponent<Collider2D>();

        zoneCollider.isTrigger = true;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        PlayerController playerController =
            other.GetComponentInParent<PlayerController>();

        if (playerController == null)
            return;

        StartCoroutine(
            RespawnPlayer(playerController)
        );
    }


    private IEnumerator RespawnPlayer(
        PlayerController playerController
    )
    {
        GameObject player =
            playerController.gameObject;

        Rigidbody2D playerRigidbody =
            player.GetComponent<Rigidbody2D>();

        Collider2D[] playerColliders =
            player.GetComponentsInChildren<Collider2D>();

        SpriteRenderer[] spriteRenderers =
            player.GetComponentsInChildren<SpriteRenderer>();


        for (int i = 0; i < playerColliders.Length; i++)
        {
            playerColliders[i].enabled = false;
        }


        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].enabled = false;
        }


        playerController.enabled = false;


        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity =
                Vector2.zero;

            playerRigidbody.angularVelocity =
                0f;

            playerRigidbody.simulated =
                false;
        }


        yield return new WaitForSeconds(
            respawnDelay
        );


        Transform spawnPoint =
            GetRandomSpawnPoint();


        if (spawnPoint != null)
        {
            player.transform.position =
                spawnPoint.position;
        }


        if (resetRotationOnRespawn)
        {
            player.transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    respawnRotationZ
                );
        }


        if (playerRigidbody != null)
        {
            playerRigidbody.simulated =
                true;

            if (resetVelocityOnRespawn)
            {
                playerRigidbody.linearVelocity =
                    Vector2.zero;

                playerRigidbody.angularVelocity =
                    0f;
            }
        }


        for (int i = 0; i < playerColliders.Length; i++)
        {
            playerColliders[i].enabled = true;
        }


        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].enabled = true;
        }


        playerController.enabled = true;
    }


    private Transform GetRandomSpawnPoint()
    {
        if (spawnPoints == null)
            return null;

        if (spawnPoints.Count == 0)
        {
            Debug.LogWarning(
                "DeathZone: Не указано ни одной точки спауна.",
                this
            );

            return null;
        }


        List<Transform> validSpawnPoints =
            new List<Transform>();


        for (int i = 0; i < spawnPoints.Count; i++)
        {
            if (spawnPoints[i] != null)
            {
                validSpawnPoints.Add(
                    spawnPoints[i]
                );
            }
        }


        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning(
                "DeathZone: Все точки спауна пустые.",
                this
            );

            return null;
        }


        int randomIndex =
            Random.Range(
                0,
                validSpawnPoints.Count
            );


        return validSpawnPoints[
            randomIndex
        ];
    }


    private void OnValidate()
    {
        respawnDelay =
            Mathf.Clamp(
                respawnDelay,
                0f,
                10f
            );
    }


    private void OnDrawGizmosSelected()
    {
        if (spawnPoints == null)
            return;


        for (int i = 0; i < spawnPoints.Count; i++)
        {
            Transform point =
                spawnPoints[i];

            if (point == null)
                continue;


            Gizmos.DrawWireSphere(
                point.position,
                0.25f
            );
        }
    }
}