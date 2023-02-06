using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Studio.Framework;
using Studio.Common;
using UnityEngine.EventSystems;
using System.Linq;
using Studio.Game.Commands;
using UnityEngine.Pool;
using GoogleSheetsToUnity;

namespace Studio.Game
{
    public class Stage : Singleton<Stage>
    {
        public const int COL = 9;
        public const int ROW = 11;
        public const int MATCH = 3;

        [SerializeField]
        private GemFactory _gemFactory;
        public GemFactory gemFactory => _gemFactory;
        [SerializeField]
        private AppConfig _config;
        public static AppConfig config { get; private set; }
        
        private Gem[,] _board = new Gem[COL, ROW];

        public int GemTypeCount => _config.gemTypeCount;

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

        protected override void Awake()
        {
            base.Awake();
            config = _config;
        }

        // Start is called before the first frame update
        IEnumerator Start()
        {
            if (!Service.ready)
                yield return Service.Init();

            yield return GoogleSheetBridge.UpdateGSTU(gameObject);

            gemFactory.onInit.Subscribe(OnInit).AddTo(this);
            yield return gemFactory.Prepare().ToObservable().ToYieldInstruction();

            var gems = new List<Gem>();
            var count = COL * ROW;
            for (var i = 0; i < count; ++i)
            {
                var g = gemFactory.Produce();
                gems.Add(g);
            }
            ArrangePosition(gems);

            Application.targetFrameRate = PlayerPrefs.GetInt("fps", 60);
        }
               
        private void ArrangePosition(List<Gem> gems)
        {
            var g = gems[0];
            var size = g.rectTransform.sizeDelta;
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
                    g = gems[i];
                    g.transform.SetSiblingIndex(i);
                    g.SetPosition(x, y);
                    ++i;
                }
            }

