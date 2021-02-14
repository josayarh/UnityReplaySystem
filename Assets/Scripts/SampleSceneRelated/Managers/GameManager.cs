using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEditorInternal.Profiling;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int noObjectSpawn;
    [SerializeField] private int triesBefoRereplayReset;
    [SerializeField] private int additionalSpheresAfterRetry;

    private static GameManager instance;
    private int firstRunFrameIndex;
    private int tryCount = 0;

    private bool isKeyDown = false;
    
    // Start is called before the first frame update
    void Start()
    {
        
        if (instance == null)
        {
            instance = this;
            
            setup();
            
            // write FPS to "profilerLog.txt"
            Profiler.logFile = Application.dataPath + "/profilerLog.txt";        
            // write Profiler Data to "profilerLog.txt.data"                                                                                        
            Profiler.enableBinaryLog = true;                                                 
            Profiler.enabled = true;
        }
        else
        {
            instance.setup();
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void setup()
    {
        for (int c = 0; c < noObjectSpawn && Pool.Instance.areObjectsRemaining(PoolableTypes.Sphere); c++)
        {
            Pool.Instance.get(PoolableTypes.Sphere, transform);
        }

        firstRunFrameIndex = ProfilerDriver.firstFrameIndex;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && isKeyDown==false)
        {
            isKeyDown = true;
            restart();
        }
        else if(Input.GetKeyUp(KeyCode.R))
        {
            isKeyDown = false;
        }

    }

    public void restart()
    {
        ++tryCount;
        Pool.Instance.recycleAllObjects();

        if (tryCount >= triesBefoRereplayReset)
        {
            GameObjectStateManager.Instance.resetDico();
            tryCount = 0;
            noObjectSpawn += additionalSpheresAfterRetry;
        }
        
        GameObjectStateManager.Instance.FrameNumber = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }
}
