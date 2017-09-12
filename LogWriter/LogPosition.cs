using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogWriter
{
    class LogPosition
    {
        public int Id { get; set; }
        public DateTime Time { get; set; }

        public string CharacterName { get; set; }

        public string OreType { get; set; }

        public int Count { get; set; }

        public DateTime UploadTime { get; set; }

        public string AdminName { get; set; }

        public string CargoName { get; set; }
    }
}
