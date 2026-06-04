using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Remotely.Server.Data;

namespace Remotely.Server.Migrations.Sqlite;

[DbContext(typeof(SqliteDbContext))]
[Migration("20240601000000_Add_PendingUpdate")]
partial class Add_PendingUpdate
{
}
