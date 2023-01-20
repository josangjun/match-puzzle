using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Studio.Framework;
using DG.Tweening;
using System.Linq;

public class SEGuidePath : SubEffect
{
    [SerializeField]
    private float _duration = 1.5f;
    public float duration => _duration;
    [SerializeField][Range(0, 1)]
    private float _midPointmodifier = 0.5f;
    [SerializeField] private List<Vector3> _points;
    public Vector3 endPoint { get => _points.LastOrDefault(); private set => _points.Add(value); }
    public void ClearPoints() { _points.Clear(); }
    public void SetEndPoint(Vector3 pos)
    {
        if (_points.Count > 0)
        {
            _points[_points.Count - 1] = pos;
            return;
        }
        endPoint = pos;
    }
    public override void Play(GameObject owner)
    {
        StartCoroutine(PlayAsync());
    }

    private IEnumerator PlayAsync()
    {
        yield return null;
        var start = transform.localPosition;
        var midPoint = new Vector3();
        midPoint.x = Mathf.Lerp(start.x, endPoint.x, Random.Range(_midPointmodifier, 1f));
        midPoint.y = Mathf.Lerp(start.y, endPoint.y, Random.Range(0.0f, _midPointmodifier));
        transform.DOLocalPath(new Vector3[] {
            //start,
            midPoint,
            endPoint
        }, _duration, PathType.CatmullRom).SetDelay(1/60f);
    }
}
