// ==============================================
// Author��qiuyukun
// Date:2020-09-21 11:16:44
// ==============================================


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fixed;
using Lockstep;
using Vector2 = Fixed.Vector2;
using Vector3 = Fixed.Vector3;
using System.Diagnostics;

public class DemoInput : IFrameInput
{
    public int frameIndex { get; set; }
    public Vector2 forward;
    public Dictionary<int, Vector2> otherForwardDict = new Dictionary<int, Vector2>();

    public DemoInput()
    {

    }

    public bool Equals(IFrameInput other)
    {
        var o = (DemoInput)other;

        foreach (var item in otherForwardDict)
        {
            if (o.otherForwardDict[item.Key] != item.Value)
            {
                return false;
            }
        }
        return frameIndex == other.frameIndex && forward == o.forward;
    }

    public void CopyTo(DemoInput other)
    {
        other.frameIndex = frameIndex;
        other.forward = forward;
        other.otherForwardDict.Clear();
        foreach (var item in otherForwardDict)
        {
            other.otherForwardDict.Add(item.Key, item.Value);
        }
    }

    public DemoInput Clone()
    {
        var clone = new DemoInput();
        CopyTo(clone);
        return clone;
    }


    public void Release()
    {
        
    }
}

public class LocalDemo : MonoBehaviour
{
    public float posDeviation = 1.0f;
    [Header("�����ʰٷֱ�")]
    [Range(0,100)]
    public int IPLR = 15;
    //����,���ֵ��һ��Ԥ��ֵ,�ɿ��ƿͻ������ȷ��������
    //һ����Ը���Pingֵ����̬�仯,
    //����Ϊ0,��Ϊ���뱣֤�ͻ�����Զ���ȷ�����,�ͻ��˷��͵��������Ч
    //����Խ��,���ֵӦ��Խ��,һ�㳬��260���Ѿ��ﵽ���Ԥ��֡����,�ٴ��û��������
    public int timeAdjust = 100;

    private LockstepEngine _engine;
    private GameObject _target;
    private FixedPoint64 _deltaTime = new FixedPoint64(33,1000);
    private DemoInput _curInput = new DemoInput();
    private FixedPoint64 _moveSpeed = new FixedPoint64(3, 1);
    private Vector3 _curLogicPos;
    private float _lerp;
    private Dictionary<int, Vector3> _rollbackDict = new Dictionary<int, Vector3>();
    private Queue<DemoInput> _localInputQueue = new Queue<DemoInput>();
    private Dictionary<int, DemoInput> _clientReceivedInputDict = new Dictionary<int, DemoInput>();
    private Dictionary<int, DemoInput> _predictiveInputDict = new Dictionary<int, DemoInput>();
    private Queue<IFrameInput> _predictiveInputCache = new Queue<IFrameInput>();
    private DemoInput _lastReceivedInput;

    public void Start()
    {
        _engine = new LockstepEngine(Excute, Rollback, Predict, PursuePredictiveFrame);
        _target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _target.transform.position = new UnityEngine.Vector3(_curLogicPos.x.Single, _curLogicPos.y.Single);
        _engine.Start(true);
        _serverStopwatch.Start();
    }

    public void OnGUI()
    {
        if (_engine == null) return;
        GUILayout.Label("Ԥ���֡�� " + (_engine.predictiveFrameIndex - _engine.confirmedFrameIndex));
    }

    private void Update()
    {
        _engine.timeAdjust = timeAdjust;

        ServerUpdate();
        lock (_localInputQueue)
        {
            while (_localInputQueue.Count > 0)
            {
                StartCoroutine(SendLocalInput(_localInputQueue.Dequeue()));
            }
        }

        //��˳��������,demoʹ���ֵ䱣֤����˳��
        if (_clientReceivedInputDict.TryGetValue(_engine.confirmedFrameIndex + 1, out DemoInput input))
        {
            _engine.OnInput(input);
            _lastReceivedInput = input;
        }

        //���ֲ�����
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        _curInput.forward = new Vector2((int)x, (int)y);

        //����׷���߼�
        var targetPos = new UnityEngine.Vector3(_curLogicPos.x.Single, _curLogicPos.y.Single);
        var len = UnityEngine.Vector3.Distance(targetPos, _target.transform.position);
        if (len > posDeviation)
        {
            _lerp += Time.deltaTime * 5;
            _target.transform.position = UnityEngine.Vector3.Lerp(_target.transform.position, targetPos, _lerp);
        }
        else
        {
            _lerp = 0;
        }
    }

