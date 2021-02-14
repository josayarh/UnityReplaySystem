using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public enum PoolableTypes
{
    Sphere = 1,
    SphereBot = 2
}

public class Pool : MonoBehaviour
{
    [System.Serializable]
    public struct PrefabToPool
    {
        public PoolableTypes key;
        public GameObject prefab;
        public int numberOfObjects;
    }

    [SerializeField] private PrefabToPool[] prefabToPoolArray;

    private static Pool instance = null;
    private Dictionary<PoolableTypes, List<GameObject>> disabledGameObjects 
        = new Dictionary<PoolableTypes, List<GameObject>>();
    private List<GameObject> activeGameObjects = new List<GameObject>();

    private void Awake()
    {
        if (instance == null)
        {
            setup();
        }
        else
        {
            Destroy(gameObject);
        }        
    }

    private void setup()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        foreach (PrefabToPool tmp in prefabToPoolArray)
        {
            List<GameObject> tmpList = null;
            if (!disabledGameObjects.TryGetValue(tmp.key, out tmpList))
            {
                tmpList = new List<GameObject>();
                disabledGameObjects.Add(tmp.key,tmpList);
            }
            
            for (uint c = 0; c < tmp.numberOfObjects; c++)
            {
                GameObject newObject = Instantiate(tmp.prefab, gameObject.transform);
                newObject.SetActive(false);
                tmpList.Add(newObject);
            }
        }
    }

    public GameObject get(PoolableTypes objectType, Transform locationData = null, Guid parentGuid = new Guid())
    {
        List<GameObject> instanciatedGameObjectsList;
        if (disabledGameObjects.TryGetValue(objectType, out instanciatedGameObjectsList))
        {
            int lastIndex = instanciatedGameObjectsList.Count - 1;

            if (instanciatedGameObjectsList.Count > 0)
            {

                GameObject go = instanciatedGameObjectsList[lastIndex];
                instanciatedGameObjectsList.RemoveAt(lastIndex);
                activeGameObjects.Add(go);
                IPoolableObject ipo = go.GetComponent<IPoolableObject>();

                if (ipo == null)
                {
                    ipo = go.GetComponentInChildren<IPoolableObject>();
                }

                if (locationData != null)
                {
                    go.transform.position = locationData.position;
                    go.transform.rotation = locationData.rotation;
                    go.transform.forward = locationData.forward;
                }

                ipo.OnPoolCreation();

                if (parentGuid != Guid.Empty)
                {
                    Guid objectId = Guid.Empty;
                    Guid getId;

                    Bot botScript = go.GetComponent<Bot>();
                    if (botScript != null)
                        objectId = botScript.Id;
                    else
                    {
                        SavableObject savableObjectScript = go.GetComponent<SavableObject>();
                        if (savableObjectScript != null)
                            objectId = savableObjectScript.Id;
                    }

                    if (objectId != Guid.Empty &&
                        !GameObjectStateManager.Instance.ParentIds.TryGetValue(objectId, out getId))
                    {
                        GameObjectStateManager.Instance.ParentIds.Add(objectId, parentGuid);
                    }
                }

                go.SetActive(true);

                return go;
            }
        }

        return null;
    }

    public void release(GameObject releasedObject)
    {
        for (int c = 0; c < prefabToPoolArray.Length; c++)
        {
            if (String.Equals(releasedObject.name, prefabToPoolArray[c].prefab.name+"(Clone)"))
            {
                release(releasedObject, prefabToPoolArray[c].key);
                break;
            }
        }
    }

    public void release(GameObject releasedObject, PoolableTypes objectType)
    {
        if (activeGameObjects.Contains(releasedObject))
        {
            List<GameObject> instanciatedGameObjectsList;

            if (disabledGameObjects.TryGetValue(objectType, out instanciatedGameObjectsList))
            {
                instanciatedGameObjectsList.Add(releasedObject);
            }

            IPoolableObject ipo = releasedObject.GetComponent<IPoolableObject>();
            
            if (ipo == null)
            {
                ipo = releasedObject.GetComponentInChildren<IPoolableObject>();
            }
            
            ipo.OnRelease();

            activeGameObjects.Remove(releasedObject);

            releasedObject.SetActive(false);
        }
    }

    public void recycleAllObjects()
    {
        List<GameObject> tmpList = new List<GameObject>(activeGameObjects);
        foreach (var aObj in tmpList)
        {
            release(aObj);
        }
    }

    public bool areObjectsRemaining(PoolableTypes poolableType)
    {
        List<GameObject> instanciatedGameObjectsList;
        bool objectsRemaining = false;
        if (disabledGameObjects.TryGetValue(poolableType, out instanciatedGameObjectsList))
        {
            if (instanciatedGameObjectsList.Count > 0)
            {
                objectsRemaining = true;
            }
        }

        return objectsRemaining;
    }

    public static Pool Instance => instance;
}
