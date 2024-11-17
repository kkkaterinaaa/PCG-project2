using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DLA2 : MonoBehaviour
{
    [Header("General Settings")]
    public VoronoiGenerator voronoiGenerator; 
    public bool drawVines = true; // Toggle for generating vines

    [Header("Segment Settings")]
    public Vector2 segmentBounds; 
    
    [System.Serializable]
    public class VineSettings
    {
        [Header("Vine Appearance")]
        public float segmentWidth = 0.1f; // Initial width of vine segments
        public float widthTaper = 0.97f; // Tapering factor for segment width
        public int curveResolutionMultiplier = 20; // Resolution for Bezier curves

        [Header("Randomness")]
        public float noiseScale = 0.5f; // Perlin noise scale for randomness
        public float controlPointRandomness = 0.2f; // Variability in Bezier control points
        public float midpointOffset = 0.1f; // Offset for midpoints along the curve

        [Header("Growth Timing")]
        public float growthDelay = 0.01f; // Delay between steps during vine growth
    }

    [System.Serializable]
    public class FloralSettings
    {
        [Header("Branch Settings")]
        public int minBranchSteps = 10; // Min steps for branch growth
        public int maxBranchSteps = 20; // Max steps for branch growth
        public float branchWidth = 0.2f; // Initial branch width
        public float branchTaper = 0.9f; // Tapering factor for branch width
        public float branchOffsetScale = 0.5f;

        [Header("Clusters")]
        public int minClusterCount = 3; // Min number of clusters per branch
        public int maxClusterCount = 6; // Max number of clusters per branch
        public float minClusterScale = 0.03f; // Min size of clusters

        [Header("Growth Timing")]
        public float growthDelay = 0.02f; // Delay between steps during branch growth

        [Header("Branching Chance")]
        public float branchingChance = 0.1f; // Probability of creating a new branch
    }

    [Header("Vine Settings")]
    public VineSettings vineSettings = new VineSettings();

    [Header("Floral Settings")]
    public FloralSettings floralSettings = new FloralSettings();

    private List<Vector2> growthPoints;
    private List<GameObject> generatedObjects = new List<GameObject>();

    void Start()
    {
        if (voronoiGenerator == null || voronoiGenerator.GetPoints() == null || voronoiGenerator.GetPoints().Count == 0)
        {
            Debug.LogError("VoronoiGenerator points are not initialized. Please check the setup.");
            return;
        }

        // Copy points from Voronoi for local use
        growthPoints = new List<Vector2>(voronoiGenerator.GetPoints());
        Debug.Log($"DLA initialized with {growthPoints.Count} points.");

        if (drawVines)
            GenerateVineLines();

        GenerateFloralPatterns();
    }

    void GenerateVineLines()
    {
        // Generate vine lines between points
        for (int i = 0; i < growthPoints.Count; i++)
        {
            Vector2 start = growthPoints[i];
            Vector2 closestPoint = FindClosestPoint(start, i);

            if (closestPoint != Vector2.zero)
            {
                StartCoroutine(GrowVineLine(start, closestPoint));
            }
        }
    }

    Vector2 FindClosestPoint(Vector2 start, int currentIndex)
    {
        // Find the closest point to the given point
        float closestDistance = float.MaxValue;
        Vector2 closestPoint = Vector2.zero;

        for (int i = 0; i < growthPoints.Count; i++)
        {
            if (i == currentIndex) continue;

            float distance = Vector2.Distance(start, growthPoints[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = growthPoints[i];
            }
        }

        return closestPoint;
    }

    private IEnumerator GrowVineLine(Vector2 start, Vector2 end)
    {
        // Grow a vine line using a cubic Bezier curve
        float distance = Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;
        Vector2 midpoint = (start + end) / 2;

        // Calculate Bezier control points
        Vector2 controlPoint1 = midpoint + Random.insideUnitCircle * vineSettings.controlPointRandomness * distance -
                                direction * vineSettings.midpointOffset * distance;
        Vector2 controlPoint2 = midpoint + Random.insideUnitCircle * vineSettings.controlPointRandomness * distance +
                                direction * vineSettings.midpointOffset * distance;

        float t = 0f;
        float segmentWidth = vineSettings.segmentWidth;
        int curveResolution = Mathf.CeilToInt(distance * vineSettings.curveResolutionMultiplier);

        while (t <= 1f)
        {
            t += 1f / curveResolution;

            // Calculate the current point along the Bezier curve
            Vector2 curvePoint = Mathf.Pow(1 - t, 3) * start +
                                 3 * Mathf.Pow(1 - t, 2) * t * controlPoint1 +
                                 3 * (1 - t) * Mathf.Pow(t, 2) * controlPoint2 +
                                 Mathf.Pow(t, 3) * end;

            // Add Perlin noise for randomness
            float noise = Mathf.PerlinNoise(curvePoint.x * vineSettings.noiseScale, curvePoint.y * vineSettings.noiseScale) * 0.05f;
            curvePoint += Random.insideUnitCircle * noise;

            // Create vine segment
            GameObject segment = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            segment.transform.position = curvePoint;
            segment.transform.localScale = Vector3.one * segmentWidth;
            segment.GetComponent<Renderer>().material.color = new Color(0, Random.Range(0.3f, 0.5f), 0, 0.8f);

            generatedObjects.Add(segment); // Track the generated object
            segmentWidth *= vineSettings.widthTaper;

            yield return new WaitForSeconds(vineSettings.growthDelay);
        }
    }

    void GenerateFloralPatterns()
    {
        foreach (var center in growthPoints)
        {
            StartCoroutine(GrowFloralBranch(center));
        }
    }

    private IEnumerator GrowFloralBranch(Vector2 center)
    {
        // Grow a branch starting from a center point
        Vector2 particlePosition = center + Random.insideUnitCircle * 0.1f;
        float angle = Random.Range(0, 360f);

        int branchSteps = Random.Range(floralSettings.minBranchSteps, floralSettings.maxBranchSteps);
        float branchWidth = floralSettings.branchWidth;

        for (int step = 0; step < branchSteps; step++)
        {
            angle += Random.Range(-20f, 20f);
            Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
            particlePosition += direction * 0.1f;

            // Generate clusters at the current branch position
            for (int i = 0; i < Random.Range(floralSettings.minClusterCount, floralSettings.maxClusterCount); i++)
            {
                Vector2 clusterOffset = Random.insideUnitCircle * branchWidth * floralSettings.branchOffsetScale;
                Vector2 clusterPosition = particlePosition + clusterOffset;

                GameObject floralParticle = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                floralParticle.transform.position = clusterPosition;
                floralParticle.transform.localScale = Vector3.one * Random.Range(floralSettings.minClusterScale, branchWidth);
                floralParticle.GetComponent<Renderer>().material.color = new Color(0, Random.Range(0.4f, 0.6f), 0);
                generatedObjects.Add(floralParticle);
            }

            branchWidth *= floralSettings.branchTaper;

            if (branchWidth < 0.05f)
                break;

            // Chance to spawn a new branch
            if (Random.value < floralSettings.branchingChance && step > 5)
            {
                StartCoroutine(GrowFloralBranch(particlePosition));
            }

            yield return new WaitForSeconds(floralSettings.growthDelay);
        }
    }

    public void Cleanup()
    {
        // Destroy all generated objects
        foreach (var obj in generatedObjects)
        {
            if (obj != null)
                Destroy(obj);
        }
        generatedObjects.Clear();
    }
}
