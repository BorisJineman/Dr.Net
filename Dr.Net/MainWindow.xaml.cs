using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;


using SharpPcap;


namespace Dr.Net
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {





        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            IPAddress[] ips = Dns.GetHostAddresses(Dns.GetHostName());
            comboBox2.Items.Clear();
            if (ips.Length > 0)
                foreach (IPAddress ipadd in ips)
                {
                    if (ipadd.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        comboBox2.Items.Add(ipadd.ToString());
                    }
                }
            if (ips.Length > 0)
                comboBox2.SelectedIndex = 0;

            CaptureDeviceList allDevices = SharpPcap.CaptureDeviceList.Instance;
            
            foreach(ICaptureDevice dev in allDevices)
            {
                Device cdev = new Device();
                cdev.DisplayName = dev.Description;
                cdev.CaptureDevice = dev;
                comboBox1.Items.Add(cdev);
            }
            

            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

        }
        Device currentDev;
        private void button1_Click(object sender, RoutedEventArgs e)
        {


            currentDev = (Device)comboBox1.SelectedItem;
            currentDev.CaptureDevice.Open();
            byte[] srcMacBytes = currentDev.CaptureDevice.MacAddress.GetAddressBytes();
            string localMAC = BitConverter.ToString(srcMacBytes).Replace("-", ":");


            textBox1.Text = localMAC;
            textBox2.Text = localMAC;

            comboBox1.IsEnabled = false;
            button3.IsEnabled = true;
            button1.IsEnabled = false;
        }


        private UInt16 msgid = Convert.ToUInt16((DateTime.Now.Second * 1000 + DateTime.Now.Millisecond) &0xFFF);

        private void button5_Click(object sender, RoutedEventArgs e)
        {

            string ErrStr;
            do
            {
                string[] strs = textBox1.Text.Split(':');
                byte[] sendermac = new byte[6];

                if (strs.Length != 6)
                {
                    ErrStr = "Error in SenderMAC.";
                    break;
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < sendermac.Length; i++)
                        {
                            sendermac[i] = Convert.ToByte(strs[i], 16);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in SenderMAC." + Environment.NewLine + ex.Message;
                        break;
                    }
                }

                strs = textBox2.Text.Split(':');
                byte[] receivermac = new byte[6];

                if (strs.Length != 6)
                {
                    ErrStr = "Error in Receiver MAC.";
                    break;
                }
                else
                {
                    try
                    {
                        for (int i = 0; i < receivermac.Length; i++)
                        {
                            receivermac[i] = Convert.ToByte(strs[i], 16);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in Receiver MAC." + Environment.NewLine + ex.Message;
                        break;
                    }
                }
                byte[] prot;
                try
                {

                    prot = BitConverter.GetBytes(Convert.ToUInt16(textBox7.Text, 16));
                    if (prot.Length == 2)
                    {
                        byte tempbyte = prot[0];
                        prot[0] = prot[1];
                        prot[1] = tempbyte;
                    }

                }
                catch (System.Exception ex)
                {
                    ErrStr = "Error in Type." + Environment.NewLine + ex.Message;
                    break;
                }
                string inputStr = "";
                for (int i = 0; i < textBox6.Text.Length; ++i)
                {
                    if (Char.IsLetterOrDigit(textBox6.Text[i]))
                        inputStr += textBox6.Text[i];
                }
                string tempstr = "";
                for (int i = 0; i < inputStr.Length; i++)
                {
                    tempstr += inputStr[i].ToString();
                    if (i % 2 == 1)
                    {
                        tempstr += " ";
                    }

                }
                inputStr = tempstr.Trim();
                List<byte> bytes = new List<byte>();
                if (inputStr != "")
                {
                    string[] hexValuesSplit = inputStr.Split(' ');                    
                    try
                    {

                        foreach (string hex in hexValuesSplit)
                        {
                            Byte value = Convert.ToByte(hex, 16);
                            bytes.Add(value);

                        }

                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in datas." + Environment.NewLine + ex.Message; ;
                        break;
                    }
                }
                byte[] senddatas = bytes.ToArray();

                textBox6.Text = inputStr;


                string previewstr = "";
                foreach (byte byteitem in receivermac)
                    previewstr += byteitem.ToString("X2") + " ";
                foreach (byte byteitem in sendermac)
                    previewstr += byteitem.ToString("X2") + " ";
                foreach (byte byteitem in prot)
                    previewstr += byteitem.ToString("X2") + " ";
                if (checkBox1.IsChecked.Value)
                {
                    byte tempbyte=0;
                    byte[] iphead = new byte[20];
                    iphead[0] = 0x45;//version and headlength
                    iphead[1] = 0x00;//TOS                   
                    Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(senddatas.Length + 8 + 20 )), 0, iphead, 2, 2);   // * 2-3 is length 
                    tempbyte = iphead[2];
                    iphead[2] = iphead[3];
                    iphead[3] = tempbyte;
                    msgid = (UInt16)((msgid++) & 0xFFF);
                    Array.Copy(BitConverter.GetBytes(msgid), 0, iphead, 4, 2);  // * 4-5 is msgid                     
                    tempbyte = iphead[4];
                    iphead[4] = iphead[5];
                    iphead[5] = tempbyte;
                    iphead[6] = 0x00|0x00;//flags and offset
                    iphead[7] = 0x00;//offset
                    iphead[8] = 0x80;//TTL
                    try
                    {
                        iphead[9] = Convert.ToByte(textBox8.Text);
                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in protocol." + Environment.NewLine + ex.Message; ;
                        break;                    	
                    }
                    
                    try
                    {
                    Array.Copy(IPAddress.Parse(comboBox2.Text).GetAddressBytes(), 0, iphead, 12, 4); //IP Address Src

                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in Sender IP Address." + Environment.NewLine + ex.Message; ;
                        break;     

                    }  
                    try
                    {
                    Array.Copy(IPAddress.Parse(textBox5.Text).GetAddressBytes(), 0, iphead, 16, 4); //IP Address Dst
                    }
                    catch (System.Exception ex)
                    {
                        ErrStr = "Error in Receiver IP Address." + Environment.NewLine + ex.Message; ;
                        break;

                    }  
                    Array.Copy(BitConverter.GetBytes(CheckSum.Check(iphead)), 0, iphead, 10, 2); // * 10-11 is head checksum

                 //  UInt16 test= CheckSum.Check(iphead);


                    foreach (byte byteitem in iphead)
                        previewstr += byteitem.ToString("X2") + " ";
                    previewstr.Trim();

                    if(checkBox2.IsChecked.Value)
                    {
                        byte[] prothead = new byte[8];

                        try
                    {
                        Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(textBox9.Text)), 0, prothead, 0, 2);
                    }
                        catch (System.Exception ex)
                        {
                            ErrStr = "Error in Sender Port." + Environment.NewLine + ex.Message; ;
                            break;

                        }  
                            
                            tempbyte = prothead[0];
                        prothead[0] = prothead[1];
                        prothead[1] = tempbyte;
                          try
                    { 
                              Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(textBox10.Text)), 0, prothead, 2, 2);
                    }
                          catch (System.Exception ex)
                          {
                              ErrStr = "Error in Receiver Port." + Environment.NewLine + ex.Message; ;
                              break;

                          }
                          
                        tempbyte = prothead[2];
                        prothead[2] = prothead[3];
                        prothead[3] = tempbyte;                     
                        
                        Array.Copy(BitConverter.GetBytes(Convert.ToUInt16(senddatas.Length + 8)), 0, prothead, 4, 2);
                        tempbyte = prothead[4];
                        prothead[4] = prothead[5];
                        prothead[5] = tempbyte;  
                        
                        
                        //checksum

                        byte[] tempudphead = new byte[20 + senddatas.Length];
                         Array.Copy(iphead, 12, tempudphead, 0, 8);
                         tempudphead[8] = 0;
                         tempudphead[9] = 17;
                         Array.Copy(prothead, 4, tempudphead, 10, 2);
                         Array.Copy(prothead, 0, tempudphead, 12, 6);
                         Array.Copy(senddatas, 0, tempudphead, 20, senddatas.Length);
                         Array.Copy(BitConverter.GetBytes(CheckSum.Check(tempudphead)), 0, prothead, 6, 2);
