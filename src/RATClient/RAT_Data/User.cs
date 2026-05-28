namespace RAT_Data
{
    public class User
    {
        public string UserName;

        public string Password; // Used to locally unhash Login Passwords

        public int ID;

        public User(string userName, string password, int iD)
        {
            UserName = userName;
            Password = password;
            ID = iD;
        }
    }
}
