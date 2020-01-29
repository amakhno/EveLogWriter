using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogWriter
{
    class Helpers
    {
        internal static double[] GetWebPrices(string text, string[] ores)
        {
            var regex = new Regex(@"^(?<price>\d+\.\d+) ISK");
            string[] htmlText = text.Split(new char[] { '\n' } );
            double[] Prices = new double[16];
            int position = 0;
            int countOfPrices = 0;
            for (int i = 0; i<ores.Length; i++)
            {
                for(int j = position; j< htmlText.Length; j++)
                {
                    Match match = regex.Match(htmlText[j]);
                    if (match.Success)
                    {
                        Prices[countOfPrices] = Convert.ToDouble(match.Groups["price"].Value.Replace(".", ","));
                        countOfPrices++;
                        position = j + 4;
                        break;
                    }
                    /*if (htmlText[j].Contains(','))
                    {
                        try
                        {
                            Prices[countOfPrices] = Convert.ToInt32(htmlText[j].Split(new char[] { ' ', '\r' }, StringSplitOptions.RemoveEmptyEntries)[1].Replace(",", null));
                        }
                        catch
                        {
                            Prices[countOfPrices] = Convert.ToInt32(htmlText[j].Substring(5).Replace(",", String.Empty).Replace("\r", String.Empty));
                        }
                        countOfPrices++;
                        if (countOfPrices == 16)
                            break;
                        jOld = j;
                    }             */       
                }
                if (countOfPrices == 16)
                    break;
            }
            return Prices;
        }
    }
}