//                          Array.Copy(prothead, 6, tempudphead, 14, 2);
// 
// 
//                            UInt16 test= CheckSum.Check(tempudphead);


//                          byte[] testbytes = new byte[] { 0xc0, 0xa8, 0x18, 0x01, 0xc0, 0xa8, 0x18, 0x80, 0x00, 0x11, 0x00, 0x0b, 0x04, 0x4b, 0x09, 0x79, 0x00, 0x0b, 0x1d, 0x30 ,0x11,0x12,0x12};
//                            UInt16 test = CheckSum.Check(testbytes);
                        foreach (byte byteitem in prothead)
                            previewstr += byteitem.ToString("X2") + " ";
                        previewstr.Trim();

                    }                  

                }
                foreach (byte byteitem in senddatas)
                    previewstr += byteitem.ToString("X2") + " ";
                previewstr.Trim();
                textBox4.Text = previewstr;

                return;
            } while (false);
            if (ErrStr != null)
                MessageBox.Show(ErrStr);

        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (t!=null&&t.ThreadState == ThreadState.Running)
            {
                t.Abort();
                button2.Content = "Send";
            }
            else
            {



                string inputStr = "";
                for (int i = 0; i < textBox4.Text.Length; ++i)
                {
                    if (Char.IsLetterOrDigit(textBox4.Text[i]))
                        inputStr += textBox4.Text[i];
                }
                string tempstr = "";
                for (int i = 0; i < inputStr.Length; i++)
                {
                    tempstr += inputStr[i].ToString();
                    if (i % 2 == 1)
                    {
                        tempstr += " ";
                    }

                }
                inputStr = tempstr.Trim();

                string[] hexValuesSplit = inputStr.Split(' ');

                List<byte> bytes = new List<byte>();
                try
                {

                    foreach (string hex in hexValuesSplit)
                    {
                        Byte value = Convert.ToByte(hex, 16);
                        bytes.Add(value);

                    }

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error in Datas.");
                    return;
                }

                byte[] senddatas = bytes.ToArray();



                int times = 0;
                try
                {
                    times = Convert.ToInt32(textBox3.Text);

                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error in Send Times." + Environment.NewLine + ex.Message);
                    return;
                }
                int delay = 0;
                try
                {

                    delay = Convert.ToInt32(textBox11.Text);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show("Error in Delay." + Environment.NewLine + ex.Message);
                    return;
                }
                t = new Thread(new ParameterizedThreadStart(SendThread));
                Dictionary<String, object> para = new Dictionary<String, object>();
                para.Add("Times", times);
                para.Add("Delay", delay);
                para.Add("Datas", senddatas);
                t.Start(para);

                button2.Content = "Stop";


            }

          
        }

        Thread t;
        void SendThread(object obj)
        {

            Dictionary<String, object> para = (Dictionary<String, object>)obj;
            int times = (int)para["Times"];
            int delay = (int)para["Delay"];
            byte[] senddatas = (byte[])para["Datas"];
         
         

            DateTime starttime = DateTime.Now;

            int sendtimes=0;
            while (true)
            {
                if ((DateTime.Now - starttime).TotalMilliseconds / delay >= sendtimes)
                {

                    this.Dispatcher.BeginInvoke(new Action(() => textBox3.Text = (times - sendtimes).ToString()));

                    if (sendtimes >= times)
                        break;
                    currentDev.CaptureDevice.SendPacket(senddatas, senddatas.Length);
                    sendtimes++;                 


                }
            }

           
            this.Dispatcher.BeginInvoke(new Action(() => button2.Content = "Send"));
            this.Dispatcher.BeginInvoke(new Action(() => textBox3.Text = "1"));
        }

       

        private void checkBox1_Checked(object sender, RoutedEventArgs e)
        {
           
                textBox7.Text = "0x0800";
                textBox7.IsReadOnly = true;
                comboBox2.IsEnabled = true;
                textBox5.IsEnabled = true;
                textBox8.IsEnabled = true;
                checkBox2.IsEnabled = true;
                if (checkBox2.IsChecked.Value)
                {
                    textBox9.IsEnabled = true;
                    textBox10.IsEnabled = true;
                }
                else
                {
                    textBox9.IsEnabled = false;
                    textBox10.IsEnabled = false;
                }
              
           
        }

  
        private void checkBox1_Unchecked(object sender, RoutedEventArgs e)
        {
            textBox7.IsReadOnly = false;
            comboBox2.IsEnabled = false;
            textBox5.IsEnabled = false;
            textBox8.IsEnabled = false;
            checkBox2.IsEnabled = false;
            textBox9.IsEnabled = false;
            textBox10.IsEnabled = false;
        }

        private void checkBox2_Checked(object sender, RoutedEventArgs e)
        {
           
                textBox8.Text = "17";
                textBox8.IsReadOnly = true;
                textBox9.IsEnabled = true;
                textBox10.IsEnabled = true;
           
        }

        private void checkBox2_Unchecked(object sender, RoutedEventArgs e)
        {
            textBox8.IsReadOnly = false;
            textBox9.IsEnabled = false;
            textBox10.IsEnabled = false;
        }

        private void button4_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            currentDev.CaptureDevice.Close();
            comboBox1.IsEnabled = true;
            button1.IsEnabled = true;
            button3.IsEnabled = false;
        }

    }
}
