using HarmonyLib;
using PulsarModLoader;
using PulsarModLoader.CustomGUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;

namespace RandomReflections
{
    public class Mod : PulsarMod
    {
        public override string Version => "0.0.1";
        public override string Author => "Mest";
        public override string Name => "RandomReflections";
        public override string HarmonyIdentifier() => "Mest.RandomReflections";
        internal class Config : ModSettingsMenu
        {
            public static SaveValue<bool> Enabled = new SaveValue<bool>("Enabled", true);
            public static SaveValue<float> ReflectionDelay = new SaveValue<float>("Reflection Delay", 30);
            public static SaveValue<bool> RandomReflections = new SaveValue<bool>("Random Reflections", false);
            public static SaveValue<float> RandomMaximum = new SaveValue<float>("Random Maximum", 60);
            public static SaveValue<float> RandomMinimum = new SaveValue<float>("Random Minimum", 10);
            public override string Name() => $"Random Reflections";
            public override void Draw()
            {
                GUILayout.BeginHorizontal();
                Enabled.Value = GUILayout.Toggle(Enabled.Value, "Mod Enabled");
                GUILayout.Label("Values will <b>ONLY AFFECT</b> players <b>AS HOST</b> and will <b>TAKE EFFECT</b> after the <b>NEXT REFLECTION</b>!");
                GUILayout.EndHorizontal();
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b><color=yellow>= = = Consistent Reflections = = =</color></b>");
                TimePicker("Reflection Delay", ref ReflectionDelay, 5, 3600);
                GUILayout.EndVertical();
                GUILayout.BeginVertical("Box");
                GUILayout.Label("<b><color=yellow>= = = Random Reflections = = =</color></b>");
                RandomReflections.Value = GUILayout.Toggle(RandomReflections.Value, "Reflect Randomly");
                TimePicker("Random Minimum", ref RandomMinimum, 5f, RandomMaximum - 1);
                TimePicker("Random Maximum", ref RandomMaximum, RandomMinimum.Value + 1, 3600);
                GUILayout.EndVertical();
            }
            private static void TimePicker(string Name, ref SaveValue<float> SavedTime, float Minimum, float Maximum)
            {
                GUILayout.BeginHorizontal();
                TimeSpan ts = TimeSpan.FromSeconds(SavedTime.Value);
                GUILayout.Label($"{Name}: {ts}");
                if (GUILayout.Button("<", GUILayout.Width(20)) && SavedTime.Value - 100 >= Minimum) SavedTime.Value -= 100;
                if (GUILayout.Button("<", GUILayout.Width(20)) && SavedTime.Value - 10 >= Minimum) SavedTime.Value -= 10;
                if (GUILayout.Button("<", GUILayout.Width(20)) && SavedTime.Value - 1 >= Minimum) SavedTime.Value -= 1;
                GUILayout.Label(Minimum.ToString(), GUILayout.Width(30));
                SavedTime.Value = (int)GUILayout.HorizontalSlider(SavedTime.Value, Minimum, Maximum);
                GUILayout.Label(Maximum.ToString(), GUILayout.Width(30));
                if (GUILayout.Button(">", GUILayout.Width(20)) && SavedTime.Value + 1 <= Maximum) SavedTime.Value += 1;
                if (GUILayout.Button(">", GUILayout.Width(20)) && SavedTime.Value + 10 <= Maximum) SavedTime.Value += 10;
                if (GUILayout.Button(">", GUILayout.Width(20)) && SavedTime.Value + 100 <= Maximum) SavedTime.Value += 100;
                GUILayout.EndHorizontal();
            }
        }
    }
    [HarmonyPatch(typeof(PLServer), "Start")]
    internal class RandomReflectionPatch
    {
        public static void Postfix()
        {
            if (PhotonNetwork.isMasterClient)
            {
                PLServer.Instance.StartCoroutine(ReflectRandomly());
            }
        }
        private static IEnumerator ReflectRandomly()
        {
            while (PLServer.Instance != null && PhotonNetwork.isMasterClient)
            {
                if (!Mod.Config.Enabled) yield return new WaitForEndOfFrame();
                else if (Mod.Config.RandomReflections.Value) yield return new WaitForSeconds(UnityEngine.Random.Range(Mod.Config.RandomMinimum.Value, Mod.Config.RandomMaximum.Value + 1f));
                else yield return new WaitForSeconds(Mod.Config.ReflectionDelay.Value);
                if (Mod.Config.Enabled) PLServer.Instance.IsReflection = !PLServer.Instance.IsReflection;
            }
        }
    }
}
