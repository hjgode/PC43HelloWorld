using Intermec.Printer;
using System;
using System.Threading;
using System.IO;

namespace HelloWorld
{
    static class Log
    {
        public static void d(String s)
        {
            System.Diagnostics.Debug.WriteLine(s);
            Console.WriteLine(s);
        }
    }
    class TestCommunicationUSBHost
    {
        public static int Test(string[] args)
        {
            Log.d("CommunicationUSBHost");

            // Get list of USB Host connected devices
            string[] usbPortNames = Communication.USBHost.GetPortNames();
            foreach (string usbPortName in usbPortNames)
            {
                Log.d("Found: " + usbPortName);                
            }

            if (usbPortNames.Length == 0)
            {
                Log.d("No usb device!");
                return -2;
            }
            // Open the first USB host device and read any data sent
            Communication.USBHost usbHost =
                new Communication.USBHost("/dev/ttyUSB0");

            usbHost.Open();

            if (usbHost.IsOpen)
            {
                Log.d(String.Format( "Opened /dev/ttyUSB0 ({0})",
                                  usbHost.HIDName));

                FileStream fileStream = usbHost.GetStream();

                try
                {
                    int timeout = 0;
                    int i;

                    while (timeout < 10)
                    {
                        Log.d(String.Format("Exiting in {0} secs", 10 - timeout));
                        Byte[] bytes = new Byte[256];

                        if ((i = fileStream.Read(bytes, 0, bytes.Length)) > 0)
                        {
                            string data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);
                            Log.d(String.Format("read {0} bytes: {1}", data.Length, data));
                            timeout = 0;
                        }
                        else
                        {
                            Thread.Sleep(1000);
                            timeout++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.d(String.Format("Exception reading /dev/ttyUSB0: {0}", ex.Message));
                }

                usbHost.Close();
            }

            // Wait a short while
            Thread.Sleep(1000);

            // Clean up 
            usbHost.Dispose();

            return 0;
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            TestCommunicationUSBHost.Test(args);
            return;

            // Set up print control and drawing
            PrintControl printControl = new PrintControl();
            Drawing drawing = new Drawing();

            // Perform test feed
            State state = printControl.TestFeed();

            // Print Hello World (if testfeed was OK)
            if (state == State.NoError)
            {
                Drawing.Text text = new Drawing.Text();
                text.Point = new Point(100, 100);
                text.Data = "Hello World";
                text.Height = 36;

                drawing += text;
                drawing.PartialRendering = true;

                state = printControl.PrintFeed(drawing, 1);

                if (state != State.NoError)
                {
                    Log.d(String.Format("PrintFeed failed: {0}", state.ToString()));
                }
            }
            else
            {
                Log.d(String.Format("TestFeed failed: {0}", state.ToString()));
            }

            Log.d("Exit");
        }
    }
}
