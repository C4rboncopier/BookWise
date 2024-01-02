using Book_Shop.Entity_Class;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Book_Shop
{
    public partial class Admin_Page : Form
    {
        private Color originalFormBackColor;
        private Color originalLabelForeColor;
        private Color originalTextboxBackColor;
        private Color originalRadioButtonForeColor;

        string imgLocation = "";
        string pendingImageLocation = "";

        private Accounts _AdminUser;
        public Admin_Page(Accounts account)
        {
            InitializeComponent();
            _AdminUser = account;
        }

        private void Admin_Page_Load(object sender, EventArgs e)
        {
            label2.Parent = sidePanel2;
            label2.BackColor = Color.Transparent;

            bookIcon.Parent = sidePanel2;
            bookIcon.BackColor = Color.Transparent;

            lblFullName.Text = $"{_AdminUser.FirstName} {_AdminUser.LastName}";
            cbCategory.SelectedIndex = -1;

            pnlBook.Visible = true;
            pnlUsers.Visible = false;
            pnlDashboard.Visible = false;
            pnlHistory.Visible = false;
            pnlSettings.Visible = false;

            btnAddBook.Visible = false;
            btnDeleteBook.Visible = false;
            btnEditBook.Visible = false;

            Costumize();
            populate();
            users();
            Dashboard();
            LoadUserProfilePicture();
            DBHelper.Instance.DisplayHistory(HistoryDGV);

            originalFormBackColor = this.BackColor;
            originalLabelForeColor = label15.ForeColor;
            originalTextboxBackColor = tbTitle.BackColor;
            originalRadioButtonForeColor = rbLightMode.ForeColor;
        }

        // Custom designs
        private void Costumize()
        {
            Font columnHeaderFont2 = new Font("Segoe UI", 16, FontStyle.Bold);

            Font rowsFont = new Font("Segoe UI", 11, FontStyle.Regular);

            BookDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Navy;
            BookDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont2;

            BookDGV.RowsDefaultCellStyle.Font = rowsFont;
            BookDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;

            UsersDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Navy;
            UsersDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont2;

            UsersDGV.RowsDefaultCellStyle.Font = rowsFont;
            UsersDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;

            HistoryDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Navy;
            HistoryDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont2;

            HistoryDGV.RowsDefaultCellStyle.Font = rowsFont;
            HistoryDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;
        }

        // Display list of books
        private void populate()
        {
            DBHelper.Instance.PopulateBooksData(BookDGV);
            List<string> categories = DBHelper.Instance.GetDistinctCategories();
            cbCategory.DataSource = categories;
        }

        // Display list of Users
        private void users()
        {
            DBHelper.Instance.DisplayUsers(UsersDGV);
        }

        // Display Dashboard
        private void Dashboard()
        {
            DBHelper.Instance.UpdateDashboardLabels(lblNumUsers, lblBookCount, lblBookStocks, lblTotalIncome);
        }


        // ---------------- PANEL BUTTONS -------------------------------------
        private void ShowPanel(Panel panelToShow)
        {
            pnlBook.Visible = false;
            pnlUsers.Visible = false;
            pnlDashboard.Visible = false;
            pnlHistory.Visible = false;
            pnlSettings.Visible = false;

            panelToShow.Visible = true;
        }

        private void btnBooks_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlBook);
        }
        private void btnUsers_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlUsers);
        }
        private void btnDashboard_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlDashboard);
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlSettings);
        }
        private void btnHistory_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlHistory);
        }

        // ---------------- BOOKS PAGE -------------------------------------

        // Side Buttons
        private void SetButtonVisibility(bool add, bool delete, bool edit)
        {
            btnAddBook.Visible = add;
            btnDeleteBook.Visible = delete;
            btnEditBook.Visible = edit;
        }
        private void btnAddOption_Click(object sender, EventArgs e)
        {
            SetButtonVisibility(true, false, false);
            nonEditOption();
        }
        private void btnEditOption_Click(object sender, EventArgs e)
        {
            SetButtonVisibility(false, false, true);
            editOption();
        }
        private void btnDeleteOption_Click(object sender, EventArgs e)
        {
            SetButtonVisibility(false, true, false);
            nonEditOption();
        }
        private void btnDoneOption_Click(object sender, EventArgs e)
        {
            SetButtonVisibility(false, false, false);
            nonEditOption();
            ClearBookFields();
        }


        // Add to bookshelf
        private void btnAddBook_Click(object sender, EventArgs e)
        {
            string title = tbTitle.Texts;
            string author = tbAuthor.Text;
            string category = cbCategory.Text;
            int quantity;
            decimal price;

            if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(author) || string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show("Please input all requirements.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(tbQuantity.Texts, out quantity) || !decimal.TryParse(tbPrice.Texts, out price))
            {
                MessageBox.Show("Invalid quantity or price. Please enter valid numeric values.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var newBook = new Books();
            newBook.title = title;
            newBook.author = author;
            newBook.category = category;
            newBook.quantity = quantity;
            newBook.price = price;

            try
            {
                bool isTitleExist = DBHelper.Instance.IsTitleExists(newBook.title);
                if (isTitleExist)
                {
                    MessageBox.Show("The title already exists. Please choose a different title.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                bool canAddBook = DBHelper.Instance.AddBookToBookshelf(newBook);
                if (!canAddBook)
                {
                    MessageBox.Show("Failed to add the book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                MessageBox.Show("Book added successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ClearBookFields();
                populate();
                Dashboard();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Auto Suggest Author names
        private void tbAuthorNames_TextChanged(object sender, EventArgs e)
        {
            AutoCompleteStringCollection authorNames = DBHelper.Instance.GetAuthorNames();

            tbAuthor.AutoCompleteCustomSource = authorNames;
            tbAuthor.AutoCompleteMode = AutoCompleteMode.Suggest;
            tbAuthor.AutoCompleteSource = AutoCompleteSource.CustomSource;
        }

        // Delete book
        private void btnDeleteBook_Click(object sender, EventArgs e)
        {
            try
            {
                if (BookDGV.SelectedRows.Count > 0)
                {
                    string selectedTitle = BookDGV.SelectedRows[0].Cells["Title"].Value.ToString();

                    DialogResult result = MessageBox.Show("Do you want to clear the selected book?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        if (DBHelper.Instance.DeleteBook(selectedTitle))
                        {
                            MessageBox.Show("The selected book has been deleted.");
                            ClearBookFields();
                            populate();
                            Dashboard();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a book to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Edit book
        private void btnEditBook_Click(object sender, EventArgs e)
        {
            string title = tbTitle.Texts;
            int newQuantity;
            decimal newPrice;

            if (!int.TryParse(tbQuantity.Texts, out newQuantity) || !decimal.TryParse(tbPrice.Texts, out newPrice))
            {
                MessageBox.Show("Invalid new quantity or price. Please enter valid numeric values.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool canEditBook = DBHelper.Instance.EditBook(title, newQuantity, newPrice);

            if (!canEditBook)
            {
                MessageBox.Show("No book updated.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show("Book updated successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            populate();
            Dashboard();
            ClearBookFields();

        }
        private void ClearBookFields()
        {
            tbTitle.Texts = "";
            tbAuthor.Text = "";
            cbCategory.SelectedIndex = -1;
            tbQuantity.Texts = "";
            tbPrice.Texts = "";
        }
        private void btnEditSelect_Click(object sender, EventArgs e)
        {
            if (BookDGV.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = BookDGV.SelectedRows[0];

                string title = selectedRow.Cells["Title"].Value.ToString();
                string author = selectedRow.Cells["Author"].Value.ToString();
                string category = selectedRow.Cells["Category"].Value.ToString();
                string quantity = selectedRow.Cells["Quantity"].Value.ToString();
                string price = selectedRow.Cells["Price"].Value.ToString();

                tbTitle.Texts = title;
                tbAuthor.Text = author;
                cbCategory.SelectedItem = category;
                tbQuantity.Texts = quantity;
                tbPrice.Texts = price;

            }
            else
            {
                MessageBox.Show("Please select a book to edit.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void editOption()
        {
            this.btnClearOption.Size = new Size(125, 53);
            this.btnClearOption.Location = new Point(785, 116);

            btnEditSelect.Visible = true;

            tbTitle.Enabled = false;
            tbAuthor.Enabled = false;
            cbCategory.Enabled = false;

            if (rbDarkMode.Checked)
            {
                tbTitle.BackColor = Color.FromArgb(38, 38, 38);
                tbAuthor.BackColor = Color.FromArgb(38,38,38);
            }
            else
            {
                tbTitle.BackColor = Color.Gainsboro;
                tbAuthor.BackColor = Color.Gainsboro;
            }
        }
        private void nonEditOption()
        {
            this.btnClearOption.Size = new Size(259, 53);
            this.btnClearOption.Location = new Point(651, 116);

            btnEditSelect.Visible = false;

            tbTitle.Enabled = true;
            tbAuthor.Enabled = true;
            cbCategory.Enabled = true;

            if (rbDarkMode.Checked)
            {
                tbTitle.BackColor = Color.FromArgb(89, 89, 89);
                tbAuthor.BackColor = Color.FromArgb(89, 89, 89);
            }
            else
            {
                tbTitle.BackColor = SystemColors.Menu;
                tbAuthor.BackColor = SystemColors.Menu;
            }

        }


        // Clear textbox button
        private void btnClearOption_Click(object sender, EventArgs e)
        {
            ClearBookFields();
        }


        // ---------------- DASHBOARD PAGE -------------------------------------

        // User Account Delete Button
        private void btnUserDelete_Click(object sender, EventArgs e)
        {
            if (UsersDGV.SelectedRows.Count > 0)
            {
                string getUsername = UsersDGV.SelectedRows[0].Cells["username"].Value.ToString();
                string getUserID = UsersDGV.SelectedRows[0].Cells["ID"].Value.ToString();
                int idToDeleteInt = int.Parse(getUserID);

                DialogResult result = MessageBox.Show("Are you sure you want to delete the account for " + getUsername + "?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    bool UserDelete = DBHelper.Instance.DeleteAccount(idToDeleteInt);

                    if (UserDelete)
                    {
                        MessageBox.Show("The account for " + getUsername + " has been deleted successfully.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        DBHelper.Instance.ClearUserCart(idToDeleteInt);
                        users();
                        Dashboard();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the account for " + getUsername + ".", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Please select a user to delete.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // ---------------- SETTINGS PAGE -------------------------------------

        // Loads Profile Picture
        private void LoadUserProfilePicture()
        {
            DBHelper dbHelper = DBHelper.Instance;
            dbHelper.LoadUserProfilePicture(ProfilePicture, 1);
            dbHelper.LoadUserProfilePicture(ProfilePicture1, 1);
        }

        // Browse Image
        private void btnBrowseImage_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "png files(*.png)|*.png|jpg files(*.jpg)|*.jpg|All files(*.*)|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                pendingImageLocation = dialog.FileName.ToString();
                ProfilePicture1.ImageLocation = pendingImageLocation;
                imgLocation = pendingImageLocation;
            }
        }

        // Reset Image
        private void btnImageReset_Click(object sender, EventArgs e)
        {
            try
            {
                Image userProfilePicture = DBHelper.Instance.GetUserProfilePicture(0);
                ProfilePicture1.Image = userProfilePicture;
                pendingImageLocation = "";
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while resetting the profile picture: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Remove Button
        private void btnRemoveImage_Click(object sender, EventArgs e)
        {
            if (DBHelper.Instance.RemoveProfilePicture(0))
            {
                string defaultImagePath = Path.Combine(Application.StartupPath, "Assets", "userIcon6.png");
                LoadUserProfilePicture();
            }
        }

        // Save Image
        private void btnImageSave_Click(object sender, EventArgs e)
        {
            var imageSave = DBHelper.Instance.UpdateUserProfilePicture(_AdminUser.Id, pendingImageLocation);

            if (!imageSave)
            {
                MessageBox.Show("Failed to upload the image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            MessageBox.Show("Image successfully uploaded.");
            LoadUserProfilePicture();
        }


        // Info Update Button
        private void btnUpdateInfo_Click(object sender, EventArgs e)
        {
            string newFirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbNewFirstName.Texts.ToLower());
            string newLastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbNewLastName.Texts.ToLower());

            if (string.IsNullOrWhiteSpace(newFirstName) || string.IsNullOrWhiteSpace(newLastName))
            {
                MessageBox.Show("Please fill in all fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = DBHelper.Instance.UpdateUserInfo(_AdminUser.Id, _AdminUser.Username, newFirstName, newLastName);

            if (success)
            {
                MessageBox.Show("User information updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblFullName.Text = $"{newFirstName} {newLastName}";

                tbNewFirstName.Texts = "";
                tbNewLastName.Texts = "";
            }
            else
            {
                MessageBox.Show("An error occurred while updating user information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Password Update Button
        private void btnUpdatePass_Click(object sender, EventArgs e)
        {
            string currentPassword = tbCurrentPass.Texts;
            string newPassword = tbNewPass.Texts;
            string confirmNewPassword = tbNewConfirmPass.Texts;

            try
            {
                if (newPassword == confirmNewPassword && DBHelper.Instance.ChangeUserPassword(_AdminUser.Id, currentPassword, newPassword))
                {
                    MessageBox.Show("Password has been changed successfully");

                    tbCurrentPass.Texts = "";
                    tbNewPass.Texts = "";
                    tbNewConfirmPass.Texts = "";
                }
                else
                {
                    MessageBox.Show("Password change failed. Please check your inputs.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    tbCurrentPass.Texts = "";
                    tbNewPass.Texts = "";
                    tbNewConfirmPass.Texts = "";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Toggle show password
        private void toggleShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (toggleShowPass.Checked)
            {
                tbCurrentPass.PasswordChar = false;
                tbNewPass.PasswordChar = false;
                tbNewConfirmPass.PasswordChar = false;
            }
            else
            {
                tbCurrentPass.PasswordChar = true;
                tbNewPass.PasswordChar = true;
                tbNewConfirmPass.PasswordChar = true;
            }
        }

        // Dark mode Button
        private void rbDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            if (rbDarkMode.Checked)
            {
                this.BackColor = Color.FromArgb(28, 29, 29);
                label15.ForeColor = Color.White;
                label23.ForeColor = Color.White;
                label22.ForeColor = Color.White;
                label18.ForeColor = Color.White;
                label12.ForeColor = Color.White;
                label13.ForeColor = Color.White;
                label14.ForeColor = Color.FromArgb(209, 209, 209);
                label19.ForeColor = Color.White;
                label1.ForeColor = Color.White;
                label3.ForeColor = Color.White;
                label5.ForeColor = Color.White;
                label6.ForeColor = Color.White;

                tbTitle.BackColor = Color.FromArgb(89, 89, 89);
                tbAuthor.BackColor = Color.FromArgb(89, 89, 89);
                cbCategory.FillColor = Color.FromArgb(89, 89, 89);
                tbQuantity.BackColor = Color.FromArgb(89, 89, 89);
                tbPrice.BackColor = Color.FromArgb(89, 89, 89);

                tbTitle.ForeColor = Color.White;
                tbAuthor.ForeColor = Color.White;
                cbCategory.ForeColor = Color.White;
                tbQuantity.ForeColor = Color.White;
                tbPrice.ForeColor = Color.White;

                tbNewFirstName.BackColor = Color.FromArgb(89, 89, 89);
                tbNewLastName.BackColor = Color.FromArgb(89, 89, 89);
                tbCurrentPass.BackColor = Color.FromArgb(89, 89, 89);
                tbNewPass.BackColor = Color.FromArgb(89, 89, 89);
                tbNewConfirmPass.BackColor = Color.FromArgb(89, 89, 89);

                tbNewFirstName.ForeColor = Color.White;
                tbNewLastName.ForeColor = Color.White;
                tbCurrentPass.ForeColor = Color.White;
                tbNewPass.ForeColor = Color.White;
                tbNewConfirmPass.ForeColor = Color.White;

                rbLightMode.ForeColor = Color.White;
                rbDarkMode.ForeColor = Color.White;
            }
            else
            {
                this.BackColor = originalFormBackColor;
                label15.ForeColor = originalLabelForeColor;
                label23.ForeColor = originalLabelForeColor;
                label22.ForeColor = originalLabelForeColor;
                label18.ForeColor = originalLabelForeColor;
                label12.ForeColor = originalLabelForeColor;
                label13.ForeColor = originalLabelForeColor;
                label14.ForeColor = Color.DimGray;
                label19.ForeColor = originalLabelForeColor;
                label1.ForeColor = originalLabelForeColor;
                label3.ForeColor = originalLabelForeColor;
                label5.ForeColor = originalLabelForeColor;
                label6.ForeColor = originalLabelForeColor;

                tbTitle.BackColor = originalTextboxBackColor;
                tbAuthor.BackColor = originalTextboxBackColor;
                cbCategory.FillColor = originalTextboxBackColor;
                tbQuantity.BackColor = originalTextboxBackColor;
                tbPrice.BackColor = originalTextboxBackColor;

                tbTitle.ForeColor = originalLabelForeColor;
                tbAuthor.ForeColor = originalLabelForeColor;
                cbCategory.ForeColor = Color.FromArgb(68, 88, 112);
                tbQuantity.ForeColor = originalLabelForeColor;
                tbPrice.ForeColor = originalLabelForeColor;

                tbNewFirstName.BackColor = originalTextboxBackColor;
                tbNewLastName.BackColor = originalTextboxBackColor;
                tbCurrentPass.BackColor = originalTextboxBackColor;
                tbNewPass.BackColor = originalTextboxBackColor;
                tbNewConfirmPass.BackColor = originalTextboxBackColor;

                tbNewFirstName.ForeColor = originalLabelForeColor;
                tbNewLastName.ForeColor = originalLabelForeColor;
                tbCurrentPass.ForeColor = originalLabelForeColor;
                tbNewPass.ForeColor = originalLabelForeColor;
                tbNewConfirmPass.ForeColor = originalLabelForeColor;

                rbLightMode.ForeColor = originalRadioButtonForeColor;
                rbDarkMode.ForeColor = originalRadioButtonForeColor;
            }
        }


        // --------------------------------------------------------------------


        // Logout Button
        private void btnLogout_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Do you want to logout?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Security security = new Security();
                security.Show();
                this.Hide();
            }
        }

        // Close the Application
        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show($"Do you want to close the program?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                Close();
                Application.Exit();
            }
        }
    }
}