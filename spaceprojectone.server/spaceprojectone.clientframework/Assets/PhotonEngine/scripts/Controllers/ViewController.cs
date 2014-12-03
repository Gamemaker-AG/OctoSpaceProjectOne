using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ExitGames.Client.Photon;


public class ViewController : IViewController
{
    private readonly View _controlledView;
    private readonly byte _subOperationCode;
    public View ControlledView
    {
        get
        {
            return _controlledView;
        }

    }
    private readonly Dictionary<byte, IPhotonOperationHandler> _operationHandlers = new Dictionary<byte, IPhotonOperationHandler>();
    private readonly Dictionary<byte, IPhotonEventHandler> _eventHandlers = new Dictionary<byte, IPhotonEventHandler>();

    public ViewController(View controlledView, byte subOperationCode = 0)
    {
        _controlledView = controlledView;
        _subOperationCode = subOperationCode;
        if (PhotonEngine.Instance == null)
        {
            Application.LoadLevel(0);
        }
        else
        {
            PhotonEngine.Instance.Controller = this;
        }
    }


    public Dictionary<byte, IPhotonOperationHandler> Operationhandlers
    {
        get
        {
            return _operationHandlers;
        }
    }

    public Dictionary<byte, IPhotonEventHandler> EventHandlers
    {
        get
        {
            return _eventHandlers;
        }
    }

    public void ApplicationQuit()
    {
        PhotonEngine.Instance.Disconnect();
    }


    #region Implementation of IViewController
    public bool IsConnected
    {
        get
        {
            return PhotonEngine.Instance.State is Connected;
        }
    }

    public void Connect()
    {
        if (!IsConnected)
        {
            PhotonEngine.Instance.Initialize();
        }
    }

    public void SendOperation(OperationRequest request, bool sendReliable, byte channelId, bool encrypt)
    {
        PhotonEngine.Instance.SendOp(request, sendReliable, channelId, encrypt);
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        _controlledView.LogDebug(string.Format("{0} - {1}", level, message));
    }

    public void OnOperationResponse(OperationResponse operationResponse)
    {
        IPhotonOperationHandler handler;
        if (operationResponse.Parameters.ContainsKey(_subOperationCode) &&
            Operationhandlers.TryGetValue(Convert.ToByte(operationResponse.Parameters[_subOperationCode]), out handler))
        {
            handler.HandleResponse(operationResponse);
        }
        else
        {
            OnUnexpectedOperationResponse(operationResponse);
        }
    }

    public void OnEvent(EventData eventData)
    {
        IPhotonEventHandler handler;
        if (EventHandlers.TryGetValue(eventData.Code, out handler))
        {
            handler.HandleEvent(eventData);
        }
        else
        {
            OnUnexpectedEvent(eventData);
        }
    }

    public void OnUnexpectedEvent(EventData eventdata)
    {
        _controlledView.LogError(string.Format("Unexpected Event {0}", eventdata.Code));
    }

    public void OnUnexpectedOperationResponse(OperationResponse operationResponse)
    {
        _controlledView.LogError(string.Format("Unexpected Operation Event {0} from Operation {1}", operationResponse.ReturnCode, operationResponse.OperationCode));
    }

    public void OnUnexpectedStatusCode(StatusCode statusCode)
    {
        _controlledView.LogError(string.Format("Unexpected Status {0}", statusCode));
    }

    public void OnDisconnected(string message)
    {
        _controlledView.Disconnected(message);
    }

    #endregion
}

