using System.Data;
using System.Linq;

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
            if ( _dt.AsEnumerable().Any(r => r.Field<string>(0) == record.properties()[0].ToString()) == false) {
                _dt.Rows.Add(record.properties());
            }
        }
        public void UpdateDB() {
            throw new System.NotImplementedException();
        }
    }
}
