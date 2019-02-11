using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Plugin_Sage.API;
using Plugin_Sage.DataContracts;
using Plugin_Sage.Helper;
using Plugin_Sage.Interfaces;
using Pub;

namespace Plugin_Sage.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private readonly Func<Settings, ISessionService> _sessionFactory;
        private readonly ServerStatus _server;
        private TaskCompletionSource<bool> _tcs;
        private ISessionService _sessionService;

        public Plugin(Func<Settings, ISessionService> sessionFactory = null)
        {
            _sessionFactory = sessionFactory ?? (s => new SessionService(s));
            _server = new ServerStatus();
        }

        /// <summary>
        /// Establishes a connection with Zoho CRM. Creates an authenticated http client and tests it.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>A message indicating connection success</returns>
        public override Task<ConnectResponse> Connect(ConnectRequest request, ServerCallContext context)
        {
            _server.Connected = false;

            Logger.Info("Connecting...");

            var settings = JsonConvert.DeserializeObject<Settings>(request.SettingsJson);

            // validate settings passed in
            try
            {
                _server.Settings = settings;
                _server.Settings.Validate();
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return Task.FromResult(new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = "",
                    OauthError = "",
                    SettingsError = e.Message
                });
            }

            // attempt to create new session service object
            try
            {
                _sessionService = _sessionFactory(settings);
                _server.Connected = true;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return Task.FromResult(new ConnectResponse
                {
                    OauthStateJson = request.OauthStateJson,
                    ConnectionError = e.Message,
                    OauthError = "",
                    SettingsError = ""
                });
            }

            Logger.Info("Connected to Sage");

            return Task.FromResult(new ConnectResponse
            {
                OauthStateJson = "",
                ConnectionError = "",
                OauthError = "",
                SettingsError = ""
            });
        }

        /// <summary>
        /// Connects the session by forwarding requests to Connect
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ConnectSession(ConnectRequest request,
            IServerStreamWriter<ConnectResponse> responseStream, ServerCallContext context)
        {
            Logger.Info("Connecting session...");

            // create task to wait for disconnect to be called
            _tcs?.SetResult(true);
            _tcs = new TaskCompletionSource<bool>();

            // call connect method
            var response = await Connect(request, context);

            await responseStream.WriteAsync(response);

            Logger.Info("Session connected.");

            // wait for disconnect to be called
            await _tcs.Task;
        }


        /// <summary>
        /// Discovers shapes located in the users Zoho CRM instance
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns>Discovered shapes</returns>
        public override async Task<DiscoverSchemasResponse> DiscoverSchemas(DiscoverSchemasRequest request,
            ServerCallContext context)
        {
            Logger.Info("Discovering Shapes...");

            DiscoverSchemasResponse discoverShapesResponse = new DiscoverSchemasResponse();

            // only return requested shapes if refresh mode selected
            if (request.Mode == DiscoverSchemasRequest.Types.Mode.Refresh)
            {
                try
                {
                    var refreshShapes = request.ToRefresh;

                    Logger.Info($"Refresh shapes attempted: {refreshShapes.Count}");

                    var tasks = refreshShapes.Select((s) =>
                        {
                            var metaJsonObject = JsonConvert.DeserializeObject<PublisherMetaJson>(s.PublisherMetaJson);
                            return GetShapeForModule(metaJsonObject.Module, s.Id);
                        })
                        .ToArray();

                    await Task.WhenAll(tasks);

                    discoverShapesResponse.Schemas.AddRange(tasks.Where(x => x.Result != null).Select(x => x.Result));

                    // return all shapes 
                    Logger.Info($"Shapes returned: {discoverShapesResponse.Schemas.Count}");
                    return discoverShapesResponse;
                }
                catch (Exception e)
                {
                    Logger.Error(e.Message);
                    throw;
                }
            }

            // attempt to get a shape for each module requested
            try
            {
                Logger.Info($"Shapes attempted: {_server.Settings.ModulesList.Length}");

                var tasks = _server.Settings.ModulesList.Select((m, i) => GetShapeForModule(m, i.ToString()))
                    .ToArray();

                await Task.WhenAll(tasks);

                discoverShapesResponse.Schemas.AddRange(tasks.Where(x => x.Result != null).Select(x => x.Result));

                // return all shapes 
                Logger.Info($"Shapes returned: {discoverShapesResponse.Schemas.Count}");
                return discoverShapesResponse;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Publishes a stream of data for a given shape
        /// </summary>
        /// <param name="request"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task ReadStream(ReadRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            var shape = request.Schema;
            var limit = request.Limit;
            var limitFlag = request.Limit != 0;

            Logger.Info($"Publishing records for shape: {shape.Name}");

            try
            {
                var recordsCount = 0;
                var metaJsonObject = JsonConvert.DeserializeObject<PublisherMetaJson>(shape.PublisherMetaJson);

                // get business object service for given module
                var busObjectService = _sessionService.MakeBusinessObject(metaJsonObject.Module);

                // get a single record
                var records = busObjectService.GetAllRecords();

                foreach (var record in records)
                {
                    var recordOutput = new Record
                    {
                        Action = Record.Types.Action.Upsert,
                        DataJson = JsonConvert.SerializeObject(record)
                    };


                    // stop publishing if the limit flag is enabled and the limit has been reached or the server is disconnected
                    if ((limitFlag && recordsCount == limit) || !_server.Connected)
                    {
                        break;
                    }

                    // publish record
                    await responseStream.WriteAsync(recordOutput);
                    recordsCount++;
                }

                Logger.Info($"Published {recordsCount} records");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Prepares the plugin to handle a write request
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<PrepareWriteResponse> PrepareWrite(PrepareWriteRequest request, ServerCallContext context)
        {
            _server.WriteConfigured = false;

            var writeSettings = new WriteSettings
            {
                CommitSLA = request.CommitSlaSeconds,
                Schema = request.Schema
            };

            _server.WriteSettings = writeSettings;
            _server.WriteConfigured = true;

            return Task.FromResult(new PrepareWriteResponse());
        }
        
        /// <summary>
        /// Takes in records and writes them out to the Sage instance then sends acks back to the client
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="responseStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task WriteStream(IAsyncStreamReader<Record> requestStream,
            IServerStreamWriter<RecordAck> responseStream, ServerCallContext context)
        {
            try
            {
                Logger.Info("Writing records to Sage...");
                var schema = _server.WriteSettings.Schema;
                var sla = _server.WriteSettings.CommitSLA;
                var inCount = 0;
                var outCount = 0;
                
                // get next record to publish while connected and configured
                while (await requestStream.MoveNext(CancellationToken.None) && _server.Connected && _server.WriteConfigured)
                {
                    var record = requestStream.Current;
                    inCount++;
                    
                    // send record to source system
                    // timeout if it takes longer than the sla
                    var task = Task.Run(() => PutRecord(schema,record));
                    if (task.Wait(TimeSpan.FromSeconds(sla)))
                    {
                        // send ack
                        var ack = new RecordAck
                        {
                            CorrelationId = record.CorrelationId,
                            Error = task.Result
                        };
                        await responseStream.WriteAsync(ack);
                        
                        if (String.IsNullOrEmpty(task.Result))
                        {
                            outCount++;
                        }
                    }
                    else
                    {
                        // send timeout ack
                        var ack = new RecordAck
                        {
                            CorrelationId = record.CorrelationId,
                            Error = "timed out"
                        };
                        await responseStream.WriteAsync(ack);
                    }
                }
                
                Logger.Info($"Wrote {outCount} of {inCount} records to Sage.");
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }

        /// <summary>
        /// Handles disconnect requests from the agent
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<DisconnectResponse> Disconnect(DisconnectRequest request, ServerCallContext context)
        {
            // clear connection
            _server.Connected = false;
            _server.Settings = null;

            // alert connection session to close
            if (_tcs != null)
            {
                _tcs.SetResult(true);
                _tcs = null;
            }

            Logger.Info("Disconnected");
            return Task.FromResult(new DisconnectResponse());
        }

        /// <summary>
        /// Gets a shape for a given module
        /// </summary>
        /// <param name="module"></param>
        /// <param name="id"></param>
        /// <returns>returns a shape or null if unavailable</returns>
        private Task<Schema> GetShapeForModule(string module, string id)
        {
            try
            {
                // base shape to be added to
                var shape = new Schema
                {
                    Id = id,
                    Name = module,
                    Description = "",
                    PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                    {
                        Module = module
                    })
                };

                // get business object service for given module
                var busObjectService = _sessionService.MakeBusinessObject(module);

                // get a single record
                var record = busObjectService.GetSingleRecord();
                var keys = busObjectService.GetKeys();
                var key = keys[0];

                // assign all properties of record to shape
                foreach (var col in record)
                {
                    if (!string.IsNullOrEmpty(col.Key))
                    {
                        var property = new Property
                        {
                            Id = col.Key,
                            Name = col.Key,
                            Type = GetPropertyType(col.Value),
                            IsKey = col.Key == key,
                            IsCreateCounter = false,
                            IsUpdateCounter = false,
                            TypeAtSource = "",
                            IsNullable = true
                        };

                        shape.Properties.Add(property);
                    }
                }

                return Task.FromResult(shape);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return null;
            }
        }

        /// <summary>
        /// Gets the property type of a value
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private PropertyType GetPropertyType(dynamic value)
        {
            try
            {
                // try datetime
                if (DateTime.TryParse(value, out DateTime d))
                {
                    return PropertyType.Date;
                }

                // try int
                if (Int32.TryParse(value, out int i))
                {
                    return PropertyType.Integer;
                }

                // try float
                if (float.TryParse(value, out float f))
                {
                    return PropertyType.Float;
                }

                // try boolean
                if (bool.TryParse(value, out bool b))
                {
                    return PropertyType.Bool;
                }

                // default return string
                return PropertyType.String;
            }
            catch (Exception e)
            {
                // try object or array
                if (value is IEnumerable)
                {
                    return PropertyType.Json;
                }

                Logger.Error(e.Message);
                throw;
            }
        }
        
        /// <summary>
        /// Writes a record out to Sage
        /// </summary>
        /// <param name="schema"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        private Task<string> PutRecord(Schema schema, Record record)
        {
            try
            {
                var metaJsonObject = JsonConvert.DeserializeObject<PublisherMetaJson>(schema.PublisherMetaJson);
                
                // get business object service for given module
                var busObjectService = _sessionService.MakeBusinessObject(metaJsonObject.Module);

                var error = busObjectService.UpdateSingleRecord(record);

                return Task.FromResult(error);
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                return Task.FromResult(e.Message);
            }
        }
    }
}