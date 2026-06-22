using cmdNet.Model;
using DocumentFormat.OpenXml.Spreadsheet;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmdNet.Services
{
  public  class TxtService
    {


        public string WriteFileTxt(string fileName,
    List<UserResultModel> data)
        {
            try
            {
               
                var sb = new StringBuilder();

                foreach (var user in data)
                {
                    sb.AppendLine(user.UserName);
                    sb.AppendLine(user.Password);
                    sb.AppendLine("\n");
                    sb.AppendLine();
                }

                File.WriteAllText(fileName, sb.ToString(), Encoding.UTF8);
                return "txt fiel Writed";
            }
            catch { throw new Exception(); }
            
        }
    }
}
