namespace RAT_Data
{
    public class User
    {
        public string UserName;

        public string Password; // Used to locally unhash Login Passwords

        public int ID;
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
