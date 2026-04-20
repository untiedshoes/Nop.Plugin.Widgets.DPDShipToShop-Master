using FluentMigrator;
using Nop.Core.Domain.Orders;
using Nop.Data.Extensions;
using Nop.Data.Migrations;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;

namespace Nop.Plugin.Widgets.DPDShipToShop.Data
{
    [NopMigration("2020/06/30 05:14:00:6455422", "Widgets.DPDShipToShop base schema")]
    public class SchemaMigration : AutoReversingMigration
    {
        protected IMigrationManager _migrationManager;

        public SchemaMigration(IMigrationManager migrationManager)
        {
            _migrationManager = migrationManager;
        }

        /// <summary>
        /// Creates the base database tables required by the plugin.
        /// </summary>
        public override void Up()
        {
            Create.TableFor<DPDShipToShopLocations>();
            Create.TableFor<DPDShipToShopLocation>();
        }
    }
}
