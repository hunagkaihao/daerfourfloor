using System.Collections.Generic;

namespace TuTa.Wms.Domain;

public class QueryDataInPage<T> where T : class
{
    public int TotalCount { get; set; }

    public List<T> Items { get; set; }
}