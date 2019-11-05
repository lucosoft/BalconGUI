using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.IO.Ports;

using Apache.NMS;
using Apache.NMS.Util;

using BusinessObject;

namespace Modbus_Poll_CS
{
    public partial class Form1 : Form
    {
        modbus mb = new modbus();
        SerialPort sp = new SerialPort();
        System.Timers.Timer timer = new System.Timers.Timer();
        string dataType;
        bool isPolling = false;
        int pollCount;

        #region GUI Delegate Declarations
        public delegate void GUIDelegate(string paramString);
        public delegate void GUIClear();
        public delegate void GUIStatus(string paramString);
        #endregion

        public Form1()
        {
            InitializeComponent();
            LoadListboxes();
            timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
        }

        #region Delegate Functions
        public void DoGUIClear()
        {
            if (this.InvokeRequired)
            {
                GUIClear delegateMethod = new GUIClear(this.DoGUIClear);
                this.Invoke(delegateMethod);
            }
            else
                this.lstRegisterValues.Items.Clear();
        }
        public void DoGUIStatus(string paramString)
        {
            if (this.InvokeRequired)
            {
                GUIStatus delegateMethod = new GUIStatus(this.DoGUIStatus);
                this.Invoke(delegateMethod, new object[] { paramString });
            }
            else
                this.lblStatus.Text = paramString;
        }
        public void DoGUIUpdate(string paramString)
        {
            if (this.InvokeRequired)
            {
                GUIDelegate delegateMethod = new GUIDelegate(this.DoGUIUpdate);
                this.Invoke(delegateMethod, new object[] { paramString });
            }
            else
                this.lstRegisterValues.Items.Add(paramString);
        }
        #endregion

        #region Timer Elapsed Event Handler
        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            PollFunction();
        }
        #endregion

        #region Load Listboxes
        private void LoadListboxes()
        {
            //Three to load - ports, baudrates, datetype.  Also set default textbox values:
            //1) Available Ports:
            string[] ports = SerialPort.GetPortNames();

            foreach (string port in ports)
            {
                lstPorts.Items.Add(port);
            }

            lstPorts.SelectedIndex = 0;

            //2) Baudrates:
            string[] baudrates = { "230400", "115200", "57600", "38400", "19200", "9600" };

            foreach (string baudrate in baudrates)
            {
                lstBaudrate.Items.Add(baudrate);
            }

            //            lstBaudrate.SelectedIndex = 1;
            lstBaudrate.SelectedIndex = 4;

            //3) Datatype:
            string[] dataTypes = { "Decimal", "Hexadecimal", "Float", "Reverse" };

            foreach (string dataType in dataTypes)
            {
                lstDataType.Items.Add(dataType);
            }

            //lstDataType.SelectedIndex = 0;
            lstDataType.SelectedIndex = 0;

            //Textbox defaults:
            //            txtRegisterQty.Text = "20";
            txtRegisterQty.Text = "10";
            txtSampleRate.Text = "1000";
            txtSlaveID.Text = "1";
            txtStartAddr.Text = "0";
        }
        #endregion

        #region Start and Stop Procedures
        private void StartPoll()
        {
            pollCount = 0;

            //Open COM port using provided settings:
            if (mb.Open(lstPorts.SelectedItem.ToString(), Convert.ToInt32(lstBaudrate.SelectedItem.ToString()),
                8, Parity.None, StopBits.One))
            {
                //Disable double starts:
                btnStart.Enabled = false;
                dataType = lstDataType.SelectedItem.ToString();

                //Set polling flag:
                isPolling = true;

                //Start timer using provided values:
                timer.AutoReset = true;
                if (txtSampleRate.Text != "")
                    timer.Interval = Convert.ToDouble(txtSampleRate.Text);
                else
                    timer.Interval = 1000;
                timer.Start();
            }

            lblStatus.Text = mb.modbusStatus;
        }
        private void StopPoll()
        {
            //Stop timer and close COM port:
            isPolling = false;
            timer.Stop();
            mb.Close();

            btnStart.Enabled = true;

            lblStatus.Text = mb.modbusStatus;
        }
        private void btnStart_Click(object sender, EventArgs e)
        {
            StartPoll();
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            StopPoll();
        }
        #endregion

