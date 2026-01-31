using Nop.Core.Domain.Orders;
using FluentMigrator.Builders.Create.Table;
using Nop.Data.Mapping.Builders;
using Nop.Plugin.Widgets.DPDShipToShop.Domain;
using System.Data;
using Nop.Data.Extensions;

namespace Nop.Plugin.Widgets.DPDShipToShop.Data
{
    public class DPDShipToShopLocationRecordBuilder : NopEntityBuilder<DPDShipToShopLocation>
    {
        public override void MapEntity(CreateTableExpressionBuilder table)
        {
            //map the additional properties as foreign keys
            table.WithColumn(nameof(DPDShipToShopLocation.OrderId)).AsInt32().ForeignKey<Order>(onDelete: Rule.Cascade);
        }
    }
}
