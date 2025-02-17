﻿using Microsoft.EntityFrameworkCore;
using MediatR;
using System.Reflection;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Entities;
using RaythaZero.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace RaythaZero.Infrastructure.Persistence;

public class RaythaDbContext : DbContext, IRaythaDbContext, IDataProtectionKeyContext
{
    private readonly IMediator _mediator;
    private readonly AuditableEntitySaveChangesInterceptor _auditableEntitySaveChangesInterceptor;
    private readonly IConfiguration _configuration;

    public RaythaDbContext(
        DbContextOptions<RaythaDbContext> options)
        : base(options)
    {
    }

    public RaythaDbContext(
        DbContextOptions<RaythaDbContext> options,
        IConfiguration configuration,
        IMediator mediator,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options)
    {
        _configuration = configuration; 
        _mediator = mediator;
        _auditableEntitySaveChangesInterceptor = auditableEntitySaveChangesInterceptor;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserGroup> UserGroups => Set<UserGroup>();
    public DbSet<VerificationCode> VerificationCodes => Set<VerificationCode>();
    public DbSet<OrganizationSettings> OrganizationSettings => Set<OrganizationSettings>();
    public DbSet<EmailTemplate> EmailTemplates => Set<EmailTemplate>();
    public DbSet<EmailTemplateRevision> EmailTemplateRevisions => Set<EmailTemplateRevision>();
    public DbSet<AuthenticationScheme> AuthenticationSchemes => Set<AuthenticationScheme>();
    public DbSet<JwtLogin> JwtLogins => Set<JwtLogin>();
    public DbSet<OneTimePassword> OneTimePasswords => Set<OneTimePassword>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();
    public DbSet<BackgroundTask> BackgroundTasks => Set<BackgroundTask>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Prompt> Prompts => Set<Prompt>();
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; } = null!;

    public DbContext DbContext => DbContext;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        base.OnModelCreating(builder);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_auditableEntitySaveChangesInterceptor);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await _mediator.DispatchDomainEventsBeforeSaveChanges(this);
        var numItems = await base.SaveChangesAsync(cancellationToken);
        await _mediator.DispatchDomainEventsAfterSaveChanges(this);

        return numItems;
    }
}