        #region Poll Function
        private void PollFunction()
        {
            //Update GUI:
            DoGUIClear();
            pollCount++;
            DoGUIStatus("Poll count: " + pollCount.ToString());

            //Create array to accept read values:
            short[] values = new short[Convert.ToInt32(txtRegisterQty.Text)];
            ushort pollStart;
            ushort pollLength;

            if (txtStartAddr.Text != "")
                pollStart = Convert.ToUInt16(txtStartAddr.Text);
            else
                pollStart = 0;
            if (txtRegisterQty.Text != "")
                pollLength = Convert.ToUInt16(txtRegisterQty.Text);
            else
                pollLength = 20;

            //Read registers and display data in desired format:
            try
            {
                while (!mb.SendFc3(Convert.ToByte(txtSlaveID.Text), pollStart, pollLength, ref values)) ;
            }
            catch(Exception err)
            {
                DoGUIStatus("Error in modbus read: " + err.Message);
            }

            string itemString;
            string[] itemStringAux = new string[pollLength];

            switch (dataType)
            {
                case "Decimal":
                    for (int i = 0; i < pollLength; i++)
                    {
                        itemString = "[" + Convert.ToString(pollStart + i + 40001) + "] , MB[" +
                            Convert.ToString(pollStart + i) + "] = " + values[i].ToString();
                        itemStringAux[i] = values[i].ToString();
                        DoGUIUpdate(itemString);
                    }
                    CheckForIllegalCrossThreadCalls = false;
                    MQ_Text1.Text = itemStringAux[0];
                    MQ_Text2.Text = itemStringAux[1];
                    MQ_Text3.Text = itemStringAux[2];
                    MQ_Text4.Text = itemStringAux[3];
                    MQ_Text5.Text = itemStringAux[4];
                    MQ_Text6.Text = itemStringAux[5];
                    MQ_Text7.Text = itemStringAux[6];
                    MQ_Text8.Text = itemStringAux[7];
                    MQ_Text9.Text = itemStringAux[8];
                    MQ_Text10.Text = itemStringAux[9];

                    if (checkBoxAutoManual.Checked)
                    {
                        queue();
                    }

                    break;
                case "Hexadecimal":
                    for (int i = 0; i < pollLength; i++)
                    {
                        itemString = "[" + Convert.ToString(pollStart + i + 40001) + "] , MB[" +
                            Convert.ToString(pollStart + i) + "] = " + values[i].ToString("X");
                        DoGUIUpdate(itemString);
                    }
                    break;
                case "Float":
                    for (int i = 0; i < (pollLength / 2); i++)
                    {
                        int intValue = (int)values[2 * i];
                        intValue <<= 16;
                        intValue += (int)values[2 * i + 1];
                        itemString = "[" + Convert.ToString(pollStart + 2 * i + 40001) + "] , MB[" +
                            Convert.ToString(pollStart + 2 * i) + "] = " +
                            (BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0)).ToString();
                        DoGUIUpdate(itemString);
                    }
                    break;
                case "Reverse":
                    for (int i = 0; i < (pollLength / 2); i++)
                    {
                        int intValue = (int)values[2 * i + 1];
                        intValue <<= 16;
                        intValue += (int)values[2 * i];
                        itemString = "[" + Convert.ToString(pollStart + 2 * i + 40001) + "] , MB[" +
                            Convert.ToString(pollStart + 2 * i) + "] = " +
                            (BitConverter.ToSingle(BitConverter.GetBytes(intValue), 0)).ToString();
                        DoGUIUpdate(itemString);
                    }
                    break;
            }
        }
        #endregion

        #region Write Function
        private void WriteFunction()
        {
            //StopPoll();

            if (txtWriteRegister.Text != "" && txtWriteValue.Text != "" && txtSlaveID.Text != "")
            {
                byte address = Convert.ToByte(txtSlaveID.Text);
                ushort start = Convert.ToUInt16(txtWriteRegister.Text);
                short[] value = new short[1];
                value[0] = Convert.ToInt16(txtWriteValue.Text);

                try
                {
                    while (!mb.SendFc16(address, start, (ushort)1, value)) ;
                }
                catch (Exception err)
                {
                    DoGUIStatus("Error in write function: " + err.Message);
                }
                DoGUIStatus(mb.modbusStatus);
            }
            else
                DoGUIStatus("Enter all fields before attempting a write");

            //StartPoll();
        }
        private void btnWrite_Click(object sender, EventArgs e)
        {
            WriteFunction();
        }
        #endregion

        #region Data Type Event Handler
        private void lstDataType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //restart the data poll if datatype is changed during the process:
            if (isPolling)
            {
                StopPoll();
                dataType = lstDataType.SelectedItem.ToString();
                StartPoll();
            }

        }
        #endregion

        private void lstBaudrate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            queue();
