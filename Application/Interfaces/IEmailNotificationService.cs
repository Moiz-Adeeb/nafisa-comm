namespace Application.Interfaces;

public sealed class EmailMessage
{
    public List<string> To { get; set; } = new();
    public string Subject { get; set; } = "";
    public string HtmlBody { get; set; } = "";
    public Dictionary<string, string> Headers { get; set; } = new();
    public List<EmailAttachment> Attachments { get; set; } = new();
}

public sealed class EmailAttachment
{
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "application/octet-stream";
    public byte[]? Bytes { get; set; }
}

// -----------------------------
// Public API
// -----------------------------

public interface IEmailNotificationService
{
    // SEMD
    Task SendSemdModRequestAsync(SemdModRequest dto, CancellationToken ct = default);

    // Chief
    Task NotifyChiefModDecisionAsync(ChiefModDecision dto, CancellationToken ct = default);
    Task NotifyChiefAssignmentAsync(ChiefAssignment dto, CancellationToken ct = default);
    Task SendChiefWorkOrderDistributionAsync(
        ChiefWorkOrder dto,
        byte[]? workOrderPdf,
        CancellationToken ct = default
    );

    // Provider
    Task SendProviderProvisionalOrderAsync(
        ProviderProvisional dto,
        byte[]? csv,
        CancellationToken ct = default
    );
    Task SendProviderUpdatedRequestAsync(
        ProviderUpdated dto,
        byte[]? csv,
        CancellationToken ct = default
    );
    Task SendProviderWorkOrderDistributionAsync(
        ProviderWorkOrder dto,
        byte[]? pdf,
        CancellationToken ct = default
    );
    Task SendProviderBillingStatementAsync(
        ProviderBillingStatement dto,
        byte[]? pdf,
        CancellationToken ct = default
    );

    // User management
    Task SendUserInviteOrResetAsync(UserInviteReset dto, CancellationToken ct = default);
}

// -----------------------------
// DTOs (simple, typed inputs)
// -----------------------------
public sealed record SemdModRequest(
    string SemdEmail,
    string SemdReplyToEmail,
    string StructureName,
    string Date,
    string SiteName,
    string ShiftName,
    string ActivityName,
    int RequestedCount,
    string Notes,
    string ChiefName,
    string Action, // "créée", "modifiée", "created", "updated", etc. (already localized if you want)
    string Link,
    bool Late
);

public sealed record ChiefModDecision(
    string ChiefEmail,
    string ChiefName,
    string Date,
    string SiteName,
    string ShiftName,
    string ActivityName,
    string Status, // e.g., "Approved" | "Rejected" (or FR text)
    string DecisionNotes,
    string Link
);

public sealed record DockerItem(string Name, string Id, string Skill);

public sealed record ChiefAssignment(
    string ChiefEmail,
    string ChiefName,
    string Date,
    string SiteName,
    string ShiftName,
    List<DockerItem> DockerList,
    string Link
);

public sealed record ChiefWorkOrder(
    string ChiefEmail,
    string ChiefName,
    string Date,
    string SiteName,
    string ShiftName,
    string Link
);

public sealed record ProviderProvisional(
    string ProviderEmail,
    string ProviderName,
    string Date,
    string SiteName,
    string ShiftName,
    string ActivityName,
    int RequestedCount
);

public sealed record ProviderUpdated(
    string ProviderEmail,
    string ProviderName,
    string Date,
    string SiteName,
    string ShiftName,
    int OldCount,
    int NewCount,
    string Reason
);

public sealed record ProviderWorkOrder(string ProviderEmail, string ProviderName, string Date);

public sealed record ProviderBillingStatement(
    string ProviderEmail,
    string ProviderName,
    string Period,
    string TotalAmount,
    string Link
);

public sealed record UserInviteReset(
    string UserEmail,
    string UserName,
    string ResetLink,
    string AdminSupportEmail
);
