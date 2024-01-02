using Book_Shop.Entity_Class;
using CreditCardValidator;
using System.Text.RegularExpressions;
using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Printing;
using System.Linq;
using System.Collections.Generic;
using ZXing;
using ZXing.Common;
using System.Text;
using ZXing.QrCode;
using static Guna.UI2.Native.WinApi;

namespace Book_Shop
{
    public partial class User_Page : Form
    {
        private string createdAt;
        private string timestamp;

        private string imgLocation = "";
        private string pendingImageLocation = "";
        private string paymentMethod;
        private string maskedPaymentDetails;

        private int totalBooks;
        private double totalPrice;
        private double userChange;

        string transactDate = DateTime.Now.ToString();


        private int xpos = 500;
        private int ypos = 800;

        private Color originalFormBackColor;
        private Color originalLabelForeColor;
        private Color originalTextboxBackColor;
        private Color originalRadioButtonForeColor;
        private Color originalTextboxForeColor;

        private Accounts _currentUser;

        public User_Page(Accounts account)
        {
            InitializeComponent();
            Customize();
            _currentUser = account;
        }

        private void User_Page_Load(object sender, EventArgs e)
        {
            label2.Parent = sidePanel2;
            label2.BackColor = Color.Transparent;

            bookIcon.Parent = sidePanel2;
            bookIcon.BackColor = Color.Transparent;

            lblFullName.Text = $"{_currentUser.FirstName} {_currentUser.LastName}";

            pnlHome.Visible = true;
            pnlBooks.Visible = false;
            pnlSettings.Visible = false;
            pnlPurchase.Visible = false;

            pnlHomeEdit.Visible = true;

            originalFormBackColor = this.BackColor;
            originalLabelForeColor = label15.ForeColor;
            originalTextboxBackColor = tbNewQty.BackColor;
            originalRadioButtonForeColor = rbLightMode.ForeColor;
            originalTextboxForeColor = tbNewQty.ForeColor;

            populate();
            CreateCart();
            LoadUserProfilePicture();
            PopulateCart();
            UpdateLabels();
        }


        // Creates the user's cart
        private void CreateCart()
        {
            createdAt = DateTime.Now.ToString();
            timestamp = DateTime.Now.ToString();

            bool isUserCartTaken = DBHelper.Instance.IsUserCartTaken(_currentUser.Id);
            if (!isUserCartTaken)
            {
                DBHelper.Instance.createCart(_currentUser.Id, createdAt, timestamp);
            }
        }


        // custom designs
        private void Customize()
        {
            Font columnHeaderFont1 = new Font("Segoe UI", 14, FontStyle.Bold);
            Font columnHeaderFont2 = new Font("Segoe UI", 16, FontStyle.Bold);

            Font rowsFont = new Font("Segoe UI", 12, FontStyle.Regular);

            //CartDGV
            CartDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Orange;
            CartDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont1;

            CartDGV.RowsDefaultCellStyle.Font = rowsFont;
            CartDGV.RowsDefaultCellStyle.Font = rowsFont;
            CartDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;

            //BookDGV
            BookDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Orange;
            BookDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont2;

            BookDGV.RowsDefaultCellStyle.Font = rowsFont;
            BookDGV.RowsDefaultCellStyle.Font = rowsFont;
            BookDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;

            //PaymentCartDGV
            PaymentCartDGV.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.Orange;
            PaymentCartDGV.ColumnHeadersDefaultCellStyle.Font = columnHeaderFont2;

            PaymentCartDGV.RowsDefaultCellStyle.Font = rowsFont;
            PaymentCartDGV.RowsDefaultCellStyle.Font = rowsFont;
            PaymentCartDGV.AlternatingRowsDefaultCellStyle.Font = rowsFont;
        }

        // Update the Labels
        private void UpdateLabels()
        {
            DBHelper.Instance.UpdateLabels(_currentUser.Id, out totalBooks, out totalPrice);

            lblTotalBooks.Text = $"Total Books in Cart: {totalBooks}";
            lblTotalPrice.Text = $"Total Price to Pay: ₱{totalPrice}";

            lblTotalPaymentBooks.Text = $"Total Books: {totalBooks}";
            lblTotalPaymentPrice.Text = $"Total Price to Pay: ₱{totalPrice}";
        }

