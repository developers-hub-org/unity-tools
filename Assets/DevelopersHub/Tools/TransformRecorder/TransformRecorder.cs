using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class TransformRecorder : MonoBehaviour
{

    [SerializeField] private uint _maxRecordData = 99999;
    [SerializeField] private float _framesSpace = 0.01f;

    private static TransformRecorder _singleton = null; public static TransformRecorder singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = FindFirstObjectByType<TransformRecorder>();
                if (_singleton == null)
                {
                    _singleton = new GameObject("TransformRecorder").AddComponent<TransformRecorder>();
                }
            }
            return _singleton;
        }
    }

    public struct TransformData
    {
        public float time;
        public Vector3 position;
        public Quaternion rotation;
    }

    public static event NoCallback OnPlaybackFinished;
    public delegate void NoCallback();
    private Transform _recordTarget = null;
    private List<TransformData> _recordData = null;
    private float _time = 0;
    private float _lastFrameTime = 0;

    public void StartRecording(Transform target)
    {
        if(target != null)
        {
            if (_recordTarget != null)
            {
                Debug.LogWarning("Previous recording was canceled by starting a new recording.");
            }
            _recordTarget = target;
            _recordData = new List<TransformData>();
            _time = 0;
            _lastFrameTime = 0;
            AddNewRecord(_time);
        }
    }

    private void AddNewRecord(float time)
    {
        if (_recordTarget != null && _recordData != null)
        {
            TransformData data = new TransformData();
            data.time = time;
            data.position = _recordTarget.position;
            data.rotation = _recordTarget.rotation;
            _recordData.Add(data);
        }
    }

    private void Update()
    {
        if(_recordTarget != null && _recordData.Count > 0)
        {
            float deltaTime = Time.deltaTime;
            _time += deltaTime;
            _lastFrameTime += deltaTime;
            if (_lastFrameTime >= _framesSpace && (_recordData[_recordData.Count - 1].position != _recordTarget.position || _recordData[_recordData.Count - 1].rotation != _recordTarget.rotation) && _recordData.Count <= _maxRecordData)
            {
                AddNewRecord(_time);
                _lastFrameTime = 0;
            }
        }
    }

    public string StopRecording()
    {
        string recordData = "";
        if (_recordData != null && _recordData.Count > 0)
        {
            recordData = Serialize(_recordData);
            _time = 0;
            _recordData = null;
            _recordTarget = null;
        }
        return recordData;
    }

    public void PlayRecording(Transform target, string recordData)
    {
        try
        {
            List<TransformData> data = Desrialize(recordData);
            PlayRecording(target, data);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void PlayRecording(Transform target, List<TransformData> recordData)
    {
        if(recordData == null || recordData.Count <= 0)
        {
            Debug.LogError("Record data is not valid.");
            return;
        }
        if (target == null)
        {
            Debug.LogError("Target is not valid.");
            return;
        }
        StartCoroutine(PlayRecordingAction(target, recordData));
    }

    private IEnumerator PlayRecordingAction(Transform target, List<TransformData> recordData)
    {
        target.position = recordData[0].position;
        target.rotation = recordData[0].rotation;
        for (int i = 1; i < recordData.Count; i++)
        {
            yield return new WaitForSeconds(recordData[i].time - recordData[i - 1].time);
            target.position = recordData[i].position;
            target.rotation = recordData[i].rotation;
        }
        if(OnPlaybackFinished != null)
        {
            OnPlaybackFinished.Invoke();
        }
    }

    private string Serialize(List<TransformData> target)
    {
        XmlSerializer xml = new XmlSerializer(typeof(List<TransformData>));
        StringWriter writer = new StringWriter();
        xml.Serialize(writer, target);
        return writer.ToString();
    }

    private List<TransformData> Desrialize(string target)
    {
        XmlSerializer xml = new XmlSerializer(typeof(List<TransformData>));
        StringReader reader = new StringReader(target);
        return (List<TransformData>)xml.Deserialize(reader);
    }

}