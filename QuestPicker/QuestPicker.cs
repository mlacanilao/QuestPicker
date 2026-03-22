using System;
using BepInEx;
using HarmonyLib;

namespace QuestPicker;

public static class ModInfo
{
    public const string Guid = "omegaplatinum.elin.questpicker";
    public const string Name = "Quest Picker";
    public const string Version = "2.0.0";
    internal const string ModOptionsGuid = "evilmask.elinplugins.modoptions";
}

[BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
internal class Plugin : BaseUnityPlugin
{
    internal static Plugin? Instance;

    private void Awake()
    {
        Instance = this;
        QuestPickerConfig.LoadConfig(config: Config);
        Harmony.CreateAndPatchAll(type: typeof(Patcher), harmonyInstanceId: ModInfo.Guid);

        var modOptionsPlugin = FindModOptionsPlugin();
        if (modOptionsPlugin != null)
        {
            try
            {
                UIController.RegisterUI();
            }
            catch (Exception ex)
            {
                LogError(message: $"An error occurred during UI registration: {ex.Message}");
            }
        }
    }

    private static BaseUnityPlugin? FindModOptionsPlugin()
    {
        try
        {
            foreach (var obj in ModManager.ListPluginObject)
            {
                if (obj is not BaseUnityPlugin plugin)
                {
                    continue;
                }

                if (plugin.Info.Metadata.GUID == ModInfo.ModOptionsGuid)
                {
                    return plugin;
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            LogError(message: $"Error while checking for Mod Options: {ex.Message}");
            return null;
        }
    }

    internal static void LogDebug(object message)
    {
        Instance?.Logger.LogDebug(data: message);
    }

    internal static void LogInfo(object message)
    {
        Instance?.Logger.LogInfo(data: message);
    }

    internal static void LogError(object message)
    {
        Instance?.Logger.LogError(data: message);
    }
}
