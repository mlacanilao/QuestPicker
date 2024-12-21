using System;
using System.Linq;
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
        internal const string ModOptionsAssemblyName = "ModOptions";
    }

    [BepInPlugin(GUID: ModInfo.Guid, Name: ModInfo.Name, Version: ModInfo.Version)]
    internal class QuestPicker : BaseUnityPlugin
    {
        internal static QuestPicker Instance { get; private set; }
        
        private void Start()
        {
            Instance = this;
            
            QuestPickerConfig.LoadConfig(config: Config);

            Harmony.CreateAndPatchAll(type: typeof(Patcher));
            
            if (IsModOptionsInstalled())
            {
                try
                {
                    UI.UIController.RegisterUI();
                }
                catch (Exception ex)
                {
                    Log(payload: $"An error occurred during UI registration: {ex.Message}");
                }
            }
            else
            {
                Log(payload: "Mod Options is not installed. Skipping UI registration.");
            }
        }
        
        private static void Log(object payload)
        {
            Instance.Logger.LogInfo(data: payload);
        }
        
        private bool IsModOptionsInstalled()
        {
            try
            {
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Any(predicate: assembly => assembly.GetName().Name == ModInfo.ModOptionsAssemblyName);
            }
            catch (Exception ex)
            {
                Log(payload: $"Error while checking for Mod Options: {ex.Message}");
                return false;
            }
        }
    }
}
