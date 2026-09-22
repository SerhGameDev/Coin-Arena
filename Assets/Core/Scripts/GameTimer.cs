using DG.Tweening;
using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [Header("Timer")]

    [SerializeField]
    [Tooltip(
        "Продолжительность матча в секундах.\n\n" +
        "Например:\n" +
        "30 — короткий матч\n" +
        "60 — стандартный матч\n" +
        "120 — длинный матч"
    )]
    private float gameTime = 60f;


    [Header("UI")]

    [SerializeField]
    [Tooltip(
        "Текст TextMeshPro, в котором будет отображаться оставшееся время.\n\n" +
        "Обычно этот текст находится сверху по центру экрана."
    )]
    private TMP_Text timerText;


    [SerializeField]
    [Tooltip(
        "Текст, который отображается перед количеством секунд.\n\n" +
        "Например:\n" +
        "\"TIME: \" даст результат TIME: 42\n\n" +
        "Можно оставить пустым и показывать только число."
    )]
    private string prefix = "";


    [Header("Animation")]

    [SerializeField]
    [Tooltip(
        "Насколько сильно текст увеличивается при смене секунды.\n\n" +
        "Например, 0.15 создаёт лёгкий эффект удара."
    )]
    private float normalPunch = 0.15f;


    [SerializeField]
    [Tooltip(
        "Насколько сильно текст увеличивается, когда времени осталось мало."
    )]
    private float warningPunch = 0.3f;


    [SerializeField]
    [Tooltip(
        "Продолжительность анимации изменения цифры."
    )]
    private float punchTime = 0.2f;


    [Header("Last Seconds")]

    [SerializeField]
    [Tooltip(
        "Начиная с какого количества секунд таймер считается заканчивающимся.\n\n" +
        "Например, значение 10 означает, что последние 10 секунд " +
        "будут отображаться с более сильной анимацией."
    )]
    private int warningSeconds = 10;


    [SerializeField]
    [Tooltip(
        "Если включено, последние секунды будут дополнительно пульсировать."
    )]
    private bool pulseLastSeconds = true;


    private float timeLeft;

    private int previousDisplayedSecond = -1;

    private Vector3 startScale;

    private bool running;


    private void Awake()
    {
        if (timerText != null)
        {
            startScale =
                timerText.transform.localScale;
        }
    }


    private void Start()
    {
        timeLeft = gameTime;
        running = true;

        UpdateTimerText(true);
    }


    private void Update()
    {
        if (!running)
            return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            running = false;
        }

        UpdateTimerText(false);
    }


    private void UpdateTimerText(bool force)
    {
        if (timerText == null)
            return;


        int displayedSecond =
            Mathf.CeilToInt(timeLeft);


        if (!force &&
            displayedSecond == previousDisplayedSecond)
        {
            return;
        }


        previousDisplayedSecond =
            displayedSecond;


        timerText.text =
            prefix + displayedSecond;


        AnimateNumber(
            displayedSecond
        );
    }


    private void AnimateNumber(int seconds)
    {
        timerText.transform.DOKill();

        timerText.transform.localScale =
            startScale;


        bool warning =
            seconds <= warningSeconds &&
            seconds > 0;


        float punchStrength =
            warning
                ? warningPunch
                : normalPunch;


        timerText.transform.DOPunchScale(
            Vector3.one * punchStrength,
            punchTime,
            5,
            0.5f
        );


        if (!pulseLastSeconds)
            return;

        if (!warning)
            return;


        timerText.transform
            .DOScale(
                startScale * 1.08f,
                punchTime * 0.5f
            )
            .SetLoops(
                2,
                LoopType.Yoyo
            );
    }


    private void OnDestroy()
    {
        if (timerText != null)
        {
            timerText.transform.DOKill();
        }
    }


    private void OnValidate()
    {
        gameTime =
            Mathf.Clamp(
                gameTime,
                1f,
                3600f
            );

        normalPunch =
            Mathf.Clamp(
                normalPunch,
                0f,
                1f
            );

        warningPunch =
            Mathf.Clamp(
                warningPunch,
                0f,
                2f
            );

        punchTime =
            Mathf.Clamp(
                punchTime,
                0.05f,
                2f
            );

        warningSeconds =
            Mathf.Clamp(
                warningSeconds,
                1,
                60
            );
    }
}