using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Synapse.Migrations
{
    /// <inheritdoc />
    public partial class AddEncryptedPrivateMessages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Messages",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Ciphertext",
                table: "Messages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedKeyForRecipient",
                table: "Messages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedKeyForSender",
                table: "Messages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Nonce",
                table: "Messages",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ciphertext",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "EncryptedKeyForRecipient",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "EncryptedKeyForSender",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "Nonce",
                table: "Messages");

            migrationBuilder.UpdateData(
                table: "Messages",
                keyColumn: "MessageContent",
                keyValue: null,
                column: "MessageContent",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "MessageContent",
                table: "Messages",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
