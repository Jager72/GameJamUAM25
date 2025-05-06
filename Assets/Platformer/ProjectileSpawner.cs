using UnityEngine;
using System.IO;
using UnityEngine.InputSystem;
using System.Collections;

[System.Serializable]
public class SpawnData
{
    public Vector2 position;
    public float angleDegrees;
    public float delaySeconds = 0f;
    public float speed = 6f;
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

    private void Start()
    {
        // spawn projectiles at start
        StartCoroutine(SpawnProjectiles());
    }

    void Update()
    {
        // e.g. press Left control to fire
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
            StartCoroutine(SpawnProjectiles());
    }

    private IEnumerator SpawnProjectiles()
    {
        foreach (var data in spawnPoints)
        {
            var holder = new GameObject("Shooter");
            holder.transform.position = data.position;
            holder.AddComponent<DestroyWhenEmpty>();
            holder.transform.SetParent(transform);

            var projGo = Instantiate(
                projectilePrefab,
                data.position,
                Quaternion.identity,
                holder.transform
            );
            var proj = projGo.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetAngle(data.angleDegrees);
                proj.SetSpeed(data.speed);
            }

            // wait before next spawn
                yield return new WaitForSeconds(data.delaySeconds);
        }
    }
}