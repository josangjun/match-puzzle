using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Opencoding.CommandHandlerSystem;
using Studio.Common;
using System.Linq;

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

        public void SetGemType(int typeInt)
        {
            var type = (GemType)typeInt;

            var g = GetFocusedGem();
            if (g)
                g.type = type;
        }

        public void SetGemType(string typeStr)
        {
            if (!System.Enum.TryParse<GemType>(typeStr, out var type))
                return;
            SetGemType((int)type);
        }

        public static IEnumerable<string> _AutoGemType() => 
            ((GemType[])System.Enum.GetValues(typeof(GemType)))
            .Where(arg => arg > GemType.None && arg < GemType.Max).Select(arg => arg.ToString()).ToArray();

        [ConsoleCommand]
        public void SetGemColor([Autocomplete(typeof(StageCheat), "_AutoGemType")] string type, string colorCode)
        {
            System.Enum.TryParse<GemType>(type, out var t);
            if (t == GemType.None)
                return;
            if (!ColorUtility.TryParseHtmlString(colorCode, out var color))
            {
                SLogger.LogError($"invalid colorCode:{colorCode}");
                return;
            }
            var info = Stage.Instance.gemFactory.visualizer.typeInfos.FirstOrDefault(arg => arg.type == t);
            if (info != null)
            {
                info.color = color;
                SLogger.Log($"type:{t}, colorCode:{colorCode}");
            }
        }

        [ConsoleCommand]
        public void LoadFromWeb()
        {
            Stage.Instance.LoadFromWeb();
        }

        public void Explode()
        {
            var g = GetFocusedGem();
            if (g)
                Stage.Instance.Explode(g);
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
            Stage.Instance.automator.enabled = tf;
        }

        public void ToggleAuto()
        {
            var tf = !Stage.Instance.automator.enabled;
            Stage.Instance.automator.enabled = tf;
        }

        [ConsoleCommand]
        public void AutoInterval(float interval)
        {
            Stage.Instance.automator.interval = interval;
        }

        [ConsoleCommand]
        public void Fps(int frameRate)
        {
            PlayerPrefs.SetInt("fps", frameRate);
            Application.targetFrameRate = frameRate;
        }

        [ConsoleCommand]
        public void GravityScale(float scale)
        {
            Stage.config.gravityScale = scale;
        }

        [ConsoleCommand]
        public void TerminalVelocity(float velocity)
        {
            Stage.config.terminalVelocity = velocity;
        }
#endif
    }
}