using UnityEngine;
using System.Collections;
using System;

public class Login : View {

    private LoginController _controller;
    public string serverAdress;
    public string applicationName;
    public bool loggingIn = false;

    public override void Awake()
    {
    }
	// Use this for initialization
	void Start () {

        Controller = new LoginController(this);
        PhotonEngine.UseExistingOrCreateNewPhotonEngine(serverAdress, applicationName);
/*
        string[] arglist = new string[0];
        if (Application.srcValue.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries).Length > 1)
        {
            arglist = Application.srcValue.Split(new[] { "?" }, StringSplitOptions.RemoveEmptyEntries)[1].Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        }

        if(arglist.Length == 2)
        {
            _controller.SendLogin(arglist[0], arglist[1]);
            loggingIn = true;
        }
 * */

	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public override IViewController Controller
    {
        get
        {
            return (IViewController)_controller;
        }
        protected set
        {
            _controller = value as LoginController;
        }
    }
}
