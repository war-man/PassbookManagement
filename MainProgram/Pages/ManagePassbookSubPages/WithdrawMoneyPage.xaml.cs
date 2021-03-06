﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MainProgram.CustomControls;
using DTO;
using DAO;
using MainProgram.Converter;
using System.Reflection;

namespace MainProgram.Pages.ManagePassbookSubPages
{
    /// <summary>
    /// Interaction logic for WithdrawMoneyPage.xaml
    /// </summary>
    public partial class WithdrawMoneyPage : Page
    {
        public WithdrawMoneyPage()
        {
            InitializeComponent();
        }
        #region functions
        private void Clearall()
        {
            this.Txt_CustomerID.Text = null;
            this.Txt_CustomerName.Text = null;
            this.Txt_CustomerCard.Text = null;
            this.Txt_CustomerAddress.Text = null;
            this.Cb_TypePassbook.ItemsSource = null;
            this.Txt_PassbookID.Text = null;
            this.Money.Text = null;
            this.Txt_CustomerID.Clear();
            this.Txt_CustomerName.Clear();
            this.Txt_CustomerCard.Clear();
            this.Txt_CustomerAddress.Clear();
            this.Cb_TypePassbook.Items.Clear();
            this.Txt_PassbookID.Clear();
            this.Money.Clear();
            this.Balance.Text = "Số dư:";
        }
        #endregion

        #region events
        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            int customerID;
            if (int.TryParse(this.Txt_CustomerID.Text, out customerID) == true)
            {
                customerID = int.Parse(this.Txt_CustomerID.Text);
                bool exist_ID = CustomerDAO.Instance.CheckExistID(customerID);
                if (exist_ID)
                {
                    this.Txt_CustomerName.Text = CustomerDAO.Instance.GetCustomerName(customerID);
                    this.Txt_CustomerCard.Text = CustomerDAO.Instance.GetCustomerCardNumber(customerID);
                    this.Txt_CustomerAddress.Text = CustomerDAO.Instance.GetCustomerAddress(customerID);
                    this.DatePicker_Time.SelectedDate = DateTime.Now;
                }
                else
                {
                    MessageBoxCustom.setContent("CustomerID is not available").ShowDialog();
                }
            }
        }

        private void Numberic_Txtbox(object sender, TextCompositionEventArgs e)
        {
            foreach (char ch in e.Text)
                if (!Char.IsDigit(ch))
                    e.Handled = true;
        }
        private void BtnWithdraw_click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.Money.Text))
            {
                MessageBoxCustom.setContent("Thiếu thông tin phiếu gởi!").ShowDialog();
                return;
            }
            else
            {
                if (PassbookDAO.Instance.GetWithdrawday(int.Parse(this.Txt_PassbookID.Text.ToString())) <= this.DatePicker_Time.SelectedDate)
                {
                    if ((this.Cb_TypePassbook.SelectedItem as TypePassbook).Typename != "Không kì hạn")
                    {
                        if (MessageBoxCustom.setContent("Ngày hoàn thành kì hạn chưa tới, Quý Khách có muốn rút?").ShowDialog() == true)
                        {
                            Clearall();
                            return;
                        }
                    }
                }
                else
                {
                    MessageBoxCustom.setContent("Chưa đến được rút, thời hạn rút tiền là 15 ngày").ShowDialog();
                    Clearall();
                    return;

                }
                WithdrawBill bill = new WithdrawBill
                {
                    Id = 1.ToString().Trim(),
                    Withdraw_passbook = int.Parse(this.Txt_PassbookID.Text.ToString()),
                    Withdrawmoney = long.Parse(this.Money.Text.ToString()),
                    Withdrawdate = this.DatePicker_Time.SelectedDate ?? DateTime.Now
                };
                WithdrawBillDAO.Instance.InsertWithdrawBill(bill);
                int id = int.Parse(this.Txt_PassbookID.Text);
                MessageBoxCustom.setContent("Tạo phiếu rút thành công! Số dư còn lại là: " + PassbookDAO.Instance.GetBalancebyIDPassbook(id).ToString()).ShowDialog();
                this.Balance.Text = "Số dư:" + PassbookDAO.Instance.GetBalancebyIDPassbook(id).ToString();
                Clearall();
            }
        }
        private void BtnPrint_Click(object sender, RoutedEventArgs e)
        {
            string name = "Bill" + WithdrawBillDAO.Instance.GetLastBillID() + ".png";
            CaptureUIElement.Instance.SaveFrameworkElementToPng(Panel_Bill, (int)Panel_Bill.ActualWidth, (int)Panel_Bill.ActualHeight, name);
            MessageBoxCustom.setContent("phiếu lưu tại: "+ System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)).ShowDialog();
        }

        private void Txt_CustomerID_LostFocus(object sender, RoutedEventArgs e)
        {
            int customerID;
            if (int.TryParse(this.Txt_CustomerID.Text, out customerID) == true)
            {
                customerID = int.Parse(this.Txt_CustomerID.Text);
                bool exist_ID = CustomerDAO.Instance.CheckExistID(customerID);
                if (exist_ID)
                {

                    this.TextBox_warning_1.Visibility = Visibility.Collapsed;
                    this.Txt_CustomerName.Text = CustomerDAO.Instance.GetCustomerName(customerID);
                    this.Txt_CustomerCard.Text = CustomerDAO.Instance.GetCustomerCardNumber(customerID);
                    this.Txt_CustomerAddress.Text = CustomerDAO.Instance.GetCustomerAddress(customerID);
                    this.Money.Clear();
                    this.Txt_PassbookID.Clear();
                    this.Balance.Text = "Số dư:";
                    this.Cb_TypePassbook.ItemsSource = null;
                    this.Cb_TypePassbook.Items.Clear();
                    this.Cb_TypePassbook.ItemsSource = TypePassbookDAO.Instance.GetListTypeByCusID(customerID);
                    this.Cb_TypePassbook.DisplayMemberPath = "Typename";
                    this.DatePicker_Time.SelectedDate = DateTime.Now;

                }
                else
                {
                    this.TextBox_warning_1.Visibility = Visibility.Visible;
                    MessageBox.Show("Mã khách hàng này không tồn tại!");
                    this.Txt_CustomerID.Clear();
                }
            }
        }

        private void Cb_TypePassbook_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.Cb_TypePassbook.ItemsSource != null)
            {
                ComboBox cb = sender as ComboBox;
                if (cb.SelectedItem != null)
                {
                    TypePassbook type = cb.SelectedItem as TypePassbook;
                    int idcustomer = int.Parse(this.Txt_CustomerID.Text);
                    string name = type.Typename;
                    this.Txt_PassbookID.Text = PassbookDAO.Instance.GetPassbookIDbyCusIDandidType(idcustomer, name).ToString();
                    int id = int.Parse(this.Txt_PassbookID.Text);
                    this.Balance.Text = "Số dư:" + PassbookDAO.Instance.GetBalancebyIDPassbook(id).ToString();
                    if (type.Kind != "Không kì hạn")
                    {
                        this.Money.Text = PassbookDAO.Instance.GetBalancebyIDPassbook(id).ToString();
                        this.Money.IsEnabled = false;
                    }
                    else this.Money.IsEnabled = true;

                }
            }
        }

        private void Txt_CustomerID_GotFocus(object sender, RoutedEventArgs e)
        {
            Clearall();
        }
        #endregion

    }
}
