using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class PolygonCollider2DFromSprite : MonoBehaviour
{
    private PolygonCollider2D col;
    public bool show;

    public bool generate = false;
    public bool canGenerateShadowCaster2D = true;

    void Start()
    {
        col = GetComponent<PolygonCollider2D>();

        if (col == null)
        {
            Debug.LogError("There is no 'PolygonCollider2D' attached to the object 'BayazitDecomposerTester' is attached to.");
        }
    }

    private void OnValidate()
    {
        if (!generate) return;

        generate = false;


        //List<Vector2> worldColPoints = new List<Vector2>();

        //foreach (Vector2 point in col.points)
        //{
        //    Vector2 currentWorldPoint = this.transform.TransformPoint(point);
        //    worldColPoints.Add(currentWorldPoint);
        //}
        //List<Vector2> vertices = worldColPoints;

        const string COLLIDER_NAME = "Collider:";
        foreach (Transform child in col.transform)
        {
            if (child.gameObject.name.Contains(COLLIDER_NAME))
            {
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
        }

        List<List<Vector2>> listOfConvexPolygonPoints = BayazitDecomposer.ConvexPartition(col.points.ToList());

        for (var index = 0; index < listOfConvexPolygonPoints.Count; index++)
        {
            List<Vector2> currentPolygonVertices = listOfConvexPolygonPoints[index];
            GameObject newGo = new GameObject(COLLIDER_NAME + $" {index}");
            newGo.transform.SetParent(col.transform, false);

            PolygonCollider2D newPoly = newGo.AddComponent<PolygonCollider2D>();

            newPoly.points = currentPolygonVertices.ToArray();
            
            if (canGenerateShadowCaster2D)
            {
                newGo.AddComponent<ShadowCaster2D>();
                var shadowFromCol = newGo.AddComponent<ShadowCaster2DFromCollider>();
                shadowFromCol.UpdateShadow();

                if (Application.isPlaying) Destroy(shadowFromCol);
                else DestroyImmediate(shadowFromCol);
            }
        }


    }


    void OnDrawGizmos()
    {
        if (!Application.isPlaying || !show)
            return;

        Gizmos.color = Color.green;

        List<Vector2> worldColPoints = new List<Vector2>();

        foreach (Vector2 point in col.points)
        {
            Vector2 currentWorldPoint = this.transform.TransformPoint(point);
            worldColPoints.Add(currentWorldPoint);
        }

        List<Vector2> vertices = worldColPoints;

        List<List<Vector2>> listOfConvexPolygonPoints = BayazitDecomposer.ConvexPartition(vertices);

        foreach (List<Vector2> pointsOfIndivualConvexPolygon in listOfConvexPolygonPoints)
        {
            List<Vector2> currentPolygonVertices = pointsOfIndivualConvexPolygon;

            for (int i = 0; i < currentPolygonVertices.Count; i++)
            {
                Vector2 currentVertex = currentPolygonVertices[i];
                Vector2 nextVertex = currentPolygonVertices[i + 1 >= currentPolygonVertices.Count ? 0 : i + 1];

                Gizmos.DrawLine(currentVertex, nextVertex);
            }
        }
    }
}