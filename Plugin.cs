using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace COM3D2.HidePenis
{
    [BepInPlugin("COM3D2.HidePenis", "Hide Penis", "1.0.0.0")]
    public class HidePenis : BaseUnityPlugin
    {
        internal static ManualLogSource logger;
        private static Tools tools = new Tools();

        private static ConfigEntry<KeyCode> hidePenisShortcut;
        private static ConfigEntry<bool> overrideChinko;
        private static ConfigEntry<bool> globaloverrideChinko;
        private static ConfigEntry<bool> rememberPenisState;
        private static ConfigEntry<bool> penisStartsHidden;
        
        private static bool isYotogi;        
        internal static bool isPenisVisible = true;
        internal int timer = 0;


        private void Awake()
        {
            //Config
            hidePenisShortcut = Config.Bind("Shortcuts", "Key", KeyCode.P, "Hide Penis Key");
            overrideChinko = Config.Bind("Yotogi InOut Compatibility", "Value", true, "Overrides the game's Method in Yotogi");
            globaloverrideChinko = Config.Bind("Global InOut Compatibility", "Value", false, "Overrides the game's Method everywhere");
            rememberPenisState = Config.Bind("Rember Penis state", "Value", false, "remember Penis hidden state across game sessions");
            penisStartsHidden = Config.Bind("Penis starts hidden", "Value", false, "Penis starts hidden");

            //Harmony
            Harmony.CreateAndPatchAll(typeof(HidePenis));

            //BepinEx Logger
            HidePenis.logger = base.Logger;

            tools = new Tools();

            SceneManager.sceneLoaded += OnSceneLoaded;

            isPenisVisible = !penisStartsHidden.Value;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            isYotogi = scene.buildIndex == 14;
        }

        private void Update()
        {
            if (Input.GetKeyDown(hidePenisShortcut.Value))
            {
                isPenisVisible = !isPenisVisible;

                if (rememberPenisState.Value)
                {
                    penisStartsHidden.Value = !isPenisVisible;
                }

                logger.LogMessage($"Change Penis Visibility");
                tools.ChangeChinkoState(isPenisVisible);
            }
            if (timer == 30)
            {
                timer = 0;
                tools.ChangeChinkoState(isPenisVisible);
            }
            timer++;
        }


        // Override this Method to bypass InOut check
        [HarmonyPostfix]
        [HarmonyPatch(typeof(TBody), nameof(TBody.GetChinkoVisible))]        
        public static void OverrideGetChinkoVisible(ref bool __result)
        {           
            if ((overrideChinko.Value && isYotogi) || globaloverrideChinko.Value)
            {
                __result = true;
            }
        }
    }

    public class Tools
    {
        internal Maid GetMan()
        {
            int manCount = GameMain.Instance.CharacterMgr.GetManCount();
            Maid man = null;

            if (manCount > 0)
            {
                man = GameMain.Instance.CharacterMgr.GetMan(0);
            }
            return man;
        }

        internal void ChangeChinkoState(bool state)
        {
            Maid man = GetMan();
            if (man != null)
            {
                man.body0.SetChinkoVisible(state);
                //HidePenis.logger.LogWarning($"man callName = {man.status.callName}");
            }
            else
            {
                //HidePenis.logger.LogWarning("No Man found");
            }
        }
    }
}
