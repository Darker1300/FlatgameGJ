using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// unitytips: ShadowCaster2DFromCollider Component
/// http://diegogiacomelli.com.br/unitytips-shadowcaster2-from-collider-component
/// <remarks>
/// Based on https://forum.unity.com/threads/can-2d-shadow-caster-use-current-sprite-silhouette.861256/
/// </remarks>
/// </summary>
[DefaultExecutionOrder(100)]
[RequireComponent(typeof(ShadowCaster2D))]
public class ShadowCaster2DFromCollider : MonoBehaviour
{

    static readonly FieldInfo _meshField;
    static readonly FieldInfo _shapePathField;
    static readonly MethodInfo _generateShadowMeshMethod;

    ShadowCaster2D _shadowCaster;

    EdgeCollider2D _edgeCollider;
    PolygonCollider2D _polygonCollider;

    public bool doUpdate = false;

    static ShadowCaster2DFromCollider()
    {
        _meshField = typeof(ShadowCaster2D).GetField("m_Mesh", BindingFlags.NonPublic | BindingFlags.Instance);
        _shapePathField = typeof(ShadowCaster2D).GetField("m_ShapePath", BindingFlags.NonPublic | BindingFlags.Instance);

        _generateShadowMeshMethod = typeof(ShadowCaster2D)
                                    .Assembly
                                    .GetType("UnityEngine.Rendering.Universal.ShadowUtility")
                                    .GetMethod("GenerateShadowMesh", BindingFlags.Public | BindingFlags.Static);
    }

    private void OnValidate()
    {
        if (!doUpdate) return;
        doUpdate = false;

        UpdateShadow();
    }

    public void UpdateShadow()
    {
        _shadowCaster = _shadowCaster != null ? _shadowCaster
            : GetComponent<ShadowCaster2D>();
        if (_shadowCaster == null) return;

        _edgeCollider = _edgeCollider != null ? _edgeCollider
            : GetComponent<EdgeCollider2D>();
        if (_edgeCollider == null)
        {
            _polygonCollider = _polygonCollider != null ? _polygonCollider
                : GetComponent<PolygonCollider2D>();

            if (_polygonCollider == null)
                return;
        }

        var points = _polygonCollider == null
            ? _edgeCollider.points
            : _polygonCollider.points;

        var points3D = new Vector3[points.Length];
        for (int i = 0; i < points.Length; i++)
            points3D[i] = points[i];

        _shapePathField.SetValue(_shadowCaster, points3D);
        _meshField.SetValue(_shadowCaster, new Mesh());
        _generateShadowMeshMethod.Invoke(_shadowCaster, new object[] { _meshField.GetValue(_shadowCaster), _shapePathField.GetValue(_shadowCaster) });
    }
}