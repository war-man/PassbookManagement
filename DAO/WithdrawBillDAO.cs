﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DTO;

namespace DAO
{
    public class WithdrawBillDAO
    {
        private static WithdrawBillDAO instance;

        public static WithdrawBillDAO Instance
        {
            get { if (instance == null) instance = new WithdrawBillDAO(); return instance; }
            private set { instance = value; }
        }
        private WithdrawBillDAO() { }

        #region queries
        public bool CheckIfExistBillID(string idBill)
        {

            int result = (int)DataProvider.Instance.ExcuteScarar("select count(*) from dbo.withdrawbill where id='" + idBill+"'");
            if (result != 0)
                return true;
            return false;

        }
        public WithdrawBill GetBill(string id)
        {
            string query = string.Format("select * from dbo.withdrawbill where id = '{0}'", id);
            DataRow row = DataProvider.Instance.ExcuteQuery(query).Rows[0];
            WithdrawBill bill = new WithdrawBill(row);
            return bill;
        }
        public List<WithdrawBill> GetListBill(string cusname, DateTime? date)
        {
            if (date == null)
            {
                //get list by cusname
                //truy van gan dung voi %cusname%
                List<WithdrawBill> list = new List<WithdrawBill>();
                string query = "select * from dbo.withdrawbill where withdraw_passbook in(select passbook.id from dbo.passbook, dbo.customer where passbook.passbook_customer=customer.id and cus_name like '%" + cusname + "%')";
                DataTable data = DataProvider.Instance.ExcuteQuery(query);
                foreach (DataRow item in data.Rows)
                {
                    WithdrawBill result = new WithdrawBill(item);
                    list.Add(result);
                }
                return list;
            }
            else
            {
                //get list by cus name and transaction date
                //truy van gan dung voi %cusname%
                List<WithdrawBill> list = new List<WithdrawBill>();
                DateTime date1 = (DateTime)date;
                int day = date1.Day;
                int month = date1.Month;
                int year = date1.Year;
                string query = "select * from dbo.withdrawbill where withdraw_passbook in(select passbook.id from dbo.passbook, dbo.customer where passbook.passbook_customer=customer.id and cus_name like '%" + cusname + "%') and day(withdrawdate)=" + day + " and month(withdrawdate)=" + month + " and year(withdrawdate)=" + year;
                DataTable data = DataProvider.Instance.ExcuteQuery(query);
                foreach (DataRow item in data.Rows)
                {
                    WithdrawBill result = new WithdrawBill(item);
                    list.Add(result);
                }
                return list;
            }
        }
        public void InsertWithdrawBill(WithdrawBill bill)
        {
            int passbook = bill.Withdraw_passbook;
            long money = bill.Withdrawmoney;
            DateTime? date = bill.Withdrawdate;
            int id = int.Parse(bill.Id.ToString());
            if (date != null)
            {
                string query = string.Format("usp_Insertwithdrawbill {0} , {1}, {2} , {3}", id, passbook, money, "'" + date.Value.ToString("yyyy/MM/dd") + "'");
                DataProvider.Instance.ExcuteNonQuery(query);
            }
            else
            {

            }
        }
        public string GetLastBillID()
        {
            string billid = (string)DataProvider.Instance.ExcuteScarar("select top 1 id from dbo.withdrawbill order by id desc");
            return billid;
        }
        #endregion
    }
}
