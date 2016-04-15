using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NinjaTurtles.TestRunnerService.Utilities
{

    public static class TestDescriptionExchanger
    {
        private const string TRANSFER_STOP = "ENDSYNC";

        public static void SendATestDescription(StreamWriter sw, TestDescription testDescription)
        {
            SendData(sw, XmlProcessing.SerializeToXml(testDescription));
        }

        public static void SendData(StreamWriter sw, string data)
        {
            sw.WriteLine(data);
            sw.WriteLine(TRANSFER_STOP);
            sw.Flush();
        }

        public static TestDescription ReadATestDescription(StreamReader sr)
        {
            string lineBuf;
            string data = "";
            while (true)
            {
                lineBuf = sr.ReadLine();
                if (lineBuf == null || lineBuf == TRANSFER_STOP)
                    break;
                data += lineBuf;
            }
            if (lineBuf == null)
                throw new IOException();
            return XmlProcessing.DeserializeFromXml<TestDescription>(data);
        }
    }
}
