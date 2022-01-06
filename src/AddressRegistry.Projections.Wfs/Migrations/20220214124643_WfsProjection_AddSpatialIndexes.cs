using Microsoft.EntityFrameworkCore.Migrations;

namespace AddressRegistry.Projections.Wfs.Migrations
{
    using Infrastructure;

    public partial class WfsProjection_AddSpatialIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                $"CREATE SPATIAL INDEX [SPATIAL_Address_Position] ON [{Schema.Wfs}].[AddressDetails] ([Position])\n" +
                @"USING  GEOMETRY_GRID
	                WITH (
		                BOUNDING_BOX =(22279.17, 153050.23, 258873.3, 244022.31),
		                GRIDS =(
			                LEVEL_1 = MEDIUM,
			                LEVEL_2 = MEDIUM,
			                LEVEL_3 = MEDIUM,
			                LEVEL_4 = MEDIUM),
	                CELLS_PER_OBJECT = 5)
	                GO");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex("[SPATIAL_Address_Position]");
        }
    }
}
