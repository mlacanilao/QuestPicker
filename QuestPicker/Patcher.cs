using HarmonyLib;

namespace QuestPicker;

internal static class Patcher
{
    [HarmonyPrefix]
    [HarmonyPatch(declaringType: typeof(LayerQuestBoard), methodName: nameof(LayerQuestBoard.RefreshQuest))]
    public static void LayerQuestBoardRefreshQuest()
    {
        LayerQuestBoardPatch.LayerQuestBoardRefreshQuestPrefix();
    }
}