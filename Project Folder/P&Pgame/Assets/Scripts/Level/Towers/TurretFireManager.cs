using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TurretFireManager : MonoBehaviour
{
    [Header("Firing Points")]
    [SerializeField] private List<Transform> levelOneFirePoints = new List<Transform>();
    [SerializeField] private List<Transform> levelTwoFirePoints = new List<Transform>();
    [SerializeField] private List<Transform> levelThreeFirePoints = new List<Transform>();

    [Header("Stats")]
    [Range(1, 3)][SerializeField] private int turretLvl = 0;
    [SerializeField] private int currentFirePointIndex = 0;

    private List<Transform> currentFirePoints;

    private void Start()
    {
        // Initialize the current fire points based on the starting turret level
        switch (turretLvl)
        {
            case 2:
                currentFirePoints = levelTwoFirePoints;
                break;
            case 3:
                currentFirePoints = levelThreeFirePoints;
                break;
            default:
                currentFirePoints = levelOneFirePoints; // Default to level 1
                break;
        }
    }

    // Public method for the controller script to get the next firing point
    public Transform GetNextFirePoint()
    {
        if (currentFirePoints == null || currentFirePoints.Count == 0)
        {
            Debug.LogError("Current fire points list is empty!");
            return null;
        }

        // Get the current firing point
        Transform firePoint = currentFirePoints[currentFirePointIndex];

        // Advance to the next index (round-robin logic)
        currentFirePointIndex++;
        if (currentFirePointIndex >= currentFirePoints.Count)
        {
            currentFirePointIndex = 0;
        }

        return firePoint;
    }
}