/*            IObjectMessage objMessage;

            OperatorRequestObject operatorRequestObject = new OperatorRequestObject();
            operatorRequestObject.Shortcode1 = MQ_Text1.Text.ToString();
            operatorRequestObject.Shortcode2 = MQ_Text2.Text.ToString();
            operatorRequestObject.Shortcode3 = MQ_Text3.Text.ToString();
            operatorRequestObject.Shortcode4 = MQ_Text4.Text.ToString();
            operatorRequestObject.Shortcode5 = MQ_Text5.Text.ToString();
            operatorRequestObject.Shortcode6 = MQ_Text6.Text.ToString();
            operatorRequestObject.Shortcode7 = MQ_Text7.Text.ToString();
            operatorRequestObject.Shortcode8 = MQ_Text8.Text.ToString();
            operatorRequestObject.Shortcode9 = MQ_Text9.Text.ToString();
            operatorRequestObject.Shortcode10 = MQ_Text10.Text.ToString();

            IConnectionFactory factory = new NMSConnectionFactory("tcp://localhost:61616");
            IConnection connection = factory.CreateConnection();
            connection = factory.CreateConnection();
            connection.Start();
            ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            IDestination QueueDestination = SessionUtil.GetDestination(session, "ExampleQueue ");
            IMessageProducer MessageProducer = session.CreateProducer(QueueDestination);
            //object OperatorRequestObject = null;
            string shortcode = MQ_Text1.Text.ToString()
                + ", " + MQ_Text2.Text.ToString()
                + ", " + MQ_Text3.Text.ToString()
                + ", " + MQ_Text4.Text.ToString()
                + ", " + MQ_Text5.Text.ToString()
                + ", " + MQ_Text6.Text.ToString()
                + ", " + MQ_Text7.Text.ToString()
                + ", " + MQ_Text8.Text.ToString()
                + ", " + MQ_Text9.Text.ToString()
                + ", " + MQ_Text10.Text.ToString()
                ;
            objMessage = session.CreateObjectMessage(operatorRequestObject);

            //MessageProducer.Send(objMessage);
            MessageProducer.Send(shortcode);
            session.Close();
            connection.Stop();
            
*/

        }

        private void queue()
        {
            IObjectMessage objMessage;

            OperatorRequestObject operatorRequestObject = new OperatorRequestObject();
            operatorRequestObject.Shortcode1 = MQ_Text1.Text.ToString();
            operatorRequestObject.Shortcode2 = MQ_Text2.Text.ToString();
            operatorRequestObject.Shortcode3 = MQ_Text3.Text.ToString();
            operatorRequestObject.Shortcode4 = MQ_Text4.Text.ToString();
            operatorRequestObject.Shortcode5 = MQ_Text5.Text.ToString();
            operatorRequestObject.Shortcode6 = MQ_Text6.Text.ToString();
            operatorRequestObject.Shortcode7 = MQ_Text7.Text.ToString();
            operatorRequestObject.Shortcode8 = MQ_Text8.Text.ToString();
            operatorRequestObject.Shortcode9 = MQ_Text9.Text.ToString();
            operatorRequestObject.Shortcode10 = MQ_Text10.Text.ToString();

            IConnectionFactory factory = new NMSConnectionFactory("tcp://localhost:61616");
            IConnection connection = factory.CreateConnection();
            connection = factory.CreateConnection();
            connection.Start();
            ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            IDestination QueueDestination = SessionUtil.GetDestination(session, "ExampleQueue ");
            IMessageProducer MessageProducer = session.CreateProducer(QueueDestination);
            //object OperatorRequestObject = null;
            string shortcode = MQ_Text1.Text.ToString()
                + ", " + MQ_Text2.Text.ToString()
                + ", " + MQ_Text3.Text.ToString()
                + ", " + MQ_Text4.Text.ToString()
                + ", " + MQ_Text5.Text.ToString()
                + ", " + MQ_Text6.Text.ToString()
                + ", " + MQ_Text7.Text.ToString()
                + ", " + MQ_Text8.Text.ToString()
                + ", " + MQ_Text9.Text.ToString()
                + ", " + MQ_Text10.Text.ToString()
                ;
            objMessage = session.CreateObjectMessage(operatorRequestObject);

            //MessageProducer.Send(objMessage);
            MessageProducer.Send(shortcode);
            session.Close();
            connection.Stop();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {

        }

        private void lstRegisterValues_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkBoxAutoManual_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoManual.Checked)
            {
                buttonQueue.Enabled = false;
            }
            else
            {
                buttonQueue.Enabled = true;
            }

        }
    }
}