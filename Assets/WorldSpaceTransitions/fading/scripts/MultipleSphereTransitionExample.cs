using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace WorldSpaceTransitions
{
    [ExecuteInEditMode]
    public class MultipleSphereTransitionExample : MonoBehaviour
    {
        public float radius_end = 3;
        public float radius_start = 0;
        public float radius_editor = 1;
        public float fwdInterval = 1.5f;
        public float bwdInterval = 0.6f;
        public int coroutineCount = 0;

        private Vector4[] centerPoints;
        //private Vector4[] AxisDirs;
        public float[] radiuses;
        public List<int> coroutineIds;
        private int newCoroutineId = 0;
        //private Transform [];

        void Start()
        {
            Shader.DisableKeyword("FADE_PLANE");
            Shader.DisableKeyword("FADE_SPHERE");
            Shader.EnableKeyword("FADE_SPHERES");
            centerPoints = new Vector4[64];
            radiuses = new float[64];
            coroutineIds = new List<int>();
            Debug.Log("start");

        }

        void OnEnable()
        {

            Shader.DisableKeyword("FADE_PLANE");
            Shader.DisableKeyword("FADE_SPHERE");
            Shader.EnableKeyword("FADE_SPHERES");
            Shader.DisableKeyword("CLIP_PLANE");
            //Shader.EnableKeyword("FADE_SPHERE");
            //Shader.EnableKeyword("CLIP_SPHERE");

            centerPoints = new Vector4[64];
            radiuses = new float[64];


            if (Application.isPlaying)
            {
                Shader.EnableKeyword("FADE_SPHERES");
                //Shader.SetGlobalInt("_FADE_SPHERES", 0);
                Shader.SetGlobalInt("_centerCount", coroutineCount);
            }
            else
            {
                int i = 0;
                foreach (Transform t in transform)
                {
                    Ray ray = new Ray(Camera.main.transform.position, t.position - Camera.main.transform.position);
                    RaycastHit hit;
                    Collider collider = t.GetComponent<Collider>();
                    if (!collider) continue;
                    if (collider.Raycast(ray, out hit, 1000f))
                    {
                        Debug.Log(hit.transform.name);
                        centerPoints[i] = hit.point;
                        radiuses[i] = radius_editor;
                        i++;
                    }
                    Debug.Log(t.name);
                }
                Shader.SetGlobalVector("_SectionPoint", centerPoints[1]);
                Debug.Log(centerPoints[0].ToString() + " | " + radiuses[0].ToString());
                Shader.SetGlobalFloat("_Radius", radiuses[1]);
                Shader.SetGlobalVectorArray("_centerPoints", centerPoints);
                Shader.SetGlobalFloatArray("_Radiuses", radiuses);//*/
                Shader.SetGlobalInt("_centerCount", i);
                Debug.Log("centerPoints " +  i.ToString());
            }
        }

        void OnDisable()
        {
            Shader.DisableKeyword("FADE_SPHERES");
            Shader.SetGlobalInt("_FADE_SPHERES", 0);
            //StopAllCoroutines();
            //coroutineCount = 0;
        }

        void Update()
        {
            //Shader.SetGlobalFloat("_Radius", 0.2f);
            if (Input.GetMouseButtonDown(0))
            {
                if (EventSystem.current.IsPointerOverGameObject()) return;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 10000f))
                {
                    if (hit.transform.parent==transform)
                    {
                        Debug.Log("hit");

                        StartCoroutine(doTransition(hit.point));
                    }
                }
            }
            if (coroutineCount > 0)
            {
                Shader.SetGlobalVectorArray("_centerPoints", centerPoints);
                Shader.SetGlobalFloatArray("_Radiuses", radiuses);//*/
                //Shader.SetGlobalInt("_centerCount", coroutineCount);
            }
        }



        IEnumerator doTransition(Vector3 hitPoint)
        {
            int coroutineId = newCoroutineId;
            coroutineIds.Add(coroutineId);

            //radiuses[].Add(0f);
            centerPoints[coroutineIds.Count-1] = hitPoint;
            newCoroutineId++;
            coroutineCount ++;
            Shader.SetGlobalInt("_centerCount", coroutineCount);
            float startTime = Time.time;
            float t = 0f;
            int coroutineIndex;
            while (t<1)
            {
                float radius = Mathf.Lerp(radius_start, radius_end, t);
                coroutineIndex = coroutineIds.IndexOf(coroutineId);
                radiuses[coroutineIndex] = radius;
                t = (Time.time - startTime)/ fwdInterval;
                yield return new WaitForFixedUpdate();
            }
            t = 0f;
            startTime = Time.time;
            while (t < 1 && bwdInterval>0)
            {
                float radius = Mathf.SmoothStep(radius_end, radius_start, t);
                coroutineIndex = coroutineIds.IndexOf(coroutineId);
                radiuses[coroutineIndex] = radius;
                t = (Time.time - startTime) / bwdInterval;
                yield return null;
            }
            coroutineIndex = coroutineIds.IndexOf(coroutineId);
            coroutineIds.RemoveAt(coroutineIndex);
            for (int i = coroutineIndex; i < 63; i++)
            {
                radiuses[i] = radiuses[i + 1];
                centerPoints[i] = centerPoints[i + 1];
            }
            radiuses[63] = 0;
            centerPoints[63] = Vector4.zero;
            coroutineCount --;
            Shader.SetGlobalInt("_centerCount", coroutineCount);
        }
    }
}
