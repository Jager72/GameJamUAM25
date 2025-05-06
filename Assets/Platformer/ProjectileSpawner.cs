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
    public float waveSpeedMultiplier = 1f;
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
    [SerializeField] private float speedMultiplier = 1f;

    private WaveData[] waves;

    private void Awake()
    {
        loadJson();
    }

    private void loadJson()
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
            loadJson();
            Debug.Log("Reloading JSON data...");
            StartCoroutine(RunAllWaves());
        }
    }

    private IEnumerator RunAllWaves()
    {
        for (int i = 0; i < waves.Length; i++)
        {
            Debug.Log($"Wave {i + 1}: Starting...");
            // Reset flag for this wave
            bool hitInWave = false;
            bool diedInWave = false;
            // Subscribe to the Projectile event
            Action onHit = () => hitInWave = true;
            Action onDead = () => diedInWave = true;

            Projectile.OnPlayerHit += onHit;
            PlayerHealth.OnPlayerDead += onDead;
            Debug.Log($"Wave {i + 1}: Waiting for {waves[i].spawnPoints.Length} projectiles to spawn... Speed multiplier: {waves[i].waveSpeedMultiplier}");

            // Spawn & wait for wave to finish
            yield return StartCoroutine(SpawnWave(waves[i].spawnPoints, waves[i].waveSpeedMultiplier));

            // Unsubscribe
            Projectile.OnPlayerHit -= onHit;
            PlayerHealth.OnPlayerDead -= onDead;

            if (diedInWave)
            {
                i = -1;
                Debug.Log($"Wave {i + 1}: Player has died! retrying...");
                FindAnyObjectByType<PlayerHealth>().Heal(5);
            }
            else if (hitInWave)
            {
                Debug.Log($"Wave {i + 1}: Player was hit! retrying...");
                i--;
            }
            else
            {
                Debug.Log($"Wave {i + 1}: Player was safe!");
            }
        }
    }

    private IEnumerator SpawnWave(SpawnData[] spawnPoints, float waveSpeedMultiplier = 1f)
    {
        var holders = new List<GameObject>();

        foreach (var data in spawnPoints)
        {
            var holder = new GameObject("Shooter");
            holder.transform.SetParent(transform, worldPositionStays: false);
            holder.transform.localPosition = data.position;

            holder.AddComponent<DestroyWhenEmpty>();

            var projGo = Instantiate(
                projectilePrefab,
                holder.transform.position,
                Quaternion.identity,
                holder.transform
            );
            var proj = projGo.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.SetAngle(data.angleDegrees);
                proj.SetSpeed(data.speed * speedMultiplier * waveSpeedMultiplier);
            }

            holders.Add(holder);
            yield return new WaitForSeconds(data.delaySeconds);
        }

        // Wait until every holder has self‐destroyed
        yield return new WaitUntil(() => holders.TrueForAll(h => h == null));
    }
}
