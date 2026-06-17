namespace RAT_Data
{
    public class User
    {
        //KI start (Claude Opus 4.8, prompt 17): these were public *fields* — WPF data binding only works on
        // *properties*, so {Binding UserName} in the access-control combo silently showed nothing. Made them
        // auto-properties (source-compatible with the existing field reads/writes) so bindings resolve.
        public string UserName { get; set; }

        public string Password { get; set; } // Used to locally unhash Login Passwords

        public int ID { get; set; }
        //KI end
        public int Privileges { private set; get; } = 10;
        public bool CanCreate { private set; get; }

        public User(string userName, string password, int iD, int privileges, bool canCreate)
        {
            UserName = userName;
            Password = password;
            ID = iD;
            Privileges = privileges;
            CanCreate = canCreate;
        }
    }
}
