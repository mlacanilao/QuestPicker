using BepInEx;
using HarmonyLib;
using QuestPicker.Config;
using UnityEngine;

namespace QuestPicker
{
    internal static class ModInfo
    {
        internal const string Guid = "omegaplatinum.elin.questpicker";
        internal const string Name = "Quest Picker";
        internal const string Version = "1.0.0.0";
        internal const string ModOptionsGuid = "evilmask.elinplugins.modoptions";
    }

    [BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
    internal class QuestPicker : BaseUnityPlugin
    {
        private void Start()
        {
            QuestPickerConfig.LoadConfig(config: Config);

            Harmony.CreateAndPatchAll(type: typeof(Patcher));
            
            try
            {
                UI.UIController.RegisterUI();
            }
            catch (System.IO.FileNotFoundException)
            {
                Debug.LogWarning("[Quest Picker] Mod Options mod is not installed/enabled. Skipping UI registration.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[Quest Picker] An unexpected error occurred: {ex.Message}");
            }
        }
    }
}
