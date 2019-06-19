using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Moq;
using PluginSage.Helper;
using PluginSage.Interfaces;
using PluginSage.Publisher;
using Xunit;
using Record = PluginSage.Publisher.Record;

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
                            {"col4", "true"},
                            {"", ""}
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
                                {"col4", "true"},
                                {"", ""}
                            },
                        });
                        mockBusObject.Setup(b => b.GetKeys()).Returns(new string[]
                        {
                            "col"
                        });

                        var records = GetTestRecords();
                        var schema = GetTestSchema();
                        
                        mockBusObject.Setup(b => b.UpdateSingleRecord(records[0])).Returns("");
                        mockBusObject.Setup(b => b.UpdateSingleRecord(records[1])).Returns("error");

                        mockBusObject.Setup(b => b.IsSourceNewer(records[0], schema)).Returns(false);
                        mockBusObject.Setup(b => b.IsSourceNewer(records[0], schema)).Returns(false);
                        
                        return mockBusObject.Object;
                    });
                    
                
                return mockService.Object;
            };
        }

        private List<Record> GetTestRecords()
        {
            return new List<Record>
            {
                new Record
                {
                    CorrelationId = "test",
                    DataJson = "{}"
                },
                new Record
                {
                    CorrelationId = "more-test",
                    DataJson = "{}"
                }
            };
        }

        private Schema GetTestSchema()
        {
            return new Schema
            {
                Name = "test",
                PublisherMetaJson = "{\"Module\":\"test module\"}",
                Properties =
                {
                    new Property
                    {
                        Id = "DATEUPDATED$",
                        IsUpdateCounter = true
                    }
                }
            };
        }

        [Fact]
        public async Task ConnectSessionTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
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
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
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
        public async Task DiscoverSchemasAllTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.All,
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Single(response.Schemas);
            Assert.Equal(4, response.Schemas[0].Properties.Count);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task DiscoverSchemasRefreshTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new DiscoverSchemasRequest
            {
                Mode = DiscoverSchemasRequest.Types.Mode.Refresh,
                ToRefresh = {new Schema
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                }}
            };

            // act
            client.Connect(connectRequest);
            var response = client.DiscoverSchemas(request);

            // assert
            Assert.IsType<DiscoverSchemasResponse>(response);
            Assert.Single(response.Schemas);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }

        [Fact]
        public async Task ReadStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new ReadRequest()
            {
                Schema = new Schema
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                }
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
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
        public async Task ReadStreamLimitTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new ReadRequest()
            {
                Schema = new Schema
                {
                    Id = "3",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                },
                Limit = 1
            };

            // act
            client.Connect(connectRequest);
            var response = client.ReadStream(request);
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
        public async Task PrepareWriteTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var request = new PrepareWriteRequest
            {
                Schema = new Schema
                {
                    Name = "test",
                    PublisherMetaJson = "{\"Module\":\"test module\"}"
                },
                CommitSlaSeconds = 1
            };

            // act
            client.Connect(connectRequest);
            var response = client.PrepareWrite(request);

            // assert
            Assert.IsType<PrepareWriteResponse>(response);

            // cleanup
            await channel.ShutdownAsync();
            await server.ShutdownAsync();
        }
        
        [Fact]
        public async Task WriteStreamTest()
        {
            // setup
            Server server = new Server
            {
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
                Ports = {new ServerPort("localhost", 0, ServerCredentials.Insecure)}
            };
            server.Start();

            var port = server.Ports.First().BoundPort;

            var channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure);
            var client = new Publisher.PublisherClient(channel);

            var connectRequest = GetConnectSettings();

            var prepareRequest = new PrepareWriteRequest()
            {
                Schema = GetTestSchema(),
                CommitSlaSeconds = 1
            };

            var records = GetTestRecords();
            
            var recordAcks = new List<RecordAck>();

            // act
            client.Connect(connectRequest);
            client.PrepareWrite(prepareRequest);
            
            using (var call = client.WriteStream())
            {
                var responseReaderTask = Task.Run(async () =>
                {
                    while (await call.ResponseStream.MoveNext())
                    {
                        var ack = call.ResponseStream.Current;
                        recordAcks.Add(ack);
                    }
                });

                foreach (Record record in records)
                {
                    await call.RequestStream.WriteAsync(record);
                }
                await call.RequestStream.CompleteAsync();
                await responseReaderTask;
            }

            // assert
            Assert.Equal(2, recordAcks.Count);
            Assert.Equal("",recordAcks[0].Error);
            Assert.Equal("test",recordAcks[0].CorrelationId);
            Assert.Equal("error",recordAcks[1].Error);
            Assert.Equal("more-test",recordAcks[1].CorrelationId);

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
                Services = {Publisher.BindService(new PluginSage.Plugin.Plugin(GetMockSessionFactory()))},
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