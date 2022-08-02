namespace IDCardReader {
    public class CardRecord {
        public string uid { get; set; }
        public string pid { get; set; }
        public object[] properties() {
            return new object[] { uid, pid };
        }
    }
}
