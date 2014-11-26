using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceProjectOne.Framework
{
    public interface IConnectionCollection<Server, Client>
    {
        void OnConnect(Server serverPeer);
        void OnDissconnect(Server serverPeer);
        void OnClientConnect(Client clientPeer);
        void OnClientDisconnect(Client clientPeer);
        Server GetServerByType(int serverType);


    }
}
