﻿using RaythaZero.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace RaythaZero.Application.Common.Interfaces;

public interface IRaythaDbContext 
{
    public DbSet<User> Users { get; }
    public DbSet<Role> Roles { get; }
    public DbSet<UserGroup> UserGroups { get; }
    public DbSet<VerificationCode> VerificationCodes { get; }
    public DbSet<Domain.Entities.OrganizationSettings> OrganizationSettings { get; }
    public DbSet<EmailTemplate> EmailTemplates { get; }
    public DbSet<EmailTemplateRevision> EmailTemplateRevisions { get; }
    public DbSet<AuthenticationScheme> AuthenticationSchemes { get; }
    public DbSet<JwtLogin> JwtLogins { get; }
    public DbSet<OneTimePassword> OneTimePasswords { get; }
    public DbSet<AuditLog> AuditLogs { get; }
    public DbSet<MediaItem> MediaItems { get; }
    public DbSet<ApiKey> ApiKeys { get; }
    public DbSet<BackgroundTask> BackgroundTasks { get; }
    public DbSet<Project> Projects { get; }
    public DbSet<Prompt> Prompts { get; }
    public DbContext DbContext { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
