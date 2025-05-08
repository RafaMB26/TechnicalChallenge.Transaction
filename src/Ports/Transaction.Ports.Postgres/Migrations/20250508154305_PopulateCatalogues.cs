using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Transaction.Ports.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class PopulateCatalogues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var populateDatabaseCommand = @"INSERT INTO public.cat_transaction_status(Description)
                         VALUES('Pending'),('Approved'),('Rejected');";
            migrationBuilder.Sql(populateDatabaseCommand);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var deleteCataloguesCommand = @"DELETE FROM public.cat_transaction_status;";
            migrationBuilder.Sql(deleteCataloguesCommand);
        }
    }
}
