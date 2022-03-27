using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class ObjectScaler : MonoBehaviour
{
    public bool m_ControlByTriggerObjectScaler = false;
    [ShowIf("m_ControlByTriggerObjectScaler")]public float m_OffsetTime;
    public GameObject m_Obj;
    public bool m_ChangeStartScale = false;
    [ShowIf("m_ChangeStartScale")] public Vector3 m_StartScale;
    public Vector3 m_From;
    public Vector3 m_To;
    public float m_Duration;
    public bool m_Reverse;
    public bool m_UnscaleTime = false;
    public bool m_CanInterupt = true;
    public bool m_TranslateScaleRatioOnEnable = false;
    public AnimationCurve m_Movement;
    public bool m_HideMesh;

    [Header("From 0 to Default Size")]
    public bool m_MakeItScaleFromZeroToDefault = false;
    [ShowIf("m_MakeItScaleFromZeroToDefault")] 
    public bool m_ScaleX, m_ScaleY, m_ScaleZ;
    public bool m_TriggerOnEnable = true;
    Coroutine Process { get; set; }
    [Button]
    public void TranslateScaleRatio() {
        m_From.x *= transform.localScale.x;
        m_From.y *= transform.localScale.y;
        m_From.z *= transform.localScale.z;

        m_To.x *= transform.localScale.x;
        m_To.y *= transform.localScale.y;
        m_To.z *= transform.localScale.z;
    }
    private void Awake() {
        if(m_Obj == null) m_Obj = gameObject;
        if (m_TranslateScaleRatioOnEnable) TranslateScaleRatio();

        if (m_MakeItScaleFromZeroToDefault)
        {
            m_From = Vector3.zero;
            m_To = transform.localScale;

            if (!m_ScaleX)
                m_From.x = transform.localScale.x;
            if (!m_ScaleY)
                m_From.y = transform.localScale.y;
            if (!m_ScaleZ)
                m_From.z = transform.localScale.z;
        }
        if (m_HideMesh) {
            var meshes = m_Obj.GetComponentsInChildren<MeshRenderer>();
            
            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].enabled = false;
            }
        }
    }
    private void OnEnable()
    {
        if(m_TriggerOnEnable)
            Trigger();
    }
    public void ShowMesh() {
        if (m_HideMesh)
        {
            var meshes = m_Obj.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].enabled = true;
            }
        }
        if (m_ChangeStartScale)
        {
            m_Obj.transform.localScale = m_StartScale;
        }
    }
    public void Trigger() {        
        
        if (m_HideMesh)
        {
            var meshes = m_Obj.GetComponentsInChildren<MeshRenderer>();

            for (int i = 0; i < meshes.Length; i++)
            {
                meshes[i].enabled = true;
            }
        }

        if (!m_UnscaleTime)
        {
            if (Process == null)
                Process = StartCoroutine(Transition());
            else if(m_CanInterupt)
            {
                StopCoroutine(Process);
                Process = StartCoroutine(Transition());
            }
        }
        else
        {
            if (Process == null)
                Process = StartCoroutine(UnscaleTransition());
            else if(m_CanInterupt)
            {
                StopCoroutine(Process);
                Process = StartCoroutine(UnscaleTransition());
            }
        }
    }

    private IEnumerator Transition() {
        m_Obj.SetActive(true);

        var startTime = Time.time;
        var endTime = m_Duration;
        //print("Start scaling");
        
        while (Time.time - startTime < endTime)
        {
            m_Obj.transform.localScale = Vector3.Lerp(m_From, m_To, m_Movement.Evaluate((Time.time - startTime) / endTime));
            yield return null;
        }

        m_Obj.transform.localScale = m_To;

        if (m_Reverse)
        {
            startTime = Time.time;

            while (Time.time - startTime < endTime)
            {
                m_Obj.transform.localScale = Vector3.Lerp(m_To, m_From, m_Movement.Evaluate((Time.time - startTime) / endTime));
                yield return null;
            }

            m_Obj.transform.localScale = m_From;
        }
        Process = null;
    }
    private IEnumerator UnscaleTransition()
    {
        m_Obj.SetActive(true);

        var startTime = Time.unscaledTime;
        var endTime = m_Duration;

        while (Time.unscaledTime - startTime < endTime)
        {
            m_Obj.transform.localScale = Vector3.Lerp(m_From, m_To, (Time.unscaledTime - startTime) / endTime);
            yield return null;
        }

        m_Obj.transform.localScale = m_To;


        if (m_Reverse)
        {
            startTime = Time.unscaledTime;

            while (Time.unscaledTime - startTime < endTime)
            {
                m_Obj.transform.localScale = Vector3.Lerp(m_To, m_From, (Time.unscaledTime - startTime) / endTime);
                yield return null;
            }

            m_Obj.transform.localScale = m_From;
        }

        Process = null;
    }
}
