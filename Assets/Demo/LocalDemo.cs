using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fixed;
using Lockstep;
using Vector2 = Fixed.Vector2;
using Vector3 = Fixed.Vector3;

public struct DemoInput : IFrameInput
{
    public int frameIndex { get; set; }
    public Vector2 forward;
    public FixedPoint64 speed;

    public bool Equals(IFrameInput other)
    {
        var o = (DemoInput)other;
        return frameIndex == other.frameIndex && forward == o.forward && speed == o.speed;
    }
}

public class LocalDemo : MonoBehaviour
{
    private LockstepEngine _engine;
    private GameObject _target;
    private FixedPoint64 _deltaTime = new FixedPoint64(33,1000);
    private DemoInput _curInput = new DemoInput() { speed = new FixedPoint64(3,1)};
    private Vector3 _curLogicPos;
    private Vector3 _lastLogicPos;
    private float _inputTime;
    private float _lerp;

    public void Start()
    {
        _engine = new LockstepEngine(Excute, Rollback, GetInput);
        _target = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        _engine.Start(true);
    }

    private void Update()
    {
        var x = Input.GetAxisRaw("Horizontal");
        var y = Input.GetAxisRaw("Vertical");
        _curInput.forward = new Vector2((int)x, (int)y);

        _inputTime -= Time.deltaTime;
        if (_inputTime <= 0)
        {
            _engine.OnInput(_curInput);
            _curInput.frameIndex += 1;
            _inputTime = Random.Range(0.05f, 0.1f);
        }

        _lerp += Time.deltaTime;
        var targetPos = new UnityEngine.Vector3(_curLogicPos.x.Single, _curLogicPos.y.Single);
         _target.transform.position = UnityEngine.Vector3.Lerp(_target.transform.position, targetPos, _lerp);
    }

    private void Excute(IFrameInput input)
    {
        var demoInput = (DemoInput)input;
        _curLogicPos += demoInput.forward * demoInput.speed * _deltaTime;

        if (_curLogicPos != _lastLogicPos)
        {
            _lerp = 0;
        }
        _lastLogicPos = _curLogicPos;
    }

    private void Rollback(int frame)
    {
        Debug.Log("rollback " + frame);
    }

    private IFrameInput GetInput()
    {
        return _curInput;
    }
}
