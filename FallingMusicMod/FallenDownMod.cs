using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace FallingMusicMod
{
    [BepInPlugin("com.holycross98.fallendownmod", "Fallen Down Mod", "1.0.0")]
    public class FallenDownMod : BaseUnityPlugin
    {
        private static FallenDownMod _instance;
        private static AudioSourcePlayer _audioPlayer;
        private static bool _isFallenDownMusicPlaying = false;

        void Awake()
        {
            _instance = this;

            var harmony = new Harmony("com.holycross98.fallendownmod.harmony");
            harmony.PatchAll();

            var audioPlayerGo = new GameObject("FallenDownAudioPlayer");
            DontDestroyOnLoad(audioPlayerGo);
            _audioPlayer = audioPlayerGo.AddComponent<AudioSourcePlayer>();

            Logger.LogInfo("Fallen Down Mod has been loaded!");
        }

        [HarmonyPatch(typeof(Character), "Update")]
        class FallenDownPatch
        {
            [HarmonyPostfix]
            static void PostFix(Character __instance) 
            {
                bool isOutOfStamina = __instance.data.currentStamina < 0.005f && __instance.data.extraStamina < 0.001f;
                bool noStaminaFall = isOutOfStamina && __instance.data.avarageVelocity.y < -9.0f;
                bool isDeadlyFall = __instance.data.avarageVelocity.y < -13.0f;
                bool afflictionsInRange = __instance.refs.afflictions.statusSum < 1f;

                if (!__instance.IsLocal)
                {
                    return;
                }
//Se la stamina finisce e averagevelocity.y < -5.0 allora la musica parte altrimenti average velocity < -10.0
                if (!__instance.data.isGrounded && (noStaminaFall || isDeadlyFall) && !_isFallenDownMusicPlaying)
                { 
                    _audioPlayer.PlayOrResume();
                    _isFallenDownMusicPlaying = true;
                    _instance.Logger.LogInfo("Started playing Fallen Down music.");
                }


                if(__instance.data.isGrounded && _isFallenDownMusicPlaying && (afflictionsInRange || __instance.data.deathTimer > 0.01f))
                {

                    _audioPlayer.InitiateFadeOut();
                    _isFallenDownMusicPlaying = false;
                    _instance.Logger.LogInfo("Stopped playing Fallen Down music. ");
                }
            }
        }

    }
}
