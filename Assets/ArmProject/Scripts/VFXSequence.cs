using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UltEvents;

public class VFXSequence : MonoBehaviour
{
    [System.Serializable]
    public struct Sequence {
        public UltEvent Event;
        public float DelayTimeToNext;
    }
    public float m_InitialDelay = 0;
    public Sequence[] m_Sequences;
    public bool m_Loop = false;
    public bool m_Trigger = false;

    public bool Trigger { get; set; }

    Coroutine Process { get; set; }

    private void Update()
    {
        if (Trigger != m_Trigger) {
            Trigger = m_Trigger;

            if (Trigger)
                StartSequence();
        }
    }
    public void StartSequence() {
        if (Process == null)
        {
            Process = StartCoroutine(Sequencer());
        }
        else {
            StopCoroutine(Sequencer());
            Process = StartCoroutine(Sequencer());
        }
    }

    private IEnumerator Sequencer() {
        yield return new WaitForSeconds(m_InitialDelay);
        for (int i = 0; i < m_Sequences.Length; i++)
        {
            m_Sequences[i].Event.Invoke();
            
            yield return new WaitForSeconds(m_Sequences[i].DelayTimeToNext);
        }

        if (m_Loop)
        {
            Process = StartCoroutine(Sequencer());
        }
    }
    public void StopCurrentSequencer() {
        StopCoroutine(Process);
    }
}
