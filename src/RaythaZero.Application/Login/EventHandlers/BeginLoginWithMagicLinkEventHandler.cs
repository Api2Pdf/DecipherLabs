﻿using MediatR;
using RaythaZero.Application.Common.Interfaces;
using RaythaZero.Domain.Common;
using RaythaZero.Domain.Entities;
using RaythaZero.Domain.Events;
using CSharpVitamins;
using RaythaZero.Application.Common.Models.RenderModels;

namespace RaythaZero.Application.Login.EventHandlers;

public class BeginLoginWithMagicLinkEventHandler : INotificationHandler<BeginLoginWithMagicLinkEvent>
{
    private readonly IEmailer _emailerService;
    private readonly IRaythaDbContext _db;
    private readonly IRenderEngine _renderEngineService;
    private readonly IRelativeUrlBuilder _relativeUrlBuilderService;
    private readonly ICurrentOrganization _currentOrganization;

    public BeginLoginWithMagicLinkEventHandler(
        ICurrentOrganization currentOrganization,
        IRaythaDbContext db, 
        IEmailer emailerService, 
        IRenderEngine renderEngineService, 
        IRelativeUrlBuilder relativeUrlBuilderService)
    {
        _db = db;
        _emailerService = emailerService;
        _renderEngineService = renderEngineService;
        _relativeUrlBuilderService = relativeUrlBuilderService;
        _currentOrganization = currentOrganization;
    }

    public async Task Handle(BeginLoginWithMagicLinkEvent notification, CancellationToken cancellationToken)
    {
        if (notification.SendEmail)
        {
            EmailTemplate renderTemplate = _db.EmailTemplates.First(p => p.DeveloperName == BuiltInEmailTemplate.LoginBeginLoginWithMagicLinkEmail);
            SendBeginLoginWithMagicLink_RenderModel entity = new SendBeginLoginWithMagicLink_RenderModel
            {
                Id = (ShortGuid)notification.User.Id,
                FirstName = notification.User.FirstName,
                LastName = notification.User.LastName,
                EmailAddress = notification.User.EmailAddress,
                LoginWithMagicLinkCompleteUrl = notification.User.IsAdmin ? _relativeUrlBuilderService.AdminLoginWithMagicLinkCompleteUrl(notification.Token, notification.ReturnUrl) : _relativeUrlBuilderService.UserLoginWithMagicLinkCompleteUrl(notification.Token, notification.ReturnUrl),
                SsoId = notification.User.SsoId,
                AuthenticationScheme = notification.User.AuthenticationScheme?.DeveloperName,
                IsAdmin = notification.User.IsAdmin,
                MagicLinkExpiresInSeconds = notification.MagicLinkExpiresInSeconds
            };

            var wrappedModel = new Wrapper_RenderModel
            {
                CurrentOrganization = CurrentOrganization_RenderModel.GetProjection(_currentOrganization),
                Target = entity
            };

            string subject = _renderEngineService.RenderAsHtml(renderTemplate.Subject, wrappedModel);
            string content = _renderEngineService.RenderAsHtml(renderTemplate.Content, wrappedModel);
            var emailMessage = new EmailMessage
            {
                Content = content,
                To = new List<string> { entity.EmailAddress },
                Subject = subject
            };
            _emailerService.SendEmail(emailMessage);
        }
    }
}
