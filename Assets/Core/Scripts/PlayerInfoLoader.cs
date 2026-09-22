using UnityEngine;

public class PlayerInfoLoader : MonoBehaviour
{
    [Header("Startup")]

    [SerializeField]
    [Tooltip(
        "Если включено, при запуске сцены PlayerInfo автоматически загрузит " +
        "сохранённые данные из PlayerPrefs.\n\n" +
        "Обычно эту настройку стоит оставить включённой."
    )]
    private bool loadOnStart = true;


    [SerializeField]
    [Tooltip(
        "Если включено, перед загрузкой текущие статические данные PlayerInfo " +
        "будут сначала сброшены в ноль.\n\n" +
        "Это полезно при повторном запуске сцены или при отключённом Domain Reload в Unity Editor."
    )]
    private bool clearMemoryBeforeLoad = true;


    [Header("Saving")]

    [SerializeField]
    [Tooltip(
        "Если включено, PlayerInfo автоматически сохранит данные " +
        "при закрытии игры."
    )]
    private bool saveOnApplicationQuit = true;


    [SerializeField]
    [Tooltip(
        "Если включено, данные будут сохраняться при отключении этого GameObject.\n\n" +
        "Обычно это не обязательно. Используйте только если объект может исчезать " +
        "при смене сцены и вам нужно сохранить данные именно в этот момент."
    )]
    private bool saveOnDisable = false;


    [Header("Development")]

    [SerializeField]
    [Tooltip(
        "Если включено, при запуске сцены полностью удаляется сохранение PlayerInfo " +
        "из PlayerPrefs.\n\n" +
        "Используйте только во время разработки и тестов.\n\n" +
        "В обычной игре эту настройку нужно оставить выключенной."
    )]
    private bool deleteSaveOnStart = false;


    private void Awake()
    {
        if (deleteSaveOnStart)
        {
            PlayerInfo.DeleteSave();
        }

        if (clearMemoryBeforeLoad)
        {
            PlayerInfo.Clear();
        }

        if (loadOnStart)
        {
            PlayerInfo.Load();
        }
    }


    private void OnApplicationQuit()
    {
        if (!saveOnApplicationQuit)
            return;

        PlayerInfo.Save();
    }


    private void OnDisable()
    {
        if (!saveOnDisable)
            return;

        PlayerInfo.Save();
    }
}