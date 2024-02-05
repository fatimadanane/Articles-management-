using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace Alimentations.Models
{
    public class DepotModel
    {
        public int ID { get; set; }
        public string Designation { get; set; }
        public string Ville { get; set; }
        public DateTime Date { get; set; }
    }

}
