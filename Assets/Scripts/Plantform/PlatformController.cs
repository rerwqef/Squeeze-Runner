using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlatformController : MonoBehaviour
{
    public static PlatformController Instance { get; private set; }
    public static event Action<Platform> OnPlatformEnd;

    [Header("Platform Prefabs")]
    public GameObject platformSmallPrefab;
    public GameObject platformMediumPrefab;
    public GameObject platformLargePrefab;

    [Header("Transition Mediators")]
    public GameObject smallToMedium;
    public GameObject mediumToLarge;
    public GameObject smallToLarge;

    [Header("Settings")]
    public int poolSize = 10;
    public float moveSpeed = 10f;
    public Transform spawnPoint;
    public GameObject activePlatformsParent;
    public int maxActivePlatforms = 10;

    private Queue<GameObject> platformPool = new();
    private Dictionary<string, Queue<GameObject>> transitionPools = new();
    public List<Platform> activePlatforms = new();

    private bool hasStarted = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable() => OnPlatformEnd += HandlePlatformEnd;
    private void OnDisable() => OnPlatformEnd -= HandlePlatformEnd;

    void Start()
    {
        InitializePlatformPool();

        for (int i = 0; i < 6; i++)
        {
            SpawnPlatform(i == 0 ? null : activePlatforms[^1]);
        }
    }

    void Update()
    {
        if (!hasStarted) return;

        for (int i = activePlatforms.Count - 1; i >= 0; i--)
        {
            Platform p = activePlatforms[i];
            p.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

            if (p.transform.position.z < -100f)
            {
                ReturnToPool(p.gameObject);
            }
        }
    }

    void InitializePlatformPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject prefab = GetRandomPlatformPrefab();
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(spawnPoint);
            platformPool.Enqueue(obj);
        }

        // Initialize transition pools
        InitTransitionPool("smallToMedium", smallToMedium);
        InitTransitionPool("mediumToLarge", mediumToLarge);
        InitTransitionPool("smallToLarge", smallToLarge);
    }

    void InitTransitionPool(string key, GameObject prefab)
    {
        transitionPools[key] = new Queue<GameObject>();
        for (int i = 0; i < 3; i++)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.transform.SetParent(spawnPoint);
            transitionPools[key].Enqueue(obj);
        }
    }

    [Button("StartTheGame")]
    public void StartTheGame()
    {
        if (hasStarted) return;
        hasStarted = true;

        foreach (var p in activePlatforms)
        {
            ReturnToPool(p.gameObject);
        }
        activePlatforms.Clear();

        for (int i = 0; i < maxActivePlatforms; i++)
        {
            SpawnPlatform(i == 0 ? null : activePlatforms[^1]);
        }
    }

    void HandlePlatformEnd(Platform endedPlatform)
    {
        if (endedPlatform == null) return;

        if (activePlatforms.Contains(endedPlatform))
        {
            ReturnToPool(endedPlatform.gameObject);
        }

        if (activePlatforms.Count < maxActivePlatforms)
        {
            SpawnPlatform(activePlatforms.Count > 0 ? activePlatforms[^1] : null);
        }
    }

    void SpawnPlatform(Platform previousPlatform)
    {
        GameObject newPlatformObj = GetPlatformToSpawn();
        Platform newPlatform = newPlatformObj.GetComponent<Platform>();
        float newLength = GetPlatformLength(newPlatformObj);
        Vector3 spawnPos;

        if (previousPlatform == null)
        {
            spawnPos = Vector3.zero;
        }
        else
        {
            float prevLength = GetPlatformLength(previousPlatform.gameObject);
            Type prevType = previousPlatform.myType;
            Type newType = newPlatform.myType;

            Vector3 prevPos = previousPlatform.transform.position;

            if (hasStarted && prevType != newType && prevType != Type.transition && newType != Type.transition)
            {
                GameObject mediator = GetMediatorPlatform(prevType, newType, out bool flip, out string key);
                if (mediator != null)
                {
                    GameObject mediatorObj = GetFromTransitionPool(key, mediator);
                    float mediatorLength = GetPlatformLength(mediatorObj);
                    Vector3 mediatorPos = prevPos + new Vector3(0, 0, (prevLength + mediatorLength) / 2f);

                    mediatorObj.transform.position = mediatorPos;
                    mediatorObj.transform.rotation = flip
                        ? Quaternion.Euler(90, 0, -90)
                        : Quaternion.Euler(90, 180f, -90);
                    mediatorObj.transform.SetParent(activePlatformsParent.transform);
                    mediatorObj.SetActive(true);

                    Platform mediatorPlatform = mediatorObj.GetComponent<Platform>();
                    mediatorPlatform.myType = Type.transition;
                    activePlatforms.Add(mediatorPlatform);

                    previousPlatform = mediatorPlatform;
                    prevLength = mediatorLength;
                }
            }

            spawnPos = previousPlatform.transform.position + new Vector3(0, 0, (prevLength + newLength) / 2f);
        }

        spawnPos.y = 0;
        newPlatformObj.transform.position = spawnPos;
        newPlatformObj.transform.SetParent(activePlatformsParent.transform);
        newPlatformObj.SetActive(true);

        newPlatform.myType = newPlatform.myType;
        activePlatforms.Add(newPlatform);
        newPlatformObj.name = $"Platform_{activePlatforms.Count}_{newPlatform.myType}";
    }

    GameObject GetPlatformToSpawn()
    {
        if (!hasStarted)
            return Instantiate(platformSmallPrefab);

        return GetFromPool();
    }

    GameObject GetFromPool()
    {
        if (platformPool.Count > 0)
            return platformPool.Dequeue();

        return Instantiate(GetRandomPlatformPrefab());
    }

    GameObject GetFromTransitionPool(string key, GameObject prefab)
    {
        if (transitionPools.ContainsKey(key) && transitionPools[key].Count > 0)
        {
            return transitionPools[key].Dequeue();
        }

        return Instantiate(prefab);
    }

    GameObject GetRandomPlatformPrefab()
    {
        int rand = UnityEngine.Random.Range(0, 3);
        return rand switch
        {
            0 => platformSmallPrefab,
            1 => platformMediumPrefab,
            2 => platformLargePrefab,
            _ => platformMediumPrefab
        };
    }

    float GetPlatformLength(GameObject obj)
    {
        if (obj == null) return 0;

        Renderer rend = obj.GetComponentInChildren<Renderer>();
        if (rend != null) return rend.bounds.size.z;

        Collider col = obj.GetComponentInChildren<Collider>();
        if (col != null) return col.bounds.size.z;

        return 16f; // fallback
    }

    GameObject GetMediatorPlatform(Type from, Type to, out bool shouldFlip, out string poolKey)
    {
        shouldFlip = false;
        poolKey = "";

        if ((from == Type.small && to == Type.medium) || (from == Type.medium && to == Type.small))
        {
            shouldFlip = from == Type.medium;
            poolKey = "smallToMedium";
            return smallToMedium;
        }

        if ((from == Type.medium && to == Type.large) || (from == Type.large && to == Type.medium))
        {
            shouldFlip = from == Type.large;
            poolKey = "mediumToLarge";
            return mediumToLarge;
        }

        if ((from == Type.small && to == Type.large) || (from == Type.large && to == Type.small))
        {
            shouldFlip = from == Type.large;
            poolKey = "smallToLarge";
            return smallToLarge;
        }

        return null;
    }

    public void ReturnToPool(GameObject platformObj)
    {
        if (platformObj == null || platformPool.Contains(platformObj)) return;

        platformObj.SetActive(false);
        platformObj.transform.SetParent(spawnPoint);

        Platform platform = platformObj.GetComponent<Platform>();
        if (platform != null)
        {
            if (activePlatforms.Contains(platform))
                activePlatforms.Remove(platform);

            if (platform.myType == Type.transition)
            {
                string key = GetTransitionKey(platformObj.name);
                if (transitionPools.ContainsKey(key))
                {
                    transitionPools[key].Enqueue(platformObj);
                }
                else
                {
                    Destroy(platformObj); // fallback
                }
            }
            else
            {
                platformPool.Enqueue(platformObj);
            }
        }
    }

    string GetTransitionKey(string name)
    {
        if (name.Contains("smallToMedium")) return "smallToMedium";
        if (name.Contains("mediumToLarge")) return "mediumToLarge";
        if (name.Contains("smallToLarge")) return "smallToLarge";
        return "";
    }

    public static void NotifyPlatformEnd(Platform platform)
    {
        OnPlatformEnd?.Invoke(platform);
    }
}