            while (judge.Match())
            {
                Shuffle(true);
            }
        }

        public class FocusedGem
        {
            private Gem _g;

            public Gem val
            {
                get => _g;
                set
                {
                    if (_g)
                        _g.image.SetAlpha(1f);
                    _g = value;
                    if (_g)
                    {
                        _g.image.SetAlpha(0.7f);
                        _g.onExplosion.TakeUntilDisable(_g).Take(1).Subscribe(_ =>
                        {
                            this.val = null;
                        });
                    }
                }
            }
        }

        public readonly FocusedGem focusedGem = new FocusedGem();

        private void OnInit(Gem g)
        {
            g.onPress.TakeUntilDisable(g).Subscribe(OnPress).AddTo(this);
            g.onDrag.TakeUntilDisable(g).Subscribe(OnDrag).AddTo(this);
            g.onClick.TakeUntilDisable(g).Subscribe(OnClick).AddTo(this);
        }

        public void OnExplosion(List<Point> points)
        {
            points.Sort((lhs, rhs) =>
            {
                var cmp = lhs.x.CompareTo(rhs.x);
                if (cmp == 0)
                    cmp = rhs.y.CompareTo(lhs.y);
                return cmp;
            });

            int c = 1;
            foreach (var point in points)
            {
                for (var i = point.y - 1; i >= 0; --i)
                {
                    var g = this[point.x, i];
                    if (g == null)
                        continue;
                    g.Fall();
                }
                commands.AddLast(new ProduceGem
                {
                    x = point.x,
                    y = -1 * c++,
                    type = gemFactory.NewRandomGemType()
                });
            }
        }

        private void OnDrag(PointerEventData eventData)
        {
            var diff = eventData.position - eventData.pressPosition;
            
            if (diff.sqrMagnitude < 600)
                return;

            focusedGem.val = null;
            var g = eventData.pointerDrag.GetComponent<Gem>();
            if (g == null)
                return;
            var h = (Mathf.Abs(diff.x) > Mathf.Abs(diff.y)) ?
                g.GetNext(diff.x < 0 ? Direction.Left : Direction.Right) :
                g.GetNext(diff.y > 0 ? Direction.Up : Direction.Down);
            Swap(g, h);
        }

        private void OnPress(Gem g)
        {
            SLogger.Log(g);
            if (focusedGem.val == null)
            {
                focusedGem.val = g;
                return;
            }

            var lhs = focusedGem.val;
            var rhs = g;
            var tf = lhs.IsAdjacency(rhs);
            focusedGem.val = g;
#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.LeftShift))
                return;
#endif
            if (tf)
            {
                Swap(lhs, rhs);
            }
            SLogger.Log($"adjacency:{lhs}, {rhs}, {tf}");
        }

        private void OnClick(Gem g)
        {
            if (g.type < GemType.Max)
                return;

            Explode(g);
        }

        public void Swap(Gem lhs, Gem rhs)
        {
            if (lhs == null || rhs == null || lhs.IsLocked() || rhs.IsLocked())
                return;

            lhs.touchTime = rhs.touchTime = Time.time;
            var cmd = new SwapGem
            {
                lhs = lhs,
                rhs = rhs
            };
            cmd.onComplete.Take(1).Subscribe(_ =>
            {
                if (lhs.type > GemType.Max)
                    Explode(lhs);
                if (rhs.type > GemType.Max)
                    Explode(rhs);
                if (!judge.MatchAndExplode())
                {
                    var ret = new SwapGem
                    {
                        lhs = rhs,
                        rhs = lhs
                    };
                    commands.AddLast(ret);
                    ret.onComplete.Take(1).Subscribe(_ =>
                    {
                        judge.MatchAndExplode();
                        if (!lhs.IsFloor())
                            lhs.Fall(); 
                        if (!rhs.IsFloor())
                            rhs.Fall();
                    });
                }
            });
            commands.AddLast(cmd);
        }

        public void Explode(Gem gem)
        {
            var list = ListPool<Gem>.Get();
            list.Add(gem);
            Explode(list);
            ListPool<Gem>.Release(list);
        }

        public void Explode(List<Gem> gems)
        {
            if (gems.Count == 0)
                return;

            var list = ListPool<Point>.Get();
            var count = 0;
            foreach (var g in gems)
            {
                var p = (Point)g;
                
                if (!list.Contains(p))
                    list.Add(p);
                
                if (!g.Explode())
                    continue;

                ++count;
                var h = g.onExplosion.Take(1).
                    Subscribe(_ =>
                    {
                        if (0 == --count)
                        {
                            OnExplosion(list);
                            ListPool<Point>.Release(list);
                        }
                    });
            }
        }

        public readonly LinkedList<ICommand> commands = new LinkedList<ICommand>();

        public readonly BatchJob job = new BatchJob();

        public readonly Judge judge = new Judge();

        [SerializeField]
        public Automator automator = new Automator();

        public void Update()
        {
            automator.Update();
        }

        // Update is called once per frame
        public void LateUpdate()
        {
            while (commands.Count > 0)
            {
                var cmd = commands.First.Value;
                if (!cmd.IsRunning())
                    cmd.Run();
                if (cmd.IsRunning())
                    break;
                commands.Remove(cmd);
            }
            if (!job.IsProcessing && job.Count > 0)
                StartCoroutine(job.Process());
        }

        private void SortGems(List<Gem> gems)
        {
            gems.Sort((lhs, rhs) =>
            {
                var cmp = lhs.x.CompareTo(rhs.x);
                if (cmp == 0)
                    cmp = lhs.y.CompareTo(rhs.y);
                return cmp;
            });

            int j = 0;
            for (var i = 0; i < gems.Count; ++i)
            {
                if (gems[i] == null)
                    continue;
                gems[i].transform.SetSiblingIndex(j++);
            }
        }

        [ContextMenu("Dump")]
        public string Dump()
        {
            var gems = new List<Gem>();
            var sb = new System.Text.StringBuilder();
            for (var i = 0; i < ROW; ++i)
            {
                for (var j = 0; j < COL; ++j)
                {
                    var g = this[j, i];
                    if (g)
                    {
                        gems.Add(g);
                        sb.Append(g.type.ToString()[0]).Append(' ');
                    }
                    else
                    {
                        sb.Append('_').Append(' ');
                    }
                }
                sb.AppendLine();
            }
            SortGems(gems);
            var txt = sb.ToString();
            return txt;
        }

        public void Shuffle(bool matchOnly = false)
        {
            if (!matchOnly)
            {
                foreach (var g in _board)
                {
                    if (g == null)
                        continue;
                    g.type = gemFactory.NewRandomGemType();
                }
            }

            var t = Time.realtimeSinceStartup;
            while (judge.Match())
            {
                do
                {
                    foreach (var s in judge.sets)
                    {
                        foreach (var g in s.gems)
                            g.type = gemFactory.NewRandomGemType();
                    }
                    var elapsed = Time.realtimeSinceStartup - t;
                    if (elapsed > 5f)
                    {
                        SLogger.LogError("shuffle timeout");
                        break;
                    }
                }
                while (!judge.HasMove());
                
            }
        }

        private MEC.CoroutineHandle hMatch;

        public void MatchAndExplode()
        {
            IEnumerator<float> M_()
            {
                yield return MEC.Timing.WaitForOneFrame;
                judge.MatchAndExplode();
            }
            hMatch = MEC.Timing.RunCoroutineSingleton(M_(), hMatch, MEC.SingletonBehavior.Overwrite);
        }
    }
}