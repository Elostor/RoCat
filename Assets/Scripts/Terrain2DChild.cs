using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Terrain2DChild : MonoBehaviour
{
    [HideInInspector]
    public Vector3 lastPos;
    public float positionOffset;
    public bool SkyMode;


    public void PositionOnTerrain()
    {
        if (SkyMode)
        {
            return;
        }
        else if (!SkyMode)
        {
            if (!transform.parent) return;
            Terrain2D t = transform.parent.GetComponent<Terrain2D>();
            if (!t) return;
            float n = t.posY[FindClosestXpos(transform.position)];
            transform.localPosition = new Vector2(transform.localPosition.x, n + t.height + positionOffset);
        }
    }

    int FindClosestXpos(Vector2 childPos)
    {
        Terrain2D t = transform.parent.GetComponent<Terrain2D>();
        int ind = 0;
        float prev = 10000000;
        for (int i = 0; i < t.posX.Length; i++)
        {
            Vector2 difference = childPos - new Vector2(t.posX[i], t.posY[i]);
            var distanceInX = Mathf.Abs(difference.x);
            //float dis = Vector2.Distance(childPos, new Vector2(t.posX[i], t.posY[i]));
            if (distanceInX < prev)
            {
                prev = distanceInX;
                ind = i;
            }
        }
        return ind;
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(Terrain2DChild))]
    [CanEditMultipleObjects]
    public class Terrain2DChildEditor : Editor
    {
        void OnEnable() { EditorApplication.update += Update; }
        void OnDisable() { EditorApplication.update -= Update; }
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            if (GUI.changed)
            {
                Terrain2DChild tar = (target as Terrain2DChild);
                tar.PositionOnTerrain();
                tar.lastPos = tar.transform.localPosition;
            }
        }
        void Update()
        {
            Terrain2DChild tar = (target as Terrain2DChild);
            if (target == null) return;
            if (tar.lastPos != tar.transform.localPosition)
            {
                tar.PositionOnTerrain();
                tar.lastPos = tar.transform.localPosition;
            }
        }
    }
#endif
}
