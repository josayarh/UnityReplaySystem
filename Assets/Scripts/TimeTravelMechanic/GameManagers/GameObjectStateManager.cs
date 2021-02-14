using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

enum DynamicTags
{
    Player,
    Dynamic,
}

public class GameObjectStateManager : MonoBehaviour
{
    private static GameObjectStateManager instance;
    private List<GameObject> dynamicGameObjectlist;
    private uint frameNumber = 0;
    
    public Dictionary<Guid, Tuple<Type ,List<string>>> frameDataDictionary 
        = new Dictionary<Guid, Tuple<Type, List<string>>>();
    private Dictionary<uint, List<Guid>> objectApperanceDictionnary = new Dictionary<uint, List<Guid>>();
    private Dictionary<Guid, GameObject> instanciatedGameobjects = new Dictionary<Guid, GameObject>();
    
    /***
     * Stores the objects ID whose existance depends on a parent object
     * Key : Object id
     * Value : Parent id 
     */
    private Dictionary<Guid, Guid> parentIds = new Dictionary<Guid, Guid>();
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        string docPath = Application.dataPath + "/ReplaySave.Json";
        string json = serializeAllDictionnaries();
        
        using (StreamWriter outputFile = new StreamWriter(docPath))
        {
            outputFile.Write(json);
        }
    }

    private void FixedUpdate()
    {
        reloadObjects();
        frameNumber++;
    }

    public void initializeDynamicObjects()
    {
        dynamicGameObjectlist = new List<GameObject>();
        GameObject[] tab = GameObject.FindObjectsOfType<GameObject>();

        if (tab != null)
        {
            foreach (var gameObject in tab)
            {
                bool isDynamic = false;
                foreach (var name in Enum.GetNames(typeof(DynamicTags)))
                {
                    if (gameObject.CompareTag(name))
                        isDynamic = true;
                }
                if(isDynamic)
                    dynamicGameObjectlist.Add(gameObject);
            }
        }
    }

    private void reloadObjects()
    {
        List<Guid> objectList;
        if (objectApperanceDictionnary.TryGetValue(frameNumber, out objectList))
        {
            foreach(Guid id in objectList)
            {
                Tuple<Type, List<string>> gameObjectTuple;
                if (frameDataDictionary.TryGetValue(id, out gameObjectTuple))
                {
                    GameObject go = null;
                    
                    Guid tryGetParentGuid = Guid.Empty;

                    if (parentIds.TryGetValue(id, out tryGetParentGuid))
                    {
                        if (!doesParentExist(id))
                            break;
                    }
                    else
                    {
                        if (gameObjectTuple.Item1 == typeof(SphereSave))
                        {
                            go = Pool.Instance.get(PoolableTypes.SphereBot);
                            if (go != null)
                            {
                                SphereBot bulletBotController =
                                    go.GetComponent<SphereBot>();
                                if (gameObjectTuple.Item2 == null)
                                {
                                    Debug.Log("No frame list associated with this object");
                                }
                                
                                bulletBotController.FrameSteps = gameObjectTuple.Item2;
                            }
                            else
                            {
                                Debug.Log("No sphere bot left in pool");
                            }
                        }
                    }

                    if (go != null)
                    {
                        addInstanciatedObject(id, go);
                    }
                }
            }
        }
    }

    public static GameObjectStateManager Instance
    {
        get => instance;
    }
    
    public void addInstanciatedObject(Guid objectId, GameObject instanciatedObject)
    {
        GameObject tryGetGameObject = null;
        if (instanciatedGameobjects.TryGetValue(objectId,out tryGetGameObject))
        {
            tryGetGameObject = instanciatedObject;
        }
        else
        {
            instanciatedGameobjects.Add(objectId,instanciatedObject);
        }
    }

    public void addDynamicObject(Guid guid, Type type, List<String> frameSave)
    {
        addDynamicObject(guid, type, frameSave, frameNumber);
    }
    
    public void addDynamicObject(Guid guid, Type type, List<String> frameSave, uint saveFrameNumber)
    {
        Tuple<Type, List<string>> tmp;
        if (frameDataDictionary.TryGetValue(guid, out tmp))
        {
            if(frameSave.Count > frameNumber + 1)
                frameSave.RemoveRange((int)frameNumber-1,(int)(frameSave.Count - frameNumber));
            tmp = new Tuple<Type, List<string>>(tmp.Item1, new List<string>(frameSave));
        }
        else
        {
            Tuple<Type, List<string>> couple = new Tuple<Type, List<string>>
                (type, new List<string>(frameSave));
            List<Guid> list = null;

            if (!objectApperanceDictionnary.TryGetValue(saveFrameNumber, out list))
            {
                list = new List<Guid>();
                objectApperanceDictionnary.Add(saveFrameNumber, list);
            }

            list.Add(guid);
            frameDataDictionary.Add(guid, couple);
        }
    }

    public bool doesParentExist(Guid childGUID)
    {
        bool existance = false;
        Guid parentGUID;

        if (parentIds.TryGetValue(childGUID, out parentGUID))
        {
            GameObject parentObject;
            if (instanciatedGameobjects.TryGetValue(parentGUID, out parentObject))
            {
                if (parentObject != null && parentObject.activeSelf)
                    existance = true;
            }   
        }

        return existance;
    }

    public GameObject getParent(Guid childGUID)
    {
        GameObject parentObject = null;
        Guid parentGUID;

        if (parentIds.TryGetValue(childGUID, out parentGUID))
        {
            GameObject tempObject = null;
            if (instanciatedGameobjects.TryGetValue(parentGUID, out tempObject))
            {
                if (tempObject != null && tempObject.activeSelf)
                    parentObject = tempObject;
            }   
        }

        return parentObject;
    }

    public string serializeAllDictionnaries()
    {
        string json = "";

        json += serializeFrameData();
        json += serializeObejectAppearance();

        return json;
    }

    private string serializeFrameData()
    {
        Dictionary<string, Tuple<string ,List<string>>> tempFDD 
            = new Dictionary<string, Tuple<string, List<string>>>();
        
        foreach (var pair in frameDataDictionary)
        {
            Tuple<string, List<string>> tuple = new Tuple<string, List<string>>(pair.Value.Item1.ToString(),new List<string>(pair.Value.Item2)),
                tuple2;
            
            tempFDD.Add(pair.Key.ToString(),tuple);

        }

        return JsonConvert.SerializeObject(tempFDD, Formatting.Indented);
    }

    private string serializeObejectAppearance()
    {
        //private Dictionary<uint, List<Guid>> objectApperanceDictionnary = new Dictionary<uint, List<Guid>>();
        Dictionary<uint, List<string>> tempOAD = new Dictionary<uint, List<string>>();
        List<string> listGuid = new List<string>();

        foreach (var pair in objectApperanceDictionnary)
        {
            foreach (var guid in pair.Value)
            {
                listGuid.Add(guid.ToString());
            }
            tempOAD.Add(pair.Key,new List<string>(listGuid));
            listGuid.Clear();
        }

        return JsonConvert.SerializeObject(tempOAD, Formatting.Indented);
    }

    public void resetDico()
    {
        instanciatedGameobjects.Clear();
        parentIds.Clear();
        objectApperanceDictionnary.Clear();
        frameDataDictionary.Clear();
    }

    public uint FrameNumber
    {
        get => frameNumber;
        set => frameNumber = value;
    }

    public Dictionary<Guid, GameObject> InstanciatedGameobjects
    {
        get => instanciatedGameobjects;
        set => instanciatedGameobjects = value;
    }

    public Dictionary<Guid, Guid> ParentIds
    {
        get => parentIds;
        set => parentIds = value;
    }

    public Dictionary<Guid, Tuple<Type, List<string>>> FrameDataDictionary
    {
        get => frameDataDictionary;
        set => frameDataDictionary = value;
    }
}