    //ִ������,��ʼ�߼�
    //������濪�����߳�,��������÷����̵߳���
    private void Excute(IFrameInput input)
    {
        var demoInput = (DemoInput)input;
        _curLogicPos += demoInput.forward * _moveSpeed * _deltaTime;
    }

    //ִ�лع�
    //������濪�����߳�,�������Ƿ����̵߳���
    private void Rollback(int frame)
    {
        UnityEngine.Debug.Log("rollback " + frame);
        lock (_rollbackDict)
        {
            _curLogicPos = _rollbackDict[frame];
        }
    }

    //׷֡
    //Ԥ��ʧ�ܴ���֮���Ԥ��֡���붼��ʧ��
    //����ʹ���µ�֡������׷��Ԥ��֡
    //������濪�����߳�,��������÷����̵߳���
    private Queue<IFrameInput> PursuePredictiveFrame(int startFrameIndex, int endFrameIndex)
    {
        _predictiveInputCache.Clear();
        while (startFrameIndex <= endFrameIndex)
        {
            if (_predictiveInputDict.TryGetValue(startFrameIndex, out DemoInput input))
            {
                //������ҵ�����ʹ�����һ��ȷ��֡������
                if (_lastReceivedInput != null)
                {
                    input.otherForwardDict.Clear();
                    foreach (var item in _lastReceivedInput.otherForwardDict)
                        input.otherForwardDict.Add(item.Key, item.Value);
                }
                _predictiveInputCache.Enqueue(input);
            }
            else
            {
                UnityEngine.Debug.LogError("pursue predictive frame error frome " + startFrameIndex + " to " + endFrameIndex);
                break;
            }
            startFrameIndex += 1;
        }
        return _predictiveInputCache;
    }

    //��ʼԤ��,������Ԥ�������
    //������濪�����߳�,��������÷����̵߳���
    private IFrameInput Predict()
    {
        _curInput.frameIndex = _engine.predictiveFrameIndex;
        //��������ó�������
        var input = new DemoInput();
        _curInput.CopyTo(input);

        //ͬʱ�����뷢�͸�������
        lock (_localInputQueue)
        {
            _localInputQueue.Enqueue(input);
        }
        lock (_rollbackDict)
        {
            //��¼״̬,���ڻع�
            _rollbackDict.Add(_engine.predictiveFrameIndex, _curLogicPos);
        }

        _predictiveInputDict.Add(_engine.predictiveFrameIndex, input.Clone());
        return input;
    }

    //ģ��ͻ���������������ӳ�
    private IEnumerator SendLocalInput(DemoInput input)
    {
        //ģ�ⶪ��
        if (Random.Range(0, 100) < IPLR)
            yield break;

        yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
        _serverInputDict.Add(input.frameIndex, input);
    }

    #region ģ�������

    private int _serverFrameIndex;
    //�������������
    private Dictionary<int, DemoInput> _serverInputDict = new Dictionary<int, DemoInput>();
    private DemoInput _lastInput = new DemoInput();
    private Stopwatch _serverStopwatch = new Stopwatch();

    //ģ���������Input���������
    private void ServerUpdate()
    {
        var frame = _serverStopwatch.ElapsedMilliseconds / 33;

        if (frame >= _serverFrameIndex)
        {
            DemoInput input;
            //���û���յ��ͻ�����֡������,��ʹ����һ֡������
            if (!_serverInputDict.TryGetValue(_serverFrameIndex, out input))
            {
                input = new DemoInput();
                _lastInput.CopyTo(input);
            }
            input.frameIndex = _serverFrameIndex;
            StartCoroutine(SendServerInput(input));
            _serverFrameIndex += 1;
            _lastInput = input;
        }
    }

    //ģ���������ͻ��˷����ӳ�
    private IEnumerator SendServerInput(DemoInput input)
    {
        yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
        //���򵽴�ͻ���,һ���������㱣֤���Ŀɿ��Ժ�˳��
        _clientReceivedInputDict.Add(input.frameIndex, input);
    }

    #endregion
}
