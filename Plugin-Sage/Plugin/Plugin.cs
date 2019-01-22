using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Newtonsoft.Json;
using Plugin_Sage.API;
using Plugin_Sage.DataContracts;
using Plugin_Sage.Helper;
using Pub;

namespace Plugin_Sage.Plugin
{
    public class Plugin : Publisher.PublisherBase
    {
        private readonly ServerStatus _server;
        private TaskCompletionSource<bool> _tcs;
        private SessionService _sessionService;

        public Plugin()
        {
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
                Task.FromResult(new ConnectResponse
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
                _sessionService = new SessionService(settings);
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
        public override async Task<DiscoverShapesResponse> DiscoverShapes(DiscoverShapesRequest request,
            ServerCallContext context)
        {
            Logger.Info("Discovering Shapes...");

            DiscoverShapesResponse discoverShapesResponse = new DiscoverShapesResponse();

            // only return requested shapes if refresh mode selected
            if (request.Mode == DiscoverShapesRequest.Types.Mode.Refresh)
            {
                try
                {
                    var refreshShapes = request.ToRefresh;

                    Logger.Info($"Refresh shapes attempted: {refreshShapes.Count}");

                    var tasks = refreshShapes.Select((x) =>
                        {
                            var metaJsonObject = JsonConvert.DeserializeObject<PublisherMetaJson>(x.PublisherMetaJson);
                            return GetShapeForModule(metaJsonObject.Module, x.Id);
                        })
                        .ToArray();

                    await Task.WhenAll(tasks);

                    discoverShapesResponse.Shapes.AddRange(tasks.Where(x => x.Result != null).Select(x => x.Result));

                    // return all shapes 
                    Logger.Info($"Shapes returned: {discoverShapesResponse.Shapes.Count}");
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

                var tasks = _server.Settings.ModulesList.Select((x, i) => GetShapeForModule(x, i.ToString()))
                    .ToArray();

                await Task.WhenAll(tasks);

                discoverShapesResponse.Shapes.AddRange(tasks.Where(x => x.Result != null).Select(x => x.Result));

                // return all shapes 
                Logger.Info($"Shapes returned: {discoverShapesResponse.Shapes.Count}");
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
        public override Task PublishStream(PublishRequest request, IServerStreamWriter<Record> responseStream,
            ServerCallContext context)
        {
            var shape = request.Shape;
            var limit = request.Limit;
            var limitFlag = request.Limit != 0;

            Logger.Info($"Publishing records for shape: {shape.Name}");

            return Task.CompletedTask;
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
        private Task<Shape> GetShapeForModule(string module, string id)
        {
            // base shape to be added to
            var shape = new Shape
            {
                Id = id,
                Name = module,
                Description = "",
                PublisherMetaJson = JsonConvert.SerializeObject(new PublisherMetaJson
                {
                    Module = module
                })
            };

            var busObjectService = new BusinessObjectService(_sessionService, module);

            var record = busObjectService.GetSingleRecord();

            var propId = 0;
            foreach (var col in record)
            {
                var property = new Property
                {
                    Id = propId.ToString(),
                    Name = col.Key,
                    Type = GetPropertyType(col.Value),
                    IsKey = false,
                    IsCreateCounter = false,
                    IsUpdateCounter = false,
                    TypeAtSource = "",
                    IsNullable = true
                };

                shape.Properties.Add(property);
                propId++;
            }

            return Task.FromResult(shape);
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
                // try object or array
                if (value is IEnumerable)
                {
                    return PropertyType.Json;
                }
                
                // try datetime
                DateTime d;
                if (DateTime.TryParse(value, out d))
                {
                    return PropertyType.Date;
                }

                // try int
                int i;
                if (Int32.TryParse(value, out i))
                {
                    return PropertyType.Integer;
                }

                // try float
                float f;
                if (float.TryParse(value, out f))
                {
                    return PropertyType.Float;
                }
                
                // try boolean
                bool b;
                if (bool.TryParse(value, out b))
                {
                    return PropertyType.Bool;
                }
                
                // default return string
                return PropertyType.String;
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
                throw;
            }
        }
    }
}