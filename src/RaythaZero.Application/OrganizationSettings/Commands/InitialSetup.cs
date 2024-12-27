﻿using CSharpVitamins;
using FluentValidation;
using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Application.Common.Models;
using RaythaZero.Application.Common.Utils;
using RaythaZero.Domain.Entities;
using RaythaZero.Domain.ValueObjects;
using System.Text.Json.Serialization;

namespace RaythaZero.Application.OrganizationSettings.Commands;

public class InitialSetup
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string FirstName { get; init; } = null!;
        public string LastName { get; init; } = null!;

        [JsonIgnore]
        public string SuperAdminEmailAddress { get; init; } = null!;

        [JsonIgnore]
        public string SuperAdminPassword { get; init; } = null!;
        public string OrganizationName { get; init; } = null!;
        public string WebsiteUrl { get; init; } = null!;
        public string TimeZone { get; init; } = null!;
        public string SmtpDefaultFromAddress { get; init; } = null!;
        public string SmtpDefaultFromName { get; init; } = null!;

        [JsonIgnore]
        public string SmtpHost { get; init; } = null!;

        [JsonIgnore]
        public int? SmtpPort { get; init; }

        [JsonIgnore]
        public string SmtpUsername { get; init; } = null!;

        [JsonIgnore]
        public string SmtpPassword { get; init; } = null!;
    }

    public class Validator : AbstractValidator<Command>
    {
        public Validator(IEmailerConfiguration emailerConfiguration)
        {
            RuleFor(x => x.SuperAdminPassword).NotEmpty().MinimumLength(8);
            RuleFor(x => x.SuperAdminEmailAddress).NotEmpty().EmailAddress();
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.SmtpHost).NotEmpty().When(p => emailerConfiguration.IsMissingSmtpEnvVars());
            RuleFor(x => x.SmtpPort).NotNull().GreaterThan(0).LessThanOrEqualTo(65535).When(p => emailerConfiguration.IsMissingSmtpEnvVars());
            RuleFor(x => x.OrganizationName).NotEmpty();
            RuleFor(x => x.TimeZone).NotEmpty().Must(DateTimeExtensions.IsValidTimeZone)
                .WithMessage(p => $"{p.TimeZone} timezone is unrecognized.");
            RuleFor(x => x.WebsiteUrl).Custom((request, context) =>
            {
                if (string.IsNullOrWhiteSpace(request))
                {
                    context.AddFailure("WebsiteUrl", "'Website url' must not be empty.");
                    return;
                }

                if (!request.IsValidUriFormat())
                {
                    context.AddFailure("WebsiteUrl", "'Website url' is not a valid URI format.");
                    return;
                }
            });
            RuleFor(x => x.SmtpDefaultFromAddress).NotEmpty().EmailAddress();
            RuleFor(x => x.SmtpDefaultFromName).NotEmpty();
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        Guid orgSettingsGuid = Guid.NewGuid();

        private readonly IRaythaDbContext _db;
        private readonly IEmailerConfiguration _emailerConfiguration;
        private readonly IFileStorageProvider _fileStorageProvider;

        public Handler(IRaythaDbContext db, IEmailerConfiguration emailerConfiguration, IFileStorageProvider fileStorageProvider)
        {
            _db = db;
            _emailerConfiguration = emailerConfiguration;
            _fileStorageProvider = fileStorageProvider;
        }

        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            InsertOrganizationSettings(request);
            InsertDefaultRolesAndSuperAdmin(request);

            InsertDefaultEmailTemplates();
            InsertDefaultAuthentications();
            await _db.SaveChangesAsync(cancellationToken);

            return new CommandResponseDto<ShortGuid>(orgSettingsGuid);
        }

        protected void InsertOrganizationSettings(Command request)
        {
            var entity = new Domain.Entities.OrganizationSettings
            {
                Id = orgSettingsGuid,
                SmtpHost = request.SmtpHost,
                SmtpPort = request.SmtpPort,
                SmtpUsername = request.SmtpUsername,
                SmtpPassword = request.SmtpPassword,
                SmtpOverrideSystem = _emailerConfiguration.IsMissingSmtpEnvVars(),
                OrganizationName = request.OrganizationName,
                TimeZone = request.TimeZone,
                DateFormat = DateTimeExtensions.DEFAULT_DATE_FORMAT,
                WebsiteUrl = request.WebsiteUrl,
                SmtpDefaultFromAddress = request.SmtpDefaultFromAddress,
                SmtpDefaultFromName = request.SmtpDefaultFromName
            };
            _db.OrganizationSettings.Add(entity);
        }

        protected void InsertDefaultRolesAndSuperAdmin(Command request)
        {
            var roles = new List<Role>();
            Role superAdminRole = new Role
            {
                Id = Guid.NewGuid(),
                Label = BuiltInRole.SuperAdmin.DefaultLabel,
                DeveloperName = BuiltInRole.SuperAdmin,
                SystemPermissions = BuiltInRole.SuperAdmin.DefaultSystemPermission,
                CreationTime = DateTime.UtcNow
            };
            roles.Add(superAdminRole);
            Role adminRole = new Role
            {
                Id = Guid.NewGuid(),
                Label = BuiltInRole.Admin.DefaultLabel,
                DeveloperName = BuiltInRole.Admin,
                SystemPermissions = BuiltInRole.Admin.DefaultSystemPermission,
                CreationTime = DateTime.UtcNow
            };
            roles.Add(adminRole);
            Role editorRole = new Role
            {
                Id = Guid.NewGuid(),
                Label = BuiltInRole.Editor.DefaultLabel,
                DeveloperName = BuiltInRole.Editor,
                SystemPermissions = BuiltInRole.Editor.DefaultSystemPermission,
                CreationTime = DateTime.UtcNow
            };
            roles.Add(editorRole);

            var salt = PasswordUtility.RandomSalt();
            var superAdmin = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.SuperAdminEmailAddress,
                Roles = roles,
                Salt = salt,
                PasswordHash = PasswordUtility.Hash(request.SuperAdminPassword, salt),
                IsActive = true,
                IsAdmin = true
            };
            _db.Users.Add(superAdmin);
        }

        protected void InsertDefaultEmailTemplates()
        {
            var list = new List<EmailTemplate>();

            foreach (var templateToBuild in BuiltInEmailTemplate.Templates)
            {
                var template = new EmailTemplate
                {
                    Id = Guid.NewGuid(),
                    Content = templateToBuild.DefaultContent,
                    Subject = templateToBuild.DefaultSubject,
                    DeveloperName = templateToBuild.DeveloperName,
                    IsBuiltInTemplate = true
                };
                list.Add(template);
            }

            _db.EmailTemplates.AddRange(list);
        }

        protected void InsertDefaultAuthentications()
        {
            var list = new List<AuthenticationScheme>
            {
                new AuthenticationScheme
                {
                    Label = "Email address and password",
                    DeveloperName = AuthenticationSchemeType.EmailAndPassword,
                    IsBuiltInAuth = true,
                    IsEnabledForAdmins = true,
                    IsEnabledForUsers = true,
                    AuthenticationSchemeType = AuthenticationSchemeType.EmailAndPassword,
                    LoginButtonText = "Login with your email and password",
                },

                new AuthenticationScheme
                {
                    Label = "Magic link",
                    DeveloperName = AuthenticationSchemeType.MagicLink,
                    IsBuiltInAuth = true,
                    IsEnabledForAdmins = false,
                    IsEnabledForUsers = false,
                    AuthenticationSchemeType = AuthenticationSchemeType.MagicLink,
                    LoginButtonText = "Email me a login link",
                    MagicLinkExpiresInSeconds = 900
                }
            };
            _db.AuthenticationSchemes.AddRange(list);
        }
    }
}
