namespace AsyncORM
{
    public static class AsyncOrmConfig
    {
        static AsyncOrmConfig()
        {
            EnableParameterCache = true;
            ConfigureAwait = false;
        }

        public static string ConnectionString { get; set; }
        public static bool ConfigureAwait { get; set; }
        public static bool EnableParameterCache { get; set; }
    }
}