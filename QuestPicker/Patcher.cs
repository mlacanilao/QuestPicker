using QuestPicker.Patches;
using HarmonyLib;

namespace QuestPicker
{
    public class Patcher
    {
        [HarmonyPrefix]
        [HarmonyPatch(declaringType: typeof(LayerQuestBoard), methodName: nameof(LayerQuestBoard.RefreshQuest))]
        public static void LayerQuestBoardRefreshQuest(LayerQuestBoard __instance)
        {
            LayerQuestBoardPatch.LayerQuestBoardRefreshQuestPrefix(__instance: __instance);
        }
    }
}