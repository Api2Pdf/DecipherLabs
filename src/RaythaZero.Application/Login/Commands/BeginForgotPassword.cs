using MediatR;
using FluentValidation;
using RaythaZero.Application.Common.Models;
using CSharpVitamins;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.ValueObjects;
using RaythaZero.Domain.Entities;
using RaythaZero.Domain.Events;
using RaythaZero.Application.Common.Utils;

namespace RaythaZero.Application.Login.Commands;

public class BeginForgotPassword
{
    public record Command : LoggableRequest<CommandResponseDto<ShortGuid>>
    {
        public string EmailAddress { get; init; } = null!;
        public bool SendEmail { get; init; } = true;
    }

    public class Validator : AbstractValidator<Command> 
    {
        public Validator(IRaythaDbContext db) 
        {
            RuleFor(x => x.EmailAddress).NotEmpty().EmailAddress();
            RuleFor(x => x).Custom((request, context) =>
            {
                var authScheme = db.AuthenticationSchemes.First(p =>
                    p.DeveloperName == AuthenticationSchemeType.EmailAndPassword.DeveloperName);

                if (!authScheme.IsEnabledForUsers && !authScheme.IsEnabledForAdmins)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "Authentication scheme is disabled.");
                    return;
                }

                var emailAddress = request.EmailAddress.ToLower().Trim();
                var entity = db.Users.FirstOrDefault(p => p.EmailAddress.ToLower() == emailAddress);

                if (entity == null)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "User not found.");
                    return;
                }

                if (!entity.IsActive)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "Your account has been deactivated.");
                    return;
                }

                if (entity.IsAdmin && !authScheme.IsEnabledForAdmins)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "Authentication scheme disabled for administrators.");
                    return;
                }

                if (!entity.IsAdmin && !authScheme.IsEnabledForUsers)
                {
                    context.AddFailure(Constants.VALIDATION_SUMMARY, "Authentication scheme disabled for public users.");
                    return;
                }
            });
        }
    }

    public class Handler : IRequestHandler<Command, CommandResponseDto<ShortGuid>>
    {
        private readonly IRaythaDbContext _db;
        public Handler(IRaythaDbContext db)
        {
            _db = db;
        }
        public async Task<CommandResponseDto<ShortGuid>> Handle(Command request, CancellationToken cancellationToken)
        {
            var entity = _db.Users.First(p => p.EmailAddress.ToLower() == request.EmailAddress.ToLower().Trim());

            ShortGuid guid = ShortGuid.NewGuid();
            var otp = new OneTimePassword
            {
                Id = PasswordUtility.Hash(guid),
                IsUsed = false,
                UserId = entity.Id,
                ExpiresAt = DateTime.UtcNow.AddSeconds(900)
            };

            _db.OneTimePasswords.Add(otp);

            entity.AddDomainEvent(new BeginForgotPasswordEvent(entity, request.SendEmail, guid));
            
            await _db.SaveChangesAsync(cancellationToken);
            return new CommandResponseDto<ShortGuid>(guid);
        }
    }
}