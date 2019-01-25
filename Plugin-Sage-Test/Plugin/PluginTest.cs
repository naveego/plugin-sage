using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using Plugin_Sage.Helper;
using Plugin_Sage.Interfaces;
using Pub;
using Xunit;
using Record = Pub.Record;

namespace Plugin_Sage_Test.Plugin
{
    public class PluginTest
    {
        private ConnectRequest GetConnectSettings()
        {
            return new ConnectRequest
            {
                SettingsJson =
                    "{\"Username\":\"test\",\"Password\":\"password\",\"CompanyCode\":\"TST\",\"HomePath\":\"path\",\"ModulesList\":[\"test module\"]}",
                OauthConfiguration = new OAuthConfiguration(),
                OauthStateJson = ""
            };
        }

        private Func<Settings, ISessionService> GetMockSessionFactory()
        {
            return s =>
            {
                var mockService = new Mock<ISessionService>();
                mockService.Setup(m => m.MakeBusinessObject("test module"))
                    .Returns(() =>
                    {
                        var mockBusObject = new Mock<IBusinessObject>();
                        mockBusObject.Setup(b => b.GetSingleRecord()).Returns(new Dictionary<string, dynamic>
                        {
                            {"col", "test value"},
                            {"col2", "1"},
                            {"col3", "1.45"},
                            {"col4", "true"}
                        });
                        mockBusObject.Setup(b => b.GetAllRecords()).Returns(new List<Dictionary<string, dynamic>>
                        {
                            new Dictionary<string, dynamic>
                            {
                                {"col", "test value"},
                                {"col2", "1"},
                                {"col3", "1.45"},
                                {"col4", "true"}
                            },
                            new Dictionary<string, dynamic>
                            {
                                {"col", "test value"},
                                {"col2", "1"},
                                {"col3", "1.45"},
                                {"col4", "true"}
                            },
                        });

                        return mockBusObject.Object;
                    });

                return mockService.Object;
            };
        }

        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();
            var disconnectRequest = new DisconnectRequest();

            // act
            var response = client.ConnectSession(request);
            var responseStream = response.ResponseStream;
            var records = new List<ConnectResponse>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
                client.Disconnect(disconnectRequest);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ConnectTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = GetConnectSettings();

            // act
            var response = client.Connect(request);

            // assert
            Assert.IsType<ConnectResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverShapesAllTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverShapesRequest
            {
                Mode = DiscoverShapesRequest.Types.Mode.All,
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverShapes(request);

            // assert
            Assert.IsType<DiscoverShapesResponse>(response);
            Assert.Single(response.Shapes);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverShapesRefreshTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverShapesRequest
            {
                Mode = DiscoverShapesRequest.Types.Mode.Refresh,
                ToRefresh = {new Shape
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                }}
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverShapes(request);

            // assert
            Assert.IsType<DiscoverShapesResponse>(response);
            Assert.Single(response.Shapes);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task PublishStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new PublishRequest()
            {
                Shape = new Shape
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.PublishStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Equal(2, records.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task PublishStreamLimitTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new PublishRequest()
            {
                Shape = new Shape
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                },
                Limit = 1
            };

            // act
            client.Connect(connectRequest);
            var response = client.PublishStream(request);
            var responseStream = response.ResponseStream;
            var records = new List<Record>();

            while (await responseStream.MoveNext())
            {
                records.Add(responseStream.Current);
            }

            // assert
            Assert.Single(records);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DisconnectTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new Plugin_Sage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var request = new DisconnectRequest();

            // act
            var response = client.Disconnect(request);

            // assert
            Assert.IsType<DisconnectResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
    }
}