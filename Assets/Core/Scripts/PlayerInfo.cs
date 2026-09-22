using System.Collections.Generic;
using UnityEngine;

public static class PlayerInfo
{
    private const string moneyKeyPrefix = "PlayerInfo_Money_";

    private static readonly Dictionary<PlayerId, int> money =
        new Dictionary<PlayerId, int>();


    static PlayerInfo()
    {
        ResetMemory();
    }


    /// <summary>
    /// Добавляет указанное количество монет выбранному игроку.
    /// </summary>
    /// <param name="player">
    /// Игрок, которому нужно добавить монеты.
    ///
    /// Например:
    /// <c>PlayerId.Player1</c> или <c>PlayerId.Player2</c>.
    /// </param>
    /// <param name="amount">
    /// Количество монет, которое нужно добавить.
    ///
    /// Если передать отрицательное значение или <c>0</c>,
    /// метод ничего не изменит.
    /// </param>
    /// <remarks>
    /// Пример:
    /// <code>
    /// PlayerInfo.AddMoney(PlayerId.Player1, 5);
    /// </code>
    ///
    /// После этого у Player1 станет на 5 монет больше.
    /// </remarks>
    public static void AddMoney(PlayerId player, int amount)
    {
        if (amount <= 0)
            return;

        money[player] += amount;
    }


    /// <summary>
    /// Удаляет указанное количество монет у выбранного игрока.
    /// </summary>
    /// <param name="player">
    /// Игрок, у которого нужно удалить монеты.
    /// </param>
    /// <param name="amount">
    /// Количество монет, которое нужно удалить.
    ///
    /// Если передать отрицательное значение или <c>0</c>,
    /// метод ничего не изменит.
    /// </param>
    /// <remarks>
    /// Количество монет никогда не станет меньше нуля.
    ///
    /// Например, если у игрока 3 монеты:
    /// <code>
    /// PlayerInfo.RemoveMoney(PlayerId.Player1, 10);
    /// </code>
    ///
    /// после этого у него будет <c>0</c> монет.
    /// </remarks>
    public static void RemoveMoney(PlayerId player, int amount)
    {
        if (amount <= 0)
            return;

        money[player] -= amount;

        if (money[player] < 0)
        {
            money[player] = 0;
        }
    }


    /// <summary>
    /// Возвращает текущее количество монет выбранного игрока.
    /// </summary>
    /// <param name="player">
    /// Игрок, количество монет которого нужно получить.
    /// </param>
    /// <returns>
    /// Текущее количество монет игрока.
    ///
    /// Значение всегда равно или больше <c>0</c>.
    /// </returns>
    /// <remarks>
    /// Пример:
    /// <code>
    /// int money = PlayerInfo.GetMoney(PlayerId.Player1);
    /// </code>
    /// </remarks>
    public static int GetMoney(PlayerId player)
    {
        return money[player];
    }


    /// <summary>
    /// Устанавливает точное количество монет выбранному игроку.
    /// </summary>
    /// <param name="player">
    /// Игрок, которому нужно установить значение.
    /// </param>
    /// <param name="amount">
    /// Новое количество монет.
    ///
    /// Отрицательные значения автоматически превращаются в <c>0</c>.
    /// </param>
    /// <remarks>
    /// В отличие от <see cref="AddMoney(PlayerId, int)"/>,
    /// этот метод не добавляет монеты, а полностью заменяет текущее значение.
    ///
    /// <code>
    /// PlayerInfo.SetMoney(PlayerId.Player1, 20);
    /// </code>
    /// </remarks>
    public static void SetMoney(PlayerId player, int amount)
    {
        money[player] = Mathf.Max(0, amount);
    }


    /// <summary>
    /// Сохраняет количество монет всех игроков в PlayerPrefs.
    /// </summary>
    /// <remarks>
    /// После вызова данные останутся сохранёнными даже после закрытия игры.
    ///
    /// Пример:
    /// <code>
    /// PlayerInfo.Save();
    /// </code>
    /// </remarks>
    public static void Save()
    {
        foreach (PlayerId player in System.Enum.GetValues(typeof(PlayerId)))
        {
            string key = GetMoneyKey(player);

            PlayerPrefs.SetInt(
                key,
                money[player]
            );
        }

        PlayerPrefs.Save();
    }


    /// <summary>
    /// Загружает количество монет всех игроков из PlayerPrefs.
    /// </summary>
    /// <remarks>
    /// Если для какого-либо игрока сохранения ещё нет,
    /// его количество монет будет установлено в <c>0</c>.
    ///
    /// Пример:
    /// <code>
    /// PlayerInfo.Load();
    /// </code>
    /// </remarks>
    public static void Load()
    {
        foreach (PlayerId player in System.Enum.GetValues(typeof(PlayerId)))
        {
            string key = GetMoneyKey(player);

            money[player] =
                PlayerPrefs.GetInt(
                    key,
                    0
                );
        }
    }


    /// <summary>
    /// Очищает текущие значения PlayerInfo только в памяти.
    /// </summary>
    /// <remarks>
    /// Этот метод НЕ удаляет сохранённые данные из PlayerPrefs.
    ///
    /// После вызова все игроки будут иметь <c>0</c> монет
    /// до следующего вызова <see cref="Load"/>.
    ///
    /// <code>
    /// PlayerInfo.Clear();
    /// </code>
    /// </remarks>
    public static void Clear()
    {
        ResetMemory();
    }


    /// <summary>
    /// Полностью удаляет сохранённые данные PlayerInfo из PlayerPrefs
    /// и одновременно обнуляет текущие значения в памяти.
    /// </summary>
    /// <remarks>
    /// Используйте этот метод, если нужно начать игру полностью заново.
    ///
    /// Он удаляет только данные PlayerInfo и не вызывает
    /// <c>PlayerPrefs.DeleteAll()</c>, поэтому чужие настройки игры
    /// останутся нетронутыми.
    /// </remarks>
    public static void DeleteSave()
    {
        foreach (PlayerId player in System.Enum.GetValues(typeof(PlayerId)))
        {
            PlayerPrefs.DeleteKey(
                GetMoneyKey(player)
            );
        }

        PlayerPrefs.Save();

        ResetMemory();
    }


    private static void ResetMemory()
    {
        money.Clear();

        foreach (PlayerId player in System.Enum.GetValues(typeof(PlayerId)))
        {
            money.Add(
                player,
                0
            );
        }
    }


    private static string GetMoneyKey(PlayerId player)
    {
        return moneyKeyPrefix + player;
    }
}