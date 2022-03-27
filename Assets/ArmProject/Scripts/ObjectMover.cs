using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UltEvents;

public class ObjectMover : MonoBehaviour
{
    [System.Serializable]
    public struct PositionData {
        public string Name;
        public Vector3 Position;
    }
    public GameObject m_Obj;
    public float m_Duration;
    public bool m_Reverse;
    public bool m_UnscaleTime = false;
    public bool m_CanInterupt = true;
    public AnimationCurve m_Movement;
    public bool m_LocalPos = false;

    [Header("From offset to Default pos")]
    public bool m_MakeItMoveFromOffsetToDefault = false;
    public Vector3 m_FromOffset = Vector3.zero;
    [Header("From current to offset")]
    public bool m_UseFromCurrentToOffset = false;


    public bool m_TriggerOnEnable = true;
    public System.Action OnFinishedMovement = delegate { };
    public UltEvent WhenFinishedMovement;
    public UltEvent WhenMoving;
    [FoldoutGroup("Position Info")] [ReadOnly] public Vector3 m_From;
    [FoldoutGroup("Position Info")] [ReadOnly] public Vector3 m_To;
    [FoldoutGroup("Position Info")] public List<PositionData> m_PositionList = new List<PositionData>();
    [FoldoutGroup("Position Info")] [ReadOnly] public Vector3 dir;
    [FoldoutGroup("Position Info")] [ReadOnly] public Vector3 currentTarget;
    [FoldoutGroup("Position Info")] [ReadOnly] public int currentIndex = 0;
    private float speed;    
    public CharacterController movingPlayer { get; set; }
    Coroutine Process { get; set; }
    [FoldoutGroup("Func")]
    [Button]
    public void UseCurrentPositionForFrom(){        
        m_From = m_LocalPos? m_Obj.transform.localPosition : m_Obj.transform.position;
    }
    [FoldoutGroup("Func")]
    [Button]
    public void UseCurrentPositionForTo(){        
        m_To = m_LocalPos? m_Obj.transform.localPosition : m_Obj.transform.position;
    }
    [FoldoutGroup("PositionList Func")]
    [Button]
    public void AddCurrentPositionToList()
    {
        PositionData posData = new PositionData(); 
        posData.Name = m_PositionList.Count.ToString();
        posData.Position = m_LocalPos ? m_Obj.transform.localPosition : m_Obj.transform.position;
        m_PositionList.Add(posData);
    }
    [FoldoutGroup("Func")]
    [Button]
    public void MoveObjectToFrom(){     
        if(m_LocalPos)
            m_Obj.transform.localPosition = m_From;
        else
            m_Obj.transform.position = m_From;
    }
    [FoldoutGroup("Func")]
    [Button]
    public void MoveObjectToTo()
    {
        if (m_LocalPos)
            m_Obj.transform.localPosition = m_To;
        else
            m_Obj.transform.position = m_To;
    }
    [FoldoutGroup("PositionList Func")]
    [Button]
    public void MoveObjectToPosition(int index)
    {
        var pos = GetPosition(index);
        if (m_LocalPos)
            m_Obj.transform.localPosition = pos;
        else
            m_Obj.transform.position = pos;
    }
    private void Awake()
    {
        if (m_MakeItMoveFromOffsetToDefault)
        {
            m_From = transform.position + m_FromOffset;
            m_To = transform.position;
            m_Obj = gameObject;
        }
        if (m_UseFromCurrentToOffset) 
        {
            SetTargetValue();
            m_Obj = gameObject;
        }

        currentTarget = m_To;
        dir = (m_To - m_From).normalized;
        speed = Vector3.Distance(m_From, m_To) / m_Duration;
    }
    private void SetTargetValue() {
        m_From = transform.position;
        m_To = transform.position + m_FromOffset;
    }
    private void OnEnable()
    {
        if(m_TriggerOnEnable)
            Trigger();
    }
    #region Position List Func    
    private Vector3 GetPosition(int index) {        
        return m_PositionList[index].Position;
    }
    private Vector3 GetPosition(string name)
    {
        for (int i = 0; i < m_PositionList.Count; i++)
        {
            if (m_PositionList[i].Name == name)
                return m_PositionList[i].Position;
        }
        return m_PositionList[0].Position;
    }
    private int GetIndex(Vector3 pos)
    {
        for (int i = 0; i < m_PositionList.Count; i++)
        {
            if (m_PositionList[i].Position == pos)
                return i;
        }
        return 0;
    }
    #endregion
    #region Public Func
    public void TriggerToNext(bool reverse)
    {
        var next = (currentIndex + 1 < m_PositionList.Count) ? currentIndex + 1 : 0;
        m_From = GetPosition(currentIndex);
        m_To = GetPosition(next);
        m_Duration = Vector3.Distance(m_From, m_To) / speed;
        m_Reverse = reverse;
        Trigger();
    }
    public void Trigger(int to, bool reverse)
    {
        m_From = GetPosition(currentIndex);
        m_To = GetPosition(to);
        m_Duration = Vector3.Distance(m_From, m_To) / speed;
        m_Reverse = reverse;
        Trigger();
    }
    public void Trigger(int from, int to, bool reverse) {
        m_From = GetPosition(from);    
        m_To = GetPosition(to);
        m_Duration = Vector3.Distance(m_From, m_To) / speed;
        m_Reverse = reverse;
        Trigger();
    }
    public void Trigger()
    {
        if (m_UseFromCurrentToOffset) SetTargetValue();
        if (!m_UnscaleTime)
        {
            if (m_CanInterupt && Process != null)            
                StopCoroutine(Process);

            Process = StartCoroutine(Transition(m_From, m_To, m_Duration, m_Reverse));
        }
        else
        {
            if (Process == null)
                Process = StartCoroutine(UnscaleTransition());
            else if (m_CanInterupt)
            {
                StopCoroutine(Process);
                Process = StartCoroutine(UnscaleTransition());
            }
        }
    }
    #endregion
    #region Coroution Logic
    private IEnumerator Transition(Vector3 from, Vector3 to, float duration, bool reverse)
    {
        m_Obj.SetActive(true);

        if (m_Obj.transform.localPosition == to) StopCoroutine(Process);

        var startTime = Time.time;
        var endTime = m_Duration;
        var step = 0f;
        var temp = from;
        int count = 1;
        if (reverse) { count = 2; }

        for (int i = 0; i < count; i++)
        {
            while (Time.time - startTime < endTime)
            {
                step = (Time.time - startTime) / endTime;

                if (!m_LocalPos)
                    m_Obj.transform.position = Vector3.Lerp(from, to, m_Movement.Evaluate(step));
                else
                    m_Obj.transform.localPosition = Vector3.Lerp(from, to, m_Movement.Evaluate(step));

                WhenMoving?.Invoke();

                if (movingPlayer)
                    movingPlayer.Move(dir * speed *(m_Movement.Evaluate(step) / step) * Time.deltaTime);

                yield return null;
            }

            if (!m_LocalPos)
                m_Obj.transform.position = to;
            else
                m_Obj.transform.localPosition = to;

            // Setting for next loop
            currentIndex = GetIndex(to);
            startTime = Time.time;
            temp = from;
            from = to;
            to = temp;
            dir = (to - from).normalized;
        }                

        WhenFinishedMovement?.Invoke();
        OnFinishedMovement?.Invoke();        
        Process = null;
    }
    
    private IEnumerator UnscaleTransition()
    {
        m_Obj.SetActive(true);

        var startTime = Time.unscaledTime;
        var endTime = m_Duration;

        while (Time.unscaledTime - startTime < endTime)
        {
            m_Obj.transform.position = Vector3.Lerp(m_From, m_To, (Time.unscaledTime - startTime) / endTime);
            yield return null;
        }

        m_Obj.transform.position = m_To;


        if (m_Reverse)
        {
            startTime = Time.unscaledTime;

            while (Time.unscaledTime - startTime < endTime)
            {
                m_Obj.transform.position = Vector3.Lerp(m_To, m_From, (Time.unscaledTime - startTime) / endTime);
                yield return null;
            }

            m_Obj.transform.position = m_From;
        }
        WhenFinishedMovement?.Invoke();
        OnFinishedMovement?.Invoke();
        Process = null;
    }
    #endregion
}
