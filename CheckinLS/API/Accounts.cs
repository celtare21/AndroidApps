namespace CheckinLS.API
{
    public readonly struct Accounts
    {
        public readonly string Username;
        public readonly string Password;

        public Accounts(string username, string password) =>
            (Username, Password) = (username, password);
    }
}
