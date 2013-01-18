

using AsyncORM.interfaces;

namespace AsyncORM
{
    public class BatchSetting : IBatchSetting
    {
        public string CommandText { get; set; }
        public object DbParams { get; set; }
    }
}