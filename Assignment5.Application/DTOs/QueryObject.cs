﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assignment5.Application.DTOs
{
    public class QueryObject
    {
        public string? Title { get; set; } = null;
        public string? Author { get; set; } = null;
        public string? ISBN { get; set; } = null;
        public string? Category { get; set; } = null;
        public string? Keyword { get; set; } = null;

        public string? SortBy { get; set; } = "bookId";
        public string? SortOrder { get; set; } = "asc";
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}