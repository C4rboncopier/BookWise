using Book_Shop.Entity_Class;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;

namespace Book_Shop
{
    public sealed class DBHelper
    {
        private static DBHelper _instance = null;

        private readonly SqlConnection _connection = null;

        // computer
        public const string DB_NAME = @"Data Source=DESKTOP-SFI66MV\SQLEXPRESS;Initial Catalog=Bookwise;Integrated Security=True";

        private DBHelper()
        {
            _connection = new SqlConnection(DB_NAME);

            // open the connection
            _connection.Open();

        }

        public static DBHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DBHelper();
                }

                return _instance;
            }
        }


        // Loads the Profile Picture
        public void LoadUserProfilePicture(PictureBox pictureControl, int ID)
        {
            try
            {
                using (SqlCommand cmd = _connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT image FROM Accounts WHERE ID = @ID";
                    cmd.Parameters.AddWithValue("@ID", ID);

                    object imageObj = cmd.ExecuteScalar();

                    if (imageObj != DBNull.Value)
                    {
                        byte[] imageData = (byte[])imageObj;

                        if (imageData != null && imageData.Length > 0)
                        {
                            using (MemoryStream ms = new MemoryStream(imageData))
                            {
                                pictureControl.Image = Image.FromStream(ms);
                            }
                        }
                    }
                    else
                    {
                        string defaultImagePath = Path.Combine(Application.StartupPath, "Assets", "userIcon6.png");
                        pictureControl.Image = Image.FromFile(defaultImagePath);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while loading the profile picture: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // --------- LOGIN USER AND ADMIN ---------
        public Accounts AuthenticateUser(string User)
        {
            string query = "SELECT * FROM Accounts WHERE username = @User";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@User", User);

                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var userId = reader.GetInt32(0);
                    var username = reader.GetString(1);
                    var password = reader.GetString(2);
                    var firstName = reader.GetString(3);
                    var lastName = reader.GetString(4);
                    var isAdmin = reader.GetString(6);

                    var accounts = new Accounts();
                    accounts.Id = userId;
                    accounts.Username = username;
                    accounts.Password = password;
                    accounts.FirstName = firstName;
                    accounts.LastName = lastName;
                    accounts.isAdmin = isAdmin;
                    reader.Close();
                    return accounts;
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
        }


        // Register
        public Accounts RegisterUser(Accounts account)
        {
            string query = $@"INSERT INTO Accounts (firstname, lastname, username, password, isAdmin) 
                VALUES (@firstname, @lastname, @username, @password, @isAdmin)";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@firstname", account.FirstName);
                cmd.Parameters.AddWithValue("@lastname", account.LastName);
                cmd.Parameters.AddWithValue("@username", account.Username);
                cmd.Parameters.AddWithValue("@password", account.Password);
                cmd.Parameters.AddWithValue("@isAdmin", account.isAdmin);

                var rowsAffected = cmd.ExecuteNonQuery();
                if (rowsAffected < 0)
                {
                    return null;
                }

                account.Id = GetUserID(account.Username);
            }

            return account;
        }
        public int GetUserID(string username)
        {
            string query = $"SELECT ID FROM Accounts WHERE username COLLATE Latin1_General_BIN = @username";

            using (SqlCommand cmd = new SqlCommand (query, _connection))
            {
                cmd.Parameters.AddWithValue("@username", username);
                var reader = cmd.ExecuteReader();

                if (!reader.Read())
                {
                    reader.Close();
                    return -1;
                }

                int userId = reader.GetInt32(0);

                reader.Close();
                return userId;
            }
        }

        // Get Transactions Id
        public Transactions getTransactId(int userId, string created_at)
        {
            string query = "SELECT * FROM Transactions WHERE user_ID = @userId AND Created_at = @created_at";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@userId", userId);
                cmd.Parameters.AddWithValue("@created_at", created_at);
                
                var reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    var ID = reader.GetInt32(0);

                    var transact = new Transactions();
                    transact.Id = ID;
                    reader.Close();
                    return transact;
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
        }

        // Creates Cart
        public bool createCart(int userId, string created_at, string updated_at)
        {
            string query = $@"INSERT INTO Carts (user_ID, created_at, updated_at)
                        VALUES ('{userId}', '{created_at}', '{updated_at}')
                        ";

            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                int rowsAffected = cmd.ExecuteNonQuery();
                return rowsAffected > 0;
            }
        }
        public bool IsUserCartTaken(int userId)
        {
            string checkUserIdQuery = $"SELECT COUNT(*) FROM Carts WHERE user_ID = '{userId}'";

            using (SqlCommand checkUserIdCmd = new SqlCommand(checkUserIdQuery, _connection))
            {
                int userIDCount = (int)checkUserIdCmd.ExecuteScalar();
                return userIDCount > 0;
            }
        }

        // Update Cart
        public void updateCart(int userId)
        {
            string updatedAt = DateTime.Now.ToString();
            string query = $"UPDATE Carts SET updated_at = '{updatedAt}' WHERE user_ID = {userId}";
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.ExecuteNonQuery();
            }
        }


        // Gets book data
        public Books getBooksData(string title)
        {
            string query = "SELECT * FROM BookTbl WHERE Title = @title";

            using (SqlCommand getBooksDataCmd = new SqlCommand(query, _connection))
            {
                getBooksDataCmd.Parameters.AddWithValue("@title", title);

                var reader = getBooksDataCmd.ExecuteReader();

                if (reader.Read())
                {
                    var Title = reader.GetString(0);
                    var Author = reader.GetString(1);
                    var Category = reader.GetString(2);
                    var Quantity = reader.GetInt32(3);
                    var Price = reader.GetDecimal(4);

                    var books = new Books();
                    books.title = Title;
                    books.author = Author;
                    books.category = Category;
                    books.quantity = Quantity;
                    books.price = Price;
                    reader.Close();
                    return books;
                }
                else
                {
                    reader.Close();
                    return null;
                }
            }
        }


        // Update Labels
        public void UpdateLabels(int userID, out int totalBooks, out double totalPrice)
        {
            totalPrice = 0.0;

            string totalBooksQuery = $"SELECT ISNULL(SUM(Amount), 0) FROM Carts_Item WHERE user_ID = '{userID}'";
            string totalPriceQuery = $"SELECT ISNULL(SUM(Total), 0) FROM Carts_Item WHERE user_ID = '{userID}'";

            using (SqlCommand totalBooksCmd = new SqlCommand(totalBooksQuery, _connection))
            using (SqlCommand totalPriceCmd = new SqlCommand(totalPriceQuery, _connection))
            {
                totalBooks = Convert.ToInt32(totalBooksCmd.ExecuteScalar());
                totalPrice = Convert.ToDouble(totalPriceCmd.ExecuteScalar());
            }
        }

        // Populate books
        public void PopulateBooksData(DataGridView dgv)
        {
            try
            {
                string query = "SELECT * FROM BookTbl";
                SqlDataAdapter sda = new SqlDataAdapter(query, _connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(sda);
                var ds = new DataSet();
                sda.Fill(ds);
                dgv.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("Populate Books: An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Populate Users
        public void DisplayUsers(DataGridView dgv)
        {
            try
            {
                string query = "SELECT ID, username, password, firstname, lastname FROM Accounts WHERE ID <> 1";
                SqlDataAdapter sda = new SqlDataAdapter(query, _connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(sda);
                var ds = new DataSet();
                sda.Fill(ds);
                dgv.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public void DisplayHistory(DataGridView dgv)
        {
            try
            {
                string query = "SELECT ID, user_ID AS 'User ID', Method, BooksQty AS 'Quantity', Total, Created_at AS 'Time & Date' FROM Transactions";
                SqlDataAdapter sda = new SqlDataAdapter(query, _connection);
                SqlCommandBuilder builder = new SqlCommandBuilder(sda);
                var ds = new DataSet();
                sda.Fill(ds);
                dgv.DataSource = ds.Tables[0];

                dgv.Columns["ID"].HeaderText = "ID";
                dgv.Columns["User ID"].HeaderText = "User ID";
                dgv.Columns["Method"].HeaderText = "Method";
                dgv.Columns["Quantity"].HeaderText = "Quantity";
                dgv.Columns["Total"].HeaderText = "Total";
                dgv.Columns["Time & Date"].HeaderText = "Time & Date";
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Get Categories
        public List<string> GetDistinctCategories()
        {
            List<string> categories = new List<string>();

            try
            {
                string query = "SELECT DISTINCT Category FROM BookTbl";
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string category = reader["Category"].ToString();
                    categories.Add(category);
                }
                reader.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return categories;
        }

        // Payment Cart
        public void PopulateUserCartOrDisplayInPayment(DataGridView dgv, int userID, bool forPayment)
        {
            try
            {
                string query;
                if (forPayment)
                {
                    query = $"SELECT Title, Price, Amount, Total FROM Carts_Item WHERE user_ID = '{userID}'";
                }
                else
                {
                    query = $"SELECT Title, Author, Category, Price, Amount, Total FROM Carts_Item WHERE user_ID = '{userID}'";
                }

                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                SqlCommandBuilder builder = new SqlCommandBuilder(sda);
                var ds = new DataSet();
                sda.Fill(ds);

                if (!forPayment)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (row["Amount"] == DBNull.Value)
                        {
                            row["Amount"] = 0;
                        }

                        if (row["Total"] == DBNull.Value)
                        {
                            row["Total"] = 0;
                        }
                    }
                }
                dgv.DataSource = ds.Tables[0];
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            dgv.Refresh();
        }
        public DataTable PopulateUserCart(int userID)
        {
            DataTable cartDataTable = new DataTable();

            try
            {
                string query = $"SELECT Title, Price, Amount, Total FROM Carts_Item WHERE user_ID = '{userID}'";
                SqlCommand cmd = new SqlCommand(query, _connection);
                SqlDataAdapter sda = new SqlDataAdapter(cmd);
                SqlCommandBuilder builder = new SqlCommandBuilder(sda);
                sda.Fill(cartDataTable);

                foreach (DataRow row in cartDataTable.Rows)
                {
                    if (row["Amount"] == DBNull.Value)
                    {
                        row["Amount"] = 0;
                    }

                    if (row["Total"] == DBNull.Value)
                    {
                        row["Total"] = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return cartDataTable;
        }

        // Edit
        public bool UpdateCartQuantity(string bookTitle, int userID, int newQuantity)
        {
            try
            {
                string timestamp = DateTime.Now.ToString();

                if (newQuantity == 0)
                {
                    string deleteQuery = $"DELETE FROM Carts_Item WHERE user_ID = '{userID}' AND Title = @bookTitle";
                    using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, _connection))
                    {
                        deleteCmd.Parameters.AddWithValue("@bookTitle", bookTitle);
                        deleteCmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    string getQuantityQuery = $"SELECT Quantity FROM BookTbl WHERE Title = @bookTitle";
                    using (SqlCommand getQtyCmd = new SqlCommand(getQuantityQuery, _connection))
                    {
                        getQtyCmd.Parameters.AddWithValue("@bookTitle", bookTitle);
                        int currentQuantity = (int)getQtyCmd.ExecuteScalar();

                        if (newQuantity > currentQuantity)
                        {
                            return false;
                        }
                        else
                        {
                            string updateQuantityQuery = $"UPDATE Carts_Item SET Amount = '{newQuantity}' WHERE user_ID = '{userID}' AND Title = @bookTitle";
                            using (SqlCommand updateCmd = new SqlCommand(updateQuantityQuery, _connection))
                            {
                                updateCmd.Parameters.AddWithValue("@bookTitle", bookTitle);
                                updateCmd.ExecuteNonQuery();
                            }

                            string calculateTotalQuery = $"SELECT Price FROM Carts_Item WHERE user_ID = '{userID}' AND Title = @bookTitle";
                            using (SqlCommand calculateTotalCmd = new SqlCommand(calculateTotalQuery, _connection))
                            {
                                calculateTotalCmd.Parameters.AddWithValue("@bookTitle", bookTitle);
                                decimal price = (decimal)calculateTotalCmd.ExecuteScalar();
                                decimal newTotal = price * newQuantity;

                                string updateTotalQuery = $"UPDATE Carts_Item SET Total = '{newTotal}' WHERE user_ID = '{userID}' AND Title = @bookTitle";
                                using (SqlCommand updateTotalCmd = new SqlCommand(updateTotalQuery, _connection))
                                {
                                    updateTotalCmd.Parameters.AddWithValue("@bookTitle", bookTitle);
                                    updateTotalCmd.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("UpdateCartQuantity: An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Delete
        public void RemoveBookFromCart(int userID, string selectedTitle)
        {
            try
            {
                string clearCartQuery = $"DELETE FROM Carts_Item WHERE user_ID = '{userID}' AND Title = @selectedTitle";
                using (SqlCommand clearCartCmd = new SqlCommand(clearCartQuery, _connection))
                {
                    clearCartCmd.Parameters.AddWithValue("@selectedTitle", selectedTitle);
                    clearCartCmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while removing the book from your order: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear user cart
        public void ClearUserCart(int userID)
        {
            string deleteQuery = $"DELETE FROM Carts_Item WHERE user_ID = '{userID}'";
            using (SqlCommand deleteCmd = new SqlCommand(deleteQuery, _connection))
            {
                deleteCmd.Parameters.AddWithValue("@UserID", userID);
                deleteCmd.ExecuteNonQuery();
            }
        }


        // User purchase
        public double GetTotalPrice(int userID)
        {
            string totalPriceQuery = $"SELECT SUM(Total) FROM Carts_Item WHERE user_ID = '{userID}'";
            using (SqlCommand totalPriceCmd = new SqlCommand(totalPriceQuery, _connection))
            {
                return Convert.ToDouble(totalPriceCmd.ExecuteScalar());
            }
        }
        public int GetTotalBooks(int userID)
        {
            string totalBooksQuery = $"SELECT SUM(Amount) FROM Carts_Item WHERE user_ID = '{userID}'";
            using (SqlCommand totalBooksCmd = new SqlCommand(totalBooksQuery, _connection))
            {
                return Convert.ToInt32(totalBooksCmd.ExecuteScalar());
            }
        }
        public void UpdateBookQuantities(DataGridView cartDGV)
        {
            foreach (DataGridViewRow row in cartDGV.Rows)
            {
                string title = row.Cells["Title"].Value.ToString();
                int amount = Convert.ToInt32(row.Cells["Amount"].Value);

                string updateQuantityQuery = $"UPDATE BookTbl SET Quantity = Quantity - '{amount}' WHERE Title = @Title";
                using (SqlCommand updateCmd = new SqlCommand(updateQuantityQuery, _connection))
                {
                    updateCmd.Parameters.AddWithValue("@Title", title);
                    updateCmd.ExecuteNonQuery();
                }
            }
        }
        public void UpdateAdminIncome(double incomeToAdd)
        {
            string isAdmin = "true";
            string fetchIncomeQuery = $"SELECT income FROM Accounts WHERE isAdmin = '{isAdmin}'";

            using (SqlCommand fetchIncomeCmd = new SqlCommand(fetchIncomeQuery, _connection))
            {
                double currentIncome = Convert.ToDouble(fetchIncomeCmd.ExecuteScalar());

                double newIncome = currentIncome + incomeToAdd;

                string updateIncomeQuery = $"UPDATE Accounts SET Income = '{newIncome}' WHERE isAdmin = '{isAdmin}'";
                SqlCommand updateIncomeCmd = new SqlCommand(updateIncomeQuery, _connection);
                updateIncomeCmd.ExecuteNonQuery();
            }
        }
        public bool CanAddToCart(int userID, string title)
        {
            string cartQuery = $"SELECT COUNT(*) FROM Carts_Item WHERE Title = @title AND user_ID = '{userID}'";
            using (SqlCommand cartCmd = new SqlCommand(cartQuery, _connection))
            {
                cartCmd.Parameters.AddWithValue("@title", title);
                int bookCountInCart = Convert.ToInt32(cartCmd.ExecuteScalar());
                return bookCountInCart == 0;
            }
        }
        public bool InsertBookIntoCart(Carts_Item cartItem)
        {
            string insertQuery = "INSERT INTO Carts_Item (user_ID, Title, Author, Category, Price, Amount, Total) " +
                                "VALUES (@UserId, @Title, @Author, @Category, @Price, @Amount, @Total)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(insertQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@UserId", cartItem.userId);
                    cmd.Parameters.AddWithValue("@Title", cartItem.title);
                    cmd.Parameters.AddWithValue("@Author", cartItem.author);
                    cmd.Parameters.AddWithValue("@Category", cartItem.category);
                    cmd.Parameters.AddWithValue("@Price", cartItem.price);
                    cmd.Parameters.AddWithValue("@Amount", cartItem.amount);
                    cmd.Parameters.AddWithValue("@Total", cartItem.total);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to insert the book into the cart: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Update Transactions
        public bool updateTransactions(Transactions transactions)
        {
            string query = "INSERT INTO Transactions (user_ID, Method, BooksQty, Total, Created_at) VALUES (@userId, @Method, @BooksQty, @Total, @Created_at)";

            try
            {
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@userId", transactions.user_ID);
                    cmd.Parameters.AddWithValue("@Method", transactions.Method);
                    cmd.Parameters.AddWithValue("@BooksQty", transactions.BooksQty);
                    cmd.Parameters.AddWithValue("@Total", transactions.Total);
                    cmd.Parameters.AddWithValue("@Created_at", transactions.Created_at);

                    cmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }


        // Search and Filter
        public DataTable GetFilteredBooks(string nameFilter, string authorFilter)
        {
            string query = "SELECT * FROM BookTbl WHERE 1=1";

            List<SqlParameter> parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(nameFilter))
            {
                query += " AND title LIKE @nameFilter";
                parameters.Add(new SqlParameter("@nameFilter", "%" + nameFilter + "%"));
            }

            if (!string.IsNullOrEmpty(authorFilter))
            {
                query += " AND author LIKE @authorFilter";
                parameters.Add(new SqlParameter("@authorFilter", "%" + authorFilter + "%"));
            }

            SqlDataAdapter adapter = new SqlDataAdapter(query, _connection);

            adapter.SelectCommand.Parameters.AddRange(parameters.ToArray());

            DataTable dataTable = new DataTable();
            adapter.Fill(dataTable);

            return dataTable;
        }


        // Profile picture save button
        public bool UpdateUserProfilePicture(int userId, string imageLocation)
        {
            try
            {
                if (string.IsNullOrEmpty(imageLocation))
                {
                    MessageBox.Show("Please select an image to upload.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                byte[] imageData = File.ReadAllBytes(imageLocation);

                string updateImageQuery = $"UPDATE Accounts SET image = @ImageData WHERE ID = '{userId}'";
                using (SqlCommand updateImageCmd = new SqlCommand(updateImageQuery, _connection))
                {
                    updateImageCmd.Parameters.AddWithValue("@ImageData", imageData);

                    int rowsAffected = updateImageCmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while uploading the image: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        // Image reset button
        public Image GetUserProfilePicture(int userId)
        {
            string getImageQuery = $"SELECT image FROM Accounts WHERE ID = '{userId}'";
            using (SqlCommand getImageCmd = new SqlCommand(getImageQuery, _connection))
            {
                object imageObj = getImageCmd.ExecuteScalar();

                if (imageObj != DBNull.Value)
                {
                    byte[] imageData = (byte[])imageObj;

                    if (imageData != null && imageData.Length > 0)
                    {
                        using (MemoryStream ms = new MemoryStream(imageData))
                        {
                            return Image.FromStream(ms);
                        }
                    }
                }
            }

            string defaultImagePath = Path.Combine(Application.StartupPath, "Assets", "userIcon6.png");
            return Image.FromFile(defaultImagePath);
        }

        // Image remove button
        public bool RemoveProfilePicture(int userId)
        {
            string checkImageQuery = $"SELECT image FROM Accounts WHERE ID = '{userId}'";
            using (SqlCommand checkImageCmd = new SqlCommand(checkImageQuery, _connection))
            {
                object imageObj = checkImageCmd.ExecuteScalar();

                if (imageObj == DBNull.Value)
                {
                    return false;
                }
                updateProfilePicture(userId);
                return true;
            }
        }
        public void updateProfilePicture(int userId)
        {
            string removeImageQuery = $"UPDATE Accounts SET image = NULL WHERE ID = '{userId}'";
            using (SqlCommand removeImageCmd = new SqlCommand(removeImageQuery, _connection))
            {
                removeImageCmd.ExecuteNonQuery();
            }
        }


        // Info Update
        public bool UpdateUserInfo(int userId, string newUsername, string newFirstName, string newLastName)
        {
            try
            {
                string updateQuery = $"UPDATE Accounts SET username = @newUsername, firstname = @newFirstName, lastname = @newLastName WHERE ID = '{userId}'";
                using (SqlCommand updateCmd = new SqlCommand(updateQuery, _connection))
                {
                    updateCmd.Parameters.AddWithValue("@newUsername", newUsername);
                    updateCmd.Parameters.AddWithValue("@newFirstName", newFirstName);
                    updateCmd.Parameters.AddWithValue("@newLastName", newLastName);

                    updateCmd.ExecuteNonQuery();
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Password Update
        public bool ChangeUserPassword(int userID, string currentPassword, string newPassword)
        {
            try
            {
                if (CheckUserPassword(userID, currentPassword))
                {
                    UpdateUserPass(userID, newPassword);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        private bool CheckUserPassword(int userID, string currentPassword)
        {
            string query = $"SELECT COUNT(*) FROM Accounts WHERE ID = '{userID}' AND password = @currentPassword";
            using (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@currentPassword", currentPassword);
                int count = Convert.ToInt32(cmd.ExecuteScalar());
                return count == 1;
            }
        }
        private void UpdateUserPass(int userID, string newPassword)
        {
            string updateQuery = $"UPDATE Accounts SET password = @newPassword WHERE ID = '{userID}'";
            using (SqlCommand updateCmd = new SqlCommand(updateQuery, _connection))
            {
                updateCmd.Parameters.AddWithValue("@newPassword", newPassword);
                updateCmd.ExecuteNonQuery();
            }
        }

        // Delete Account
        public bool DeleteAccount(int userId)
        {
            try
            {
                string deleteAccountQuery = $"DELETE FROM Accounts WHERE ID = '{userId}'";
                using (SqlCommand deleteAccountCmd = new SqlCommand(deleteAccountQuery, _connection))
                {
                    int rowsAffected = deleteAccountCmd.ExecuteNonQuery();

                    if (rowsAffected <= 0)
                    {
                        return false;
                    }
                }

                string deleteCartQuery = $"DELETE FROM CARTS WHERE user_ID = '{userId}'";
                using (SqlCommand  delCartCmd = new SqlCommand(deleteCartQuery, _connection))
                {
                    delCartCmd.ExecuteNonQuery();
                }

                ClearUserCart(userId);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while deleting the account: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        // Dashboard
        public void UpdateDashboardLabels(Label lblNumUsers, Label lblBookCount, Label lblBookStocks, Label lblTotalIncome)
        {
            try
            {
                string numUsersQuery = "SELECT COUNT(*) FROM Accounts";
                string numBooksQuery = "SELECT COUNT(*) FROM BookTbl";
                string bookStocksQuery = "SELECT ISNULL(SUM(Quantity), 0) FROM BookTbl";
                string incomeQuery = "SELECT income FROM Accounts WHERE username = 'Admin'";

                SqlCommand numUsersCmd = new SqlCommand(numUsersQuery, _connection);
                SqlCommand numBooksCmd = new SqlCommand(numBooksQuery, _connection);
                SqlCommand bookStocksCmd = new SqlCommand(bookStocksQuery, _connection);
                SqlCommand incomeCmd = new SqlCommand(incomeQuery, _connection);

                int userCount = (int)numUsersCmd.ExecuteScalar() - 1;
                int booksCount = (int)numBooksCmd.ExecuteScalar();
                int bookStocks = (int)bookStocksCmd.ExecuteScalar();
                decimal income = (decimal)incomeCmd.ExecuteScalar();

                lblNumUsers.Text = userCount.ToString();
                lblBookCount.Text = booksCount.ToString();
                lblBookStocks.Text = bookStocks.ToString();
                lblTotalIncome.Text = income.ToString("C");
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex);
            }
        }

        // Add books to Database
        public bool IsTitleExists(string title)
        {
            try
            {
                string query = "SELECT COUNT(*) FROM BookTbl WHERE Title = @title";
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@title", title);
                    int count = (int)cmd.ExecuteScalar();
                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        public bool AddBookToBookshelf(Books books)
        {
            try
            {
                string query = $@"INSERT INTO BookTbl (Title, Author, Category, Quantity, Price)
                    VALUES (@title, @author, @category, @quantity, @price)";
                using (SqlCommand cmd = new SqlCommand(query, _connection))
                {
                    cmd.Parameters.AddWithValue("@title", books.title);
                    cmd.Parameters.AddWithValue("@author", books.author);
                    cmd.Parameters.AddWithValue("@category", books.category);
                    cmd.Parameters.AddWithValue("@quantity", books.quantity);
                    cmd.Parameters.AddWithValue("@price", books.price);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public bool AddAuthors(string author)
        {
            string query = "INSERT INTO Authors (Author) VALUES (@AuthorName)";
            using  (SqlCommand cmd = new SqlCommand(query, _connection))
            {
                cmd.Parameters.AddWithValue("@AuthorName", author);

                int rowsAffected = cmd.ExecuteNonQuery();

                if (rowsAffected > 0)
                {
                    return true;
                }
                return false;
            }
        }

        public AutoCompleteStringCollection GetAuthorNames()
        {
            AutoCompleteStringCollection authorNames = new AutoCompleteStringCollection();

            string query = "SELECT DISTINCT Author FROM BookTbl";
            SqlCommand cmd = new SqlCommand(query, _connection);
            SqlDataReader reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                authorNames.Add(reader["Author"].ToString());
            }

            reader.Close();
            return authorNames;
        }

        // Delete book from Database
        public bool DeleteBook(string selectedTitle)
        {
            try
            {
                string clearCartQuery = "DELETE FROM BookTbl WHERE Title = @selectedTitle";
                using (SqlCommand clearCartCmd = new SqlCommand(clearCartQuery, _connection))
                {
                    clearCartCmd.Parameters.AddWithValue("@selectedTitle", selectedTitle);
                    clearCartCmd.ExecuteNonQuery();

                    string deleteFromCartQuery = "DELETE FROM Carts_Item WHERE Title = @selectedTitle";
                    using (SqlCommand deleteFromCartCmd = new SqlCommand(deleteFromCartQuery, _connection))
                    {
                        deleteFromCartCmd.Parameters.AddWithValue("@selectedTitle", selectedTitle);
                        deleteFromCartCmd.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }


        // Edit book
        public bool EditBook(string title, int newQuantity, decimal newPrice)
        {
            try
            {
                string updateQuery = "UPDATE BookTbl SET Quantity = @Quantity, Price = @Price WHERE Title = @title";
                using (SqlCommand cmd = new SqlCommand(updateQuery, _connection))
                {
                    cmd.Parameters.AddWithValue("@Quantity", newQuantity);
                    cmd.Parameters.AddWithValue("@Price", newPrice);
                    cmd.Parameters.AddWithValue("@title", title);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        string updateUserCartsQuery = "UPDATE Carts_Item SET Price = @Price WHERE Title = @title";
                        using (SqlCommand updateUserCartsCmd = new SqlCommand(updateUserCartsQuery, _connection))
                        {
                            updateUserCartsCmd.Parameters.AddWithValue("@Price", newPrice);
                            updateUserCartsCmd.Parameters.AddWithValue("@title", title);
                            updateUserCartsCmd.ExecuteNonQuery();
                        }
                    }
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

    }
}