        // Display user's cart
        private void PopulateCart()
        {
            DBHelper.Instance.PopulateUserCartOrDisplayInPayment(CartDGV, _currentUser.Id, false);
            DBHelper.Instance.PopulateUserCartOrDisplayInPayment(PaymentCartDGV, _currentUser.Id, true);
        }

        // Display list of books
        private void populate()
        {
            DBHelper.Instance.PopulateBooksData(BookDGV);
        }



        // ---------------- PANEL BUTTONS -------------------------------------

        private void ShowPanel(Panel panelToShow)
        {
            pnlHome.Visible = panelToShow == pnlHome;
            pnlBooks.Visible = panelToShow == pnlBooks;
            pnlSettings.Visible = panelToShow == pnlSettings;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlHome);
        }
        private void btnBooks_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlBooks);
        }
        private void btnSettings_Click(object sender, EventArgs e)
        {
            ShowPanel(pnlSettings);
        }



        // ---------------- HOME PAGE -------------------------------------


        // Edit Button
        private void btnEditQty_Click(object sender, EventArgs e)
        {
            try
            {
                if (CartDGV.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = CartDGV.SelectedRows[0];
                    string bookTitle = selectedRow.Cells["Title"].Value.ToString();

                    if (int.TryParse(tbNewQty.Texts, out int newQuantity) && newQuantity >= 0)
                    {
                        bool editQty = DBHelper.Instance.UpdateCartQuantity(bookTitle, _currentUser.Id, newQuantity);
                        if (!editQty)
                        {
                            MessageBox.Show("Not enough stocks. Please input again.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        MessageBox.Show("Quantity updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        PopulateCart();
                        UpdateLabels();
                        DBHelper.Instance.updateCart(_currentUser.Id);
                        tbNewQty.Texts = "";
                    }
                    else
                    {
                        MessageBox.Show("Please enter a valid quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a book from the cart to edit its quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete Button
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (CartDGV.SelectedRows.Count == 0)
                {
                    MessageBox.Show("Please select a row to remove from your order.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string selectedTitle = CartDGV.SelectedRows[0].Cells["Title"].Value.ToString();

                DialogResult result = MessageBox.Show("Do you want to remove the selected book from your order?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    DBHelper.Instance.RemoveBookFromCart(_currentUser.Id, selectedTitle);

                    MessageBox.Show("The selected book has been removed from your order.");
                    PopulateCart();
                    UpdateLabels();
                    DBHelper.Instance.updateCart(_currentUser.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear Cart Button
        private void btnClearOrder_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to clear your order?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                DBHelper.Instance.ClearUserCart(_currentUser.Id);
                MessageBox.Show("Your order has been cleared.");
                PopulateCart();
                UpdateLabels();
                DBHelper.Instance.updateCart(_currentUser.Id);
            }
        }

        // Order Button
        private void btnOrder_Click(object sender, EventArgs e)
        {
            try
            {
                if (CartDGV.Rows.Count == 0)
                {
                    MessageBox.Show("Your cart is empty. Add items to your cart before placing an order.", "Empty Cart", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    pnlPurchase.Visible = true;
                    pnlHome.Visible = false;
                    tbPaymentDetails.Enabled = false;
                    tbUserPayment.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Cancel Button
        private void btnCancelPayment_Click(object sender, EventArgs e)
        {
            pnlHome.Visible = true;
            pnlPurchase.Visible = false;
            rbCash.Checked = false;
            rbCard.Checked = false;
            rbGCash.Checked = false;
            rbPayMaya.Checked = false;

            tbUserPayment.Enabled = false;
            tbPaymentDetails.Enabled = false;

            tbUserPayment.Texts = "";
            tbPaymentDetails.Texts = "";

        }

        // Payment Option
        private void rbCash_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCash.Checked)
            {
                paymentMethod = "Cash";
                tbPaymentDetails.Enabled = false;
                tbUserPayment.Enabled = true;

                tbPaymentDetails.Texts = "";
                tbUserPayment.Texts = "";

                if (rbDarkMode.Checked)
                {
                    tbPaymentDetails.BackColor = Color.FromArgb(51, 51, 51);
                    tbUserPayment.BackColor = Color.FromArgb(89, 89, 89);
                }
                else
                {
                    tbPaymentDetails.BackColor = Color.Gainsboro;
                }
            }
            else
            {
                tbPaymentDetails.BackColor = originalTextboxBackColor;
            }
        }
        private void rbCard_CheckedChanged(object sender, EventArgs e)
        {
            if (rbCard.Checked)
            {
                double cardTotalPrice = totalPrice;

                tbPaymentDetails.Enabled = true;
                tbUserPayment.Enabled = false;

                tbUserPayment.BackColor = Color.Gainsboro;
                tbUserPayment.Texts = $"{cardTotalPrice}";

                if (rbDarkMode.Checked)
                {
                    tbPaymentDetails.BackColor = Color.FromArgb(89, 89, 89);
                    tbUserPayment.BackColor = Color.FromArgb(51, 51, 51);
                }
            }
            else
            {
                tbUserPayment.BackColor = originalTextboxBackColor;
            }
        }
        private void rbGCash_CheckedChanged(object sender, EventArgs e)
        {
            if (rbGCash.Checked)
            {
                double cardTotalPrice = totalPrice;

                tbPaymentDetails.Enabled = true;
                tbUserPayment.Enabled = false;

                tbUserPayment.BackColor = Color.Gainsboro;
                tbUserPayment.Texts = $"{cardTotalPrice}";

                if (rbDarkMode.Checked)
                {
                    tbPaymentDetails.BackColor = Color.FromArgb(89, 89, 89);
                    tbUserPayment.BackColor = Color.FromArgb(51, 51, 51);
                }
            }
            else
            {
                tbUserPayment.BackColor = originalTextboxBackColor;
            }
        }
        private void rbPayMaya_CheckedChanged(object sender, EventArgs e)
        {
            if (rbPayMaya.Checked)
            {
                double cardTotalPrice = totalPrice;

                tbPaymentDetails.Enabled = true;
                tbUserPayment.Enabled = false;

                tbUserPayment.BackColor = Color.Gainsboro;
                tbUserPayment.Texts = $"{cardTotalPrice}";

                if (rbDarkMode.Checked)
                {
                    tbPaymentDetails.BackColor = Color.FromArgb(89, 89, 89);
                    tbUserPayment.BackColor = Color.FromArgb(51, 51, 51);
                }
            }
            else
            {
                tbUserPayment.BackColor = originalTextboxBackColor;
            }
        }

        // Purchase Button
        private void tbUserPurchase_Click(object sender, EventArgs e)
        {
            try
            {
                if(string.IsNullOrEmpty(tbPaymentDetails.Texts) && string.IsNullOrEmpty(tbUserPayment.Texts))
                {
                    MessageBox.Show("Missing inputs. Please try again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }


                if (!double.TryParse(tbUserPayment.Texts, out double userPayment) || userPayment <= 0)
                {
                    MessageBox.Show("Please enter a valid number.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (rbCard.Checked)
                {
                    string payment = CardPaymentDetails();
                    if (payment == null)
                    {
                        MessageBox.Show("Card Invalid. Please try again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                    paymentMethod = $"Card - {payment}";
                }

                if (rbGCash.Checked || rbPayMaya.Checked)
                {
                    string selectedPaymentMethod = rbGCash.Checked ? "GCash" : "PayMaya";
                    paymentMethod = selectedPaymentMethod;
                    bool payment = SimPaymentDetails();
                    if (!payment)
                    {
                        MessageBox.Show($"{selectedPaymentMethod} Number Invalid. Please try again", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                double totalPrice = DBHelper.Instance.GetTotalPrice(_currentUser.Id);
                int totalBooks = DBHelper.Instance.GetTotalBooks(_currentUser.Id);

                if (userPayment >= totalPrice)
                {
                    string book = (totalBooks > 1) ? "books" : "book";

                    DialogResult result = MessageBox.Show($"Do you want to purchase {totalBooks} {book} for ₱{totalPrice}?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {

                        userChange = userPayment - totalPrice;
                        userChange = Math.Round(userChange,2);



                        if (userChange > 0)
                        {
                            MessageBox.Show($"Your change is: ₱{userChange:F2}.\nReceipt is now available.\n\nThank you for your purchase!", "Purchase Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                        else
                        {
                            MessageBox.Show("Receipt is now available.\n\nThank you for your purchase!", "Purchase Confirmation", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }


                        var transact = new Transactions();
                        transact.user_ID = _currentUser.Id;
                        transact.Method = paymentMethod;
                        transact.BooksQty = totalBooks;
                        transact.Total = totalPrice;
                        transact.Created_at = transactDate;
                        DBHelper.Instance.updateTransactions(transact);

                        printDocument1.DefaultPageSettings.PaperSize = new PaperSize("pprnm", xpos, ypos);
                        using (PrintPreviewDialog printPreviewDialog1 = new PrintPreviewDialog())
                        {
                            printPreviewDialog1.ClientSize = new Size(500, 800);
                            printPreviewDialog1.StartPosition = FormStartPosition.CenterScreen;
                            printPreviewDialog1.Document = printDocument1;
                            if (printPreviewDialog1.ShowDialog() == DialogResult.OK)
                            {
                                printDocument1.Print();
                            }
                        }


                        DBHelper.Instance.UpdateAdminIncome(totalPrice);
                        DBHelper.Instance.ClearUserCart(_currentUser.Id);
                        DBHelper.Instance.UpdateBookQuantities(CartDGV);
                        PopulateCart();
                        UpdateLabels();
                        populate();
                        DBHelper.Instance.updateCart(_currentUser.Id);

                        pnlHome.Visible = true;
                        pnlPurchase.Visible = false;

                        rbCash.Checked = false;
                        rbCard.Checked = false;
                        rbGCash.Checked = false;
                        rbPayMaya.Checked = false;

                        tbPaymentDetails.Texts = "";
                        tbUserPayment.Texts = "";

                    }
                }
                else
                {
                    MessageBox.Show("Sorry, insufficient amount of payment.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Checks if the card is valid
        private string CardPaymentDetails()
        {
            string paymentDetails;
            if (string.IsNullOrWhiteSpace(tbPaymentDetails.Texts) || Regex.IsMatch(tbPaymentDetails.Texts, @"[\p{L}\p{S}]"))
            {
                return null;
            }
            else
            {
                paymentDetails = tbPaymentDetails.Texts;
            }

            string cardNumber = paymentDetails;
            CreditCardDetector detector = new CreditCardDetector(cardNumber);

            bool cardValid = detector.IsValid();
            if (!cardValid)
            {
                return null;
            }

            return detector.BrandName;
        }

        // Checks if the number is valid
        private bool SimPaymentDetails()
        {
            string simNumber = tbPaymentDetails.Texts;
            string simPattern = @"^09\d{9}$";

            if (Regex.IsMatch(simNumber, simPattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Prints a receipt
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            decimal.TryParse(tbUserPayment.Texts, out decimal userPayment);
            var transactId = DBHelper.Instance.getTransactId(_currentUser.Id, transactDate);
            int titleWidth = 35;

            e.Graphics.DrawString("BOOKWISE", new Font("Century Gothic", 14, FontStyle.Bold), Brushes.Black, new Point(xpos / 2 - 50, 7));
            e.Graphics.DrawString("Sale Receipt", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(xpos / 2 - 53, 25));

            for (int x = -3; x < 500; x += 1)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, 37));
            }

            e.Graphics.DrawString("Order Id: " + transactId.Id, new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, 70));
            e.Graphics.DrawString($"Name: {_currentUser.FirstName} {_currentUser.LastName}", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, 90));
            e.Graphics.DrawString("Date: " + transactDate, new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, 115));

            for (int x = 10; x < 490; x += 10)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, 130));
            }

            e.Graphics.DrawString("Title", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, 150));
            e.Graphics.DrawString("Price", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(270, 150));
            e.Graphics.DrawString("Amount", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(335, 150));
            e.Graphics.DrawString("Total", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(410, 150));

            for (int x = 10; x < 490; x += 10)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, 160));
            }

            DataTable cartDataTable = DBHelper.Instance.PopulateUserCart(_currentUser.Id);
            int pos = 185;

            int titleLineHeight = 20;

            foreach (DataRow row in cartDataTable.Rows)
            {
                string title = row["Title"].ToString();
                decimal price = Convert.ToDecimal(row["Price"]);
                int amount = Convert.ToInt32(row["Amount"]);
                decimal total = Convert.ToDecimal(row["Total"]);

                string[] titleLines = SplitTitle(title, titleWidth);

                for (int i = 0; i < titleLines.Length; i++)
                {
                    if (i == 0)
                    {
                        e.Graphics.DrawString(price.ToString(), new Font("Century Gothic", 11), Brushes.Black, new Point(270, pos));
                        e.Graphics.DrawString(amount.ToString(), new Font("Century Gothic", 11), Brushes.Black, new Point(355, pos));
                        e.Graphics.DrawString(total.ToString(), new Font("Century Gothic", 11), Brushes.Black, new Point(410, pos));
                    }

                    e.Graphics.DrawString(titleLines[i], new Font("Century Gothic", 11, FontStyle.Bold), Brushes.Black, new Point(20, pos));
                    pos += titleLineHeight;
                }
                pos += 10;
            }

            for (int x = 10; x < 490; x += 10)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, pos));
            }

            decimal totalPrice = cartDataTable.AsEnumerable().Sum(row => row.Field<decimal>("Total"));

            StringFormat rightAlignFormat = new StringFormat();
            rightAlignFormat.Alignment = StringAlignment.Far;

            e.Graphics.DrawString("Total Price: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
            e.Graphics.DrawString($"₱{totalPrice}", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);

            e.Graphics.DrawString("Cash Tendered: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
            e.Graphics.DrawString($"₱{userPayment}", new Font("Century Gothic", 12), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);

            e.Graphics.DrawString("Change: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
            e.Graphics.DrawString($"₱{userChange}", new Font("Century Gothic", 12), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);
            pos += 20;

            for (int x = 10; x < 490; x += 10)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, pos));
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, pos + 8));
            }

            if (rbCash.Checked)
            {
                e.Graphics.DrawString("Payment Method: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 30));
                e.Graphics.DrawString(paymentMethod, new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);
            }

            string fullPaymentDetails = tbPaymentDetails.Texts;
            if (rbGCash.Checked || rbPayMaya.Checked)
            {
                maskedPaymentDetails = new string('⁎', fullPaymentDetails.Length - 4) + fullPaymentDetails.Substring(fullPaymentDetails.Length - 4);
                e.Graphics.DrawString("Payment Method: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 30));
                e.Graphics.DrawString(paymentMethod, new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);
                e.Graphics.DrawString("GCash Number: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
                e.Graphics.DrawString(maskedPaymentDetails, new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Rectangle(375, pos, 100, 20), rightAlignFormat);

            }

            if (rbCard.Checked)
            {
                string payment = CardPaymentDetails();
                maskedPaymentDetails = new string('⁎', fullPaymentDetails.Length - 4) + fullPaymentDetails.Substring(fullPaymentDetails.Length - 4);

                e.Graphics.DrawString("Payment Method: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 30));

                int rightAlignX = 475;
                int cardX = rightAlignX - (int)e.Graphics.MeasureString("Card", new Font("Century Gothic", 12, FontStyle.Bold)).Width;
                e.Graphics.DrawString("Card", new Font("Century Gothic", 12), Brushes.Black, new Point(cardX, pos));
                e.Graphics.DrawString("Card Type: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
                int paymentX = rightAlignX - (int)e.Graphics.MeasureString(payment, new Font("Century Gothic", 12, FontStyle.Bold)).Width;
                e.Graphics.DrawString(payment, new Font("Century Gothic", 12), Brushes.Black, new Point(paymentX, pos));
                e.Graphics.DrawString("Card Number: ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(20, pos += 20));
                int maskedPaymentX = rightAlignX - (int)e.Graphics.MeasureString(maskedPaymentDetails, new Font("Century Gothic", 12)).Width;
                e.Graphics.DrawString(maskedPaymentDetails, new Font("Century Gothic", 12), Brushes.Black, new Point(maskedPaymentX, pos));
            }

            pos += 20;

            for (int x = 10; x < 490; x += 10)
            {
                e.Graphics.DrawString("-", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(x, pos));
            }

            e.Graphics.DrawString("Thank you for Shopping! ", new Font("Century Gothic", 12, FontStyle.Bold), Brushes.Black, new Point(xpos/2 - 100, pos += 25));

            string paymentDetails;
            if (tbPaymentDetails.Texts == "")
            {
                paymentDetails = "None";
            }
            else
            {
                paymentDetails = tbPaymentDetails.Texts;
            }


            string bookDetails = GetBookDetails(cartDataTable);

            string receiptInfo = "Order Id: " + transactId.Id + Environment.NewLine +
                                "Name: " + _currentUser.FirstName + " " + _currentUser.LastName + Environment.NewLine +
                                "Date: " + transactDate + Environment.NewLine +
                                "\nTotal Price: " + totalPrice + Environment.NewLine +
                                "Cash Tendered: " + userPayment + Environment.NewLine +
                                "Change: " + userChange + Environment.NewLine +
                                "\nPayment Method: " + paymentMethod + Environment.NewLine +
                                "Payment Details: " + maskedPaymentDetails + Environment.NewLine +
                                "\nBooks Purchased:\n" + Environment.NewLine + bookDetails;

            using (Bitmap qrCode = GenerateQRCode(receiptInfo))
            {
                e.Graphics.DrawImage(qrCode, new Point(xpos/2 - 100, pos + 23));
            }
        }
        private string[] SplitTitle(string title, int maxLength)
        {
            List<string> lines = new List<string>();
            string[] words = title.Split(' ');

            string currentLine = string.Empty;

            foreach (string word in words)
            {
                if ((currentLine + word).Length <= maxLength)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                        currentLine += " ";
                    currentLine += word;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = word;
                }
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines.ToArray();
        }
        private string GetBookDetails(DataTable cartDataTable)
        {
            StringBuilder bookDetails = new StringBuilder();

            foreach (DataRow row in cartDataTable.Rows)
            {
                string title = row["Title"].ToString();
                decimal price = Convert.ToDecimal(row["Price"]);
                int amount = Convert.ToInt32(row["Amount"]);
                decimal total = Convert.ToDecimal(row["Total"]);

                bookDetails.Append("Title: " + title + Environment.NewLine);
                bookDetails.Append("Price: " + price + Environment.NewLine);
                bookDetails.Append("Amount: " + amount + Environment.NewLine);
                bookDetails.Append("Total: " + total + Environment.NewLine);
                bookDetails.Append(Environment.NewLine);
            }
            return bookDetails.ToString();
        }
        private Bitmap GenerateQRCode(string receiptInfo)
        {
            var barcodeWriter = new BarcodeWriter();
            barcodeWriter.Format = BarcodeFormat.QR_CODE;
            barcodeWriter.Options = new QrCodeEncodingOptions
            {
                Width = 200,
                Height = 200,
            };
            return barcodeWriter.Write(receiptInfo);
        }


        // ---------------- BOOKS PAGE -------------------------------------


        // Book Search Button
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string nameFilterText = tbNameFilter.Texts.Trim();
            string authorFilterText = tbAuthorFilter.Texts.Trim();

            if (BookDGV.DataSource is DataTable dataTable)
            {
                dataTable = DBHelper.Instance.GetFilteredBooks(nameFilterText, authorFilterText);

                BookDGV.DataSource = dataTable;
                BookDGV.Refresh();
            }
        }

        // Filter Reset Button
        private void btnFilterReset_Click(object sender, EventArgs e)
        {
            if (BookDGV.DataSource is DataTable dataTable)
            {
                dataTable = DBHelper.Instance.GetFilteredBooks("", "");

                BookDGV.DataSource = dataTable;
                BookDGV.Refresh();

                tbNameFilter.Texts = "";
                tbAuthorFilter.Texts = "";
            }
        }


        // Add to cart Button
        private void btnAddCart_Click(object sender, EventArgs e)
        {
            try
            {
                if (BookDGV.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = BookDGV.SelectedRows[0];
                    if (selectedRow == null)
                    {
                        MessageBox.Show("Unable to retrieve book information.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (TryGetBookDetails(selectedRow, out string title, out string author, out string category, out decimal price))
                    {
                        if (TryGetQuantity(out int amount) && amount > 0)
                        {
                            var booksData = DBHelper.Instance.getBooksData(title);

                            if (booksData.quantity >= amount)
                            {
                                if (DBHelper.Instance.CanAddToCart(_currentUser.Id, title))
                                {
                                    decimal totalPrice = price * amount;

                                    Carts_Item cartItem = new Carts_Item
                                    {
                                        userId = _currentUser.Id,
                                        title = title,
                                        author = author,
                                        category = category,
                                        price = price,
                                        amount = amount,
                                        total = totalPrice
                                    };

                                    if (DBHelper.Instance.InsertBookIntoCart(cartItem))
                                    {
                                        MessageBox.Show("Book added to cart successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                        tbAmount.Texts = "";
                                        PopulateCart();
                                        populate();
                                        UpdateLabels();
                                        DBHelper.Instance.updateCart(_currentUser.Id);

                                    }
                                    else
                                    {
                                        MessageBox.Show("Failed to add the book to the cart.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    MessageBox.Show("This book is already in your cart.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                }
                            }
                            else
                            {
                                MessageBox.Show("Sorry, there are not enough books in stock to fulfill your request.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            }
                        }
                        else
                        {
                            MessageBox.Show("Please enter a valid quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Failed to get book details.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a book to add to the cart.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private bool TryGetBookDetails(DataGridViewRow row, out string title, out string author, out string category, out decimal price)
        {
            title = author = category = string.Empty;
            price = 0;

            if (row != null)
            {
                title = row.Cells["Title"].Value?.ToString();
                author = row.Cells["Author"].Value?.ToString();
                category = row.Cells["Category"].Value?.ToString();

                if (decimal.TryParse(row.Cells["Price"].Value?.ToString(), out price))
                {
                    return true;
                }
            }
            return false;
        }
        private bool TryGetQuantity(out int Amount)
        {
            return int.TryParse(tbAmount.Texts, out Amount) && Amount > 0;
        }



        // ---------------- SETTINGS PAGE -------------------------------------

        // Loads User Profile Picture
        private void LoadUserProfilePicture()
        {
            DBHelper dbHelper = DBHelper.Instance;
            dbHelper.LoadUserProfilePicture(ProfilePicture, _currentUser.Id);
            dbHelper.LoadUserProfilePicture(ProfilePicture1, _currentUser.Id);
        }

        // Browse Button
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

        // Save Button
        private void btnImageSave_Click(object sender, EventArgs e)
        {
            var imageSave = DBHelper.Instance.UpdateUserProfilePicture(_currentUser.Id, pendingImageLocation);

            if (!imageSave)
            {
                MessageBox.Show("Failed to upload the image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            MessageBox.Show("Image successfully uploaded.");
            LoadUserProfilePicture();
        }

        // Image Reset Button
        private void btnImageReset_Click(object sender, EventArgs e)
        {
            try
            {
                Image userProfilePicture = DBHelper.Instance.GetUserProfilePicture(_currentUser.Id);
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
            DialogResult result = MessageBox.Show("Do you want to remove your profile picture?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {

                if (DBHelper.Instance.RemoveProfilePicture(_currentUser.Id))
                {
                    string defaultImagePath = Path.Combine(Application.StartupPath, "Assets", "userIcon6.png");
                    MessageBox.Show("Picture was successfully removed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadUserProfilePicture();
                }
                else
                {
                    MessageBox.Show("No profile picture is being used.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


        // Info Update Button
        private void btnUpdateInfo_Click(object sender, EventArgs e)
        {
            string newUsername = tbNewUsername.Texts;
            string newFirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbNewFirstName.Texts.ToLower());
            string newLastName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(tbNewLastName.Texts.ToLower());

            if (string.IsNullOrWhiteSpace(newUsername) || string.IsNullOrWhiteSpace(newFirstName) || string.IsNullOrWhiteSpace(newLastName))
            {
                MessageBox.Show("Please fill in all fields.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            bool success = DBHelper.Instance.UpdateUserInfo(_currentUser.Id, newUsername, newFirstName, newLastName);

            if (success)
            {
                string FirstName = newFirstName;
                string LastName = newLastName;

                MessageBox.Show("User information updated successfully", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                lblFullName.Text = $"{FirstName} {LastName}";

                tbNewFirstName.Texts = "";
                tbNewLastName.Texts = "";
                tbNewUsername.Texts = "";
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
                if (newPassword == confirmNewPassword && DBHelper.Instance.ChangeUserPassword(_currentUser.Id, currentPassword, newPassword))
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

        // Account Delete Button
        private void btnAccDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete your account?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                bool accDelete = DBHelper.Instance.DeleteAccount(_currentUser.Id);
                if (accDelete)
                {
                    MessageBox.Show("Your account has been deleted successfully.");
                    Security security = new Security();
                    security.Show();
                    this.Hide();
                }
                else
                {
                    MessageBox.Show("Failed to delete your account.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                label8.ForeColor = Color.White;
                label9.ForeColor = Color.White;
                label10.ForeColor = Color.White;
                label11.ForeColor = Color.White;
                label12.ForeColor = Color.White;
                label13.ForeColor = Color.White;
                label14.ForeColor = Color.FromArgb(209, 209, 209);
                label17.ForeColor = Color.White;
                label19.ForeColor = Color.White;
                label20.ForeColor = Color.White;

                tbNewQty.BackColor = Color.FromArgb(89, 89, 89);
                tbUserPayment.BackColor = Color.FromArgb(89, 89, 89);
                tbNameFilter.BackColor = Color.FromArgb(89, 89, 89);
                tbAuthorFilter.BackColor = Color.FromArgb(89, 89, 89);
                tbAmount.BackColor = Color.FromArgb(89, 89, 89);

                tbNewQty.ForeColor = Color.White;
                tbUserPayment.ForeColor = Color.White;
                tbNameFilter.ForeColor = Color.White;
                tbAuthorFilter.ForeColor = Color.White;
                tbAmount.ForeColor = Color.White;

                rbLightMode.ForeColor = Color.White;
                rbDarkMode.ForeColor = Color.White;
                tbNewFirstName.BackColor = Color.FromArgb(89, 89, 89);
                tbNewLastName.BackColor = Color.FromArgb(89, 89, 89);
                tbNewUsername.BackColor = Color.FromArgb(89, 89, 89);
                tbCurrentPass.BackColor = Color.FromArgb(89, 89, 89);
                tbNewPass.BackColor = Color.FromArgb(89, 89, 89);
                tbNewConfirmPass.BackColor = Color.FromArgb(89, 89, 89);
                tbPaymentDetails.BackColor = Color.FromArgb(89, 89, 89);

                tbNewFirstName.ForeColor = Color.White;
                tbNewLastName.ForeColor = Color.White;
                tbNewUsername.ForeColor = Color.White;
                tbCurrentPass.ForeColor = Color.White;
                tbNewPass.ForeColor = Color.White;
                tbNewConfirmPass.ForeColor = Color.White;
                tbPaymentDetails.ForeColor = Color.White;

                rbCash.ForeColor = Color.White;
                rbCard.ForeColor = Color.White;
                rbGCash.ForeColor = Color.White;
                rbPayMaya.ForeColor = Color.White;

            }
            else
            {
                this.BackColor = originalFormBackColor;
                label15.ForeColor = originalLabelForeColor;
                label8.ForeColor = originalLabelForeColor;
                label9.ForeColor = originalLabelForeColor;
                label10.ForeColor = originalLabelForeColor;
                label11.ForeColor = originalLabelForeColor;
                label12.ForeColor = originalLabelForeColor;
                label13.ForeColor = originalLabelForeColor;
                label14.ForeColor = Color.DimGray;
                label17.ForeColor = originalLabelForeColor;
                label19.ForeColor = originalLabelForeColor;
                label20.ForeColor = originalLabelForeColor;

                tbNewQty.BackColor = originalTextboxBackColor;
                tbUserPayment.BackColor = originalTextboxBackColor;
                tbNameFilter.BackColor = originalTextboxBackColor;
                tbAuthorFilter.BackColor = originalTextboxBackColor;
                tbAmount.BackColor = originalTextboxBackColor;

                tbNewQty.ForeColor = originalTextboxForeColor;
                tbUserPayment.ForeColor = originalTextboxForeColor;
                tbNameFilter.ForeColor = originalTextboxForeColor;
                tbAuthorFilter.ForeColor = originalTextboxForeColor;
                tbAmount.ForeColor = originalTextboxForeColor;

                rbLightMode.ForeColor = originalRadioButtonForeColor;
                rbDarkMode.ForeColor = originalRadioButtonForeColor;
                tbNewFirstName.BackColor = originalTextboxBackColor;
                tbNewLastName.BackColor = originalTextboxBackColor;
                tbNewUsername.BackColor = originalTextboxBackColor;
                tbCurrentPass.BackColor = originalTextboxBackColor;
                tbNewPass.BackColor = originalTextboxBackColor;
                tbNewConfirmPass.BackColor = originalTextboxBackColor;
                tbPaymentDetails.BackColor = originalTextboxBackColor;

                tbNewFirstName.ForeColor = originalTextboxForeColor;
                tbNewLastName.ForeColor = originalTextboxForeColor;
                tbNewUsername.ForeColor = originalTextboxForeColor;
                tbCurrentPass.ForeColor = originalTextboxForeColor;
                tbNewPass.ForeColor = originalTextboxForeColor;
                tbNewConfirmPass.ForeColor = originalTextboxForeColor;
                tbPaymentDetails.ForeColor = originalLabelForeColor;


                rbCash.ForeColor = originalLabelForeColor;
                rbCard.ForeColor = originalLabelForeColor;
                rbGCash.ForeColor = originalLabelForeColor;
                rbPayMaya.ForeColor = originalLabelForeColor;
            }
        }


        // --------------------------------------------------------------------


        // Logout user
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