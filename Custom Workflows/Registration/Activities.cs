using System.Net;
using System.Text.RegularExpressions;
using Elsa.Activities;
using Elsa.ActivityResults;
using Elsa.Services;
using Elsa.Services.Models;

using ElsaGuides.ContentApproval.Web.Entity;

namespace ElsaGuides.ContentApproval.Web.Custom_Workflows.Registration;

public class ValidateCompliance : Activity
{
    public readonly IHttpContextAccessor _httpContextAccessor;

    public ValidateCompliance(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

   protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
   {
        // Obtén los datos de la variable "data".
        var data = context.GetVariable<dynamic>("data");
        var username = data.username;
        var password = data.password;
        var email = data.email;

        // Validar que el nombre de usuario no esté vacío
        if (string.IsNullOrWhiteSpace(username))
        {
            return Fault("El nombre de usuario no puede estar vacío.");
        }

        // Validar que la contraseña no esté vacía
        if (string.IsNullOrWhiteSpace(password))
        {
            return Fault("La contraseña no puede estar vacía.");
        }

        // Validar que el correo electrónico no esté vacío
        if (string.IsNullOrWhiteSpace(email))
        {
            return Fault("El correo electrónico no puede estar vacío.");
        }

        // Validar que el correo electrónico tenga el formato correcto
        if (!Regex.IsMatch(email, @"^[\w-]+(\.[\w-]+)*@([\w-]+\.)+[a-zA-Z]{2,7}$"))
        {
            return Fault("El correo electrónico no tiene un formato válido.");
        }

        return Done();
   }
}


public class SaveDataActivity : Activity
{
    private readonly MyDbContext _dbContext;

    public SaveDataActivity(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");

        // Crea una nueva entidad con los datos.
        var entity = new Register
        {
            Username = data.username,
            Password = data.password,
            Email = data.email
        };

        // Guarda la entidad en la base de datos.
        await _dbContext.MyEntities.AddAsync(entity);
        await _dbContext.SaveChangesAsync();

        return Done();
    }
}


public class ApprovalActivity1 : Activity
{
    private readonly MyDbContext _dbContext;

    public ApprovalActivity1(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");

        // Busca la entidad en la base de datos.
        var entity = await _dbContext.MyEntities.FindAsync(data.Id);

        if (entity == null)
        {
            // Si la entidad no se encuentra, puedes detener el flujo de trabajo aquí.
            return Fault("La entidad no se encontró en la base de datos.");
        }

        entity.IsConfirmed1 = true;
        _dbContext.MyEntities.Update(entity);
        await _dbContext.SaveChangesAsync();

        return Done();
    }
}


public class RechazedActivity1 : Activity
{
    private readonly MyDbContext _dbContext;

    public RechazedActivity1(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");

        // Busca la entidad en la base de datos.
        var entity = await _dbContext.MyEntities.FindAsync(data.Id);

        if (entity == null)
        {
            // Si la entidad no se encuentra, puedes detener el flujo de trabajo aquí.
            return Fault("La entidad no se encontró en la base de datos.");
        }

        entity.IsConfirmed1 = false;
        _dbContext.MyEntities.Update(entity);
        await _dbContext.SaveChangesAsync();

        return Done();
    }
}



public class ApprovalActivity2 : Activity
{
    private readonly MyDbContext _dbContext;

    public ApprovalActivity2(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");

        // Busca la entidad en la base de datos.
        var entity = await _dbContext.MyEntities.FindAsync(data.Id);

        if (entity == null)
        {
            // Si la entidad no se encuentra, puedes detener el flujo de trabajo aquí.
            return Fault("La entidad no se encontró en la base de datos.");
        }

        entity.IsConfirmed2 = true;
        _dbContext.MyEntities.Update(entity);
        await _dbContext.SaveChangesAsync();

        return Done();
    }
}


public class RechazedActivity2 : Activity
{
    private readonly MyDbContext _dbContext;

    public RechazedActivity2(MyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");

        // Busca la entidad en la base de datos.
        var entity = await _dbContext.MyEntities.FindAsync(data.Id);

        if (entity == null)
        {
            // Si la entidad no se encuentra, puedes detener el flujo de trabajo aquí.
            return Fault("La entidad no se encontró en la base de datos.");
        }

        entity.IsConfirmed1 = false;
        _dbContext.MyEntities.Update(entity);
        await _dbContext.SaveChangesAsync();

        return Done();
    }
}


public class FinalizeActivity : Activity
{
    private readonly MyDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public FinalizeActivity(MyDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async ValueTask<IActivityExecutionResult> OnExecuteAsync(ActivityExecutionContext context)
    {
        var data = context.GetVariable<dynamic>("data");
        var aprobacion1 = context.GetVariable<bool>("Aprobacion1");
        var aprobacion2 = context.GetVariable<bool>("Aprobacion2");

        // Busca la entidad en la base de datos.
        var entity = await _dbContext.MyEntities.FindAsync(data.Id);

        if (entity == null)
        {
            // Si la entidad no se encuentra, puedes detener el flujo de trabajo aquí.
            return Fault("La entidad no se encontró en la base de datos.");
        }

        if (!entity.IsConfirmed1 && !entity.IsConfirmed2)
        {
            return Fault("La entidad no ha sido aprobada por ambos administradores.");
        }

        entity.IsActivate = true;
        _dbContext.MyEntities.Update(entity);
        await _dbContext.SaveChangesAsync();

        // Comprueba si ambas aprobaciones son verdaderas.
        if (aprobacion1 && aprobacion2)
        {
            // Si ambas aprobaciones son verdaderas, envía una respuesta HTTP con el mensaje "¡Ambas aprobaciones recibidas!".
            var response = _httpContextAccessor.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html";
            await response.WriteAsync("¡Ambas aprobaciones recibidas! Haz sido registrado con éxito.");
        }
        else
        {
            // Si no, envía una respuesta HTTP con el mensaje "No se recibieron ambas aprobaciones.".
            var response = _httpContextAccessor.HttpContext.Response;
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = "text/html";
            await response.WriteAsync("No se recibieron ambas aprobaciones.");
        }
        
        return Done();
    }
}
