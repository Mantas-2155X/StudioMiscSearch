using BepInEx;
using HarmonyLib;
using Studio;

namespace HS2_StudioMiscSearch
{
    [BepInProcess("StudioNEOV2")]
    [BepInPlugin(nameof(HS2_StudioMiscSearch), nameof(HS2_StudioMiscSearch), VERSION)]
    public class HS2_StudioMiscSearch : BaseUnityPlugin
    {
        public const string VERSION = "1.2.1";

        public static MapList mapList;
        public static BackgroundList backgroundList;
        
        public static BGMCtrl bgmControl;
        public static ENVCtrl envControl;
        public static OutsideSoundCtrl externalControl;

        private void Awake()
        {
            Harmony.CreateAndPatchAll(typeof(HS2_StudioMiscSearch));
        }

        [HarmonyPostfix, HarmonyPatch(typeof(Studio.Studio), "Init")]
        private static void Studio_Init_Postfix(Studio.Studio __instance, BackgroundList ___m_BackgroundList)
        {
            mapList = __instance.mapList;
            backgroundList = ___m_BackgroundList;
            bgmControl = __instance.bgmCtrl;
            envControl = __instance.envCtrl;
            externalControl = __instance.outsideSoundCtrl;
            
            Tools.CreateUI();
        }
    }
}