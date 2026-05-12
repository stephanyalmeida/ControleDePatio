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

// ROTA 1: Inicializar o Pátio Físico e os 10 Tratores
app.MapPost("/patio/iniciar", async (AppDbContext db) =>
{
    if (db.Vagas.Any() || db.Veiculos.Any())
        return Results.BadRequest("O sistema já foi inicializado.");

    // Passo A: Construir as 10 vagas físicas primeiro
    var novasVagas = new List<Vaga>();
    for (int i = 1; i <= 10; i++)
    {
        novasVagas.Add(new Vaga { Identificacao = $"Vaga {i:D2}", EstaOcupada = true });
    }
    db.Vagas.AddRange(novasVagas);
    await db.SaveChangesAsync(); // Salvamos para gerar os IDs das vagas

    // Passo B: Comprar os 10 tratores e estacionar cada um em uma vaga
    for (int i = 1; i <= 10; i++)
    {
        var veiculo = new Veiculo
        {
            Modelo = $"Trator Modelo {i}",
            Placa = $"TRT-000{i}",
            EstaNoPatio = true,
            VagaId = novasVagas[i - 1].Id // Vincula o trator à vaga correspondente
        };
        db.Veiculos.Add(veiculo);
    }

    await db.SaveChangesAsync();
    return Results.Ok("Pátio inicializado! 10 vagas e 10 tratores criados e vinculados.");
});

// ROTA 2: Registrar Saída (Libera a vaga)
app.MapPost("/patio/saida/{veiculoId}", async (int veiculoId, SaidaRequest request, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null || !veiculo.EstaNoPatio)
        return Results.BadRequest("Veículo não encontrado ou já está na rua.");

    // 1. Acha a vaga que ele estava ocupando e libera ela
    var vaga = await db.Vagas.FindAsync(veiculo.VagaId);
    if (vaga != null) vaga.EstaOcupada = false;

    // 2. Registra o histórico
    db.Movimentacoes.Add(new Movimentacao
    {
        VeiculoId = veiculoId,
        NomeCondutor = request.NomeCondutor,
        HorarioSaida = DateTime.Now
    });

    // 3. Tira o trator do pátio e desvincula da vaga (VagaId vira nulo)
    veiculo.EstaNoPatio = false;
    veiculo.VagaId = null;

    await db.SaveChangesAsync();
    return Results.Ok($"O {veiculo.Modelo} saiu com o {request.NomeCondutor}. A {vaga?.Identificacao} agora está LIVRE!");
});

// ROTA 3: Registrar Retorno (Estaciona em qualquer vaga livre)
app.MapPost("/patio/retorno/veiculo/{veiculoId}/vaga/{vagaId}", async (int veiculoId, int vagaId, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null || veiculo.EstaNoPatio)
        return Results.BadRequest("Veículo não encontrado ou já está no pátio.");

    // 1. Verifica se a vaga escolhida existe e se está realmente livre!
    var vagaEscolhida = await db.Vagas.FindAsync(vagaId);
    if (vagaEscolhida == null || vagaEscolhida.EstaOcupada)
        return Results.BadRequest("Esta vaga não existe ou já está ocupada por outro trator!");

    // 2. Fecha o histórico em aberto
    var movimentacao = await db.Movimentacoes.FirstOrDefaultAsync(m => m.VeiculoId == veiculoId && m.HorarioRetorno == null);
    if (movimentacao != null) movimentacao.HorarioRetorno = DateTime.Now;

    // 3. Atualiza o status do trator e da vaga
    veiculo.EstaNoPatio = true;
    veiculo.VagaId = vagaEscolhida.Id; // Vincula à nova vaga
    vagaEscolhida.EstaOcupada = true;  // Bloqueia a vaga

    await db.SaveChangesAsync();
    return Results.Ok($"Retorno registrado! O {veiculo.Modelo} estacionou na {vagaEscolhida.Identificacao}.");
});

// ROTA 4: Ver todos os veículos, seus detalhes e o status atual das vagas
app.MapGet("/patio/veiculos", async (AppDbContext db) =>
{
    // Busca todos os veículos no banco de dados
    var veiculos = await db.Veiculos.ToListAsync();

    return Results.Ok(veiculos);
});

