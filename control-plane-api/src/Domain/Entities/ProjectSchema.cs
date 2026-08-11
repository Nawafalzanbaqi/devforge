using System;
using System.Collections.Generic;

namespace DevForge.ControlPlane.Domain.Entities;

public class Project : BaseEntity
{
    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid? ClientId { get; set; }
    public Client? Client { get; set; }
    
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? RepositoryUrl { get; set; }
    public string Status { get; set; } = "Draft";
    
    public ICollection<Schema> Schemas { get; set; } = new List<Schema>();
    public ICollection<GenerationJob> GenerationJobs { get; set; } = new List<GenerationJob>();
    public ICollection<Deployment> Deployments { get; set; } = new List<Deployment>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
    public ICollection<AiUsage> AiUsages { get; set; } = new List<AiUsage>();
    public ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
}

public class Schema : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    
    public int Version { get; set; } = 1;
    public string RawJson { get; set; } = "{}";
    public bool IsActive { get; set; } = true;
    
    public ICollection<Entity> Entities { get; set; } = new List<Entity>();
    public ICollection<GenerationJob> GenerationJobs { get; set; } = new List<GenerationJob>();
}

public class Entity : BaseEntity
{
    public Guid SchemaId { get; set; }
    public Schema Schema { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    public ICollection<Attribute> Attributes { get; set; } = new List<Attribute>();
}

public class Attribute : BaseEntity
{
    public Guid EntityId { get; set; }
    public Entity Entity { get; set; } = null!;
    
    public string Name { get; set; } = string.Empty;
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = "string";
    
    public bool IsNullable { get; set; } = true;
    public bool IsPrimaryKey { get; set; } = false;
    
    public bool IsForeignKey { get; set; } = false;
    public Guid? ReferencedEntityId { get; set; }
    public Entity? ReferencedEntity { get; set; }
    
    public string? ValidationRules { get; set; }
}
