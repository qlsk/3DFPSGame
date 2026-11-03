using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 배경 하늘에 지그재그 번개 줄기를 그리고, 화면을 짧게 두 번 번쩍이게 한다.
public class UI_LightningEffect : MonoBehaviour
{
    [Header("화면 번쩍임")]
    [SerializeField] private Image _flashImage;
    [SerializeField] private float _flashAlpha = 0.45f;
    [SerializeField]
    private AnimationCurve _intensityCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.05f, 0.25f),
        new Keyframe(0.1f, 0.85f),
        new Keyframe(0.16f, 0.3f),
        new Keyframe(0.5f, 0f));

    [Header("번개 줄기 (카메라 기준 위치)")]
    [SerializeField] private float _distance = 40f;
    [SerializeField] private float _topMinY = 18f;
    [SerializeField] private float _topMaxY = 22f;
    [SerializeField] private float _bottomY = 2f;
    [SerializeField] private float _sideMinX = 22f;
    [SerializeField] private float _sideMaxX = 34f;
    [SerializeField] private int _segmentCount = 14;
    [SerializeField] private float _jaggedness = 1.6f;

    [Header("번개 모양")]
    [SerializeField] private float _boltWidth = 0.4f;
    [SerializeField] private float _glowWidth = 2f;
    [SerializeField] private float _glowAlpha = 0.45f;
    [SerializeField] private Color _boltColor = new Color(0.85f, 0.92f, 1f, 1f);

    private LineRenderer _bolt;
    private LineRenderer _glow;
    private LineRenderer _branch;
    private Coroutine _strikeCoroutine;

    public bool IsStriking => _strikeCoroutine != null;

    private void Awake()
    {
        // 번개 줄기는 실행할 때 만든다. 셰이더는 항상 빌드에 포함되는 Sprites/Default를 쓴다.
        Material material = new Material(Shader.Find("Sprites/Default"));
        _bolt = CreateLine("Bolt", material, 2);
        _glow = CreateLine("BoltGlow", material, 1);
        _branch = CreateLine("Branch", material, 2);

        SetIntensity(0f);
    }

    private void OnDisable()
    {
        _strikeCoroutine = null;
        SetIntensity(0f);
    }

    public void Strike()
    {
        if (_strikeCoroutine != null)
        {
            StopCoroutine(_strikeCoroutine);
        }

        _strikeCoroutine = StartCoroutine(Strike_Coroutine());
    }

    private IEnumerator Strike_Coroutine()
    {
        // 1. 매번 다른 모양의 번개를 만든다.
        BuildBolt();

        // 2. 밝기 곡선을 따라 번쩍였다가 사라진다.
        float duration = _intensityCurve.keys[_intensityCurve.length - 1].time;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            SetIntensity(Mathf.Clamp01(_intensityCurve.Evaluate(elapsedTime)));
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        SetIntensity(0f);
        _strikeCoroutine = null;
    }

    private void BuildBolt()
    {
        // 1. 화면 가운데 패널을 피해 왼쪽 또는 오른쪽 하늘에서 시작한다.
        float side = Random.value < 0.5f ? -1f : 1f;
        float x = side * Random.Range(_sideMinX, _sideMaxX);
        float topY = Random.Range(_topMinY, _topMaxY);

        // 2. 아래로 내려가며 좌우로 꺾인 점들을 만든다.
        Vector3[] points = new Vector3[_segmentCount + 1];
        for (int i = 0; i <= _segmentCount; i++)
        {
            float t = (float)i / _segmentCount;
            if (i > 0)
            {
                x += Random.Range(-_jaggedness, _jaggedness);

                // 가운데 제목과 패널 뒤로 꺾여 들어가지 않게 바깥쪽에 머문다.
                if (Mathf.Abs(x) < _sideMinX)
                {
                    x = side * (_sideMinX + Random.Range(0f, _jaggedness));
                }
            }

            points[i] = new Vector3(x, Mathf.Lerp(topY, _bottomY, t), _distance);
        }

        _bolt.positionCount = points.Length;
        _bolt.SetPositions(points);
        _glow.positionCount = points.Length;
        _glow.SetPositions(points);

        // 3. 줄기 중간에서 옆으로 뻗는 짧은 가지를 하나 만든다.
        int startIndex = Random.Range(3, _segmentCount - 4);
        int branchCount = _segmentCount / 3;
        float branchDirection = Random.value < 0.5f ? -1f : 1f;
        Vector3 point = points[startIndex];
        _branch.positionCount = branchCount + 1;
        for (int i = 0; i <= branchCount; i++)
        {
            if (i > 0)
            {
                point += new Vector3(branchDirection * Random.Range(0.4f, 1.4f), -Random.Range(0.6f, 1.2f), 0f);
            }

            _branch.SetPosition(i, point);
        }
    }

    private void SetIntensity(float intensity)
    {
        if (_flashImage != null)
        {
            Color flashColor = _flashImage.color;
            flashColor.a = _flashAlpha * intensity;
            _flashImage.color = flashColor;
        }

        SetLineAlpha(_bolt, intensity);
        SetLineAlpha(_branch, intensity * 0.8f);
        SetLineAlpha(_glow, intensity * _glowAlpha);
    }

    private void SetLineAlpha(LineRenderer line, float alpha)
    {
        if (line == null)
        {
            return;
        }

        Color color = _boltColor;
        color.a = alpha;
        line.startColor = color;
        line.endColor = color;
        line.enabled = alpha > 0.001f;
    }

    private LineRenderer CreateLine(string lineName, Material material, int sortingOrder)
    {
        GameObject lineObject = new GameObject(lineName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.sharedMaterial = material;
        line.sortingOrder = sortingOrder;
        line.numCapVertices = 2;
        line.numCornerVertices = 2;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        line.receiveShadows = false;

        float width = lineName == "BoltGlow" ? _glowWidth : _boltWidth;
        line.widthCurve = new AnimationCurve(new Keyframe(0f, width), new Keyframe(1f, width * 0.3f));
        line.enabled = false;
        return line;
    }
}
