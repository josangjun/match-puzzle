using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Studio.Framework;
using Studio.Common;

namespace Studio.Game
{
    public class Stage : Singleton<Stage>
    {
        public const int COL = 8;
        public const int ROW = 8;
        public const int MATCH = 3;

        [SerializeField]
        private GemFactory _gemFactory;
        public GemFactory gemFactory => _gemFactory;

        private List<Gem> _gems = new List<Gem>();

        private Gem[,] _board = new Gem[COL, ROW];

        public Gem this[int x, int y]
        {
            get
            {
                if (x < 0 || x >= COL ||
                    y < 0 || y >= ROW)
                    return default;
                return _board[x, y];
            }
            set
            {
                if (x < 0 || x >= COL ||
                    y < 0 || y >= ROW)
                    return;
                _board[x, y] = value;
            }
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (!Service.ready)
                yield return Service.Init();

            gemFactory.onInit.Subscribe(OnInit).AddTo(this);
            yield return gemFactory.Prepare().ToObservable().ToYieldInstruction();

            var count = COL * ROW;
            for (var i = 0; i < count; ++i)
            {
                gemFactory.Produce();
            }
            
            ArrangePosition();
        }

        private void ArrangePosition()
        {
            var g = _gems[0];
            var size = g.rect.sizeDelta;
            var board = (RectTransform)gemFactory.transform;
            var offset = board.sizeDelta * 0.5f;
            offset.x -= size.x * 0.5f;
            offset.y -= size.y * 0.5f;

            Gem.Init(size, offset);
            
            int i = 0;
            for (var x = 0; x < COL; ++x)
            {
                for (var y = 0; y < ROW; ++y)
                {
                    g = _gems[i];
                    g.transform.SetSiblingIndex(i);
                    g.SetPosition(x, y);
                    ++i;
                }
            }
        }

        private class PressedGem
        {
            private Gem _g;

            public Gem val
            {
                get => _g;
                private set
                {
                    if (_g)
                        _g.image.SetAlpha(1f);
                    _g = value;
                    if (_g)
                        _g.image.SetAlpha(0.7f);
                }
            }

            public void Set(Gem j)
            {
                val = j;
            }
        }

        private PressedGem pressedGem = new PressedGem();

        private void OnInit(Gem g)
        {
            g.onPress.TakeUntilDisable(g).Subscribe(OnPress).AddTo(this);
            _gems.Add(g);
        }

        private void OnPress(Gem g)
        {
            SLogger.Log(g);
            if (pressedGem.val == null)
            {
                pressedGem.Set(g);
                return;
            }

            var lhs = pressedGem.val;
            var rhs = g;
            var tf = lhs.IsAdjacency(rhs);
            pressedGem.Set(null);
            if (tf)
            {
                commands.AddFirst(new Commands.SwapGem
                {
                    lhs = lhs,
                    rhs = rhs
                });
            }
            SLogger.Log($"adjacency:{lhs}, {rhs}, {tf}");
        }

        public readonly LinkedList<ICommand> commands = new LinkedList<ICommand>();

        public readonly BatchJob job = new BatchJob();

        // Update is called once per frame
        public void Update()
        {
            while (commands.Count > 0)
            {
                var cmd = commands.First.Value;
                if (!cmd.IsRunning())
                    cmd.Run();
                if (!cmd.IsRunning())
                    commands.Remove(cmd);
            }
            if (!job.IsProcessing && job.Count > 0)
                StartCoroutine(job.Process());
        }

        public readonly Judge judge = new Judge();
    }
}