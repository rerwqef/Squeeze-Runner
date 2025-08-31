using System;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlatformController : MonoBehaviour
{
    public static PlatformController Instance { get; private set; }
    public static event Action<Platform> OnPlatformEnd;

    [Header("Platform Prefabs")]
    public GameObject platformStartingPrefab;
    public GameObject platformSmallPrefab;
    public GameObject platformMediumPrefab;
    public GameObject platformLargePrefab;

    [Header("Transition Mediators")]
    public GameObject smallToMedium;
    public GameObject mediumToLarge;
    public GameObject smallToLarge;

    [Header("Settings")]
    public float moveSpeed = 10f;
    public Transform spawnPoint;
    public GameObject activePlatformsParent;
    public int maxActivePlatforms = 10;

    public List<Platform> activePlatforms = new();

    private Platform startingPlatform;   // showcase starting platform
    public bool hasStarted = true;

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
        try
        {
            // Spawn the permanent starting platform for showcase
            GameObject startObj = Instantiate(platformStartingPrefab, transform.position, Quaternion.identity);
            startingPlatform = startObj.GetComponent<Platform>();
            if (startingPlatform == null)
            {
                Debug.LogError("Starting platform prefab missing Platform component!");
            }
            else
            {
                startObj.transform.SetParent(activePlatformsParent?.transform);
                activePlatforms.Add(startingPlatform);
            }

            // Spawn a few showcase platforms after starting one
            for (int i = 0; i < 5; i++)
            {
                SpawnPlatform(activePlatforms[^1]);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"PlatformController.Start crashed: {ex.Message}");
        }
    }

    void Update()
    {
        try
        {
            for (int i = activePlatforms.Count - 1; i >= 0; i--)
            {
                if (activePlatforms[i] == null) continue;
                Platform p = activePlatforms[i];

                // Move platforms
                p.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime, Space.World);

                // Skip despawn check for starting platform
                if (p == startingPlatform) continue;

                // Despawn other platforms
                if (p.transform.position.z < -8f)
                {
                    HandlePlatformEnd(p);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"PlatformController.Update crashed: {ex.Message}");
        }
    }

    [Button("StartTheGame")]
    public void StartTheGame()
    {
        try
        {
            if (hasStarted) return;
            hasStarted = true;

            // Spawn new platforms to reach max count
            for (int i = activePlatforms.Count; i < maxActivePlatforms; i++)
            {
                SpawnPlatform(activePlatforms[^1]);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"StartTheGame crashed: {ex.Message}");
        }
    }

    void HandlePlatformEnd(Platform endedPlatform)
    {
        if (endedPlatform == null) return;

        // never despawn starting platform
        if (endedPlatform == startingPlatform) return;

        if (activePlatforms.Contains(endedPlatform))
        {
            activePlatforms.Remove(endedPlatform);
            Destroy(endedPlatform.gameObject);
        }

        if (activePlatforms.Count < maxActivePlatforms)
            SpawnPlatform(activePlatforms.Count > 0 ? activePlatforms[^1] : null);
    }
    public Type previousType;
    public Type newType;
    void SpawnPlatform(Platform previousPlatform)
    {
        try
        {
            GameObject newPlatformObj = GetPlatformToSpawn();
            if (newPlatformObj == null) return;

            Platform newPlatform = newPlatformObj.GetComponent<Platform>();
            if (newPlatform == null)
            {
                Debug.LogError("SpawnPlatform: Missing Platform component!");
                return;
            }

            float newLength = GetPlatformLength(newPlatformObj);
            Vector3 spawnPos;

            if (previousPlatform == null)
            {
                spawnPos = transform.position;
            }
            else
            {
                float prevLength = GetPlatformLength(previousPlatform.gameObject);
                 previousType = previousPlatform.myType;
                 newType = newPlatform.myType;
                Vector3 prevPos = previousPlatform.transform.position;

                // Insert mediator if types mismatch
                if (hasStarted && previousType!=null && newType!=null && previousType != newType && previousType != Type.transition && newType != Type.transition)
                {
                    GameObject mediatorPrefab = GetMediatorPlatform(previousType, newType, out bool flip);
                    if (mediatorPrefab != null)
                    {
                        Debug.Log($"mediyatorprefab is not null"+mediatorPrefab.name);
                     //   GameObject mediatorObj = Instantiate(mediatorPrefab);
                      //  if (mediatorObj == null) { Debug.Log($"mediatorObj is null"); }
                       // float mediatorLength = GetPlatformLength(mediatorObj);
                          //   Debug.Log($"mediatorLength is {mediatorLength}");
                        //     Vector3 mediatorPos = prevPos + new Vector3(0, 0, (prevLength + mediatorLength) / 2f);

                        //     mediatorObj.transform.position = mediatorPos;
                        //     mediatorObj.transform.rotation = flip
                        //         ? Quaternion.Euler(90, 0, -90)
                        //         : Quaternion.Euler(90, 180f, -90);
                        //     mediatorObj.transform.SetParent(activePlatformsParent?.transform);

                        //     Platform mediatorPlatform = mediatorObj.GetComponent<Platform>();
                        //     if (mediatorPlatform != null)
                        //     {
                        //         mediatorPlatform.myType = Type.transition;
                        //         activePlatforms.Add(mediatorPlatform);
                        //         previousPlatform = mediatorPlatform;
                        //         prevLength = mediatorLength;
                        //     }
                    }
                }

                spawnPos = previousPlatform.transform.position + new Vector3(0, 0, (prevLength + newLength) / 2f);
            }

            spawnPos.y = transform.position.y;
            newPlatformObj.transform.position = spawnPos;
            newPlatformObj.transform.SetParent(activePlatformsParent?.transform);

            activePlatforms.Add(newPlatform);
            newPlatformObj.name = $"Platform_{activePlatforms.Count}_{newPlatform.myType}";
        }
        catch (Exception ex)
        {
            Debug.LogError($"SpawnPlatform crashed: {ex.Message}");
        }
    }

    GameObject GetPlatformToSpawn()
    {
        if (!hasStarted && activePlatforms.Count == 0)
            return Instantiate(platformStartingPrefab);

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
        Platform p = obj.GetComponent<Platform>();
        if (p != null && p.myType == Type.Starting) return 7;

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

    public static void NotifyPlatformEnd(Platform platform)
    {
        OnPlatformEnd?.Invoke(platform);
    }
}