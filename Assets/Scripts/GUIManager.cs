using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GUIManager : Singleton<GUIManager>, IEventListener<GameEvent>
{
    /// The pause screen game object.
    public GameObject PauseScreen;
    /// The game-over screen game object.
    public GameObject GameOverScreen;
    /// The fuel indicator object.
    public GameObject FuelContainer;
    public GameObject FuelContainer2;
    /// The Minimap container object. TODO
    //public GameObject MinimapContainer;
    /// This holds a joystick.
    public CanvasGroup Joystick;
    /// This holds the accelarator pedal.
    public CanvasGroup AccPedal;
    /// This hold the brake pedal.
    public CanvasGroup BrakePedal;
    /// The points counter.
    public Text PointsCounter;
    public string PointsCounterPattern = "0000000000000";
    /// The Catnips Counter
    public Text CatnipsCounter;
    public string CatnipsCounterPattern = "0000";
    /// The level display.
    public Text LevelDisplay;
    /// The countdown at the start of a level.
    public Text CountdownTxt;
    /// The screen used for all fades.
    public Image Fader;

    // Let's initialize
    public virtual void Initialize()
    {
        RefreshPoints();
        RefreshCatnips();
        InitializeFuel();

//        if (CountdownTxt != null | CountdownTxt.text == null)
  //      {
    //        CountdownTxt.enabled = false;
      //  }
    }

    /// Initializes fuel display.
    public virtual void InitializeFuel()
    {
        //TODO
    }

    /// A placeholder for overriding.
    public virtual void OnGameStart()
    {
        _instance.RefreshPoints();
        _instance.RefreshCatnips();
    }

    /// For setting pauseScreen.
    public virtual void SetPause (bool state) 
    {
        if (!PauseScreen) {return;}
        
        PauseScreen.SetActive(state);
    }

    /// For setting the countdown display.
    public virtual void SetCountdownActive (bool state)
    {
        if (!CountdownTxt) {return;}

        CountdownTxt.enabled = state;
    }

    public virtual void SetCountdownTxt (string newTxt)
    {
        if (!CountdownTxt) {return;}

        CountdownTxt.text = newTxt;
    }

    /// For setting the gameover display.
    public virtual void SetGameOverScreen (bool state)
    {
        GameOverScreen.SetActive(state);

        Text gameOverScreenTxt = GameOverScreen.transform.Find("gameOverScreenTxt").GetComponent<Text>();

        if (gameOverScreenTxt != null) 
        {
            gameOverScreenTxt.text = "Your cat has left the rocket. Sadge" ; //TODO make a better game over screen
        }
    }

    /// Works with gameManager to set points.
    public virtual void RefreshPoints ()
    {
        if (!PointsCounter) return;

        PointsCounter.text = GameManager.Instance.Points.ToString(PointsCounterPattern);
    }

    public virtual void RefreshCatnips ()
    {
        if (!CatnipsCounter) return;

        CatnipsCounter.text = GameManager.Instance.CatNips.ToString(CatnipsCounterPattern);
    }

    /// For setting the level name/tier.
    public virtual void SetLevel(string name)
    {
        if (!LevelDisplay) return;

        LevelDisplay.text = name;
    }

    /// Fades in/out depending on the state.
    public virtual void FaderOn (bool state, float duration)
    {
        if (!Fader) {return;}

        Fader.gameObject.SetActive(true);

        if (state)
           StartCoroutine(FadeEffect.FadeImage(Fader, duration, new Color(0, 0, 0, 1f)));
        else
           StartCoroutine(FadeEffect.FadeImage(Fader, duration, new Color(0, 0, 0, 0f)));
    }

    public virtual void FaderTo (Color newColor, float duration)
    {
        if (!Fader) {return;}

        Fader.gameObject.SetActive(true);

        StartCoroutine (FadeEffect.FadeImage(Fader, duration, newColor));
    }


    public virtual void OnEvent(GameEvent gameEvent)
    {
        switch (gameEvent.EventName)
        {
            case "PauseOn":
                 SetPause(true);
                 break;
            case "PauseOff":
                 SetPause(false);
                 break;
            case "GameStart":
                 OnGameStart();
                 break;
        }
    }

    protected virtual void OnEnable()
    {
        this.StartListeningEvent<GameEvent>();
    }

    protected virtual void OnDisable()
    {
        this.StopListeningEvent<GameEvent>();
    }
}
