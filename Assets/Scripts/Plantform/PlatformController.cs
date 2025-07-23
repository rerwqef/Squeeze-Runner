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

    private Queue<GameObject> platformPool = new();
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
    }

    [Button("StartTheGame")]
    public void StartTheGame()
    {
        if (hasStarted) return;

        hasStarted = true;

        platformPool.Clear();
        for (int i = 0; i < 20; i++)
        {
            SpawnPlatform(activePlatforms.Count > 0 ? activePlatforms[^1] : null);
        }
    }

    void AddToPool(GameObject prefab)
    {
        GameObject obj = Instantiate(prefab);
        obj.SetActive(false);
        obj.transform.SetParent(spawnPoint);
        platformPool.Enqueue(obj);
    }

    void HandlePlatformEnd(Platform endedPlatform)
    {
        if (endedPlatform == null) return;

        int index = activePlatforms.IndexOf(endedPlatform);
        if (index != -1 && index > 2)
        {
            Platform removingPlatform = activePlatforms[0];
            activePlatforms.Remove(removingPlatform);
            ReturnToPool(removingPlatform.gameObject);
        }

        SpawnPlatform(activePlatforms.Count > 0 ? activePlatforms[^1] : null);
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
                GameObject mediator = GetMediatorPlatform(prevType, newType, out bool flip);
                if (mediator != null)
                {
                    float mediatorLength = GetPlatformLength(mediator);
                    Vector3 mediatorPos = prevPos + new Vector3(0, 0, (prevLength + mediatorLength) / 2f);

                    GameObject mediatorObj = Instantiate(mediator, mediatorPos, Quaternion.identity);
                    mediatorObj.transform.rotation = flip
                         ? Quaternion.Euler(90, 0, -90)
                         : Quaternion.Euler(90, 180f, -90);
                    mediatorObj.transform.SetParent(activePlatformsParent.transform);

                    Platform mediatorPlatform = mediatorObj.GetComponent<Platform>();
                    mediatorPlatform?.Init(moveSpeed);
                    mediatorPlatform.myType = Type.transition;
                    activePlatforms.Add(mediatorPlatform);

                    // Update reference for spawn position
                    previousPlatform = mediatorPlatform;
                    prevPos = mediatorPos;
                    prevLength = mediatorLength;
                }
            }

            spawnPos = previousPlatform.transform.position + new Vector3(0, 0, (prevLength + newLength) / 2f);
        }

        spawnPos.y = 0;

        newPlatformObj.transform.position = spawnPos;
        newPlatformObj.transform.SetParent(activePlatformsParent.transform);
        newPlatformObj.SetActive(true);

        newPlatform.Init(moveSpeed);
        activePlatforms.Add(newPlatform);

        int indexInList = activePlatforms.IndexOf(newPlatform);
        newPlatformObj.name = $"Platform_{indexInList}_{newPlatform.myType}";
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

    GameObject GetMediatorPlatform(Type from, Type to, out bool shouldFlip)
    {
        shouldFlip = false;

        if ((from == Type.small && to == Type.medium) || (from == Type.medium && to == Type.small))
        {
            shouldFlip = from == Type.medium;
            return smallToMedium;
        }

        if ((from == Type.medium && to == Type.large) || (from == Type.large && to == Type.medium))
        {
            shouldFlip = from == Type.large;
            return mediumToLarge;
        }

        if ((from == Type.small && to == Type.large) || (from == Type.large && to == Type.small))
        {
            shouldFlip = from == Type.large;
            return smallToLarge;
        }

        return null;
    }

    public void ReturnToPool(GameObject platformObj)
    {
        if (platformObj == null) return;

        Platform platform = platformObj.GetComponent<Platform>();
        if (platform != null && activePlatforms.Contains(platform))
        {
            activePlatforms.Remove(platform);
        }

        platformObj.SetActive(false);
        platformObj.transform.SetParent(spawnPoint);
        platformPool.Enqueue(platformObj);
    }

    public static void NotifyPlatformEnd(Platform platform)
    {
        OnPlatformEnd?.Invoke(platform);
    }
}
