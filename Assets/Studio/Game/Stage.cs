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
        [SerializeField]
        private Lanes lanes = new Lanes();

        public static Lanes Lanes => Instance.lanes;

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

            gemFactory.onInit.Subscribe(OnInit).AddTo(this);
            if (config.syncSheet)
                yield return LoadFromWeb();

            yield return gemFactory.Prepare().ToObservable().ToYieldInstruction();

            var gems = new List<Gem>();
            var count = COL * ROW;
            for (var i = 0; i < count; ++i)
            {
                var g = gemFactory.Produce();
                gems.Add(g);
            }
            ArrangePosition(gems);

            _paused.Subscribe(tf => FreeFall.Pause(tf)).AddTo(this);
            _pauseLock.AsObservable().Subscribe(l =>
            {
                _paused.Value = l.count > 0;
            }).AddTo(this);

            Application.targetFrameRate = PlayerPrefs.GetInt("fps", 60);
        }

        public Coroutine LoadFromWeb()
        {
            IEnumerator Batch_()
            {
                yield return GoogleSheetBridge.UpdateGSTU(gameObject);

                config.OnValidate();
                for (var i = 0; i < gemFactory.visualizer.typeInfos.Count; ++i)
                {
                    var info = gemFactory.visualizer.typeInfos[i];
                    var index = (int)info.type;
                    var fieldInfo = config.GetType().GetField($"gem{index}");
                    if (fieldInfo == null)
                        continue;
                    var strColor = (string)fieldInfo.GetValue(config);
                    if (ColorUtility.TryParseHtmlString(strColor, out var color))
                    {
                        info.color = color;
                    }
                }
            }
            return StartCoroutine(Batch_());
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

        public void AddLane(int index, Gem gem)
        {
            lanes[index].Add(gem);
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

        public int moveCount
        {
            get => _moveCount.Value;
            set => _moveCount.Value = value;
        }

        [SerializeField]
        private ReactiveProperty<int> _moveCount = new ReactiveProperty<int>();
        [SerializeField]
        private ReactiveProperty<bool> _paused = new ReactiveProperty<bool>();

        public bool paused
        {
            get => _paused.Value;
            set => _paused.Value = value;
        }

        private CountingLock _pauseLock = new();

        public System.IDisposable Pause()
        {
            return _pauseLock.Lock();
        }

        public System.IObservable<int> OnMoveCountAsObservable() => _moveCount;

        public System.IObservable<int> OnMoveCountZeroAsObservable()
        {
            return _moveCount.Where(c => c == 0).Take(1);
        }

        public readonly FocusedGem focusedGem = new FocusedGem();

        private void OnInit(Gem g)
        {
            g.onPress.TakeUntilDisable(g).Subscribe(OnPress).AddTo(this);
            g.onDrag.TakeUntilDisable(g).Subscribe(OnDrag).AddTo(this);
            g.onClick.TakeUntilDisable(g).Subscribe(OnClick).AddTo(this);
        }

        public void OnExplosion()
        {
            commands.AddLast(ProduceGem.Default);
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
            SLogger.Log($"{g}", g);
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
            //SLogger.Log($"adjacency:{lhs}, {rhs}, {tf}");
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
            commands.AddLast(cmd);
        }

        public bool Explode(Gem gem)
        {
            if (gem == null || gem.IsLocked())
                return false;
            var list = ListPool<Gem>.Get();
            list.Add(gem);
            var ret = Explode(list);
            ListPool<Gem>.Release(list);
            return ret > 0;
        }

        public int Explode(List<Gem> gems)
        {
            if (gems.Count == 0)
                return 0;

            var count = 0;
            foreach (var g in gems)
            {
                var p = (Point)g;

                if (g.IsLocked())
                    continue;

                if (!g.Explode())
                    continue;

                ++count;
                var h = g.onExplosion.Take(1).
                    Subscribe(_ =>
                    {
                        if (0 == --count)
                        {
                            OnExplosion();
                        }
                    });
            }
            return count;
        }

        public readonly LinkedList<ICommand> commands = new LinkedList<ICommand>();

        private readonly BatchJob _job = new BatchJob();

        public static BatchJob job => Instance._job;

        private readonly Judge _judge = new Judge();

        public static Judge judge => Instance._judge;

        [SerializeField]
        public Automator automator = new Automator();

        public void Update()
        {
            if (!_paused.Value)
            {
                automator.Update();
                lanes.Update();
            }
            FreeFall.Update();
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
            //UnityEngine.Assertions.Assert.IsTrue(gemFactory.transform.childCount == COL * ROW);
            var parent = gemFactory.transform;
            for (var i = 0; i < parent.childCount; ++i)
            {
                var child = parent.GetChild(i);
                var g = child.GetComponent<Gem>();
                var q = this[g.x, g.y];
                if (g == null)
                    continue;
                if (g.y < 0)
                {
                    SLogger.LogError($"stuck : {g}", g);
                }
                else if (!g.Equals(q))
                {
                    SLogger.LogError($"mismatch : {g}, {q}", g);
                }
            }
        }

        [ContextMenu("Dump")]
        public string Dump()
        {
            var gems = ListPool<Gem>.Get();
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
            ListPool<Gem>.Release(gems);
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

        private void OnDisable()
        {
            hMatch.IsRunning = false;
        }

        private MEC.CoroutineHandle hMatch;

        public void MatchAndExplode()
        {
            IEnumerator<float> M_()
            {
                yield return MEC.Timing.WaitForOneFrame;
                judge.MatchAndExplode();
            }
#if UNITY_EDITOR
            if (!this)
                return;
#endif
            hMatch = MEC.Timing.RunCoroutineSingleton(M_(), hMatch, MEC.SingletonBehavior.Overwrite);
        }
    }
}