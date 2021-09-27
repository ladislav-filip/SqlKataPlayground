using System.Data.Common;

namespace SqlKataFirebird.Infrastructure
{
    public interface IConnectionFactory
    {
        DbConnection Connection { get; }
    }
}