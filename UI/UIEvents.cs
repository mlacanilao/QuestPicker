namespace QuestPicker;

internal static class UIEvents
{
    public static void OnToggleValueChanged(string questId, bool isChecked)
    {
        QuestPickerConfig.SetQuestSelected(questId: questId, isSelected: isChecked);
    }
}
