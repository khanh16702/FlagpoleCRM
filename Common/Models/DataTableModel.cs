﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Models
{
    public class DataTableModel
    {
        public string Msg { get; set; }
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public List<Column> columns { get; set; }
        public Search search{ get; set; }
        public List<TableOrder> order{ get; set; }
        public string orderColumnName
        {
            get
            {
                if (columns != null && columns.Any() && order != null && order.Any())
                {
                    return columns[order[0].column].name;
                }
                return "";
            }
        }
    }

    public class Column
    {
        public string data { get; set; }
        public string name { get; set; }
        public bool searchable { get; set; }
        public bool orderable { get; set; }
        public Search search { get; set; }
    }

    public class Search
    {
        public string value { get; set; }
        public string regex { get; set; }
    }

    public class TableOrder
    {
        public int column { get; set; }
        public string dir { get; set; }
    }
}
