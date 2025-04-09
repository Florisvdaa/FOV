using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the player's Field of View (FOV), detecting visible targets and rendering the FOV mesh.
/// </summary>
public class FieldOfView : MonoBehaviour
{
    [Header("FOV settings")]
    [SerializeField] private float viewRadius;                  // Radius of the field of view
    [Range(0,360)]
    [SerializeField] private float viewAngle;                   // Angle of the field of view

    [Header("Player Settings")]
    [SerializeField] private Transform playerVisualTransform;   // Reference to the player's visual transform

    [Header("LayerMasks")]
    [SerializeField] private LayerMask targetMask;              // Layers considered as valid targets
    [SerializeField] private LayerMask obstacleMask;            // Layers considered as obstacles

    [Header("Mesh Settings")]
    [SerializeField] private float meshResolution;              // Resolution of the FOV mesh
    [SerializeField] private MeshFilter viewMeshFilter;         // Mesh filter to display FOV mesh
    [SerializeField] private int edgeResolveIterations;         // Edge detection iterations
    [SerializeField] private float edgeDistanceThreshold;       // Distance threshold for edge detection

    private Mesh viewMesh;
    [SerializeField] private List<Transform> visibleTargets = new List<Transform>();

    private void Start()
    {
        //viewMesh = new Mesh();
        //viewMesh.name = "ViewMesh";
        //viewMeshFilter.mesh = viewMesh;

        //StartCoroutine("FindTargetsWithDelay", 0.2f);
        viewMesh = new Mesh { name = "ViewMesh" };
        viewMeshFilter.mesh = viewMesh;
        StartCoroutine(FindTargetsWithDelay(0.2f));
    }
    private void LateUpdate()
    {
        DrawFOV();
    }
    /// <summary>
    /// Periodically finds visible targets.
    /// </summary>
    private IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisableTargets();
        }
    }

    /// <summary>
    /// Detects targets within the FOV and checks for obstacles.
    /// </summary>
    private void FindVisableTargets()
    {
        visibleTargets.Clear();

        Collider[] targetInViewRadius = Physics.OverlapSphere(playerVisualTransform.position, viewRadius, targetMask);

        for (int i = 0; i < targetInViewRadius.Length; i++)
        {
            Transform target = targetInViewRadius[i].transform;

            Vector3 dirToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(playerVisualTransform.forward,dirToTarget) < viewAngle / 2)
            {
                float distToTarget = Vector3.Distance(playerVisualTransform.position, target.position); // Check if there is an obstacle in the FOV

                if (!Physics.Raycast(playerVisualTransform.position, dirToTarget, distToTarget, obstacleMask))
                {
                    // there are no obstacles in the way, so we can see the target.
                    // If the target is in View
                    // Do something to the target
                    visibleTargets.Add(target);
                }
            }
        }
    }
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += playerVisualTransform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
    /// <summary>
    /// Draws the FOV mesh using raycasting.
    /// </summary>
    private void DrawFOV()
    {
        int rayCount = Mathf.RoundToInt(viewAngle * meshResolution);
        float rayAngleSize = viewAngle / rayCount;

        List<Vector3> viewPoints = new List<Vector3>();

        ViewCastInfo oldViewCast = new ViewCastInfo();

        for (int i = 0; i <= rayCount; i++)
        {
            float angle = playerVisualTransform.eulerAngles.y - viewAngle / 2 + rayAngleSize * i; // angle is current rotation of the player

            ViewCastInfo newViewCast = ViewCast(angle);

            if (i > 0)
            {
                bool edgeDistanceThresholdExceeded = Mathf.Abs(oldViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;
                if (oldViewCast.hit != newViewCast.hit || (oldViewCast.hit && newViewCast.hit && edgeDistanceThresholdExceeded)) 
                {
                    EdgeInfo edge = FindEdge(oldViewCast, newViewCast);
                    if (edge.pointA != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointA);
                    }
                    if (edge.pointB != Vector3.zero)
                    {
                        viewPoints.Add(edge.pointB);
                    }
                }
            }

            viewPoints.Add(newViewCast.point);
            oldViewCast = newViewCast;
        }

        int vertexCount = viewPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < vertexCount - 1; i++)
        {
            vertices[i + 1] = playerVisualTransform.InverseTransformPoint(viewPoints[i]);

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
        }

        viewMesh.Clear();

        viewMesh.vertices = vertices;
        viewMesh.triangles = triangles;
        viewMesh.RecalculateNormals();
    }

    private EdgeInfo FindEdge(ViewCastInfo minViewCast, ViewCastInfo maxViewCast)
    {
        float minAngle = minViewCast.angle;
        float maxAngle = maxViewCast.angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            ViewCastInfo newViewCast = ViewCast(angle);

            bool edgeDistanceThresholdExceeded = Mathf.Abs(minViewCast.distance - newViewCast.distance) > edgeDistanceThreshold;

            if (newViewCast.hit == minViewCast.hit && !edgeDistanceThresholdExceeded)
            {
                minAngle = angle;
                minPoint = newViewCast.point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = newViewCast.point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private ViewCastInfo ViewCast(float globalAngle)
    {
        Vector3 dir = DirFromAngle(globalAngle, true);
        RaycastHit hit;

        if (Physics.Raycast(playerVisualTransform.position, dir, out hit, viewRadius, obstacleMask))
        {
            return new ViewCastInfo(true, hit.point, hit.distance, globalAngle);
        }
        else
        {
            return new ViewCastInfo(false, playerVisualTransform.position + dir * viewRadius, viewRadius, globalAngle);
        }
    }
    public List<Transform> GetVisibleTargets() => visibleTargets;
    public float GetFOVRadius() => viewRadius;
    public float GetFOVAngle() => viewAngle;
    //public List<Transform> GetVisibleTargets()
    //{
    //    return visibleTargets;
    //}

    //public float GetFOVRadius()
    //{
    //    return viewRadius;
    //}
    //public float GetFOVAngle()
    //{
    //    return viewAngle;
    //}

    public struct ViewCastInfo
    {
        public bool hit;
        public Vector3 point;
        public float distance;
        public float angle;

        public ViewCastInfo(bool _hit, Vector3 _point, float _distance, float _angle)
        {
            hit = _hit;
            point = _point;
            distance = _distance;
            angle = _angle;
        }
    }

    public struct EdgeInfo
    {
        public Vector3 pointA;
        public Vector3 pointB;

        public EdgeInfo(Vector3 _pointA, Vector3 _pointB)
        {
            pointA = _pointA;
            pointB = _pointB;
        }
    }
}
