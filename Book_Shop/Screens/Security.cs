using Book_Shop.Entity_Class;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Drawing;
using System.Globalization;
using System.Security.Principal;
using System.Windows.Forms;

namespace Book_Shop
{
    public partial class Security : Form
    {
        public Security()
        {
            InitializeComponent();
            LoadProfilePicture();


            panelReg.Visible = false;
            panelAdmin.Visible = false;
        }

        private void Security_Load(object sender, EventArgs e)
        {
            label1.Parent = background;
            label1.BackColor = Color.Transparent;

            bookIcon.Parent = background;
            bookIcon.BackColor = Color.Transparent;

            label2.Parent = background;
            label2.BackColor = Color.Transparent;

        }

        // Load Profile Picture
        private void LoadProfilePicture()
        {
            DBHelper dbHelper = DBHelper.Instance;
            dbHelper.LoadUserProfilePicture(adminPicture, 1);
        }

        // Clear Inputs in Textboxes
        private void ClearInputs()
        {
            UNameTb.Texts = "";
            UPassTb.Texts = "";

            UAdminPassTb.Texts = "";

            tbFirstName.Texts = "";
            tbLastName.Texts = "";
            tbUsername.Texts = "";
            tbPassword.Texts = "";
            tbConfirmPass.Texts = "";
        }


        // bool for panel visibilty
        private void SetPanelVisibility(bool loginVisible, bool regVisible, bool adminVisible)
        {
            panelLogin.Visible = loginVisible;
            panelReg.Visible = regVisible;
            panelAdmin.Visible = adminVisible;
        }

        // All clickable panels
        private void logAcc_Click(object sender, EventArgs e)
        {
            SetPanelVisibility(true, false, false);
            ClearInputs();
        }
        private void regAcc_Click(object sender, EventArgs e)
        {
            SetPanelVisibility(false, true, false);
            ClearInputs();
        }
        private void btnAdmin_Click(object sender, EventArgs e)
        {
            SetPanelVisibility(false, false, true);
            ClearInputs();
        }
        private void btnbckLogin_Click(object sender, EventArgs e)
        {
            SetPanelVisibility(true, false, false);
            ClearInputs();
        }



        // Login Button and Auth
        private void btnLogin_Click(object sender, EventArgs e)
        {
            var _username = UNameTb.Texts;
            var _password = UPassTb.Texts;

            var User = DBHelper.Instance.AuthenticateUser(_username);

            if (User == null)
            {
                MessageBox.Show("User not found. Please try again.", "Invalid User", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!_password.Equals(User.Password))
            {
                MessageBox.Show("Invalid Credentials. Please try again.", "Invalid Credentials", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var UserPageForm = new User_Page(User);
            UserPageForm.Show();
            this.Hide();
        }



        // Register Button and Confirmations
        private void btnReg_Click(object sender, EventArgs e)
        {
            var _firstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbFirstName.Texts.ToLower());
            var _lastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbLastName.Texts.ToLower());
            var _username = tbUsername.Texts;
            var _password = tbPassword.Texts;
            var _isAdmin = "false";

            if (string.IsNullOrWhiteSpace(tbFirstName.Texts) ||
                string.IsNullOrWhiteSpace(tbLastName.Texts) ||
                string.IsNullOrWhiteSpace(tbUsername.Texts) ||
                string.IsNullOrWhiteSpace(tbPassword.Texts) ||
                string.IsNullOrWhiteSpace(tbConfirmPass.Texts))
            {
                MessageBox.Show("Please input all requirements.");
                return;
            }

            if (tbPassword.Texts != tbConfirmPass.Texts)
            {
                MessageBox.Show("Passwords do not match. Please try again.");
                tbPassword.Texts = "";
                tbConfirmPass.Texts = "";
                return;
            }

            var newUser = new Accounts();
            newUser.Username = _username;
            newUser.Password = _password;
            newUser.FirstName = _firstName;
            newUser.LastName = _lastName;
            newUser.isAdmin = _isAdmin;

            var isUserFound = DBHelper.Instance.GetUserID(newUser.Username);
            if (isUserFound > 0)
            {
                MessageBox.Show("Username already used. Please try again.", "Username Taken", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var insertedUser = DBHelper.Instance.RegisterUser(newUser);
            if (insertedUser == null)
            {
                MessageBox.Show("User failed to create.", "User Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            ClearInputs();
            MessageBox.Show("Registration successful. You can now log in.");
            panelLogin.Visible = true;
            panelReg.Visible = false;
        }


        // Admin Login Button and Auth
        private void btnAdminLogin_Click(object sender, EventArgs e)
        {
            var _password = UAdminPassTb.Texts;

            var User = DBHelper.Instance.AuthenticateUser("Admin");

            if (!_password.Equals(User.Password))
            {
                MessageBox.Show("Wrong Password. Please try again", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                UAdminPassTb.Texts = "";
                return;
            }

            var AdminPageForm = new Admin_Page(User);
            AdminPageForm.Show();
            this.Hide();

        }


        // Toggle show passwords
        private void toggleShowPass_CheckedChanged(object sender, EventArgs e)
        {
            if (toggleShowPass.Checked)
            {
                UPassTb.PasswordChar = false;
            }
            else
            {
                UPassTb.PasswordChar = true;
            }
        }
        private void toggleShowPass2_CheckedChanged(object sender, EventArgs e)
        {
            if (toggleShowPass2.Checked)
            {
                tbPassword.PasswordChar = false;
                tbConfirmPass.PasswordChar = false;
            }
            else
            {
                tbPassword.PasswordChar = true;
                tbConfirmPass.PasswordChar = true;
            }
        }
        private void toggleShowPass3_CheckedChanged(object sender, EventArgs e)
        {
            if (toggleShowPass3.Checked)
            {
                UAdminPassTb.PasswordChar = false;
            }
            else
            {
                UAdminPassTb.PasswordChar = true;
            }
        }


        // Close the Application
        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
            Application.Exit();
        }
    }
}