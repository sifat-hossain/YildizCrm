using MediatR;

namespace Yildiz.CRM.Applications.Actions.Auth.Commands;

public record LoginCommand : IRequest<LoginModel>;

