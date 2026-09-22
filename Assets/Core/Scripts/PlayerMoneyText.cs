using TMPro;
using UnityEngine;

public class PlayerMoneyText : MonoBehaviour
{
    [Header("Player")]

    [SerializeField]
    [Tooltip(
        "Игрок, информацию о котором нужно отображать.\n\n" +
        "None — ничего не отображать\n" +
        "Player1 — показывать монеты первого игрока\n" +
        "Player2 — показывать монеты второго игрока\n" +
        "Player3 — показывать монеты третьего игрока\n" +
        "Player4 — показывать монеты четвёртого игрока"
    )]
    private PlayerId player = PlayerId.None;


    [Header("UI")]

    [SerializeField]
    [Tooltip(
        "TextMeshPro-компонент, в который будет выводиться информация об игроке.\n\n" +
        "Если поле не назначено, скрипт попробует найти TMP_Text на этом же GameObject."
    )]
    private TMP_Text text;


    [Header("Text")]

    [SerializeField]
    [Tooltip(
        "Текст, который будет написан перед номером игрока.\n\n" +
        "Например:\n" +
        "\"Player \" даст результат:\n" +
        "Player 1: 12"
    )]
    private string playerPrefix = "Player ";


    [SerializeField]
    [Tooltip(
        "Текст между номером игрока и количеством монет.\n\n" +
        "Например значение \": \" даст результат:\n" +
        "Player 1: 12"
    )]
    private string separator = ": ";


    [SerializeField]
    [Tooltip(
        "Текст после количества монет.\n\n" +
        "Например:\n" +
        "\" coins\" даст результат:\n" +
        "Player 1: 12 coins\n\n" +
        "Можно оставить пустым."
    )]
    private string moneySuffix = "";


    private int previousMoney = -1;
    private PlayerId previousPlayer = (PlayerId)(-1);


    private void Awake()
    {
        if (text == null)
        {
            text = GetComponent<TMP_Text>();
        }
    }


    private void Start()
    {
        RefreshText(true);
    }


    private void Update()
    {
        RefreshText(false);
    }


    private void RefreshText(bool force)
    {
        if (text == null)
            return;

        if (player == PlayerId.None)
        {
            if (text.text != "")
            {
                text.text = "";
            }

            previousPlayer = player;
            previousMoney = -1;

            return;
        }


        int currentMoney =
            PlayerInfo.GetMoney(player);


        if (!force &&
            previousPlayer == player &&
            previousMoney == currentMoney)
        {
            return;
        }


        previousPlayer = player;
        previousMoney = currentMoney;


        int playerNumber =
            GetPlayerNumber(player);


        text.text =
            playerPrefix +
            playerNumber +
            separator +
            currentMoney +
            moneySuffix;
    }


    private int GetPlayerNumber(PlayerId playerId)
    {
        switch (playerId)
        {
            case PlayerId.Player1:
                return 1;

            case PlayerId.Player2:
                return 2;

            case PlayerId.Player3:
                return 3;

            case PlayerId.Player4:
                return 4;

            default:
                return 0;
        }
    }
}