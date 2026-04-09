using EvilMask.Elin.ModOptions;
using EvilMask.Elin.ModOptions.UI;
using System.IO;
using System.Reflection;

namespace QuestPicker;

public static class UIController
{
    public static void RegisterUI()
    {
        var assemblyLocation = Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location) ?? string.Empty;
        var xmlPath = Path.Combine(path1: assemblyLocation, path2: "QuestPickerConfig.xml");
        QuestPickerConfig.InitializeXmlPath(xmlPath: xmlPath);

        var xlsxPath = Path.Combine(path1: assemblyLocation, path2: "translations.xlsx");
        QuestPickerConfig.InitializeTranslationXlsxPath(xlsxPath: xlsxPath);
        var controller = ModOptionController.Register(guid: ModInfo.Guid, tooptipId: "mod.tooltip");
        if (controller == null)
        {
            QuestPicker.LogError(message: "Failed to register Mod Options controller.");
            return;
        }

        if (File.Exists(path: QuestPickerConfig.XmlPath))
        {
            controller.SetPreBuildWithXml(xml: File.ReadAllText(path: QuestPickerConfig.XmlPath));
        }
        else
        {
            QuestPicker.LogError(message: $"Mod Options XML not found: {xmlPath}");
        }

        if (File.Exists(path: QuestPickerConfig.TranslationXlsxPath))
        {
            controller.SetTranslationsFromXslx(path: QuestPickerConfig.TranslationXlsxPath);
        }
        else
        {
            QuestPicker.LogError(message: $"Mod Options translations not found: {xlsxPath}");
        }

        RegisterEvents(controller: controller);
    }

    private static void SetTranslations(ModOptionController controller)
    {
        foreach (var questId in QuestPickerConfig.AvailableQuestIds)
        {
            var (name, nameJP, nameCN) = GetQuestNames(id: questId);
            controller.SetTranslation(id: $"{questId}.toggle.tooltip",
                en: name,
                jp: nameJP,
                cn: nameCN);
        }
    }

    public static (string name, string nameJP, string nameCN) GetQuestNames(string id)
    {
        if (EClass.sources.quests.map.TryGetValue(key: id, value: out var questRow))
        {
            string localizedName = GetLocalizedQuestName(questRow: questRow);
            return (name: questRow.name, nameJP: questRow.name_JP, nameCN: localizedName);
        }

        return (name: "Unknown Quest", nameJP: "Unknown Quest", nameCN: "Unknown Quest");
    }

    private static string GetLocalizedQuestName(SourceQuest.Row questRow)
    {
        if (!string.IsNullOrWhiteSpace(value: questRow.name_L))
        {
            return questRow.name_L;
        }

        string localizedName = questRow.GetText(id: "name", returnNull: false);
        if (!string.IsNullOrWhiteSpace(value: localizedName))
        {
            return localizedName;
        }

        return questRow.name;
    }

    private static void RegisterEvents(ModOptionController controller)
    {
        controller.OnBuildUI += builder =>
        {
            SetTranslations(controller: controller);
            foreach (var questId in QuestPickerConfig.AvailableQuestIds)
            {
                var toggle = GetRequiredPreBuild<OptToggle>(builder: builder, id: $"{questId}Toggle");
                if (toggle == null)
                {
                    continue;
                }

                // Prebuilt XML toggles resolve tooltip IDs before OnBuildUI, so refresh the
                // live tooltip after injecting runtime quest-name translations.
                RefreshToggleTooltip(toggle: toggle, controller: controller, questId: questId);
                toggle.Checked = QuestPickerConfig.IsQuestSelected(questId: questId);
                toggle.OnValueChanged += isChecked =>
                {
                    UIEvents.OnToggleValueChanged(questId: questId, isChecked: isChecked);
                };
            }
        };
    }

    private static void RefreshToggleTooltip(OptToggle toggle, ModOptionController controller, string questId)
    {
        string tooltipText = controller.Tr(contentId: $"{questId}.toggle.tooltip");
        if (string.IsNullOrWhiteSpace(value: tooltipText))
        {
            return;
        }

        toggle.Base.tooltip.text = tooltipText;
        toggle.Base.tooltip.enable = true;
    }

    private static T? GetRequiredPreBuild<T>(OptionUIBuilder builder, string id) where T : OptUIElement
    {
        T? element = builder.GetPreBuild<T>(id: id);
        if (element == null)
        {
            QuestPicker.LogError(message: $"Missing Mod Options prebuilt element: {id}");
        }

        return element;
    }
}
