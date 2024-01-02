namespace Book_Shop.Entity_Class
{
    public class Accounts
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string isAdmin { get; set; }


        //updated information
        public string newUsername {  get; set; }
        public string newPassword { get; set; }
        public string newFirstName { get; set; }
        public string newLastName { get; set; }
    }
}
