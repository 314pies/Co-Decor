using System;
using System.Collections.Generic;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using Sirenix.OdinInspector;

namespace CoDeco
{
    public class DiscoveryRequest : NetworkMessage
    {
        // Add properties for whatever information you want sent by clients
        // in their broadcast messages that servers will consume.
    }

    public class DiscoveryResponse : NetworkMessage
    {
        // Add properties for whatever information you want the server to return to
        // clients for them to display or consume for establishing a connection.

        // this is a property so that it is not serialized,  but the
        // client fills this up after we receive it
        public IPEndPoint EndPoint { get; set; }
        // this is a property so that it is not serialized,  but the
        // client fills this up after we receive it
        public float recievedTime { get; set; }

        public string RoomName;

        public string ModeName;


        public Uri uri;

        public long serverId;
    }

    public class VirtualSpaceDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse>
    {
        public VirtualSpaceNetworkManager networkManager;

        public static VirtualSpaceDiscovery Singleton { get; private set; }

        public long ServerId { get; private set; }

        [Sirenix.OdinInspector.ShowInInspector]
        public Dictionary<long, DiscoveryResponse> FoundedServers = new Dictionary<long, DiscoveryResponse>();

        [Tooltip("Transport to be advertised during discovery")]
        public Transport transport;

        public override void Start()
        {
            ServerId = RandomLong();
            if (transport == null)
                transport = Transport.activeTransport;

            base.Start();
            Singleton = this;
        }

        #region Server

        /// <summary>
        /// Reply to the client to inform it of this server
        /// </summary>
        /// <remarks>
        /// Override if you wish to ignore server requests based on
        /// custom criteria such as language, full server game mode or difficulty
        /// </remarks>
        /// <param name="request">Request comming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        protected override void ProcessClientRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {
            base.ProcessClientRequest(request, endpoint);
        }

        /// <summary>
        /// Process the request from a client
        /// </summary>
        /// <remarks>
        /// Override if you wish to provide more information to the clients
        /// such as the name of the host player
        /// </remarks>
        /// <param name="request">Request comming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        /// <returns>A message containing information about this server</returns>
        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint)
        {

            try
            {
                // this is an example reply message,  return your own
                // to include whatever is relevant for your game
                return new DiscoveryResponse
                {
                    RoomName = networkManager.RoomName,
                    ModeName = networkManager.GameModeName,
                    serverId = ServerId,
                    uri = transport.ServerUri()
                };
            }
            catch (NotImplementedException)
            {
                Debug.LogError($"Transport {transport} does not support network discovery");
                throw;
            }
        }

        #endregion

        #region Client

        /// <summary>
        /// Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        /// Override if you wish to include additional data in the discovery message
        /// such as desired game mode, language, difficulty, etc... </remarks>
        /// <returns>An instance of ServerRequest with data to be broadcasted</returns>
        protected override DiscoveryRequest GetRequest()
        {
            return new DiscoveryRequest();
        }

        

        /// <summary>
        /// Process the answer from a server
        /// </summary>
        /// <remarks>
        /// A client receives a reply from a server, this method processes the
        /// reply and raises an event
        /// </remarks>
        /// <param name="response">Response that came from the server</param>
        /// <param name="endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint)
        {

            Debug.Log("LWNetworkDiscovery.ProcessResponse(), RoomName: " + response.RoomName);

            // we received a message from the remote endpoint
            response.EndPoint = endpoint;
            response.recievedTime = Time.time;

            // although we got a supposedly valid url, we may not be able to resolve
            // the provided host
            // However we know the real ip address of the server because we just
            // received a packet from it,  so use that as host.
            UriBuilder realUri = new UriBuilder(response.uri)
            {
                Host = response.EndPoint.Address.ToString()
            };
            response.uri = realUri.Uri;

            if (!FoundedServers.ContainsKey(response.serverId))
            {
                FoundedServers.Add(response.serverId, response);
            }
            else
            {
                FoundedServers[response.serverId] = response;
            }
        }

        /// <summary>
        /// Will be clean up by <see cref="CleanUpTimeoutServer"/> from <see cref="FoundedServers"/> if the time to last recieve is past this value
        /// </summary>
        public float DiscoveredServerInfoTimeout = 20.0f;
        public void CleanUpTimeoutServer()
        {
            List<long> removals = new List<long>();
            foreach (var serverID in FoundedServers.Keys)
            {
                if (Time.time - FoundedServers[serverID].recievedTime > DiscoveredServerInfoTimeout)
                {
                    removals.Add(serverID);
                }
            }
            foreach (var removalId in removals)
            {
                FoundedServers.Remove(removalId);
            }
        }

        #endregion
    }
}