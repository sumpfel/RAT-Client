//KI start (Claude Opus 4.8, prompt: link the C# frontend with the RAT-Backend database)
// Real IDatabaseConnection implementation that talks to the FastAPI RAT-Backend over HTTP.
//
// Auth: the backend uses OAuth2 password flow -> POST /user/login returns a JWT bearer
// token which we then send as "Authorization: Bearer <token>" on every other request.
//
// The backend has no single "graph" endpoint, so GetNetworkGraph() composes the topology
// client-side out of /networkObject, /networkObjectInterface, /networkObjectConnection and
// /networkObjectPermission (this keeps the backend untouched apart from the small additions
// documented in RAT-Backend/doc/markdown/AI_usage.md, KI-10).
//
// Methods the backend has no endpoint for (EditUser / DeleteUser) throw NotSupportedException
// with a clear message instead of silently doing nothing.
using RAT_Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RAT_Data
{
    public class DatabaseConnection : IDatabaseConnection
    {
        private string _ip = "127.0.0.1";
        public string Ip
        {
            get => _ip;
            set
            {
                if (IP.IsIpv4Valid(value)) { _ip = value; }
                else { throw new FormatException("this is not an IP"); }
            }
        }

        private int _port = 8000;
        public int Port
        {
            get => _port;
            set
            {
                if (IP.IsPortVlaid(value)) { _port = value; }
                else { throw new FormatException("this is not a Port"); }
            }
        }

        public User User { get; set; }

        // One HttpClient for the lifetime of the connection; the bearer token is set after Login().
        private readonly HttpClient _http = new HttpClient();

        // JSON: the backend uses snake_case, so map every DTO property explicitly via [JsonPropertyName].
        private static readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        /// <summary>Base address of the backend, e.g. http://127.0.0.1:8000 .</summary>
        private string BaseUrl => $"http://{Ip}:{Port}";

        public DatabaseConnection(User user, string ip, int port)
        {
            User = user;
            Ip = ip;
            Port = port;
        }

        // ----- low level helpers -------------------------------------------------

        /// <summary>Throws a readable exception if the response is not a 2xx.</summary>
        private static async Task EnsureOk(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode) { return; }
            string body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Backend returned {(int)response.StatusCode} {response.ReasonPhrase}: {body}");
        }

        private async Task<T> GetJson<T>(string path)
        {
            HttpResponseMessage response = await _http.GetAsync($"{BaseUrl}{path}");
            await EnsureOk(response);
            string body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body, _json)!;
        }

        private async Task<T> PostJson<T>(string path, object payload)
        {
            HttpResponseMessage response = await _http.PostAsJsonAsync($"{BaseUrl}{path}", payload, _json);
            await EnsureOk(response);
            string body = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(body, _json)!;
        }

        // ----- DTOs that mirror the backend pydantic models ----------------------

        private class UserDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("username")] public string Username { get; set; } = "";
            [JsonPropertyName("is_admin")] public bool IsAdmin { get; set; }
            [JsonPropertyName("can_create")] public bool CanCreate { get; set; }
        }

        private class TokenDto
        {
            [JsonPropertyName("access_token")] public string AccessToken { get; set; } = "";
            [JsonPropertyName("token_type")] public string TokenType { get; set; } = "";
        }

        private class NetworkObjectDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; } = "";
            [JsonPropertyName("type")] public string Type { get; set; } = "";
            [JsonPropertyName("x")] public int X { get; set; }
            [JsonPropertyName("y")] public int Y { get; set; }
            [JsonPropertyName("os")] public string Os { get; set; } = "";
            [JsonPropertyName("cpu")] public string Cpu { get; set; } = "";
            [JsonPropertyName("gpu")] public string Gpu { get; set; } = "";
            [JsonPropertyName("ram")] public string Ram { get; set; } = "";
            [JsonPropertyName("specs")] public string Specs { get; set; } = "";
        }

        private class InterfaceDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("network_object_id")] public int NetworkObjectId { get; set; }
            [JsonPropertyName("network_object_connection_id")] public int? NetworkObjectConnectionId { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; } = "";
            [JsonPropertyName("max_speed")] public int MaxSpeed { get; set; }
            [JsonPropertyName("is_up")] public bool IsUp { get; set; }
            [JsonPropertyName("ipv4")] public string Ipv4 { get; set; } = "";
            [JsonPropertyName("ipv6")] public string Ipv6 { get; set; } = "";
            [JsonPropertyName("ipv4_subnet_mask")] public string Ipv4SubnetMask { get; set; } = "";
            [JsonPropertyName("ipv6_prefix_length")] public int Ipv6PrefixLength { get; set; }
            [JsonPropertyName("ipv4_gateway")] public string Ipv4Gateway { get; set; } = "";
        }

        private class ConnectionDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("name")] public string Name { get; set; } = "";
            [JsonPropertyName("speed")] public int Speed { get; set; }
            [JsonPropertyName("type")] public string Type { get; set; } = "";
            [JsonPropertyName("note")] public string Note { get; set; } = "";
        }

        private class PermissionDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("network_object_id")] public int NetworkObjectId { get; set; }
            [JsonPropertyName("user_id")] public int UserId { get; set; }
            [JsonPropertyName("permissions")] public int Permissions { get; set; }
        }

        private class LoginDto
        {
            [JsonPropertyName("id")] public int Id { get; set; }
            [JsonPropertyName("network_object_permission_id")] public int NetworkObjectPermissionId { get; set; }
            [JsonPropertyName("port")] public int Port { get; set; }
            [JsonPropertyName("type")] public string Type { get; set; } = "";
            [JsonPropertyName("username")] public string Username { get; set; } = "";
            [JsonPropertyName("password")] public string Password { get; set; } = "";
        }

        // ----- mapping helpers ---------------------------------------------------

        private static NetworkObjectType ParseType(string type) =>
            Enum.TryParse(type, ignoreCase: true, out NetworkObjectType t) ? t : NetworkObjectType.PC;

        private static LoginType ParseLoginType(string type) =>
            Enum.TryParse(type, ignoreCase: true, out LoginType t) ? t : LoginType.SSH;

        private static object ToNetworkObjectPayload(NetworkObject nO) => new
        {
            name = nO.Name,
            type = nO.Type.ToString(),
            x = nO.X,
            y = nO.Y,
            os = nO.Os ?? "",
            cpu = nO.Cpu ?? "",
            gpu = nO.Gpu ?? "",
            ram = nO.Ram ?? "",
            specs = nO.Specs ?? ""
        };

        // Cache of (objectId -> my permission row id), filled by GetNetworkGraph(); logins are
        // keyed by network_object_permission_id on the backend, so we need it to add/read logins.
        private readonly Dictionary<int, int> _myPermissionIdByObjectId = new Dictionary<int, int>();

        // ----- IDatabaseConnection: User / Login --------------------------------

        public async Task<User> Login()
        {
            // OAuth2 password flow expects an application/x-www-form-urlencoded body.
            FormUrlEncodedContent form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", User.UserName),
                new KeyValuePair<string, string>("password", User.Password)
            });

            HttpResponseMessage response = await _http.PostAsync($"{BaseUrl}/user/login", form);
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new AccessViolationException("Wrong user or password for the server.");
            }
            await EnsureOk(response);

            TokenDto token = JsonSerializer.Deserialize<TokenDto>(
                await response.Content.ReadAsStringAsync(), _json)!;
            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token.AccessToken);

            // Pull the full account (id / is_admin / can_create) now that we are authenticated.
            UserDto me = await GetJson<UserDto>("/user/me");
            User = new User(me.Username, User.Password, me.Id, me.IsAdmin ? 100 : 10, me.CanCreate);
            return User;
        }

        public async Task<List<User>> GetAllUsers()
        {
            List<UserDto> dtos = await GetJson<List<UserDto>>("/user/");
            // Password is unknown for other users (never returned by the API) -> empty string.
            return dtos
                .Select(d => new User(d.Username, "", d.Id, d.IsAdmin ? 100 : 10, d.CanCreate))
                .ToList();
        }

        public async Task<User> AddUser(User user)
        {
            UserDto created = await PostJson<UserDto>("/user/register", new
            {
                username = user.UserName,
                password = user.Password,
                is_admin = user.Privileges >= 100,
                can_create = user.CanCreate
            });
            return new User(created.Username, user.Password, created.Id,
                created.IsAdmin ? 100 : 10, created.CanCreate);
        }

        public Task EditUser(User user) =>
            throw new NotSupportedException("The backend has no endpoint to edit a user.");

        public Task DeleteUser(User user) =>
            throw new NotSupportedException("The backend has no endpoint to delete a user.");

        // ----- IDatabaseConnection: Graph ---------------------------------------

        public async Task<NetworkObjectGraph> GetNetworkGraph()
        {
            List<NetworkObjectDto> objectDtos = await GetJson<List<NetworkObjectDto>>("/networkObject/");
            List<InterfaceDto> interfaceDtos = await GetJson<List<InterfaceDto>>("/networkObjectInterface/");
            List<ConnectionDto> connectionDtos = await GetJson<List<ConnectionDto>>("/networkObjectConnection/");
            List<PermissionDto> permissionDtos = await GetJson<List<PermissionDto>>("/networkObjectPermission/");

            // Remember which permission row is *mine* per object, so logins can be added/read later.
            _myPermissionIdByObjectId.Clear();
            foreach (PermissionDto p in permissionDtos.Where(p => p.UserId == User.ID))
            {
                _myPermissionIdByObjectId[p.NetworkObjectId] = p.Id;
            }

            // Build the NetworkObjects first so we can attach interfaces to them.
            Dictionary<int, NetworkObject> objectsById = new Dictionary<int, NetworkObject>();
            foreach (NetworkObjectDto dto in objectDtos)
            {
                objectsById[dto.Id] = new NetworkObject
                {
                    ID = dto.Id,
                    Name = dto.Name,
                    Type = ParseType(dto.Type),
                    X = dto.X,
                    Y = dto.Y,
                    Os = dto.Os,
                    Cpu = dto.Cpu,
                    Gpu = dto.Gpu,
                    Ram = dto.Ram,
                    Specs = dto.Specs
                };
            }

            // Build interfaces and attach them to their object; remember them by id so a
            // connection can find the two interfaces it joins.
            Dictionary<int, NetworkObjectInterface> interfacesById = new Dictionary<int, NetworkObjectInterface>();
            foreach (InterfaceDto dto in interfaceDtos)
            {
                NetworkObjectInterface iface = new NetworkObjectInterface
                {
                    Name = dto.Name,
                    MaxSpeed = dto.MaxSpeed,
                    IsUp = dto.IsUp,
                    IP = new IP
                    {
                        IPv4 = dto.Ipv4 ?? "",
                        IPv6 = dto.Ipv6 ?? "",
                        IPv4SubnetMask = dto.Ipv4SubnetMask ?? "",
                        IPv6PrefixLength = dto.Ipv6PrefixLength,
                        IPv4Gateway = dto.Ipv4Gateway ?? ""
                    }
                };
                interfacesById[dto.Id] = iface;

                if (objectsById.TryGetValue(dto.NetworkObjectId, out NetworkObject? owner))
                {
                    owner.NetworkInterfaces.Add(iface);
                }
            }

            // Re-create the NetworkConnection objects. A connection joins exactly the interfaces
            // whose network_object_connection_id points at it. We only build it when both ends are
            // present (a half-visible connection where the other device is Hidden is skipped).
            foreach (ConnectionDto dto in connectionDtos)
            {
                List<NetworkObjectInterface> ends = interfaceDtos
                    .Where(i => i.NetworkObjectConnectionId == dto.Id)
                    .Select(i => interfacesById[i.Id])
                    .ToList();

                if (ends.Count != 2) { continue; }

                NetworkConnection connection = new NetworkConnection(
                    ends[0], ends[1], dto.Speed,
                    dto.Type.Equals("Wireless", StringComparison.OrdinalIgnoreCase)
                        ? NetworkConnectionType.Wireless
                        : NetworkConnectionType.Wired,
                    dto.Note ?? "", dto.Name);

                ends[0].Connection = connection;
                ends[1].Connection = connection;
            }

            return new NetworkObjectGraph { networkObjects = objectsById.Values.ToList() };
        }

        // ----- IDatabaseConnection: NetworkObject -------------------------------

        public async Task<NetworkObject> AddNetworkObject(NetworkObject networkObject)
        {
            NetworkObjectDto created = await PostJson<NetworkObjectDto>(
                "/networkObject/", ToNetworkObjectPayload(networkObject));
            networkObject.ID = created.Id; // the backend assigned the real id
            return networkObject;
        }

        public async Task EditNetworkObject(NetworkObject networkObject)
        {
            HttpResponseMessage response = await _http.PutAsJsonAsync(
                $"{BaseUrl}/networkObject/{networkObject.ID}", ToNetworkObjectPayload(networkObject), _json);
            await EnsureOk(response);
        }

        public async Task DeleteNetworkObject(NetworkObject networkObject)
        {
            HttpResponseMessage response = await _http.DeleteAsync(
                $"{BaseUrl}/networkObject/{networkObject.ID}");
            await EnsureOk(response);
        }

        // ----- IDatabaseConnection: UserDeviceLogins ----------------------------

        /// <summary>Resolves the current user's NetworkObjectPermission id for an object.</summary>
        private async Task<int> GetMyPermissionId(NetworkObject networkObject)
        {
            if (_myPermissionIdByObjectId.TryGetValue(networkObject.ID, out int cached))
            {
                return cached;
            }
            // Cache miss (e.g. no graph loaded yet) -> look it up fresh.
            List<PermissionDto> permissions = await GetJson<List<PermissionDto>>("/networkObjectPermission/");
            PermissionDto? mine = permissions.FirstOrDefault(
                p => p.UserId == User.ID && p.NetworkObjectId == networkObject.ID);
            if (mine == null)
            {
                throw new InvalidOperationException(
                    $"No permission row for the current user on network object {networkObject.ID}.");
            }
            _myPermissionIdByObjectId[networkObject.ID] = mine.Id;
            return mine.Id;
        }

        public async Task<List<Login>> GetUserDeviceLogin(NetworkObject networkObject)
        {
            int permissionId = await GetMyPermissionId(networkObject);
            List<LoginDto> dtos = await GetJson<List<LoginDto>>("/login/");
            return dtos
                .Where(d => d.NetworkObjectPermissionId == permissionId)
                .Select(d => new Login(d.Username, d.Password, d.Port, ParseLoginType(d.Type)) { ID = d.Id })
                .ToList();
        }

        public async Task<Login> AddUserDeviceLogin(Login login, NetworkObject networkObject)
        {
            int permissionId = await GetMyPermissionId(networkObject);
            LoginDto created = await PostJson<LoginDto>("/login/", new
            {
                network_object_permission_id = permissionId,
                port = login.Port,
                type = login.Type.ToString(),
                username = login.Username,
                password = login.Password
            });
            login.ID = created.Id;
            return login;
        }

        public async Task EditUserDeviceLogin(Login login)
        {
            // The backend keeps the login attached to its permission row; resend the same one.
            // We re-read it to find the permission id the row currently belongs to.
            List<LoginDto> dtos = await GetJson<List<LoginDto>>("/login/");
            LoginDto? existing = dtos.FirstOrDefault(d => d.Id == login.ID);
            if (existing == null)
            {
                throw new InvalidOperationException($"Login {login.ID} not found for the current user.");
            }
            HttpResponseMessage response = await _http.PutAsJsonAsync($"{BaseUrl}/login/{login.ID}", new
            {
                network_object_permission_id = existing.NetworkObjectPermissionId,
                port = login.Port,
                type = login.Type.ToString(),
                username = login.Username,
                password = login.Password
            }, _json);
            await EnsureOk(response);
        }

        public async Task DeletetUserDeviceLogin(Login login)
        {
            HttpResponseMessage response = await _http.DeleteAsync($"{BaseUrl}/login/{login.ID}");
            await EnsureOk(response);
        }

        // ----- IDatabaseConnection: UserSettings --------------------------------

        public async Task EditUserSettings(UserSettings userSettings)
        {
            HttpResponseMessage response = await _http.PutAsJsonAsync($"{BaseUrl}/user/settings/", new
            {
                zoom = userSettings.Zoom,
                show_ports = userSettings.ShowPorts,
                show_interfaces = userSettings.ShowInterfaces
            }, _json);
            await EnsureOk(response);
        }
    }
}
//KI end
