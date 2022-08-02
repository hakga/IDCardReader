using PCSC;
using PCSC.Iso7816;
using System;
using System.Windows.Forms;

namespace IDCardReader {
    public partial class Form1 : Form {
        public CardData CardRecord;
        public Form1() {
            InitializeComponent();
            CardRecord = new CardData();
        }

        private void Form1_Load(object sender, EventArgs events) {
            try {
                var contextFactory = ContextFactory.Instance;
                using (var context = contextFactory.Establish(SCardScope.System)) {
                    var readerNames = context.GetReaders();
                    foreach (var reader in readerNames) {
                        listBox1.Items.Add(reader);
                    }
                    listBox1.SelectedIndex = listBox1.Items.Count - 1;
                }
            } catch (SystemException e) {
                MessageBox.Show(e.Message);
            }
            dataGridView1.DataSource = CardRecord.Setup();
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.Columns[0].Width = 200;
        }

        private void button1_Click(object sender, EventArgs events) {
            ReaderStatus readerStatus;
            string error;
            var newRecord = new CardRecord();
            var contextFactory = ContextFactory.Instance;
            try {
                using (var context = contextFactory.Establish(SCardScope.System)) {
                    var readerName = listBox1.SelectedItem.ToString();
                    if (readerName != null) {
                        using (var reader = context.ConnectReader(readerName, SCardShareMode.Direct, SCardProtocol.Unset)) {
                            readerStatus = reader.GetStatus();
                        }
                        using (var rfidReader = context.ConnectReader(readerName, SCardShareMode.Shared, SCardProtocol.Any)) {
                            var apduGetData = new CommandApdu(IsoCase.Case2Short, rfidReader.Protocol) { CLA = 0xFF, Instruction = InstructionCode.GetData, P1 = 0x00, P2 = 0x00, Le = 0 };
                            var sendPci = SCardPCI.GetPci(rfidReader.Protocol);
                            var receivePci = new SCardPCI(); // IO returned protocol control information.
                            var receiveBuffer = new byte[256];
                            var commandGetData = apduGetData.ToArray();
                            var bytesReceived = rfidReader.Transmit(
                                sendPci, // Protocol Control Information (T0, T1 or Raw)
                                commandGetData, // command APDU
                                commandGetData.Length,
                                receivePci, // returning Protocol Control Information
                                receiveBuffer,
                                receiveBuffer.Length); // data buffer
                            var responseGetData = new ResponseApdu(receiveBuffer, bytesReceived, IsoCase.Case2Short, rfidReader.Protocol);
                            if (responseGetData.HasData) {
                                newRecord.uid = BitConverter.ToString(responseGetData.GetData());
                            } else {
                                throw new ApplicationException("cannot read uid");
                            }
                            byte[] dataIn = { 0x0B, 0x01 };
                            var apduSelectFile = new CommandApdu(IsoCase.Case4Short, rfidReader.Protocol) { CLA = 0xFF, Instruction = InstructionCode.SelectFile, P1 = 0x00, P2 = 0x01, Data = dataIn, Le = 0 };
                            var commandSelectFile = apduSelectFile.ToArray();
                            var bytesReceivedSelectedFile = rfidReader.Transmit(
                                    sendPci, // Protocol Control Information (T0, T1 or Raw)
                                    commandSelectFile, // command APDU
                                    commandSelectFile.Length,
                                    receivePci, // returning Protocol Control Information
                                    receiveBuffer,
                                    receiveBuffer.Length); // data buffer
                            var responseApdu = new ResponseApdu(receiveBuffer, bytesReceivedSelectedFile, IsoCase.Case2Short, rfidReader.Protocol);
                            if (responseApdu.IsValid == true) {
                                var apduReadBinary = new CommandApdu(IsoCase.Case2Short, rfidReader.Protocol) { CLA = 0xFF, Instruction = InstructionCode.ReadBinary, P1 = 0x00, P2 = 0x00, Le = 0 };
                                var commandReadBinary = apduReadBinary.ToArray();
                                var bytesReceivedReadBinary = rfidReader.Transmit(
                                    sendPci, // Protocol Control Information (T0, T1 or Raw)
                                    commandReadBinary, // command APDU
                                    commandReadBinary.Length,
                                    receivePci, // returning Protocol Control Information
                                    receiveBuffer,
                                    receiveBuffer.Length); // data buffer
                                var responseApduReadBinary = new ResponseApdu(receiveBuffer, bytesReceivedReadBinary, IsoCase.Case2Short, rfidReader.Protocol);
                                if (responseApduReadBinary.HasData == true) {
                                    var hex = BitConverter.ToString(receiveBuffer, 0, 16);
                                    byte[] buf = new byte[16];
                                    for (var i = 0; i < 16; i++) buf[i] = receiveBuffer[i];
                                    string info = System.Text.Encoding.ASCII.GetString(buf);
                                    newRecord.pid = info.Trim('\0');
                                } else {
                                    throw new ApplicationException("cannot read pid");
                                }
                            }
                        }
                    } else {
                        throw new ApplicationException("no card reader");
                    }
                }
                CardRecord.Add(newRecord);
                 return;
            } catch (PCSC.Exceptions.NoServiceException e) {
                error = e.Message;
            } catch (PCSC.Exceptions.PCSCException e) {
                error = e.Message;
            } catch (ApplicationException e) {
                error = e.Message;
            } catch (SystemException e) {
                error = e.Message;
            }
            MessageBox.Show(error);
        }
    }
}
