using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class MainManager : PersistentSingleton<MainManager>
{
    public int TargetFrameRate;
    // Is this the first time opening the game after installing?
    protected bool theFirsTimeRunning = PlayerPrefs.GetInt("FirstTime", 1) == 1;
    // To manage the scenes, 
    private string currentSceneName;
    private string nextSceneName;
    // Basic states of the application.
    private enum BasicState {Reset, PreLoad, Load, UnLoad, PostLoad, Ready, Run, Count};
    private BasicState basicState;
    // Async operations to control various states.
    private AsyncOperation resourceUnload;
    private AsyncOperation sceneLoad;
    // Delegate to minimize the resource usage.
    private delegate void UpdateDelegate();
    private UpdateDelegate[] updateDelegates;

    protected override void Awake ()
    {
        base.Awake();
        // Add Maydanoz Scene
        // Add if(firsTime) into video scene
        // Set firstTime to false then save it.
        
        if (theFirsTimeRunning)
        {
            Handheld.PlayFullScreenMovie("FirstTime.mp4");
        }
        
        updateDelegates = new UpdateDelegate[(int)BasicState.Count];
        updateDelegates[(int)BasicState.Reset] = UpdateSceneReset;
        updateDelegates[(int)BasicState.PreLoad] = UpdateScenePreLoad;
        updateDelegates[(int)BasicState.Load] = UpdateSceneLoad;
        updateDelegates[(int)BasicState.UnLoad] = UpdateSceneUnLoad;
        updateDelegates[(int)BasicState.PostLoad] = UpdateScenePostLoad;
        updateDelegates[(int)BasicState.Ready] = UpdateSceneReady;
        updateDelegates[(int)BasicState.Run] = UpdateSceneRun;

        if (theFirsTimeRunning)
        {
            nextSceneName = "CharacterSelection";
            PlayerPrefs.SetInt("FirstTime", 0);
        }
        else
        {
            nextSceneName = "MainMenu";   
        }
        basicState = BasicState.Reset;
    }
    protected virtual void Start ()
    {
        Application.targetFrameRate = TargetFrameRate;
        DOTween.Init(true, false, LogBehaviour.Default).SetCapacity(200, 50);
    }

    protected virtual void Update () 
    {
        if (updateDelegates[(int)basicState] != null)
        {
            updateDelegates[(int)basicState]();
        }
    }

    protected virtual void OnDestroy ()
    {
        if (updateDelegates != null)
        {
            for (int i = 0; i < (int)BasicState.Count; i++)
            {
                updateDelegates[i] = null;
            }
            updateDelegates = null;
        }

        if (_instance != null)
        {
            _instance = null;
        }
    }

    protected virtual void OnDisable () 
    { 

    }

    protected virtual void OnApplicationQuit ()
    {
        //EventManager.TriggerEvent(new GameEvent("Save"));
    }

    protected virtual void UpdateSceneReset () 
    {
        System.GC.Collect();
        basicState = BasicState.PreLoad;
    }

    protected virtual void UpdateScenePreLoad ()
    {
        sceneLoad = SceneManager.LoadSceneAsync(nextSceneName);
        basicState = BasicState.Load;
        EventManager.TriggerEvent(BasicState.Load); // LoadingManager brings loading scene after this.
    }

    protected virtual void UpdateSceneLoad ()
    {
        if (sceneLoad.isDone)
        {
            basicState = BasicState.UnLoad;
        } 
        else 
        { 
            // TODO a possible scene-loading scene here, LoadingManager can be used since the basicState is "Load".
        }
    }

    protected virtual void UpdateSceneUnLoad ()
    {
        if (resourceUnload == null)
        {
            resourceUnload = Resources.UnloadUnusedAssets();
        }
        else
        {
            if (resourceUnload.isDone)
            {
                resourceUnload = null;
                basicState = BasicState.PostLoad;
            }
        }
    }

    protected virtual void UpdateScenePostLoad ()
    {
        currentSceneName = nextSceneName;
        basicState = BasicState.Ready;
    }

    protected virtual void UpdateSceneReady ()
    {
        System.GC.Collect();
        basicState = BasicState.Run;
    }

    protected virtual void UpdateSceneRun ()
    {
        if (currentSceneName != nextSceneName)
        {
            basicState = BasicState.Reset;
        }
    }

    public static void SwitchScene (string nextSceneName) 
    {
        if (_instance != null)
        {
            if (_instance.currentSceneName != nextSceneName)
            {
                _instance.nextSceneName = nextSceneName;
            }
        }
    }
}
