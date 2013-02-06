

using AsyncORM.interfaces;

namespace AsyncORM
{
    public class BatchItem : IBatchItem
    {
        public string CommandText { get; set; }
        public object DbParams { get; set; }
    }
}