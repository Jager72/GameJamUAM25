using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;

[System.Serializable]
public class SpawnData
{
    public Vector2 position;
    public float angleDegrees;
}

[System.Serializable]
public class SpawnDataList
{
    public SpawnData[] spawnPoints;
}

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    private SpawnData[] spawnPoints;
    [SerializeField] private string jsonFileName = "spawnData.json";

    private void Awake()
    {
        // read JSON from StreamingAssets
        var path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);
            spawnPoints = JsonUtility.FromJson<SpawnDataList>(json).spawnPoints;
        }
        else
        {
            Debug.LogWarning($"Could not find spawn JSON at: {path}");
        }
    }

    void Update()
    {
        // e.g. press Left control to fire
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            SpawnProjectiles();
    }

    private void SpawnProjectiles()
    {
        foreach (var data in spawnPoints)
        {
            // 1) create an empty owner at the given coords
            var holder = new GameObject("Shooter");
            holder.transform.position = data.position;
            holder.AddComponent<DestroyWhenEmpty>();
            holder.transform.SetParent(transform);

            // 2) spawn the projectile as a child, set its direction
            var projGo = Instantiate(
                projectilePrefab,
                data.position,
                Quaternion.identity,
                holder.transform
            );

            var proj = projGo.GetComponent<Projectile>();
            if (proj != null)
                proj.SetAngle(data.angleDegrees);

        }
    }
}