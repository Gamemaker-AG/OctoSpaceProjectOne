using ExitGames.Client.Photon;
class LoginController : ViewController
{
    public LoginController(View controlledView, byte subOperationCode = 0) : base(controlledView, subOperationCode)
    {

    }

    public void SendLogin(string username, string password)
    {
        SendOperation(new OperationRequest(), true, 0, false);
    }
}
