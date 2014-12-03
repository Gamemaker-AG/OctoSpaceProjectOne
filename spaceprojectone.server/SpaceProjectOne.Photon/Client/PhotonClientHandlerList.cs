﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExitGames.Logging;
using SpaceProjectOne.Framework;

namespace SpaceProjectOne.Photon.Client
{
    public class PhotonClientHandlerList
    {
        protected readonly ILogger Log = LogManager.GetCurrentClassLogger();
        private readonly Dictionary<int, PhotonClientHandler> _requestHandlerList;

        public PhotonClientHandlerList(IEnumerable<PhotonClientHandler> handlers)
        {
            _requestHandlerList = new Dictionary<int, PhotonClientHandler>();

            foreach (var handler in handlers)
            {
                if (!RegisterHandler(handler))
                {
                    Log.WarnFormat("Attempted to register handler {0} for type {1}:{2}", handler.GetType().Name, handler.Type, handler.Code);
                }
            }

        }

        public bool RegisterHandler(PhotonClientHandler handler)
        {

            var registered = false;

            if ((handler.Type & MessageType.Request) == MessageType.Request)
            {
                if (handler.SubCode.HasValue && _requestHandlerList.ContainsKey(handler.SubCode.Value))
                {
                    _requestHandlerList.Add(handler.SubCode.Value, handler);
                    registered = true;
                }
                else if (!_requestHandlerList.ContainsKey(handler.Code))
                {
                    _requestHandlerList.Add(handler.Code, handler);
                    registered = true;
                }
                else
                {
                    Log.ErrorFormat("RequestHandlerList already contains handler for {0} - cannot add {1}", handler.Code, handler.GetType().Name);
                }
            }

            return registered;

        }

        public bool HandleMessage(IMessage message, PhotonClientPeer peer)
        {

            bool handled = false;
            switch (message.Type)
            {
                case MessageType.Request:
                    if (message.SubCode.HasValue && _requestHandlerList.ContainsKey(message.SubCode.Value))
                    {
                        _requestHandlerList[message.SubCode.Value].HandleMessage(message, peer);
                        handled = true;
                    }
                    else if (!message.SubCode.HasValue && _requestHandlerList.ContainsKey(message.Code))
                    {
                        _requestHandlerList[message.Code].HandleMessage(message, peer);
                        handled = true;
                    }
                    break;
            }

            return handled;
        }

    }
}