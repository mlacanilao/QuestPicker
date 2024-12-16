using System.IO;
using System.Reflection;
using BepInEx;
using HarmonyLib;

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
            
            var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
            var xmlPath = Path.Combine(path1: assemblyLocation, path2: "QuestPickerConfig.xml");
            QuestPickerConfig.InitializeXmlPath(xmlPath: xmlPath);
            
            var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
            QuestPickerConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);
            
            UI.UIController.RegisterUI();

            Harmony.CreateAndPatchAll(type: typeof(Patcher));
        }
    }
}
