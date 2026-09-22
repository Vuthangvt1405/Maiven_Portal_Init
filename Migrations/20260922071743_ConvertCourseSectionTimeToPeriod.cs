using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCourseSectionTimeToPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_time_range",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropColumn(
                name: "end_time",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropColumn(
                name: "start_time",
                table: "COURSE_SECTIONS");

            migrationBuilder.AddColumn<int>(
                name: "end_period",
                table: "COURSE_SECTIONS",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "start_period",
                table: "COURSE_SECTIONS",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_end_period",
                table: "COURSE_SECTIONS",
                sql: "[end_period] BETWEEN 1 AND 10");

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_period_range",
                table: "COURSE_SECTIONS",
                sql: "[start_period] < [end_period]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_start_period",
                table: "COURSE_SECTIONS",
                sql: "[start_period] BETWEEN 1 AND 10");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_end_period",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_period_range",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_start_period",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropColumn(
                name: "end_period",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropColumn(
                name: "start_period",
                table: "COURSE_SECTIONS");

            migrationBuilder.AddColumn<TimeOnly>(
                name: "end_time",
                table: "COURSE_SECTIONS",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<TimeOnly>(
                name: "start_time",
                table: "COURSE_SECTIONS",
                type: "time",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_time_range",
                table: "COURSE_SECTIONS",
                sql: "[start_time] < [end_time]");
        }
    }
}
