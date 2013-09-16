namespace AsyncORM
{
    public static class AsyncOrmConfig
    {
        static AsyncOrmConfig()
        {
            EnableParameterCache = true;
            ConfigureAwait = false;
            EnableAttributes = true;
        }

        public static string ConnectionString { get; set; }
        public static bool ConfigureAwait { get; set; }
        public static bool EnableParameterCache { get; set; }
        public static bool EnableAttributes { get; set; }
    }
}