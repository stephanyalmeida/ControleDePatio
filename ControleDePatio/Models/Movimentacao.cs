namespace ControleDePatio.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }

        // Avisa qual veículo/vaga (de 1 a 10) fez essa movimentação
        public int VeiculoId { get; set; }

        public string NomeCondutor { get; set; } = string.Empty;

        public DateTime HorarioSaida { get; set; }

        // O ponto de interrogação (?) é crucial aqui. Significa que pode ser Nulo.
        // Se estiver nulo, o trator ainda está na rua trabalhando!
        public DateTime? HorarioRetorno { get; set; }
    }
}