// ROTA 5: Ver o histórico completo de movimentações (entradas e saídas)
app.MapGet("/patio/historico", async (AppDbContext db) =>
{
    // Busca o histórico e organiza pela data de saída (do mais recente pro mais antigo)
    var historico = await db.Movimentacoes
        .OrderByDescending(m => m.HorarioSaida)
        .ToListAsync();

    return Results.Ok(historico);
});

// GERENCIAR FROTA: Comprar/Adicionar um trator novo
app.MapPost("/patio/veiculos", async (NovoVeiculoRequest request, AppDbContext db) =>
{
    var novoVeiculo = new Veiculo
    {
        Modelo = request.Modelo,
        Placa = request.Placa,
        EstaNoPatio = false, // Por padrão, nasce fora do pátio
        VagaId = null
    };

    // Se informaram uma vaga na hora do cadastro, tenta estacionar ele lá
    if (request.VagaIdDesejada.HasValue)
    {
        var vaga = await db.Vagas.FindAsync(request.VagaIdDesejada.Value);
        if (vaga == null || vaga.EstaOcupada)
            return Results.BadRequest("A vaga desejada não existe ou já está ocupada!");

        novoVeiculo.EstaNoPatio = true;
        novoVeiculo.VagaId = vaga.Id;
        vaga.EstaOcupada = true; // Tranca a vaga
    }

    db.Veiculos.Add(novoVeiculo);
    await db.SaveChangesAsync();

    string status = novoVeiculo.EstaNoPatio ? $"estacionado na vaga {novoVeiculo.VagaId}" : "na rua";
    return Results.Ok($"Trator {novoVeiculo.Modelo} cadastrado com sucesso e está {status}!");
});

// GERENCIAR FROTA: Vender/Excluir um trator
app.MapDelete("/patio/veiculos/{veiculoId}", async (int veiculoId, AppDbContext db) =>
{
    var veiculo = await db.Veiculos.FindAsync(veiculoId);
    if (veiculo == null) return Results.NotFound("Veículo não encontrado.");

    // Se o trator for vendido enquanto estiver estacionado, precisamos liberar a vaga dele!
    if (veiculo.EstaNoPatio && veiculo.VagaId.HasValue)
    {
        var vaga = await db.Vagas.FindAsync(veiculo.VagaId.Value);
        if (vaga != null) vaga.EstaOcupada = false; // Libera o asfalto
    }

    db.Veiculos.Remove(veiculo);
    await db.SaveChangesAsync();
    return Results.Ok($"Trator {veiculo.Modelo} removido da frota definitivamente.");
});

// GERENCIAR VAGAS: Criar uma nova vaga física
app.MapPost("/patio/vagas", async (NovaVagaRequest request, AppDbContext db) =>
{
    var novaVaga = new Vaga { Identificacao = request.Identificacao, EstaOcupada = false };
    db.Vagas.Add(novaVaga);
    await db.SaveChangesAsync();
    return Results.Ok($"Vaga física '{novaVaga.Identificacao}' construída com sucesso! (ID: {novaVaga.Id})");
});

// ROTA 8: Ver todas as vagas físicas do pátio e seus status
app.MapGet("/patio/vagas", async (AppDbContext db) =>
{
    // Busca todas as vagas cadastradas no banco de dados
    var vagas = await db.Vagas.ToListAsync();

    return Results.Ok(vagas);
});

// GERENCIAR VAGAS: Demolir (Excluir) uma vaga
app.MapDelete("/patio/vagas/{vagaId}", async (int vagaId, AppDbContext db) =>
{
    var vaga = await db.Vagas.FindAsync(vagaId);
    if (vaga == null) return Results.NotFound("Vaga não encontrada.");

    // Regra de segurança: Não pode demolir uma vaga com um trator em cima!
    if (vaga.EstaOcupada) return Results.BadRequest("Não é possível excluir uma vaga que está ocupada.");

    db.Vagas.Remove(vaga);
    await db.SaveChangesAsync();
    return Results.Ok($"Vaga {vaga.Identificacao} removida do sistema.");
});


app.Run();

public record SaidaRequest(string NomeCondutor);
public record NovaVagaRequest(string Identificacao);
public record NovoVeiculoRequest(string Modelo, string Placa, int? VagaIdDesejada);


