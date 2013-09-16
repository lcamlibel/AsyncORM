using System;

namespace AsyncORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AsyncColumnMapAttribute : Attribute
    {
        private string _columnName;

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }
        public AsyncColumnMapAttribute(string columnName)
        {
            _columnName = columnName;
        }
    }
}
