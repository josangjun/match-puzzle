using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Opencoding.CommandHandlerSystem;
using Studio.Common;
using UnityEngine.UI;

namespace Studio.Game
{
    public class StageCheat : MonoBehaviour
    {
#if !FINAL_BUILD
        public void Start()
        {
            CommandHandlers.RegisterCommandHandlers(this);
        }

        private Gem GetFocusedGem()
        {
            if (Stage.Instance.focusedGem.val)
            {
                var g = Stage.Instance.focusedGem.val;
                Stage.Instance.focusedGem.val = null;
                return g;
            }

            var screenPt = Input.mousePosition;
            var go = GameObject.FindWithTag("UICamera");
            var camera = go.GetComponent<Camera>();
            var worldPt = camera.ScreenToWorldPoint(screenPt);
            var root = Stage.Instance.transform;
            worldPt.z = root.position.z;
            var localPt = root.InverseTransformPoint(worldPt);
            return (Gem)(Point)localPt;
        }

        public void SetGemType(string strType)
        {
            System.Enum.TryParse<GemType>(strType, out var type);

            var g = GetFocusedGem();
            if (g)
                g.type = type;
        }

        public void Explode()
        {
            var g = GetFocusedGem();
            if (g)
                Stage.Instance.Explode(new List<Gem> { g });
        }

        public void Fall()
        {
            var g = GetFocusedGem();
            if (g)
                g.Fall();
        }

        [ConsoleCommand]
        public void DumpBoard()
        {
            var txt = Stage.Instance.Dump();
            SLogger.Log(txt);
            FileTool.Write("board.txt", txt);
        }

        [ConsoleCommand]
        public void Shuffle()
        {
            Stage.Instance.Shuffle();
        }

        [ConsoleCommand]
        public void HasMove()
        {
            var match = Stage.Instance.judge.Match();
            var moves = Stage.Instance.judge.CollectMove();
            SLogger.Log($"hasMove:{moves.ToJson()}, match:{match}");
        }

        [ConsoleCommand]
        public void Auto(bool tf)
        {
            Stage.Instance.simulator.enabled = tf;
        }

        public void ToggleAuto()
        {
            var tf = !Stage.Instance.simulator.enabled;
            Stage.Instance.simulator.enabled = tf;
        }

        [ConsoleCommand]
        public void AutoInterval(float interval)
        {
            Stage.Instance.simulator.interval = interval;
        }

        [ConsoleCommand]
        public void Fps(int frameRate)
        {
            PlayerPrefs.SetInt("fps", frameRate);
            Application.targetFrameRate = frameRate;
        }
#endif
    }
}