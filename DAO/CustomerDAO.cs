﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace DAO
{
    public class CustomerDAO
    {
        private static CustomerDAO instance;

        public static CustomerDAO Instance
        {
            get { if (instance == null) instance = new CustomerDAO(); return instance; }
            private set { instance = value; }
        }
        private CustomerDAO() { }
        #region insert
        public void InsertCustomer(Customer cus)
        {
            string cmnd = cus.Cmnd;
            string name = cus.Cus_name;
            string address = cus.Cus_address;
            DataProvider.Instance.ExcuteNonQuery("usp_InsertCustomer @cmnd , @name , @address", new object[] { cmnd, name, address });
            //cmnd,name,address
        }
        #endregion

        #region update
        public void UpdateCustomer(Customer cus)
        {
            int id = cus.Id;
            string name = cus.Cus_name;
            string address = cus.Cus_address;
            string cmnd = cus.Cmnd;
            DataProvider.Instance.ExcuteNonQuery("usp_Update_cus @id , @name , @address , @cmnd ", new object[] { id, name, address, cmnd });
        }
        #endregion

        #region queries
        public int GetCurrentMaxCustomerID()
        {
            int value = 0;
            try
            {
                value = (int)DataProvider.Instance.ExcuteScarar("select MAX(ID) from dbo.CUSTOMER");
            }
            catch { }
            return value;
        }
        public bool CheckExistID(int ID)
        {
            int result = (int)DataProvider.Instance.ExcuteScarar("select count(*) from dbo.customer where id=" + ID);
            if (result == 0)
                return false;
            return true;
        }
        public string GetCustomerName(int IDCustomer)
        {
  
            string query = "select * from dbo.customer where id='" + IDCustomer + "'";
            DataTable data = DataProvider.Instance.ExcuteQuery(query);
            if (data.Rows.Count > 0)
            {
                Customer cus = new Customer(data.Rows[0]);
                return cus.Cus_name;
            }
            return "";
        }
        public string GetCustomerCardNumber(int IDCustomer)
        {
            DataRow row = DataProvider.Instance.ExcuteQuery("select * from dbo.customer where id=" + IDCustomer).Rows[0];
            Customer result = new Customer(row);
            return result.Cmnd;
        }
        public string GetCustomerAddress(int IDCustomer)
        {
            string query = "select * from dbo.customer where id=" + IDCustomer;
            DataTable data = DataProvider.Instance.ExcuteQuery(query);
            if (data.Rows.Count > 0)
            {
                Customer cus = new Customer(data.Rows[0]);
                return cus.Cus_address;
            }
            return "Khong co ID khach hang";
        }
        public bool CheckCustomerHasAccountType(int customerID , int? TypeID)
        {
            if(TypeID!=null)
            {
                bool check = false;
                string query = "select count(*) from dbo.passbook, dbo.typepassbook where passbook.passbook_customer= " + customerID + " and passbook.passbook_type=typepassbook.id and typepassbook.id=" + TypeID;
                int result = (int)DataProvider.Instance.ExcuteScarar(query);
                if (result != 0)
                    check = true;
                return check;
            }
            return false;
        }
        public Customer GetCustomer(int IDcustomer)
        {
            string query = string.Format("select * from dbo.customer where id = {0}", IDcustomer);
            DataRow row = DataProvider.Instance.ExcuteQuery(query).Rows[0];
            Customer cus = new Customer(row);
            return cus;
        }
        
        public DataTable GetCusInfoByName(string CusName)
        {
            //tìm kiếm gần đúng %CusName%
            //trả về thông bảng gồm nhiều hàng có các cột được đặt tên STT, CusID, CusName, CusCMND, CusAddress, FinalTransactionDay. (FinalTransactionDay là ngày thực hiện giao dịch cuối cùng của khách hàng)
            string query = "select row_number()over(order by id) STT, id CusID, cus_name CusName, cmnd CusCMND, cus_address CusAddress, dbo.find_date(id) FinalTransactionDay from dbo.customer where cus_name like'%" + CusName + "%'";
            DataTable data = DataProvider.Instance.ExcuteQuery(query);
            return data;
        }
        public DataTable GetCusInfoByCardID(string CusCardID)
        {
            //tìm kiếm chính xác %cardid%
            //trả về thông bảng gồm nhiều hàng có các cột được đặt tên STT, CusID, CusName, CusCMND, CusAddress, FinalTransactionDay. (FinalTransactionDay là ngày thực hiện giao dịch cuối cùng của khách hàng)
            string query = "select row_number()over(order by id) STT, id CusID, cus_name CusName, cmnd CusCMND, cus_address CusAddress, dbo.find_date(id) FinalTransactionDay from dbo.customer where cmnd like'%" + CusCardID + "%'";
            DataTable data = DataProvider.Instance.ExcuteQuery(query);
            return data;
        }
        public string GetCustomerNameByCollectBillID(string collectBillID)
        {
            string query = "select cus_name from dbo.customer,dbo.passbook, dbo.collectbill where collectbill.id='" + collectBillID + "' and customer.id=  passbook_customer and passbook.id= collect_passbook";
            string result = (string)DataProvider.Instance.ExcuteScarar(query).ToString();
            return result;
        }
        public string GetCustomerNameByWithdrawBillID(string WithdrawID)
        {
            string query = "select cus_name from dbo.customer,dbo.passbook, dbo.withdrawbill where withdrawbill.id='" + WithdrawID + "' and customer.id=  passbook_customer and passbook.id= withdraw_passbook";
            string result = (string)DataProvider.Instance.ExcuteScarar(query).ToString();
            return result;
        }        
        public bool CheckCardIDexist(string CardID)
        {
            string query = "select count(*) from dbo.customer where cmnd= " + CardID;
            int result = (int)DataProvider.Instance.ExcuteScarar(query);
            if (result >0)
                return false;
            else
                return true;
        }
        #endregion
    }
}
