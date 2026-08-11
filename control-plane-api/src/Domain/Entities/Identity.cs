using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    
    public ICollection<OrganizationMember> Memberships { get; set; } = new List<OrganizationMember>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
}

public class Organization : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid OwnerId { get; set; }
    public User Owner { get; set; } = null!;
    
    public ICollection<OrganizationMember> Members { get; set; } = new List<OrganizationMember>();
    public ICollection<Client> Clients { get; set; } = new List<Client>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public Billing Billing { get; set; } = null!;
}

public class OrganizationMember
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    
    public string Role { get; set; } = "Member";
    public DateTimeOffset JoinedAt { get; set; } = DateTimeOffset.UtcNow;
}
