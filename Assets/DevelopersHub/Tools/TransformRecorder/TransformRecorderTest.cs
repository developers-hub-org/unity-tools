using UnityEngine;
using TMPro;

public class TransformRecorderTest : MonoBehaviour
{

    [SerializeField] private Transform _testTarget = null;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] public TextMeshProUGUI _statusText = null;
    private Status _status = Status.Idle;
    private string _recordData = "";

    public enum Status
    {
        Idle = 0, Recording = 1, Playing = 2
    }

    private void Start()
    {
        TransformRecorder.OnPlaybackFinished += OnPlaybackFinished;
    }

    private void OnPlaybackFinished()
    {
        _status = Status.Idle;
        _statusText.text = "Idle: press R to start recording or P to reply.";
    }

    private void Update()
    {
        if(_testTarget != null)
        {
            if(_status == Status.Idle)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    TransformRecorder.singleton.StartRecording(_testTarget);
                    _status = Status.Recording;
                    _statusText.text = "Recording: press S to stop";
                }
                else if (Input.GetKeyDown(KeyCode.P) && string.IsNullOrEmpty(_recordData) == false)
                {
                    TransformRecorder.singleton.PlayRecording(_testTarget, _recordData);
                    _status = Status.Playing;
                    _statusText.text = "Playing: wait for it to finish.";
                }
                else
                {
                    if (string.IsNullOrEmpty(_recordData))
                    {
                        _statusText.text = "Idle: press R to start recording";
                    }
                    else
                    {
                        _statusText.text = "Idle: press R to start recording or P to reply.";
                    }
                }
            }
            else if (_status == Status.Recording && Input.GetKeyDown(KeyCode.S))
            {
                _recordData = TransformRecorder.singleton.StopRecording();
                _status = Status.Idle;
                _statusText.text = "Idle: press R to start recording or P to reply.";
            }

            Vector2 input = Vector2.zero;
            input.x = Input.GetKey(KeyCode.LeftArrow) ? -1 : Input.GetKey(KeyCode.RightArrow) ? 1 : 0;
            input.y = Input.GetKey(KeyCode.DownArrow) ? -1 : Input.GetKey(KeyCode.UpArrow) ? 1 : 0;
            if (_status != Status.Playing && input != Vector2.zero)
            {
                input = input * Time.deltaTime * _moveSpeed;
                _testTarget.Translate(input);
            }
        }
    }

}