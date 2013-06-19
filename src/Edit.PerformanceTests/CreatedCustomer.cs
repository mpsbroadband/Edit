﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Edit.PerformanceTests
{
    public class CreatedCustomer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public byte[] PayLoad { get; set; }

        public CreatedCustomer()
        {

        }

        public CreatedCustomer(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
