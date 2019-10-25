using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Apache.NMS;
using Apache.NMS.Util;

using BusinessObject;

namespace Queue
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            reciever();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void reciever() 
        {
            IConnectionFactory factory = new NMSConnectionFactory("tcp://localhost:61616");
            IConnection connection = factory.CreateConnection();
            connection.Start();
            ISession session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            IDestination destination = SessionUtil.GetDestination(session, "ExampleQueue");
            IMessageConsumer receiver = session.CreateConsumer(destination);
            receiver.Listener += new MessageListener(Message_Listener);
        }
        private void Message_Listener(IMessage message)
        {
            IObjectMessage objMessage = message as IObjectMessage;
            OperatorRequestObject operatorRequestObject = ((BusinessObject.OperatorRequestObject) (objMessage.Body));
            MessageBox.Show(
                operatorRequestObject.Shortcode1
                + ", "
                + operatorRequestObject.Shortcode2
                + ", "
                + operatorRequestObject.Shortcode3
                + ", "
                + operatorRequestObject.Shortcode4
                + ", "
                + operatorRequestObject.Shortcode5
                + ", "
                + operatorRequestObject.Shortcode6
                + ", "
                + operatorRequestObject.Shortcode7
                + ", "
                + operatorRequestObject.Shortcode8
                + ", "
                + operatorRequestObject.Shortcode9
                + ", "
                + operatorRequestObject.Shortcode10
                );    
        }
    }
}
