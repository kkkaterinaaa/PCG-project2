using UnityEngine;
using System.Collections.Generic;

public class InfiniteWorldGenerator : MonoBehaviour
{
    public VoronoiGenerator voronoiPrefab; 
    public DLA2 dlaPrefab;             
    public Vector2 segmentSize = new Vector2(10, 10); // Dimensions of each world segment
    public Camera mainCamera; // Camera used to track position

    // Dictionary to keep track of active world segments
    private Dictionary<Vector2Int, GameObject> activeSegments = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int currentSegment;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        UpdateSegments(); // Initialize the first set of segments
    }

    void Update()
    {
        UpdateSegments();
    }

    private void UpdateSegments()
    {
        Vector3 cameraPosition = mainCamera.transform.position;

        // Calculate the current segment index based on camera position
        Vector2Int newSegment = new Vector2Int(
            Mathf.FloorToInt(cameraPosition.x / segmentSize.x),
            Mathf.FloorToInt(cameraPosition.y / segmentSize.y)
        );

        // Load new segments if the camera enters a different segment
        if (newSegment != currentSegment)
        {
            currentSegment = newSegment;
            LoadNearbySegments();
        }
    }

    private void LoadNearbySegments()
    {
        int loadRange = 1; // Number of segments to load around the current segment

        // Identify segments that need to be unloaded
        List<Vector2Int> segmentsToRemove = new List<Vector2Int>();
        foreach (var segment in activeSegments.Keys)
        {
            if (Mathf.Abs(segment.x - currentSegment.x) > loadRange ||
                Mathf.Abs(segment.y - currentSegment.y) > loadRange)
            {
                segmentsToRemove.Add(segment);
            }
        }

        // Unload and clean up segments
        foreach (var segment in segmentsToRemove)
        {
            if (activeSegments.TryGetValue(segment, out GameObject segmentObject))
            {
                VoronoiGenerator voronoi = segmentObject.GetComponentInChildren<VoronoiGenerator>();
                if (voronoi != null)
                    voronoi.Cleanup(); // Clean up Voronoi markers

                DLA2 dla = segmentObject.GetComponentInChildren<DLA2>();
                if (dla != null)
                    dla.Cleanup(); // Clean up DLA-generated objects

                Destroy(segmentObject); // Destroy the entire segment object
                activeSegments.Remove(segment);
            }
        }

        // Load segments within the defined range
        for (int x = -loadRange; x <= loadRange; x++)
        {
            for (int y = -loadRange; y <= loadRange; y++)
            {
                Vector2Int segment = currentSegment + new Vector2Int(x, y);
                if (!activeSegments.ContainsKey(segment))
                {
                    GenerateSegment(segment);
                }
            }
        }
    }

    private void GenerateSegment(Vector2Int segment)
    {
        Vector3 segmentPosition = new Vector3(
            segment.x * segmentSize.x, // Position based on segment index and size
            segment.y * segmentSize.y,
            0
        );

        // Create a parent GameObject to group Voronoi and DLA objects
        GameObject segmentParent = new GameObject($"Segment_{segment}");
        segmentParent.transform.position = segmentPosition;
        
        GameObject voronoiObject = Instantiate(voronoiPrefab.gameObject, segmentParent.transform);
        VoronoiGenerator voronoi = voronoiObject.GetComponent<VoronoiGenerator>();
        if (voronoi != null)
        {
            voronoi.screenBounds = segmentSize / 2; // Set bounds based on segment size
        }
        
        GameObject dlaObject = Instantiate(dlaPrefab.gameObject, segmentParent.transform);
        DLA2 dla = dlaObject.GetComponent<DLA2>();
        if (dla != null)
        {
            dla.segmentBounds = segmentSize / 2; // Set DLA bounds based on segment size
            dla.voronoiGenerator = voronoi;
        }
        
        activeSegments[segment] = segmentParent;
    }
}
