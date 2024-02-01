﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace AddressRegistry.Projections.Wms.Migrations
{
    using Infrastructure;

    public partial class Add_AddressWmsItemV2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AddressWmsV2",
                schema: "wms.address",
                columns: table => new
                {
                    AddressPersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    ParentAddressPersistentLocalId = table.Column<int>(type: "int", nullable: true),
                    StreetNamePersistentLocalId = table.Column<int>(type: "int", nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HouseNumberLabel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HouseNumberLabelLength = table.Column<int>(type: "int", nullable: true),
                    LabelType = table.Column<int>(type: "int", nullable: false),
                    BoxNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    OfficiallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    Position = table.Column<Point>(type: "sys.geometry", nullable: false),
                    PositionX = table.Column<double>(type: "float", nullable: false),
                    PositionY = table.Column<double>(type: "float", nullable: false),
                    PositionMethod = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PositionSpecification = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Removed = table.Column<bool>(type: "bit", nullable: false),
                    VersionTimestamp = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    VersionAsString = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddressWmsV2", x => x.AddressPersistentLocalId)
                        .Annotation("SqlServer:Clustered", true);
                });

            migrationBuilder.Sql(
                $"CREATE SPATIAL INDEX [SPATIAL_Address_Position] ON [{Schema.Wms}].[AddressWmsV2] ([Position])\n" +
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

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV2_ParentAddressPersistentLocalId",
                schema: "wms.address",
                table: "AddressWmsV2",
                column: "ParentAddressPersistentLocalId");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV2_PositionX_PositionY_Removed_Status",
                schema: "wms.address",
                table: "AddressWmsV2",
                columns: new[] { "PositionX", "PositionY", "Removed", "Status" })
                .Annotation("SqlServer:Include", new[] { "StreetNamePersistentLocalId" });

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV2_Status",
                schema: "wms.address",
                table: "AddressWmsV2",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_AddressWmsV2_StreetNamePersistentLocalId",
                schema: "wms.address",
                table: "AddressWmsV2",
                column: "StreetNamePersistentLocalId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AddressWmsV2",
                schema: "wms.address");
        }
    }
}
