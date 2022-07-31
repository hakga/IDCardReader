using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace IDCardReader {
    public class CardRecord {
        private DataTable data { get; set; }
        public CardRecord() {
            data = new DataTable("CardRecord");
        }
        public DataTable Setup() {
            data.Columns.Add("uid");
            data.Columns.Add("pid");
            return data;
        }
        public void Add(string uid, string pid) {
            data.Rows.Add(uid, pid);
        }
        public void Add(CardFields record) {
            data.Rows.Add(record.uid, record.pid);
        }
    }
}
