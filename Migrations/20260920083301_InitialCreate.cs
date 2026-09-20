using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ACADEMIC_YEARS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ACADEMIC_YEARS", x => x.id);
                    table.CheckConstraint("CK_ACADEMIC_YEARS_date_range", "[start_date] <= [end_date]");
                    table.CheckConstraint("CK_ACADEMIC_YEARS_status", "[status] IN ('PLANNED', 'ACTIVE', 'COMPLETED')");
                });

            migrationBuilder.CreateTable(
                name: "COURSES",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    course_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    credits = table.Column<int>(type: "int", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COURSES", x => x.id);
                    table.CheckConstraint("CK_COURSES_credits", "[credits] >= 0");
                    table.CheckConstraint("CK_COURSES_status", "[status] IN ('ACTIVE', 'INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "ROLES",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ROLES", x => x.id);
                    table.CheckConstraint("CK_ROLES_status", "[status] IN ('ACTIVE', 'INACTIVE')");
                });

            migrationBuilder.CreateTable(
                name: "USERS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "varchar(320)", maxLength: 320, nullable: false),
                    password_hash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false),
                    full_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    date_of_birth = table.Column<DateOnly>(type: "date", nullable: true),
                    gender = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    phone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    address = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    avatar_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USERS", x => x.id);
                    table.CheckConstraint("CK_USERS_gender", "[gender] IN ('MALE', 'FEMALE', 'OTHER')");
                });

            migrationBuilder.CreateTable(
                name: "SEMESTERS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    academic_year_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SEMESTERS", x => x.id);
                    table.CheckConstraint("CK_SEMESTERS_date_range", "[start_date] <= [end_date]");
                    table.CheckConstraint("CK_SEMESTERS_status", "[status] IN ('PLANNED', 'ACTIVE', 'COMPLETED')");
                    table.ForeignKey(
                        name: "FK_SEMESTERS_ACADEMIC_YEARS_academic_year_id",
                        column: x => x.academic_year_id,
                        principalTable: "ACADEMIC_YEARS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "USER_ROLES",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_USER_ROLES", x => x.id);
                    table.CheckConstraint("CK_USER_ROLES_status", "[status] IN ('ACTIVE', 'INACTIVE')");
                    table.ForeignKey(
                        name: "FK_USER_ROLES_ROLES_role_id",
                        column: x => x.role_id,
                        principalTable: "ROLES",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_USER_ROLES_USERS_user_id",
                        column: x => x.user_id,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "REGISTRATION_PERIODS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    semester_id = table.Column<long>(type: "bigint", nullable: false),
                    start_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    end_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_REGISTRATION_PERIODS", x => x.id);
                    table.CheckConstraint("CK_REGISTRATION_PERIODS_date_range", "[start_at] < [end_at]");
                    table.CheckConstraint("CK_REGISTRATION_PERIODS_status", "[status] IN ('UPCOMING', 'OPEN', 'CLOSED')");
                    table.ForeignKey(
                        name: "FK_REGISTRATION_PERIODS_SEMESTERS_semester_id",
                        column: x => x.semester_id,
                        principalTable: "SEMESTERS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_REGISTRATION_PERIODS_USERS_created_by",
                        column: x => x.created_by,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "COURSE_SECTIONS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    course_id = table.Column<long>(type: "bigint", nullable: false),
                    semester_id = table.Column<long>(type: "bigint", nullable: false),
                    teacher_user_role_id = table.Column<long>(type: "bigint", nullable: false),
                    section_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    capacity = table.Column<int>(type: "int", nullable: false),
                    day_of_week = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    start_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time", nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COURSE_SECTIONS", x => x.id);
                    table.CheckConstraint("CK_COURSE_SECTIONS_capacity", "[capacity] >= 0");
                    table.CheckConstraint("CK_COURSE_SECTIONS_date_range", "[start_date] <= [end_date]");
                    table.CheckConstraint("CK_COURSE_SECTIONS_day_of_week", "[day_of_week] IN ('MONDAY', 'TUESDAY', 'WEDNESDAY', 'THURSDAY', 'FRIDAY', 'SATURDAY', 'SUNDAY')");
                    table.CheckConstraint("CK_COURSE_SECTIONS_status", "[status] IN ('PLANNED', 'OPEN', 'CLOSED', 'COMPLETED', 'CANCELLED')");
                    table.CheckConstraint("CK_COURSE_SECTIONS_time_range", "[start_time] < [end_time]");
                    table.ForeignKey(
                        name: "FK_COURSE_SECTIONS_COURSES_course_id",
                        column: x => x.course_id,
                        principalTable: "COURSES",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_COURSE_SECTIONS_SEMESTERS_semester_id",
                        column: x => x.semester_id,
                        principalTable: "SEMESTERS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_COURSE_SECTIONS_USER_ROLES_teacher_user_role_id",
                        column: x => x.teacher_user_role_id,
                        principalTable: "USER_ROLES",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ANNOUNCEMENTS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    section_id = table.Column<long>(type: "bigint", nullable: true),
                    title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ANNOUNCEMENTS", x => x.id);
                    table.CheckConstraint("CK_ANNOUNCEMENTS_status", "[status] IN ('DRAFT', 'PUBLISHED', 'ARCHIVED')");
                    table.ForeignKey(
                        name: "FK_ANNOUNCEMENTS_COURSE_SECTIONS_section_id",
                        column: x => x.section_id,
                        principalTable: "COURSE_SECTIONS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ANNOUNCEMENTS_USERS_created_by",
                        column: x => x.created_by,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "ENROLLMENTS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    student_user_role_id = table.Column<long>(type: "bigint", nullable: false),
                    section_id = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ENROLLMENTS", x => x.id);
                    table.CheckConstraint("CK_ENROLLMENTS_status", "[status] IN ('ENROLLED', 'DROPPED', 'COMPLETED')");
                    table.ForeignKey(
                        name: "FK_ENROLLMENTS_COURSE_SECTIONS_section_id",
                        column: x => x.section_id,
                        principalTable: "COURSE_SECTIONS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_ENROLLMENTS_USER_ROLES_student_user_role_id",
                        column: x => x.student_user_role_id,
                        principalTable: "USER_ROLES",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "GRADE_COMPONENTS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    section_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GRADE_COMPONENTS", x => x.id);
                    table.CheckConstraint("CK_GRADE_COMPONENTS_weight", "[weight] >= 0 AND [weight] <= 100");
                    table.ForeignKey(
                        name: "FK_GRADE_COMPONENTS_COURSE_SECTIONS_section_id",
                        column: x => x.section_id,
                        principalTable: "COURSE_SECTIONS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "COURSE_RESULTS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    final_score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    letter_grade = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    grade_point = table.Column<decimal>(type: "decimal(4,2)", precision: 4, scale: 2, nullable: true),
                    result_status = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_COURSE_RESULTS", x => x.id);
                    table.CheckConstraint("CK_COURSE_RESULTS_final_score", "[final_score] >= 0 AND [final_score] <= 100");
                    table.CheckConstraint("CK_COURSE_RESULTS_grade_point", "[grade_point] >= 0");
                    table.CheckConstraint("CK_COURSE_RESULTS_result_status", "[result_status] IN ('PASS', 'FAIL', 'INCOMPLETE')");
                    table.ForeignKey(
                        name: "FK_COURSE_RESULTS_ENROLLMENTS_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "ENROLLMENTS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "STUDENT_SCORES",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    enrollment_id = table.Column<long>(type: "bigint", nullable: false),
                    component_id = table.Column<long>(type: "bigint", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    updated_by = table.Column<long>(type: "bigint", nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_STUDENT_SCORES", x => x.id);
                    table.CheckConstraint("CK_STUDENT_SCORES_score", "[score] >= 0 AND [score] <= 100");
                    table.ForeignKey(
                        name: "FK_STUDENT_SCORES_ENROLLMENTS_enrollment_id",
                        column: x => x.enrollment_id,
                        principalTable: "ENROLLMENTS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_STUDENT_SCORES_GRADE_COMPONENTS_component_id",
                        column: x => x.component_id,
                        principalTable: "GRADE_COMPONENTS",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_STUDENT_SCORES_USERS_updated_by",
                        column: x => x.updated_by,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ACADEMIC_YEARS_isDelete_status",
                table: "ACADEMIC_YEARS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ANNOUNCEMENTS_created_by",
                table: "ANNOUNCEMENTS",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_ANNOUNCEMENTS_isDelete_status",
                table: "ANNOUNCEMENTS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ANNOUNCEMENTS_section_id",
                table: "ANNOUNCEMENTS",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_COURSE_RESULTS_isDelete",
                table: "COURSE_RESULTS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "UX_COURSE_RESULTS_enrollment_id",
                table: "COURSE_RESULTS",
                column: "enrollment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_COURSE_SECTIONS_course_id",
                table: "COURSE_SECTIONS",
                column: "course_id");

            migrationBuilder.CreateIndex(
                name: "IX_COURSE_SECTIONS_isDelete_status",
                table: "COURSE_SECTIONS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_COURSE_SECTIONS_teacher_user_role_id",
                table: "COURSE_SECTIONS",
                column: "teacher_user_role_id");

            migrationBuilder.CreateIndex(
                name: "UX_COURSE_SECTIONS_semester_id_section_code",
                table: "COURSE_SECTIONS",
                columns: new[] { "semester_id", "section_code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_COURSES_isDelete_status",
                table: "COURSES",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "UX_COURSES_course_code",
                table: "COURSES",
                column: "course_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENTS_isDelete_status",
                table: "ENROLLMENTS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENTS_section_id",
                table: "ENROLLMENTS",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "UX_ENROLLMENTS_student_user_role_id_section_id",
                table: "ENROLLMENTS",
                columns: new[] { "student_user_role_id", "section_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GRADE_COMPONENTS_isDelete",
                table: "GRADE_COMPONENTS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "UX_GRADE_COMPONENTS_section_id_name",
                table: "GRADE_COMPONENTS",
                columns: new[] { "section_id", "name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_REGISTRATION_PERIODS_created_by",
                table: "REGISTRATION_PERIODS",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_REGISTRATION_PERIODS_isDelete_status",
                table: "REGISTRATION_PERIODS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_REGISTRATION_PERIODS_semester_id",
                table: "REGISTRATION_PERIODS",
                column: "semester_id");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_isDelete_status",
                table: "ROLES",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "UX_ROLES_code",
                table: "ROLES",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SEMESTERS_academic_year_id",
                table: "SEMESTERS",
                column: "academic_year_id");

            migrationBuilder.CreateIndex(
                name: "IX_SEMESTERS_isDelete_status",
                table: "SEMESTERS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_STUDENT_SCORES_component_id",
                table: "STUDENT_SCORES",
                column: "component_id");

            migrationBuilder.CreateIndex(
                name: "IX_STUDENT_SCORES_isDelete",
                table: "STUDENT_SCORES",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "IX_STUDENT_SCORES_updated_by",
                table: "STUDENT_SCORES",
                column: "updated_by");

            migrationBuilder.CreateIndex(
                name: "UX_STUDENT_SCORES_enrollment_id_component_id",
                table: "STUDENT_SCORES",
                columns: new[] { "enrollment_id", "component_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_isDelete_status",
                table: "USER_ROLES",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_role_id",
                table: "USER_ROLES",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "UX_USER_ROLES_user_id_role_id",
                table: "USER_ROLES",
                columns: new[] { "user_id", "role_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_USERS_isDelete",
                table: "USERS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "UX_USERS_email",
                table: "USERS",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ANNOUNCEMENTS");

            migrationBuilder.DropTable(
                name: "COURSE_RESULTS");

            migrationBuilder.DropTable(
                name: "REGISTRATION_PERIODS");

            migrationBuilder.DropTable(
                name: "STUDENT_SCORES");

            migrationBuilder.DropTable(
                name: "ENROLLMENTS");

            migrationBuilder.DropTable(
                name: "GRADE_COMPONENTS");

            migrationBuilder.DropTable(
                name: "COURSE_SECTIONS");

            migrationBuilder.DropTable(
                name: "COURSES");

            migrationBuilder.DropTable(
                name: "SEMESTERS");

            migrationBuilder.DropTable(
                name: "USER_ROLES");

            migrationBuilder.DropTable(
                name: "ACADEMIC_YEARS");

            migrationBuilder.DropTable(
                name: "ROLES");

            migrationBuilder.DropTable(
                name: "USERS");
        }
    }
}
