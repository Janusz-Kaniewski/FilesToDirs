using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesToDirs.Models
{
    public class FileModel
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
        public string Type { get; set; }
    }
}
