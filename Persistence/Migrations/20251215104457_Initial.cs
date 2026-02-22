using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    concurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetRoles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    chatId = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "text", nullable: true),
                    createdDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    profilePicture = table.Column<string>(type: "text", nullable: true),
                    lastSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    userName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    normalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    emailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    passwordHash = table.Column<string>(type: "text", nullable: true),
                    securityStamp = table.Column<string>(type: "text", nullable: true),
                    concurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    phoneNumber = table.Column<string>(type: "text", nullable: true),
                    phoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    twoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    accessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetUsers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictApplications",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    applicationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    clientId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    clientSecret = table.Column<string>(type: "text", nullable: true),
                    clientType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    concurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    consentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    displayName = table.Column<string>(type: "text", nullable: true),
                    displayNames = table.Column<string>(type: "text", nullable: true),
                    jsonWebKeySet = table.Column<string>(type: "text", nullable: true),
                    permissions = table.Column<string>(type: "text", nullable: true),
                    postLogoutRedirectUris = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redirectUris = table.Column<string>(type: "text", nullable: true),
                    requirements = table.Column<string>(type: "text", nullable: true),
                    settings = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictApplications", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictScopes",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    concurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    descriptions = table.Column<string>(type: "text", nullable: true),
                    displayName = table.Column<string>(type: "text", nullable: true),
                    displayNames = table.Column<string>(type: "text", nullable: true),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    resources = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictScopes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    roleId = table.Column<string>(type: "text", nullable: false),
                    claimType = table.Column<string>(type: "text", nullable: true),
                    claimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetRoleClaims", x => x.id);
                    table.ForeignKey(
                        name: "fK_AspNetRoleClaims_AspNetRoles_roleId",
                        column: x => x.roleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    userId = table.Column<string>(type: "text", nullable: false),
                    claimType = table.Column<string>(type: "text", nullable: true),
                    claimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetUserClaims", x => x.id);
                    table.ForeignKey(
                        name: "fK_AspNetUserClaims_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    loginProvider = table.Column<string>(type: "text", nullable: false),
                    providerKey = table.Column<string>(type: "text", nullable: false),
                    providerDisplayName = table.Column<string>(type: "text", nullable: true),
                    userId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetUserLogins", x => new { x.loginProvider, x.providerKey });
                    table.ForeignKey(
                        name: "fK_AspNetUserLogins_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    roleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetUserRoles", x => new { x.userId, x.roleId });
                    table.ForeignKey(
                        name: "fK_AspNetUserRoles_AspNetRoles_roleId",
                        column: x => x.roleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fK_AspNetUserRoles_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    userId = table.Column<string>(type: "text", nullable: false),
                    loginProvider = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_AspNetUserTokens", x => new { x.userId, x.loginProvider, x.name });
                    table.ForeignKey(
                        name: "fK_AspNetUserTokens_AspNetUsers_userId",
                        column: x => x.userId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "conversation",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    conversationId = table.Column<string>(type: "text", nullable: true),
                    user1 = table.Column<string>(type: "text", nullable: true),
                    user2 = table.Column<string>(type: "text", nullable: true),
                    lastMessageTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    createdDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_conversation", x => x.id);
                    table.ForeignKey(
                        name: "fK_conversation_AspNetUsers_user1",
                        column: x => x.user1,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fK_conversation_AspNetUsers_user2",
                        column: x => x.user2,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    applicationId = table.Column<string>(type: "text", nullable: true),
                    concurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    scopes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictAuthorizations", x => x.id);
                    table.ForeignKey(
                        name: "fK_OpenIddictAuthorizations_OpenIddictApplications_application~",
                        column: x => x.applicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    conversationId = table.Column<string>(type: "text", nullable: true),
                    senderId = table.Column<string>(type: "text", nullable: true),
                    receiverId = table.Column<string>(type: "text", nullable: true),
                    messageContent = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sentTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deliveredTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    readTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    createdDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updatedDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    isDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_messages", x => x.id);
                    table.ForeignKey(
                        name: "fK_messages_AspNetUsers_receiverId",
                        column: x => x.receiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fK_messages_AspNetUsers_senderId",
                        column: x => x.senderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fK_messages_conversation_conversationId",
                        column: x => x.conversationId,
                        principalTable: "conversation",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    applicationId = table.Column<string>(type: "text", nullable: true),
                    authorizationId = table.Column<string>(type: "text", nullable: true),
                    concurrencyToken = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    creationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    payload = table.Column<string>(type: "text", nullable: true),
                    properties = table.Column<string>(type: "text", nullable: true),
                    redemptionDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    referenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    subject = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    type = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pK_OpenIddictTokens", x => x.id);
                    table.ForeignKey(
                        name: "fK_OpenIddictTokens_OpenIddictApplications_applicationId",
                        column: x => x.applicationId,
                        principalTable: "OpenIddictApplications",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fK_OpenIddictTokens_OpenIddictAuthorizations_authorizationId",
                        column: x => x.authorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "iX_AspNetRoleClaims_roleId",
                table: "AspNetRoleClaims",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "normalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_AspNetUserClaims_userId",
                table: "AspNetUserClaims",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_AspNetUserLogins_userId",
                table: "AspNetUserLogins",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "iX_AspNetUserRoles_roleId",
                table: "AspNetUserRoles",
                column: "roleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "normalizedEmail");

            migrationBuilder.CreateIndex(
                name: "iX_AspNetUsers_chatId",
                table: "AspNetUsers",
                column: "chatId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "normalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_conversation_conversationId",
                table: "conversation",
                column: "conversationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_conversation_user1_lastMessageTime",
                table: "conversation",
                columns: new[] { "user1", "lastMessageTime" })
                .Annotation("Npgsql:IndexInclude", new[] { "conversationId", "user2" });

            migrationBuilder.CreateIndex(
                name: "iX_conversation_user1_user2",
                table: "conversation",
                columns: new[] { "user1", "user2" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_conversation_user2_lastMessageTime",
                table: "conversation",
                columns: new[] { "user2", "lastMessageTime" });

            migrationBuilder.CreateIndex(
                name: "iX_messages_conversationId_sentTime",
                table: "messages",
                columns: new[] { "conversationId", "sentTime" });

            migrationBuilder.CreateIndex(
                name: "iX_messages_receiverId_status_sentTime",
                table: "messages",
                columns: new[] { "receiverId", "status", "sentTime" });

            migrationBuilder.CreateIndex(
                name: "iX_messages_senderId",
                table: "messages",
                column: "senderId");

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictApplications_clientId",
                table: "OpenIddictApplications",
                column: "clientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictAuthorizations_applicationId_status_subject_type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "applicationId", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictScopes_name",
                table: "OpenIddictScopes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_applicationId_status_subject_type",
                table: "OpenIddictTokens",
                columns: new[] { "applicationId", "status", "subject", "type" });

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_authorizationId",
                table: "OpenIddictTokens",
                column: "authorizationId");

            migrationBuilder.CreateIndex(
                name: "iX_OpenIddictTokens_referenceId",
                table: "OpenIddictTokens",
                column: "referenceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "OpenIddictScopes");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "conversation");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OpenIddictApplications");
        }
    }
}
