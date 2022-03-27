using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

public class TentacleGenerator : MonoBehaviour
{
    public GameObject m_Tentacle;
    [FoldoutGroup("Tentacle Randomize Settings")] [MinMaxSlider(0, 30, true)]public Vector2 m_AmountRange;
    [FoldoutGroup("Tentacle Randomize Settings")] [MinMaxSlider(-25, 25, true)] public Vector2 m_TentacleAngleXRange;
    [FoldoutGroup("Tentacle Randomize Settings")] [MinMaxSlider(-0.1f, 0.1f, true)] public Vector2 m_PositionZRange;
    [FoldoutGroup("Tentacle Randomize Settings")] [MinMaxSlider(0.5f, 1.5f, true)] public Vector2 m_ScaleRange;    
    public List<DynamicBoneColliderBase> m_DynamicBoneColliders;
    public List<Transform> m_GenerateOnThese;
    [ReadOnly]public List<GameObject> CloneList = new List<GameObject>();
    private float m_TentacleBurstGlowingDuration = 1;
    List<EmissionRaiseSteps> EmissionRaiseControlList = new List<EmissionRaiseSteps>();
    private float originalEmissionRaiseValue;

    Coroutine BurstProcess { get; set; }

    private void Awake()
    {
        //Generate();
    }
    [Button]
    public void Generate() {
        bool hasClones = (CloneList.Count != 0);
        if (hasClones) { CleanInstance();}

        GameObject clone;
        foreach (var item in m_GenerateOnThese) 
        {
            int amount = (int)Mathf.Floor(Random.Range(m_AmountRange.x, m_AmountRange.y));
            for (int i = 0; i < amount; i++)
            {
                clone = Instantiate(m_Tentacle, item);
                RandomizedPosition(clone);
                RandomizedRotation(clone, i, amount);
                RandomizedScale(clone);
                clone.GetComponent<DynamicBone>().m_Colliders = m_DynamicBoneColliders;
                CloneList.Add(clone);
            }
        }
    }
    private void RandomizedPosition(GameObject clone)
    {
        var pos = clone.transform.localPosition;
        pos.z = Random.Range(m_PositionZRange.x, m_PositionZRange.y);
        clone.transform.localPosition = pos;
    }
    private void RandomizedRotation(GameObject clone, int index, int amount) {
        var angle = clone.transform.localEulerAngles;
        angle.x = Random.Range(m_TentacleAngleXRange.x, m_TentacleAngleXRange.y);
        angle.z = index * (360 / amount);
        clone.transform.localEulerAngles = angle;
    }
    private void RandomizedScale(GameObject clone)
    {
        var scale = Vector3.one * Random.Range(m_ScaleRange.x, m_ScaleRange.y);
        clone.transform.localScale = scale;
    }
    private void CleanInstance() {
        for (int i = 0; i < CloneList.Count; i++)
        {
            Destroy(CloneList[i]);
        }
        CloneList.Clear();
    }
    public void ChangeEmissionRaiseTargetVale(int valueIndex, float targetValue) {
        if (BurstProcess == null) BurstProcess = StartCoroutine(Delayer(valueIndex));
        else return;

        if(EmissionRaiseControlList.Count == 0)
            EmissionRaiseControlList = new List<EmissionRaiseSteps>(GetComponentsInChildren<EmissionRaiseSteps>());

        originalEmissionRaiseValue = EmissionRaiseControlList[0].m_ValueTargets[valueIndex];
        for (int i = 0; i < EmissionRaiseControlList.Count; i++)
        {
            EmissionRaiseControlList[i].m_ValueTargets[valueIndex] = targetValue;
        }
    }
    private void ResetValues(int valueIndex) {
        for (int i = 0; i < EmissionRaiseControlList.Count; i++)
        {
            EmissionRaiseControlList[i].m_ValueTargets[valueIndex] = originalEmissionRaiseValue;
        }
    }
    private IEnumerator Delayer(int valueIndex) {
        yield return new WaitForSeconds(m_TentacleBurstGlowingDuration);
        ResetValues(valueIndex);
        StopCoroutine(BurstProcess);
        BurstProcess = null;
    }
}
