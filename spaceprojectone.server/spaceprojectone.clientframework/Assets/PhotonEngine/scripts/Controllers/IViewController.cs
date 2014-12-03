using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExitGames.Client.Photon;

public interface IViewController
{
    bool IsConnected { get; }


    void Connect();
    void SendOperation(OperationRequest request, bool sendReliable, byte channelId, bool encrypt);
    void ApplicationQuit();



    #region Implementation of IPhotonPeerListener

    void DebugReturn(DebugLevel level, string message);
    void OnOperationResponse(OperationResponse operationResponse);
    void OnEvent(EventData eventData);

    void OnUnexpectedEvent(EventData eventdata);
    void OnUnexpectedOperationResponse(OperationResponse operationResponse);
    void OnUnexpectedStatusCode(StatusCode statusCode);

    void OnDisconnected(string message);


    #endregion

}
