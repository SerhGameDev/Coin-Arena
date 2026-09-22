using UnityEngine;

public class PlayerIdentity : MonoBehaviour
{
    [SerializeField]
    [Tooltip(
        "Какому игроку принадлежит этот персонаж.\n\n" +
        "Player1 — первый игрок\n" +
        "Player2 — второй игрок\n" +
        "Player3 — третий игрок\n" +
        "Player4 — четвёртый игрок"
    )]
    private PlayerId player;


    /// <summary>
    /// Возвращает номер игрока, которому принадлежит этот персонаж.
    /// </summary>
    /// <returns>
    /// Идентификатор игрока из перечисления <see cref="PlayerId"/>.
    /// Например, <c>PlayerId.Player1</c>.
    /// </returns>
    public PlayerId GetPlayer()
    {
        return player;
    }
}