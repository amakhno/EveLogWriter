using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogWriter
{
    class Helpers
    {
        internal static int[] GetWebPrices(HtmlDocument document, string[] ores)
        {
            string[] htmlText = document.Body.OuterText.Split(new char[] { '\n' } );
            int[] Prices = new int[16];
            int jOld = 0;
            int countOfPrices = 0;
            for (int i = 0; i<ores.Length; i++)
            {
                for(int j = jOld; j< htmlText.Length; j++)
                {
                    if (htmlText[j].Contains(','))
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
                    }                    
                }
                if (countOfPrices == 16)
                    break;
            }
            return Prices;
        }
    }
}
