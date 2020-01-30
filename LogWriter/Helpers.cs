using System;
using System.Text.RegularExpressions;

namespace LogWriter
{
    public class Helpers
    {
        internal static double[] GetWebPrices(string text, string[] ores)
        {
            var regex = new Regex(@"^(?<price>-?\d+(?: \d+)?(?:\.\d+)?) ISK");
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
                }
                if (countOfPrices == 16)
                    break;
            }
            return Prices;
        }
    }
}
