using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineTargetGroup))]
public class MultiplayerCamera : MonoBehaviour
{
    [Header("Cinemachine")]

    [SerializeField]
    [Tooltip(
        "Cinemachine Camera, которая будет следить за всеми игроками.\n\n" +
        "Перетащите сюда объект с компонентом CinemachineCamera.\n\n" +
        "Скрипт автоматически назначит Target Group как Tracking Target этой камеры."
    )]
    private CinemachineCamera cinemachineCamera;


    [Header("Player Detection")]

    [SerializeField]
    [Tooltip(
        "Вес каждого игрока внутри Cinemachine Target Group.\n\n" +
        "Обычно для всех игроков используется одинаковое значение 1.\n\n" +
        "Чем больше вес, тем сильнее позиция этого игрока влияет на центр камеры."
    )]
    private float playerWeight = 1f;


    [SerializeField]
    [Tooltip(
        "Примерный радиус одного игрока для расчёта границ группы.\n\n" +
        "Cinemachine использует это значение, чтобы понимать, сколько места персонаж занимает в кадре.\n\n" +
        "Для небольшого 2D-персонажа обычно хорошо подходят значения примерно 0.4–0.8."
    )]
    private float playerRadius = 0.6f;


    [Header("Automatic Refresh")]

    [SerializeField]
    [Tooltip(
        "Если включено, скрипт периодически проверяет сцену и автоматически добавляет новых игроков.\n\n" +
        "Для вашей игры это можно оставить включённым. " +
        "Это особенно удобно, если персонажи когда-нибудь будут создаваться во время матча."
    )]
    private bool refreshPlayersAutomatically = true;


    [SerializeField]
    [Tooltip(
        "Как часто проверять сцену на появление новых игроков.\n\n" +
        "Значение указано в секундах.\n\n" +
        "Например, 1 означает одну проверку в секунду."
    )]
    private float refreshInterval = 1f;


    private CinemachineTargetGroup targetGroup;

    private readonly List<Transform> registeredPlayers =
        new List<Transform>();

    private float refreshTimer;


    private void Awake()
    {
        targetGroup =
            GetComponent<CinemachineTargetGroup>();

        SetupCamera();
    }


    private void Start()
    {
        RefreshPlayers();
    }


    private void Update()
    {
        if (!refreshPlayersAutomatically)
            return;

        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;

        RefreshPlayers();
    }


    private void SetupCamera()
    {
        if (cinemachineCamera == null)
        {
            Debug.LogWarning(
                "MultiplayerCamera: Cinemachine Camera не назначена.",
                this
            );

            return;
        }

        CameraTarget cameraTarget =
            cinemachineCamera.Target;

        cameraTarget.TrackingTarget =
            targetGroup.transform;

        cameraTarget.CustomLookAtTarget =
            false;

        cinemachineCamera.Target =
            cameraTarget;
    }


    private void RefreshPlayers()
    {
        PlayerController[] players =
            FindObjectsByType<PlayerController>(
                FindObjectsSortMode.None
            );


        for (int i = registeredPlayers.Count - 1; i >= 0; i--)
        {
            Transform registeredPlayer =
                registeredPlayers[i];

            if (registeredPlayer != null)
                continue;

            registeredPlayers.RemoveAt(i);
        }


        for (int i = 0; i < players.Length; i++)
        {
            PlayerController player =
                players[i];

            if (player == null)
                continue;

            Transform playerTransform =
                player.transform;

            if (registeredPlayers.Contains(playerTransform))
                continue;

            targetGroup.AddMember(
                playerTransform,
                playerWeight,
                playerRadius
            );

            registeredPlayers.Add(
                playerTransform
            );
        }
    }


    private void OnValidate()
    {
        playerWeight =
            Mathf.Clamp(
                playerWeight,
                0.01f,
                10f
            );

        playerRadius =
            Mathf.Clamp(
                playerRadius,
                0.01f,
                5f
            );

        refreshInterval =
            Mathf.Clamp(
                refreshInterval,
                0.1f,
                10f
            );
    }
}