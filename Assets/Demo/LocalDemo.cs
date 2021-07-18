// ==============================================
// Author：qiuyukun
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
    [Header("丢包率百分比")]
    [Range(0,100)]
    public int IPLR = 15;
    //毫秒,这个值是一个预估值,可控制客户端领先服务器多久
    //一般可以根据Ping值来动态变化,
    //不能为0,因为必须保证客户端永远领先服务器,客户端发送的输入才有效
    //网络越慢,这个值应该越大,一般超过260就已经达到最大预测帧数了,再大就没有意义了
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
        GUILayout.Label("预测的帧数 " + (_engine.predictiveFrameIndex - _engine.confirmedFrameIndex));
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

        //按顺序传入输入,demo使用字典保证包的顺序
        if (_clientReceivedInputDict.TryGetValue(_engine.confirmedFrameIndex + 1, out DemoInput input))
        {
            _engine.OnInput(input);
            _lastReceivedInput = input;
        }

        //表现层输入
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        _curInput.forward = new Vector2((int)x, (int)y);

        //表现追赶逻辑
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

    //执行输入,开始逻辑
    //如果引擎开启了线程,则这里调用非主线程调用
    private void Excute(IFrameInput input)
    {
        var demoInput = (DemoInput)input;
        _curLogicPos += demoInput.forward * _moveSpeed * _deltaTime;
    }

    //执行回滚
    //如果引擎开启了线程,则这里是非主线程调用
    private void Rollback(int frame)
    {
        UnityEngine.Debug.Log("rollback " + frame);
        lock (_rollbackDict)
        {
            _curLogicPos = _rollbackDict[frame];
        }
    }

    //追帧
    //预测失败代表之后的预测帧输入都会失败
    //重新使用新的帧输入来追赶预测帧
    //如果引擎开启了线程,则这里调用非主线程调用
    private Queue<IFrameInput> PursuePredictiveFrame(int startFrameIndex, int endFrameIndex)
    {
        _predictiveInputCache.Clear();
        while (startFrameIndex <= endFrameIndex)
        {
            if (_predictiveInputDict.TryGetValue(startFrameIndex, out DemoInput input))
            {
                //其它玩家的输入使用最后一次确认帧的输入
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

    //开始预测,并返回预测的输入
    //如果引擎开启了线程,则这里调用非主线程调用
    private IFrameInput Predict()
    {
        _curInput.frameIndex = _engine.predictiveFrameIndex;
        //这里可以用池来管理
        var input = new DemoInput();
        _curInput.CopyTo(input);

        //同时把输入发送给服务器
        lock (_localInputQueue)
        {
            _localInputQueue.Enqueue(input);
        }
        lock (_rollbackDict)
        {
            //记录状态,用于回滚
            _rollbackDict.Add(_engine.predictiveFrameIndex, _curLogicPos);
        }

        _predictiveInputDict.Add(_engine.predictiveFrameIndex, input.Clone());
        return input;
    }

    //模拟客户端向服务器发送延迟
    private IEnumerator SendLocalInput(DemoInput input)
    {
        //模拟丢包
        if (Random.Range(0, 100) < IPLR)
            yield break;

        yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
        _serverInputDict.Add(input.frameIndex, input);
    }

    #region 模拟服务器

    private int _serverFrameIndex;
    //服务器输入队列
    private Dictionary<int, DemoInput> _serverInputDict = new Dictionary<int, DemoInput>();
    private DemoInput _lastInput = new DemoInput();
    private Stopwatch _serverStopwatch = new Stopwatch();

    //模拟服务器向Input中添加输入
    private void ServerUpdate()
    {
        var frame = _serverStopwatch.ElapsedMilliseconds / 33;

        if (frame >= _serverFrameIndex)
        {
            DemoInput input;
            //如果没有收到客户端这帧的输入,则使用上一帧的输入
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

    //模拟服务器向客户端发送延迟
    private IEnumerator SendServerInput(DemoInput input)
    {
        yield return new WaitForSeconds(Random.Range(0.02f, 0.05f));
        //无序到达客户端,一般会在网络层保证包的可靠性和顺序
        _clientReceivedInputDict.Add(input.frameIndex, input);
    }

    #endregion
}
