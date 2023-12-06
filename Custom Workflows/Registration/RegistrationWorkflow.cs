using System.Net;
using System.Net.Http;
using Elsa.Activities.ControlFlow;
using Elsa.Activities.Email;
using Elsa.Activities.Http;
using Elsa.Activities.Http.Extensions;
using Elsa.Activities.Http.Models;
using Elsa.Activities.Primitives;
using Elsa.Activities.Temporal;
using Elsa.Builders;
using NodaTime;

using ElsaGuides.ContentApproval.Web.Custom_Workflows.Registration;

namespace ElsaGuides.ContentApproval.Web;

public class RegistroWorkflow : IWorkflow
{
    public void Build(IWorkflowBuilder builder)
    {
        builder
            .HttpEndpoint(
                x => x
                    .WithMethod("POST")
                    .WithPath("register")
                    .WithReadContent())
            .SetVariable("data", context => context.GetInput<HttpRequestModel>()!.Body)
            .Then<ValidateCompliance>()
            .Then<SaveDataActivity>()
            .SendEmail(activity => activity
                .WithSender("non.reply@avangenio.com")
                .WithRecipient("approve1@avangenio.com")
                .WithSubject(context => $"Confirmación de registro")
                .WithBody(context =>
                {
                    var data = context.GetVariable<dynamic>("data")!;
                    return $"Nuevo registro de {data.username}. <br><a href=\"{context.GenerateSignalUrl("Aprobacion1")}\">Confirmar</a> or <a href=\"{context.GenerateSignalUrl("Rechazo1")}\">Rechaza</a>";
                }))
            .SendEmail(activity => activity
                .WithSender("non.reply@avangenio.com")
                .WithRecipient("approve1@avangenio.com")
                .WithSubject(context => $"Confirmación de registro")
                .WithBody(context =>
                {
                    var data = context.GetVariable<dynamic>("data")!;
                    return $"Nuevo registro de {data.username}. <br><a href=\"{context.GenerateSignalUrl("Aprobacion2")}\">Confirmar</a> or <a href=\"{context.GenerateSignalUrl("Rechazo2")}\">Rechaza</a>";
                }))
            .WriteHttpResponse(
                    HttpStatusCode.OK,
                    "<h1>El Registro está esperando por aprobación</h1>",
                    "text/html")
            // .Then<ConfirmRegistrationActivity>()
            .Then<Fork>(activity => activity.WithBranches("Aprobacion1", "Aprobacion2"), fork =>
            {
                fork
                    .When("Aprobacion1")
                    .SignalReceived("Aprobacion1")
                    .Then<ApprovalActivity1>()
                    .ThenNamed("Join");

                fork
                    .When("Rechazo1")
                    .SignalReceived("Rechazo1")
                    .Then<RechazedActivity1>()
                    .ThenNamed("Join");
                
                fork
                    .When("Aprobacion1")
                    .SignalReceived("Aprobacion1")
                    .Then<ApprovalActivity1>()
                    .ThenNamed("Join");

                fork
                    .When("Rechazo2")
                    .SignalReceived("Rechazo2")
                    .Then<RechazedActivity2>()
                    .ThenNamed("Join");
            })
            .Add<Join>(join => join.WithMode(Join.JoinMode.WaitAny)).WithName("Join")
            .Then<FinalizeActivity>();
    }
}
