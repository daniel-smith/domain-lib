namespace DomainLib.Projections.Sql
{
    public class SqlContextSettings
    {
        public static SqlContextSettings Default { get; } = new SqlContextSettings(true, true);

        public SqlContextSettings(bool useTransactionBeforeCaughtUp, bool handleLiveEventsInTransaction)
        {
            UseTransactionBeforeCaughtUp = useTransactionBeforeCaughtUp;
            HandleLiveEventsInTransaction = handleLiveEventsInTransaction;
        }

        public bool UseTransactionBeforeCaughtUp { get; }
        public bool HandleLiveEventsInTransaction { get; }
    }
}