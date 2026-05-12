using ControleDePatio.Data;
using ControleDePatio.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=patio.db"));

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();


app.UseHttpsRedirection();

app.MapPost("/patio/iniciar", async (AppDbContext db) =>
{
    if (db.Vagas.Any() || db.Veiculos.Any())
        return Results.BadRequest("O sistema já foi inicializado.");

    var novasVagas = new List<Vaga>();
    for (int i = 1; i <= 10; i++)
    {
        novasVagas.Add(new Vaga { Identificacao = $"Vaga {i:D2}", EstaOcupada = true });
    }
    db.Vagas.AddRange(novasVagas);
    await db.SaveChangesAsync(); 

    for (int i = 1; i <= 10; i++)
    {
        var veiculo = new Veiculo
        {
            Modelo = $"Trator Modelo {i}",
            Placa = $"TRT-000{i}",
            EstaNoPatio = true,
            VagaId = novasVagas[i - 1].Id 
        };
        db.Veiculos.Add(veiculo);
    }

    await db.SaveChangesAsync();
    return Results.Ok("Pátio inicializado! 10 vagas e 10 tratores criados e vinculados.");
});

app.MapPost("/patio/saida/{veiculoId}", async (int veiculoId, SaidaRequest request, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null || !veiculo.EstaNoPatio)
        return Results.BadRequest("Veículo não encontrado ou já está na rua.");

    var vaga = await db.Vagas.FindAsync(veiculo.VagaId);
    if (vaga != null) vaga.EstaOcupada = false;

    db.Movimentacoes.Add(new Movimentacao
    {
        VeiculoId = veiculoId,
        NomeCondutor = request.NomeCondutor,
        HorarioSaida = DateTime.Now
    });

    veiculo.EstaNoPatio = false;
    veiculo.VagaId = null;

    await db.SaveChangesAsync();
    return Results.Ok($"O {veiculo.Modelo} saiu com o {request.NomeCondutor}. A {vaga?.Identificacao} agora está LIVRE!");
});

app.MapPost("/patio/retorno/veiculo/{veiculoId}/vaga/{vagaId}", async (int veiculoId, int vagaId, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null || veiculo.EstaNoPatio)
        return Results.BadRequest("Veículo não encontrado ou já está no pátio.");

    var vagaEscolhida = await db.Vagas.FindAsync(vagaId);
    if (vagaEscolhida == null || vagaEscolhida.EstaOcupada)
        return Results.BadRequest("Esta vaga não existe ou já está ocupada por outro trator!");

    var movimentacao = await db.Movimentacoes.FirstOrDefaultAsync(m => m.VeiculoId == veiculoId && m.HorarioRetorno == null);
    if (movimentacao != null) movimentacao.HorarioRetorno = DateTime.Now;

    veiculo.EstaNoPatio = true;
    veiculo.VagaId = vagaEscolhida.Id; 
    vagaEscolhida.EstaOcupada = true;  

    await db.SaveChangesAsync();
    return Results.Ok($"Retorno registrado! O {veiculo.Modelo} estacionou na {vagaEscolhida.Identificacao}.");
});

app.MapGet("/patio/veiculos", async (AppDbContext db) =>
{
    var veiculos = await db.Veiculos.ToListAsync();

    return Results.Ok(veiculos);
});

app.MapGet("/patio/historico", async (AppDbContext db) =>
{
    var historico = await db.Movimentacoes
        .OrderByDescending(m => m.HorarioSaida)
        .ToListAsync();

    return Results.Ok(historico);
});


app.MapPost("/patio/veiculos", async (NovoVeiculoRequest request, AppDbContext db) =>
{
    var novoVeiculo = new Veiculo
    {
        Modelo = request.Modelo,
        Placa = request.Placa,
        EstaNoPatio = false, 
        VagaId = null
    };

    if (request.VagaIdDesejada.HasValue)
    {
        var vaga = await db.Vagas.FindAsync(request.VagaIdDesejada.Value);
        if (vaga == null || vaga.EstaOcupada)
            return Results.BadRequest("A vaga desejada não existe ou já está ocupada!");

        novoVeiculo.EstaNoPatio = true;
        novoVeiculo.VagaId = vaga.Id;
        vaga.EstaOcupada = true; 
    }

    db.Veiculos.Add(novoVeiculo);
    await db.SaveChangesAsync();

    string status = novoVeiculo.EstaNoPatio ? $"estacionado na vaga {novoVeiculo.VagaId}" : "na rua";
    return Results.Ok($"Trator {novoVeiculo.Modelo} cadastrado com sucesso e está {status}!");
});

app.MapDelete("/patio/veiculos/{veiculoId}", async (int veiculoId, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null) return Results.NotFound("Veículo não encontrado.");

    if (veiculo.EstaNoPatio && veiculo.VagaId.HasValue)
    {
        var vaga = await db.Vagas.FindAsync(veiculo.VagaId.Value);
        if (vaga != null) vaga.EstaOcupada = false; 
    }

    db.Veiculos.Remove(veiculo);
    await db.SaveChangesAsync();
    return Results.Ok($"Trator {veiculo.Modelo} removido da frota definitivamente.");
});

app.MapPost("/patio/vagas", async (NovaVagaRequest request, AppDbContext db) =>
{
    var novaVaga = new Vaga { Identificacao = request.Identificacao, EstaOcupada = false };
    db.Vagas.Add(novaVaga);
    await db.SaveChangesAsync();
    return Results.Ok($"Vaga física '{novaVaga.Identificacao}' construída com sucesso! (ID: {novaVaga.Id})");
});

app.MapGet("/patio/vagas", async (AppDbContext db) =>
{
    var vagas = await db.Vagas.ToListAsync();

    return Results.Ok(vagas);
});

app.MapDelete("/patio/vagas/{vagaId}", async (int vagaId, AppDbContext db) =>
{
    var vaga = await db.Vagas.FindAsync(vagaId);
    if (vaga == null) return Results.NotFound("Vaga não encontrada.");

    if (vaga.EstaOcupada) return Results.BadRequest("Não é possível excluir uma vaga que está ocupada.");

    db.Vagas.Remove(vaga);
    await db.SaveChangesAsync();
    return Results.Ok($"Vaga {vaga.Identificacao} removida do sistema.");
});


app.Run();

public record SaidaRequest(string NomeCondutor);
public record NovaVagaRequest(string Identificacao);
public record NovoVeiculoRequest(string Modelo, string Placa, int? VagaIdDesejada);


