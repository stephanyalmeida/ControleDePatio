namespace ControleDePatio.Models
{
    public class Movimentacao
    {
        public int Id { get; set; }

        public int VeiculoId { get; set; }

        public string NomeCondutor { get; set; } = string.Empty;

        public DateTime HorarioSaida { get; set; }

        public DateTime? HorarioRetorno { get; set; }
    }
}
