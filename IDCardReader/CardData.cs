using System.Data;

namespace IDCardReader {
    public class CardData {
        private DataTable _dt;
        public CardData() {
            _dt = new DataTable("CardRecord");
        }
        public DataTable Setup() {
            foreach ( var property in typeof(CardRecord).GetProperties()) {
                _dt.Columns.Add(property.Name, property.PropertyType);
            }
            return _dt;
        }
        public void Add(CardRecord record) {
//         dt.Rows.Add(record.uid, record.pid);
            _dt.Rows.Add(record.properties());
        }
    }
}
