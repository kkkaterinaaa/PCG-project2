using UnityEngine;
using System.Collections.Generic;

public class VoronoiGenerator : MonoBehaviour
{
    public int pointCount = 10; // Number of points
    public Vector2 screenBounds; // Screen boundaries in world space
    private List<Vector2> points; // List of generated points
    private List<GameObject> voronoiMarkers = new List<GameObject>(); // Visual markers for the points

    void Start()
    {
        points = new List<Vector2>();
        screenBounds = Camera.main.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height)); // Calculate screen bounds

        GenerateRandomPoints(); // Generate seed points
        VisualizePoints(); // visualize the points
    }

    void GenerateRandomPoints()
    {
        Vector2 localBounds = screenBounds;

        for (int i = 0; i < pointCount; i++)
        {
            Vector2 randomPoint = new Vector2(
                Random.Range(-localBounds.x, localBounds.x),
                Random.Range(-localBounds.y, localBounds.y)
            );
            randomPoint += (Vector2)transform.position; // Offset points based on the object's position
            points.Add(randomPoint); // Add point to the list
        }
    }

    void VisualizePoints()
    {
        foreach (var point in points)
        {
            GameObject pointMarker = GameObject.CreatePrimitive(PrimitiveType.Sphere); // Create a sphere for visualization
            pointMarker.transform.position = point; // Place the marker at the point's position
            pointMarker.transform.localScale = Vector3.one * 0.1f; // Scale down the marker
            pointMarker.GetComponent<Renderer>().material.color = Color.green;
            voronoiMarkers.Add(pointMarker); // Store the marker for cleanup
        }
    }
    
    public void Cleanup()
    {
        foreach (var marker in voronoiMarkers)
        {
            if (marker != null)
                Destroy(marker);
        }
        voronoiMarkers.Clear(); // Clear the list
    }

    public List<Vector2> GetPoints()
    {
        return points; // Return the generated points
    }
}
