using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public class SpawnData
{
    public Vector2 position;
    public float angleDegrees;
    public float delaySeconds = 0f;
    public float speed = 6f;
}

[System.Serializable]
public class WaveData
{
    public SpawnData[] spawnPoints;
}

[System.Serializable]
public class WaveDataList
{
    public WaveData[] waves;
}

public class ProjectileSpawner : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private string jsonFileName = "spawnData.json";

    private WaveData[] waves;

    private void Awake()
    {
        // load waves from JSON
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            waves = JsonUtility.FromJson<WaveDataList>(json).waves;
        }
        else
        {
            Debug.LogWarning($"Could not find spawn JSON at: {path}");
            waves = new WaveData[0];
        }
    }

    private void Start()
    {
        StartCoroutine(RunAllWaves());
    }

    void Update()
    {
        if (Keyboard.current.leftCtrlKey.wasPressedThisFrame)
        {
            StopAllCoroutines();
            StartCoroutine(RunAllWaves());
        }
    }

    private IEnumerator RunAllWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            // Reset flag for this wave
            bool hitInWave = false;
            // Subscribe to the Projectile event
            Action onHit = () => hitInWave = true;
            Projectile.OnPlayerHit += onHit;

            // Spawn & wait for wave to finish
            yield return StartCoroutine(SpawnWave(waves[i].spawnPoints));

            // Unsubscribe
            Projectile.OnPlayerHit -= onHit;

            // Log result
            if (hitInWave)
                Debug.Log($"Wave {i + 1}: Player was hit!");
            else
                Debug.Log($"Wave {i + 1}: Player was safe!");
        }
    }

    private IEnumerator SpawnWave(SpawnData[] spawnPoints)
    {
        var holders = new List<GameObject>();

        foreach (var data in spawnPoints)
        {
            var holder = new GameObject("Shooter");
            holder.transform.SetParent(transform, worldPositionStays: true);
            holder.transform.position = data.position;
            holder.AddComponent<DestroyWhenEmpty>();

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

            holders.Add(holder);
            yield return new WaitForSeconds(data.delaySeconds);
        }

        // Wait until every holder has self‐destroyed
        yield return new WaitUntil(() => holders.TrueForAll(h => h == null));
    }
}
