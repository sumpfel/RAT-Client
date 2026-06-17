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
// DeleteUser has no backend endpoint and throws NotSupportedException with a clear message
// instead of silently doing nothing. (EditUser is supported via PUT /user/{id}.)
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

        //KI start (Claude Opus 4.8, prompt 16): edit a user via PUT /user/{id}. An empty Password means
        // "leave it unchanged" (so editing a user without resetting their password is possible). The backend
        // enforces who may change what: an admin may change anyone/any field; a normal user only themselves
        // (username + password; is_admin/can_create are ignored for a non-admin self-edit).
        public async Task EditUser(User user)
        {
            HttpResponseMessage response = await _http.PutAsJsonAsync($"{BaseUrl}/user/{user.ID}", new
            {
                username = user.UserName,
                password = string.IsNullOrEmpty(user.Password) ? null : user.Password,
                is_admin = user.Privileges >= 100,
                can_create = user.CanCreate
            }, _json);
            await EnsureOk(response);
        }
        //KI end

        public Task DeleteUser(User user) =>
            throw new NotSupportedException("The backend has no endpoint to delete a user.");

        // ----- IDatabaseConnection: Graph ---------------------------------------

        public async Task<NetworkObjectGraph> GetNetworkGraph()
        {
            List<NetworkObjectDto> objectDtos = await GetJson<List<NetworkObjectDto>>("/networkObject/");
            List<InterfaceDto> interfaceDtos = await GetJson<List<InterfaceDto>>("/networkObjectInterface/");
            List<ConnectionDto> connectionDtos = await GetJson<List<ConnectionDto>>("/networkObjectConnection/");
            List<PermissionDto> permissionDtos = await GetJson<List<PermissionDto>>("/networkObjectPermission/");
            //KI start (Claude Opus 4.8, prompt 14): also load all users so permission rows can show a real name.
            List<UserDto> userDtos = await GetJson<List<UserDto>>("/user/");
            Dictionary<int, NetworkUser> usersById = userDtos.ToDictionary(
                u => u.Id,
                u => new NetworkUser(u.Username, u.Id, canCreate: u.CanCreate, privileges: u.IsAdmin ? 100 : 10));
            //KI end

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
                    ID = dto.Id,                            // KI (prompt 14): keep the db id for edit/delete
                    NetworkObjectId = dto.NetworkObjectId,  // KI (prompt 14): link back to the owning object
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
                    dto.Note ?? "", dto.Name)
                {
                    ID = dto.Id // KI (prompt 14): keep the db id for delete
                };

                ends[0].Connection = connection;
                ends[1].Connection = connection;
            }

            //KI start (Claude Opus 4.8, prompt 14): attach the access rights to each object so the Access Control
            // tab shows the real per-user rights. A permission of 0 (Hidden) means "no entry", so it is skipped.
            foreach (PermissionDto p in permissionDtos)
            {
                if (!objectsById.TryGetValue(p.NetworkObjectId, out NetworkObject? obj)) { continue; }
                if (p.Permissions <= 0) { continue; }
                NetworkUser user = usersById.TryGetValue(p.UserId, out NetworkUser? u)
                    ? u
                    : new NetworkUser($"user#{p.UserId}", p.UserId);
                obj.AccessRights.Add(new AccessRight(user, (AccesRights)p.Permissions) { ID = p.Id });
            }
            //KI end

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

        // ----- IDatabaseConnection: NetworkObjectInterface ----------------------
        //KI start (Claude Opus 4.8, prompt 14): interface persistence over /networkObjectInterface.

        private static object ToInterfacePayload(NetworkObjectInterface iface, int networkObjectId) => new
        {
            network_object_id = networkObjectId,
            // connection id is managed by the connection routes, not here -> always send null
            network_object_connection_id = (int?)(iface.Connection?.ID > 0 ? iface.Connection.ID : null),
            name = iface.Name ?? "",
            max_speed = iface.MaxSpeed,
            is_up = iface.IsUp,
            ipv4 = iface.IP?.IPv4 ?? "",
            ipv6 = iface.IP?.IPv6 ?? "",
            ipv4_subnet_mask = iface.IP?.IPv4SubnetMask ?? "",
            ipv6_prefix_length = iface.IP?.IPv6PrefixLength ?? 0,
            ipv4_gateway = iface.IP?.IPv4Gateway ?? ""
        };

        public async Task<NetworkObjectInterface> AddInterface(NetworkObjectInterface networkObjectInterface, NetworkObject networkObject)
        {
            InterfaceDto created = await PostJson<InterfaceDto>(
                "/networkObjectInterface/", ToInterfacePayload(networkObjectInterface, networkObject.ID));
            networkObjectInterface.ID = created.Id;
            networkObjectInterface.NetworkObjectId = created.NetworkObjectId;
            return networkObjectInterface;
        }

        public async Task EditInterface(NetworkObjectInterface networkObjectInterface)
        {
            HttpResponseMessage response = await _http.PutAsJsonAsync(
                $"{BaseUrl}/networkObjectInterface/{networkObjectInterface.ID}",
                ToInterfacePayload(networkObjectInterface, networkObjectInterface.NetworkObjectId), _json);
            await EnsureOk(response);
        }

        public async Task DeleteInterface(NetworkObjectInterface networkObjectInterface)
        {
            HttpResponseMessage response = await _http.DeleteAsync(
                $"{BaseUrl}/networkObjectInterface/{networkObjectInterface.ID}");
            await EnsureOk(response);
        }
        //KI end

        // ----- IDatabaseConnection: NetworkConnection ---------------------------
        //KI start (Claude Opus 4.8, prompt 14): connection persistence over /networkObjectConnection.

        public async Task<NetworkConnection> AddConnection(NetworkConnection networkConnection, NetworkObjectInterface interface1, NetworkObjectInterface interface2)
        {
            ConnectionDto created = await PostJson<ConnectionDto>("/networkObjectConnection/", new
            {
                name = networkConnection.Name ?? "",
                speed = networkConnection.Speed,
                type = networkConnection.Type.ToString(),
                note = networkConnection.Note ?? "",
                nO1 = interface1.ID, // the backend expects the two interface ids
                nO2 = interface2.ID
            });
            networkConnection.ID = created.Id;
            return networkConnection;
        }

        public async Task DeleteConnection(NetworkConnection networkConnection)
        {
            HttpResponseMessage response = await _http.DeleteAsync(
                $"{BaseUrl}/networkObjectConnection/{networkConnection.ID}");
            await EnsureOk(response);
        }
        //KI end

        // ----- IDatabaseConnection: AccessRights / permissions ------------------
        //KI start (Claude Opus 4.8, prompt 14): permission persistence over /networkObjectPermission.

        public async Task<List<AccessRight>> GetNetworkObjectPermissions(NetworkObject networkObject)
        {
            List<PermissionDto> permissions = await GetJson<List<PermissionDto>>("/networkObjectPermission/");
            List<UserDto> users = await GetJson<List<UserDto>>("/user/");
            Dictionary<int, NetworkUser> usersById = users.ToDictionary(
                u => u.Id,
                u => new NetworkUser(u.Username, u.Id, canCreate: u.CanCreate, privileges: u.IsAdmin ? 100 : 10));

            return permissions
                .Where(p => p.NetworkObjectId == networkObject.ID && p.Permissions > 0)
                .Select(p => new AccessRight(
                    usersById.TryGetValue(p.UserId, out NetworkUser? u) ? u : new NetworkUser($"user#{p.UserId}", p.UserId),
                    (AccesRights)p.Permissions) { ID = p.Id })
                .ToList();
        }

        public async Task SetPermission(NetworkObject networkObject, NetworkUser targetUser, AccesRights right)
        {
            // POST upserts: the backend updates the existing (user, object) row or creates a new one.
            // Granting Hidden(0) is the same as removing the row -> route it through DeletePermission instead.
            if (right == AccesRights.Hidden)
            {
                AccessRight? existing = networkObject.AccessRights.FirstOrDefault(a => a.User.ID == targetUser.ID);
                if (existing != null) { await DeletePermission(networkObject, existing); }
                return;
            }

            HttpResponseMessage response = await _http.PostAsJsonAsync($"{BaseUrl}/networkObjectPermission/", new
            {
                network_object_id = networkObject.ID,
                target_user_id = targetUser.ID,
                permissions = (int)right
            }, _json);
            await EnsureOk(response);
        }

        public async Task DeletePermission(NetworkObject networkObject, AccessRight accessRight)
        {
            if (accessRight.ID <= 0) { return; } // nothing persisted to remove
            HttpResponseMessage response = await _http.DeleteAsync(
                $"{BaseUrl}/networkObjectPermission/{accessRight.ID}");
            await EnsureOk(response);
        }
        //KI end

        // ----- IDatabaseConnection: UserDeviceLogins ----------------------------

        //KI start (Claude Opus 4.8, prompt 16): per-user logins are stored against the current user's
        // NetworkObjectPermission row. A global admin can open an object they have NO permission row on, which
        // used to throw. Now: looking up the id returns null instead of throwing (reads just show no logins),
        // and ADDING a login first makes sure a row exists (a global admin / Admin / Owner may self-grant one).

        /// <summary>The current user's permission-row id for an object, or null if they have none.</summary>
        private async Task<int?> TryGetMyPermissionId(NetworkObject networkObject)
        {
            if (_myPermissionIdByObjectId.TryGetValue(networkObject.ID, out int cached))
            {
                return cached;
            }
            List<PermissionDto> permissions = await GetJson<List<PermissionDto>>("/networkObjectPermission/");
            PermissionDto? mine = permissions.FirstOrDefault(
                p => p.UserId == User.ID && p.NetworkObjectId == networkObject.ID);
            if (mine == null) { return null; }
            _myPermissionIdByObjectId[networkObject.ID] = mine.Id;
            return mine.Id;
        }

        /// <summary>Like <see cref="TryGetMyPermissionId"/> but creates the row (See) if missing.</summary>
        private async Task<int> EnsureMyPermissionId(NetworkObject networkObject)
        {
            int? existing = await TryGetMyPermissionId(networkObject);
            if (existing is int id) { return id; }

            // No row yet (e.g. a global admin). Grant ourselves at least See so the login has somewhere to live.
            NetworkUser self = new NetworkUser(User.UserName, User.ID,
                canCreate: User.CanCreate, privileges: User.Privileges);
            await SetPermission(networkObject, self, AccesRights.See);

            // re-read to get the new row id
            _myPermissionIdByObjectId.Remove(networkObject.ID);
            int? created = await TryGetMyPermissionId(networkObject);
            if (created is int newId) { return newId; }
            throw new InvalidOperationException(
                $"Could not create a permission row for the current user on network object {networkObject.ID}.");
        }

        public async Task<List<Login>> GetUserDeviceLogin(NetworkObject networkObject)
        {
            int? permissionId = await TryGetMyPermissionId(networkObject);
            if (permissionId is not int pid) { return new List<Login>(); } // no row -> no per-user logins yet
            List<LoginDto> dtos = await GetJson<List<LoginDto>>("/login/");
            return dtos
                .Where(d => d.NetworkObjectPermissionId == pid)
                .Select(d => new Login(d.Username, d.Password, d.Port, ParseLoginType(d.Type)) { ID = d.Id })
                .ToList();
        }

        public async Task<Login> AddUserDeviceLogin(Login login, NetworkObject networkObject)
        {
            int permissionId = await EnsureMyPermissionId(networkObject);
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
