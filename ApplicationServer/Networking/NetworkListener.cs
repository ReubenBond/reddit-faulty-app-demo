﻿using System.Net.Sockets;
using ApplicationServer.Networking.Client;
using Microsoft.Extensions.Logging;

namespace ApplicationServer.Networking;

public class NetworkListener
{
    private readonly ILogger<NetworkListener> _logger;
    private readonly TcpListener _listener;
    private readonly NetworkClientRepository _clientRepository;
    private readonly NetworkClientFactory _clientFactory;

    public NetworkListener(
        ILogger<NetworkListener> logger, 
        TcpListener listener, 
        NetworkClientRepository clientRepository, 
        NetworkClientFactory clientFactory)
    {
        _logger = logger;
        _listener = listener;
        _clientRepository = clientRepository;
        _clientFactory = clientFactory;
    }

    public void Start(int backlog = 100)
    {
        _listener.Start(backlog);
    }

    public async Task ListenAsync()
    {
        _logger.LogInformation($"We are listening for connections");
            
        while (true)
        {
            var client = await _listener.AcceptTcpClientAsync();
            await AcceptClient(client);
        }
    }
        
    private async Task AcceptClient(TcpClient client)
    {
        var clientId = Guid.NewGuid();
        var networkClient = _clientFactory.CreateClient(clientId, client);
        
        _clientRepository.AddClient(clientId, networkClient);

        _logger.LogWarning("Someone is trying to connect...");
        
        await networkClient.ListenAsync();
    }
}