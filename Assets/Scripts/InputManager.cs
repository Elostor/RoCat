using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Singleton<InputManager>
{
    protected virtual void Update ()
    {
        HandleControls();
    }

    protected virtual void HandleControls ()
    {
    	if (Input.GetButtonDown("Pause")) { PauseButtonDown(); }
	    if (Input.GetButtonUp("Pause")) { PauseButtonUp(); }
		if (Input.GetButton("Pause")) { PauseButtonPressed(); }

		if (Input.GetButtonDown("MainAction")) { MainActionButtonDown(); }
		if (Input.GetButtonUp("MainAction")) { MainActionButtonUp(); }
		if (Input.GetButton("MainAction")) { MainActionButtonPressed(); }
		
		if (Input.GetButtonDown("Left")) { LeftButtonDown(); }
		if (Input.GetButtonUp("Left")) { LeftButtonUp(); }
		if (Input.GetButton("Left")) { LeftButtonPressed(); }

		if (Input.GetButtonDown("Right")) { RightButtonDown(); }
		if (Input.GetButtonUp("Right")) { RightButtonUp(); }
		if (Input.GetButton("Right")) { RightButtonPressed(); }
		
		if (Input.GetButtonDown("Up")) { UpButtonDown(); }
		if (Input.GetButtonUp("Up")) { UpButtonUp(); }
		if (Input.GetButton("Up")) { UpButtonPressed(); }
		
		if (Input.GetButtonDown("Down")) { DownButtonDown(); }
		if (Input.GetButtonUp("Down")) { DownButtonUp(); }
		if (Input.GetButton("Down")) { DownButtonPressed(); }
    }

    public virtual void PauseButtonDown () 
    {
        GameManager.Instance.PauseOn();
    }
    public virtual void PauseButtonUp () {}
    public virtual void PauseButtonPressed () {}

    public virtual void MainActionButtonDown ()
    {
    }
    public virtual void MainActionButtonPressed () {}
    public virtual void MainActionButtonUp () {}

    public virtual void LeftButtonDown () 
    {}
    public virtual void LeftButtonPressed () 
    {
        Player.PlayerIns.LeftPress();
    }
    public virtual void LeftButtonUp () 
    {
        Player.PlayerIns.LeftRelease();
    }

    public virtual void RightButtonDown () 
    {}
    public virtual void RightButtonPressed () 
    {
        Player.PlayerIns.RightPress();
    }
    public virtual void RightButtonUp () 
    {
        Player.PlayerIns.RightRelease();
    }

    public virtual void UpButtonDown () 
    {
        Player.PlayerIns.UpPress();
    }
    public virtual void UpButtonPressed () {}
    public virtual void UpButtonUp () 
    {
        Player.PlayerIns.UpRelease();
    }

    public virtual void DownButtonDown () 
    {
        Player.PlayerIns.DownPress();
    }
    public virtual void DownButtonPressed () {}
    public virtual void DownButtonUp () 
    {
        Player.PlayerIns.DownRelease();
    }
}
